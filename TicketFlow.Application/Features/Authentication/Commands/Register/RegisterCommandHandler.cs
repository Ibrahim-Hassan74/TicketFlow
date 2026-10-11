using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.Domain.Domain.IdentityEntities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.Application.Features.Authentication.Commands.Register
{
    public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RegisterCommandHandler(UserManager<ApplicationUser> userManager, ILogger<RegisterCommandHandler> logger, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (await _userManager.FindByEmailAsync(request.Email) is not null)
            {
                _logger.LogWarning("Registration failed: Email {Email} is already registered.", request.Email);
                throw new EmailAlreadyExistsException("This email is already in use.");
            }

            ApplicationUser user = new ApplicationUser()
            {
                DisplayName = request.UserName,
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.Phone,
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.Now
            };

            IdentityResult result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for email {Email} due to identity errors.", request.Email);

                throw new RegistrationFailedException(result.Errors.Select(e => e.Description).ToArray());
            }

            if(!await _roleManager.RoleExistsAsync(UserRole.Customer.ToString()))
                await _roleManager.CreateAsync(new ApplicationRole { Name = UserRole.Customer.ToString() });

            await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());

            _logger.LogInformation("User {UserId} registered successfully with email {Email}.", user.Id, user.Email);

            return new RegisterResponse { Message = "Registration successful." };
        }
    }
}
