using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.Application.Features.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<CreateUserCommandHandler> _logger;

        public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ILogger<CreateUserCommandHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Validate role is a valid enum value
            if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            {
                _logger.LogWarning("Failed to create user: Invalid role '{Role}'.", request.Role);
                throw new BadRequestException($"Invalid role '{request.Role}'. Valid roles: {string.Join(", ", Enum.GetNames<UserRole>())}");
            }

            // Check for duplicate email
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                _logger.LogWarning("Failed to create user: A user with email {Email} already exists.", request.Email);
                throw new ConflictException("A user with this email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName,
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("Failed to create user {Email} due to identity errors.", request.Email);
                var errors = createResult.Errors.Select(e => e.Description).ToArray();
                throw new IdentityOperationException("Failed to create user.", errors);
            }

            var roleName = role.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

            await _userManager.AddToRoleAsync(user, roleName);

            var response = new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName,
                Role = roleName,
                IsActive = !user.IsDeleted,
                CreatedAt = user.CreatedAt
            };

            _logger.LogInformation("Admin successfully created user {UserId} with role {RoleName}.", user.Id, roleName);

            return response;
        }
    }
}
