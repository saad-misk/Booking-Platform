using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingPlatform.Infrastructure.Persistence;
using BookingPlatform.Infrastructure.JWT;
using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Infrastructure.Services;
using BookingPlatform.Infrastructure.Services.Hotels;
using BookingPlatform.Infrastructure.Services.Bookings;
using BookingPlatform.Infrastructure.Services.Cities;
using BookingPlatform.Infrastructure.Services.Cart;
using BookingPlatform.Infrastructure.Services.Checkout;
using BookingPlatform.Application.Interfaces.HelperServices;
using BookingPlatform.Infrastructure.Services.Payments;
using BookingPlatform.Domain.Models;
using BookingPlatform.Infrastructure.Services.HelperServices;
using BookingPlatform.Application.Interfaces.Services.Admin;
using BookingPlatform.Infrastructure.Services.Admin;

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
            services.AddScoped<IAdminHotelsService, AdminHotelsService>();
            services.AddScoped<IAdminCitiesService, AdminCitiesService>();
            services.AddScoped<IAdminRoomsService, AdminRoomsService>();
            services.AddScoped<IAdminSearchService, AdminSearchService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IHotelsService, HotelsService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ISingleItemCheckoutService, CheckoutService>();
            services.AddScoped<ICitiesService, CitiesService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            
            services.AddScoped<IPaymentService, StripePaymentService>();
            
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}