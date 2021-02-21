using System;
using Microsoft.Extensions.DependencyInjection;

namespace AndWeHaveAPlan.Mimic.AspExtensions
{
    public static class MimicAspExtensions
    {
        public static IServiceCollection AddScopedMimic<TInterface, TProtocol>(this IServiceCollection provider)
            where TProtocol : IMimicWorker
        {
            var mimic = new Mimic<TInterface, TProtocol>();
            provider.AddScoped(typeof(TInterface), mimic.Type);
            return provider;
        }
        
        public static IServiceCollection AddTransientMimic<TInterface, TProtocol>(this IServiceCollection provider)
            where TProtocol : IMimicWorker
        {
            var mimic = new Mimic<TInterface, TProtocol>();
            provider.AddTransient(typeof(TInterface), mimic.Type);
            return provider;
        }
        
        public static IServiceCollection AddSingletonMimic<TInterface, TProtocol>(this IServiceCollection provider)
            where TProtocol : IMimicWorker
        {
            var mimic = new Mimic<TInterface, TProtocol>();
            provider.AddSingleton(typeof(TInterface), mimic.Type);
            return provider;
        }
    }
}