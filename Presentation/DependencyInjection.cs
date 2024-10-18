using Microsoft.Extensions.DependencyInjection;

namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        _ = services.AddControllers().AddApplicationPart(AssemblyReference.Assembly);
        return services;
    }
}