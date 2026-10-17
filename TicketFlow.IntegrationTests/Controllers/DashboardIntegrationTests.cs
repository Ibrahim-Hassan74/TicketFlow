using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TicketFlow.Application.Common.Models;
using TicketFlow.Domain.Enums;
using TicketFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketFlow.IntegrationTests.Controllers
{
    public class DashboardIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public DashboardIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetStats_WhenUnauthenticated_ShouldReturn401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/dashboard/stats");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetStats_WhenCustomer_ShouldReturn403Forbidden()
        {
            // Arrange
            var (client, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Customer));

            // Act
            var response = await client.GetAsync("/api/v1/dashboard/stats");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetStats_WhenAdmin_ShouldReturn200AndValidStatsFromPostgreSQL()
        {
            // Arrange
            var (client, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Admin));

            // Act
            var response = await client.GetAsync("/api/v1/dashboard/stats");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var stats = await response.Content.ReadFromJsonAsync<DashboardStatsResponse>();
            stats.Should().NotBeNull();
            stats!.TotalTickets.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetAgentWorkload_WhenAdmin_ShouldReturn200AndWorkloadList()
        {
            // Arrange
            var (client, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Admin));

            // Act
            var response = await client.GetAsync("/api/v1/dashboard/agent-workload");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var workload = await response.Content.ReadFromJsonAsync<List<AgentWorkloadResponse>>();
            workload.Should().NotBeNull();
        }

        [Fact]
        public async Task GetTicketTrends_WhenAdmin_ShouldReturn200AndTrendList()
        {
            // Arrange
            var (client, _) = await _factory.CreateClientWithRoleAsync(nameof(UserRole.Admin));

            // Act
            var response = await client.GetAsync("/api/v1/dashboard/ticket-trends?days=7");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var trends = await response.Content.ReadFromJsonAsync<List<TicketTrendResponse>>();
            trends.Should().NotBeNull();
            trends!.Count.Should().Be(7);
        }
    }
}
