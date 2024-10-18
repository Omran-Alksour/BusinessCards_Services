using Domain.Abstractions;
using Domain.Entities;
using Domain.Entities.BusinessCard;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories;

namespace Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("Database");

            _ = services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                _ = options.UseSqlServer(connectionString, b => b.MigrationsAssembly("APIs"));
            });

            _ = services.AddIdentity<BusinessCard, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            _ = services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());

            _ = services.AddScoped<IUnitOfWork, UnitOfWork>();

            _ = services.AddHttpContextAccessor();

            return services;
        }
    }
}