using Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddMediatR(config =>
        {
            _ = config.RegisterServicesFromAssembly(AssemblyReference.Assembly);
            _ = config.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });
        return services;
    }
}