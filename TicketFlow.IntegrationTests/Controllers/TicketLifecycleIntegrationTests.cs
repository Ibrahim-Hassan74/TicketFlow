using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Activities.DTOs;
using TicketFlow.Application.Features.Comments.DTOs;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Application.Features.TimeEntries.DTOs;
using TicketFlow.Domain.Enums;
using TicketFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketFlow.IntegrationTests.Controllers
{
    public class TicketLifecycleIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public TicketLifecycleIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task FullTicketLifecycle_E2E_Success()
        {
            // 1. Arrange Roles & Clients
            var (customerClient, customer) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Customer));
            var (adminClient, admin) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Admin));
            var (agentClient, agent) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.SupportAgent));

            // 2. Customer creates a ticket
            var createRequest = new CreateTicketRequest
            {
                Title = "E2E Integration Test Ticket",
                Description = "This is a detailed description for E2E testing.",
                Priority = TicketPriority.High
            };

            var createResponse = await customerClient.PostAsJsonAsync("/api/v1/tickets", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketResponse>();
            createdTicket.Should().NotBeNull();
            var ticketId = createdTicket!.Id;
            ticketId.Should().NotBeEmpty();
            createdTicket.Status.Should().Be(TicketStatus.Open.ToString());

            // 3. Customer lists their tickets
            var listResponse = await customerClient.GetAsync("/api/v1/tickets");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var paginated = await listResponse.Content.ReadFromJsonAsync<PaginatedResponse<TicketResponse>>();
            paginated.Should().NotBeNull();
            paginated!.Items.Should().Contain(t => t.Id == ticketId);

            // 4. Admin assigns the ticket to Support Agent
            var assignRequest = new AssignTicketRequest { AgentId = agent.Id };
            var assignResponse = await adminClient.PatchAsJsonAsync($"/api/v1/tickets/{ticketId}/assign", assignRequest);
            assignResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 5. Agent updates ticket status to InProgress
            var statusRequest = new UpdateTicketStatusRequest { Status = TicketStatus.InProgress };
            var statusResponse = await agentClient.PatchAsJsonAsync($"/api/v1/tickets/{ticketId}/status", statusRequest);
            statusResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 6. Customer adds a comment
            var commentRequest = new CreateCommentRequest { Content = "Hello, checking in on the progress." };
            var commentResponse = await customerClient.PostAsJsonAsync($"/api/v1/tickets/{ticketId}/comments", commentRequest);
            commentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 7. Verify comments via Dapper stored function fn_get_ticket_comments
            var getCommentsResponse = await customerClient.GetAsync($"/api/v1/tickets/{ticketId}/comments");
            getCommentsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var comments = await getCommentsResponse.Content.ReadFromJsonAsync<List<CommentResponse>>();
            comments.Should().NotBeNull();
            comments!.Should().Contain(c => c.Content == "Hello, checking in on the progress.");

            // 8. Agent logs a time entry
            var timeEntryRequest = new CreateTimeEntryRequest
            {
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 45,
                Description = "Investigated ticket issue in test environment."
            };
            var timeEntryResponse = await agentClient.PostAsJsonAsync($"/api/v1/tickets/{ticketId}/time-entries", timeEntryRequest);
            timeEntryResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 9. Verify time entries via Dapper stored function fn_get_ticket_time_entries
            var getTimeEntriesResponse = await agentClient.GetAsync($"/api/v1/tickets/{ticketId}/time-entries");
            getTimeEntriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var timeEntries = await getTimeEntriesResponse.Content.ReadFromJsonAsync<TicketTimeEntriesResponse>();
            timeEntries.Should().NotBeNull();
            timeEntries!.TotalDurationMinutes.Should().BeGreaterThanOrEqualTo(45);
            timeEntries.Entries.Should().Contain(e => e.Description == "Investigated ticket issue in test environment.");

            // 10. Verify activity audit trail via Dapper stored function fn_get_ticket_activities
            var getActivitiesResponse = await customerClient.GetAsync($"/api/v1/tickets/{ticketId}/activities");
            getActivitiesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var activities = await getActivitiesResponse.Content.ReadFromJsonAsync<List<ActivityResponse>>();
            activities.Should().NotBeNull();
            activities!.Should().Contain(a => a.Type == ActivityType.Created.ToString());
            activities.Should().Contain(a => a.Type == ActivityType.AgentAssigned.ToString());
            activities.Should().Contain(a => a.Type == ActivityType.StatusChanged.ToString());
            activities.Should().Contain(a => a.Type == ActivityType.CommentAdded.ToString());
        }
    }
}
