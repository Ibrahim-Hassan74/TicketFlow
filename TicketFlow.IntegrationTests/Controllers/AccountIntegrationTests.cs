using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TicketFlow.Application.Features.Authentication.Commands.Login;
using TicketFlow.Application.Features.Authentication.Commands.Register;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TicketFlow.IntegrationTests.Controllers
{
    public class AccountIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AccountIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturn200AndSuccessMessage()
        {
            // Arrange
            var client = _factory.CreateClient();
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var uniqueEmail = $"user_{uniqueId}@testflow.com";
            var command = new RegisterCommand
            {
                UserName = $"User_{uniqueId}",
                Email = uniqueEmail,
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/v1/account/register", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            result.Should().NotBeNull();
            result!.Message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturn409Conflict()
        {
            // Arrange
            var client = _factory.CreateClient();
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var uniqueEmail = $"dup_{uniqueId}@testflow.com";
            var command = new RegisterCommand
            {
                UserName = $"Dup_{uniqueId}",
                Email = uniqueEmail,
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // First registration
            var firstResponse = await client.PostAsJsonAsync("/api/v1/account/register", command);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act: second registration with same email
            var duplicateResponse = await client.PostAsJsonAsync("/api/v1/account/register", command);

            // Assert
            duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_ShouldReturn200AndJwtToken()
        {
            // Arrange
            var client = _factory.CreateClient();
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var uniqueEmail = $"login_{uniqueId}@testflow.com";
            var registerCommand = new RegisterCommand
            {
                UserName = $"Login_{uniqueId}",
                Email = uniqueEmail,
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var regResponse = await client.PostAsJsonAsync("/api/v1/account/register", registerCommand);
            regResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginCommand = new LoginCommand
            {
                Email = uniqueEmail,
                Password = "Password123!",
                RememberMe = false
            };

            // Act
            var loginResponse = await client.PostAsJsonAsync("/api/v1/account/login", loginCommand);

            // Assert
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
            auth.Should().NotBeNull();
            auth!.Token.Should().NotBeNullOrWhiteSpace();
            auth.Email.Should().Be(uniqueEmail);
        }

        [Fact]
        public async Task Login_WithIncorrectPassword_ShouldReturn401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var uniqueEmail = $"wrong_{uniqueId}@testflow.com";
            var registerCommand = new RegisterCommand
            {
                UserName = $"Wrong_{uniqueId}",
                Email = uniqueEmail,
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            await client.PostAsJsonAsync("/api/v1/account/register", registerCommand);

            var loginCommand = new LoginCommand
            {
                Email = uniqueEmail,
                Password = "WrongPassword999!",
                RememberMe = false
            };

            // Act
            var loginResponse = await client.PostAsJsonAsync("/api/v1/account/login", loginCommand);

            // Assert
            loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
