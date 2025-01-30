using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.DBContext;
using BookingPlatform.Infrastructure.Persistence.Repositories;
using BookingPlatform.Domain.Interfaces.Persistence;

namespace BookingPlatform.Infrastructure.Persistence{
  public static class PersistenceConfiguration
  {
    public static IServiceCollection AddPersistence(
      this IServiceCollection services, 
      IConfiguration configuration)
    {
      services.AddDbContext(configuration)
        .AddPasswordHashing()
        .AddRepositories();
      
      return services;
    }

    private static IServiceCollection AddDbContext(
      this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddDbContext<AppDbContext>(options =>
        { 
            options.UseSqlServer(connectionString,
                optionsBuilder => optionsBuilder.EnableRetryOnFailure(2));
        });    
        return services;
    }

    private static IServiceCollection AddPasswordHashing(this IServiceCollection services)
    {
      services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

      return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();      
        services.AddScoped<IUserRepository, UserRepository>();
        return services;

    }
    
  }
}