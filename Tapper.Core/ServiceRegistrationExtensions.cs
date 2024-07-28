using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tapper.Core.Transpilers.System;

namespace Tapper.Core;

public static class ServiceRegistrationExtensions
{
    private static readonly Assembly _defaultTranspilersAssembly = typeof(StringTranspiler).Assembly;

    public static IServiceCollection AddTranspilation(this IServiceCollection services, Assembly assembly, bool includeReferencedAssemblies = false)
    {
        if (!includeReferencedAssemblies)
            return AddTranspilation(services, [assembly]);

        var assemblies = assembly
            .GetReferencedAssemblies()
            .Select(assemblyName => Assembly.Load(assemblyName))
            .Except([_defaultTranspilersAssembly])
            .Append(assembly)
            .ToImmutableList();

        return AddTranspilation(services, assemblies);
    }

    public static IServiceCollection AddTranspilation(this IServiceCollection services, IReadOnlyList<Assembly> assemblies)
    {
        services.AddScoped<ITranspilationRunner, TranspilationRunner>();

        services.AddTranspilationProviders(assemblies);

        services.AddDefaultTypeTranspilers();
        services.AddTypeTranspilers(assemblies);

        return services;
    }

    private static void AddTranspilationProviders(this IServiceCollection services, IReadOnlyList<Assembly> assemblies)
    {
        services.AddScoped<ITypeTranspilerProvider, DefaultTypeTranspilerProvider>();

        services.AddScoped<ITranspilationRootAssembliesProvider>(
            serviceProvider => ActivatorUtilities.CreateInstance<TranspilationRootAssembliesProvider>(serviceProvider, assemblies)
        );

        services.AddScoped<ITranspilationRootTypesProvider, TranspilationRootTypesProvider>();
    }

    public static IServiceCollection AddTypeTranspilers(this IServiceCollection services, IReadOnlyList<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            services.AddTranspilers(assembly);
        }

        return services;
    }

    public static IServiceCollection AddTranspilers(this IServiceCollection services, Assembly assembly)
    {
        var customTranspilerTypes = GetCustomTypeTranspilers(assembly);

        foreach (var (transpilerType, transpiledSourceType) in customTranspilerTypes)
        {
            var serviceType = typeof(ITypeTranspiler<>).MakeGenericType(transpiledSourceType);

            services.AddScoped(serviceType, transpilerType);
        }

        return services;
    }

    public static IServiceCollection AddDefaultTypeTranspilers(this IServiceCollection services)
    {
        return services.AddTranspilers(_defaultTranspilersAssembly);
    }

    private static IReadOnlyList<(Type transpilerType, Type transpiledSourceType)> GetCustomTypeTranspilers(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsGenericType)
            .Select(type => new
            {
                type,
                implementedInterfaceTypes = type.GetInterfaceImplementations(typeof(ITypeTranspiler<>)),
            })
            .Where(x => x.implementedInterfaceTypes.Length == 1)
            .Select(x => (x.type, x.implementedInterfaceTypes[0].GetGenericArguments()[0]))
            .ToImmutableList();
    }
}
