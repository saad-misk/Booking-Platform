using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingPlatform.Infrastructure.Persistence;
using BookingPlatform.Infrastructure.JWT;
using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Infrastructure.Services;

namespace BookingPlatform.Infrastructure{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services
                .AddPersistence(configuration)        // Database, Repositories, UoW
                .AddAuthInfrastructure(configuration) // JWT, Auth Services
                .AddServices(configuration);          // Application services
            return services;
        }

        private static IServiceCollection AddServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            return services;
        }
    }
}