using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Contracts;
using TicketFlow.Infrastructure.Persistence.Repository;
using TicketFlow.Infrastructure.Services;

namespace TicketFlow.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IJwtService, JwtService>();

            services.AddScoped<IDashboardRepository, DashboardRepository>();

            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
