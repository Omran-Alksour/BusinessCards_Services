using Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
//using Quartz;

namespace Infrastructure;

public static class DependencyInjection
{
    [Obsolete]
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
 

        return services;
    }
}