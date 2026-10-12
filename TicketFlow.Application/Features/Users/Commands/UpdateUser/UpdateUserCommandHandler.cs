using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Data;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.Application.Features.Users.Commands.UpdateUser
{
    public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UpdateUserCommandHandler> _logger;
        public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager, ILogger<UpdateUserCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
            {
                _logger.LogWarning("Failed to update user: User {UserId} not found.", request.Id);
                throw new NotFoundException("User not found.");
            }

            user.DisplayName = request.UpdateUser.DisplayName;
            user.IsDeleted = !request.UpdateUser.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogWarning("Failed to update user {UserId} due to identity errors.", request.Id);
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                throw new RegistrationFailedException("Failed to update user.", errors);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? "";
            var response = new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName,
                Role = roleName,
                IsActive = !user.IsDeleted,
                CreatedAt = user.CreatedAt
            };

            _logger.LogInformation("Admin successfully updated user {UserId}.", user.Id);
            return response;
        }
    }
}
