using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TicketFlow.Application.Common.Behaviors;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Services;

namespace TicketFlow.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(assembly);
                options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            services.AddScoped<ITicketAuthorizationService, TicketAuthorizationService>();

            return services;
        }
    }
}
