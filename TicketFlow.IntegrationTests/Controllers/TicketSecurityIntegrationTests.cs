using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Comments.DTOs;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Application.Features.TimeEntries.DTOs;
using TicketFlow.Domain.Enums;
using TicketFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketFlow.IntegrationTests.Controllers
{
    public class TicketSecurityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public TicketSecurityIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task IDOR_CustomerCannotAccessAnotherCustomersTicket()
        {
            // Arrange: Two distinct customers
            var (clientCustomerA, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Customer));
            var (clientCustomerB, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Customer));

            // Customer A creates Ticket A
            var createRequest = new CreateTicketRequest
            {
                Title = "Customer A Private Ticket",
                Description = "Confidential customer details.",
                Priority = TicketPriority.Medium
            };
            var createResp = await clientCustomerA.PostAsJsonAsync("/api/v1/tickets", createRequest);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var ticketA = await createResp.Content.ReadFromJsonAsync<TicketResponse>();
            var ticketAId = ticketA!.Id;

            // Act: Customer B tries to view Ticket A by ID
            var getByIdResp = await clientCustomerB.GetAsync($"/api/v1/tickets/{ticketAId}");

            // Assert: Must return 404 Not Found (IDOR prevention)
            getByIdResp.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Act: Customer B lists their tickets
            var listResp = await clientCustomerB.GetAsync("/api/v1/tickets");
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var paginated = await listResp.Content.ReadFromJsonAsync<PaginatedResponse<TicketResponse>>();

            // Assert: Customer B's list must not contain Ticket A
            paginated!.Items.Should().NotContain(t => t.Id == ticketAId);

            // Act: Customer B tries to comment on Ticket A
            var commentResp = await clientCustomerB.PostAsJsonAsync($"/api/v1/tickets/{ticketAId}/comments", new CreateCommentRequest
            {
                Content = "Sneaky comment attempt."
            });

            // Assert: Must return 404 Not Found
            commentResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Security_CustomerCannotAccessTimeEntriesEndpoint()
        {
            // Arrange
            var (customerClient, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Customer));
            var dummyTicketId = Guid.NewGuid();

            // Act: Customer attempts to get time entries (Endpoint restricted to Admin and SupportAgent)
            var response = await customerClient.GetAsync($"/api/v1/tickets/{dummyTicketId}/time-entries");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Security_UnassignedAgentCannotViewOrLogTimeEntries()
        {
            // Arrange: Customer creates a ticket, Agent 1 assigned, Agent 2 unassigned
            var (customerClient, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Customer));
            var (adminClient, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Admin));
            var (assignedAgentClient, assignedAgent) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.SupportAgent));
            var (unassignedAgentClient, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.SupportAgent));

            var createResp = await customerClient.PostAsJsonAsync("/api/v1/tickets", new CreateTicketRequest
            {
                Title = "Agent Assignment Test Ticket",
                Description = "Testing agent scoping",
                Priority = TicketPriority.Low
            });
            var ticket = await createResp.Content.ReadFromJsonAsync<TicketResponse>();
            var ticketId = ticket!.Id;

            // Admin assigns only Agent 1
            await adminClient.PatchAsJsonAsync($"/api/v1/tickets/{ticketId}/assign", new AssignTicketRequest
            {
                AgentId = assignedAgent.Id
            });

            // Act: Unassigned Agent 2 attempts to view time entries
            var getTimeResp = await unassignedAgentClient.GetAsync($"/api/v1/tickets/{ticketId}/time-entries");

            // Assert: Must return 404 Not Found (not assigned to you)
            getTimeResp.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Act: Unassigned Agent 2 attempts to log a time entry
            var addTimeResp = await unassignedAgentClient.PostAsJsonAsync($"/api/v1/tickets/{ticketId}/time-entries", new CreateTimeEntryRequest
            {
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 30,
                Description = "Unauthorized time entry attempt"
            });

            // Assert: Must return 404 Not Found
            addTimeResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
