using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TodoAPI.Middleware;

namespace TodoAPI.Middleware
{
    public static class WSDependencyInjectionExtensions
    {
        public static IServiceCollection AddWSDependencies(this IServiceCollection services)
        {
            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(Hub)) {
                    services.TryAddScoped(type);
                }
            }
            return services;
        }
    }
}