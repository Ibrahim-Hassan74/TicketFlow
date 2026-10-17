using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.IntegrationTests.Infrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            var apiProjectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../TicketFlow.API"));
            if (Directory.Exists(apiProjectPath))
            {
                builder.UseContentRoot(apiProjectPath);
            }
        }

        public async Task<HttpClient> CreateClientWithAuthAsync(ApplicationUser user)
        {
            var client = CreateClient();
            using var scope = Services.CreateScope();
            var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
            var authResponse = await jwtService.CreateJwtToken(user, rememberMe: false);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authResponse.Token);

            return client;
        }

        public async Task<(HttpClient Client, ApplicationUser User)> CreateClientWithRoleAsync(string role, string? email = null)
        {
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var normalizedRole = role.Trim();
            if (!await roleManager.RoleExistsAsync(normalizedRole))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = normalizedRole });
            }

            var uniqueEmail = email ?? $"{normalizedRole.ToLower()}_{Guid.NewGuid():N}@testflow.com";
            var user = await userManager.FindByEmailAsync(uniqueEmail);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = uniqueEmail,
                    Email = uniqueEmail,
                    DisplayName = $"Test {normalizedRole}",
                    EmailConfirmed = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var createResult = await userManager.CreateAsync(user, "TestP@ssw0rd123!");
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create test user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                await userManager.AddToRoleAsync(user, normalizedRole);
            }

            var client = await CreateClientWithAuthAsync(user);
            return (client, user);
        }
    }
}
