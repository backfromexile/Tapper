using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tapper.Core.Transpilers;
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
        services.AddTranspilationRootTranspilers(assemblies);

        return services;
    }

    private static void AddTranspilationRootTranspilers(this IServiceCollection services, IReadOnlyList<Assembly> assemblies)
    {
        var transpilationRootTypes = GetTranspilationRootTypes(assemblies);

        services.AddScoped<ITranspilationRootTypesProvider>(serviceProvider => new TranspilationRootTypesProvider(transpilationRootTypes));

        foreach (var transpilationRootType in transpilationRootTypes)
        {
            var transpilerImplementationType = typeof(ObjectTypeTranspiler<>).MakeGenericType(transpilationRootType);

            var typeDefTranspilerServiceType = typeof(ITypeDefinitionTranspiler<>).MakeGenericType(transpilationRootType);
            services.AddScoped(typeDefTranspilerServiceType, transpilerImplementationType);

            var typeRefTranspilerServiceType = typeof(ITypeReferenceTranspiler<>).MakeGenericType(transpilationRootType);
            services.AddScoped(typeRefTranspilerServiceType, transpilerImplementationType);
        }
    }

    private static void AddTranspilationProviders(this IServiceCollection services, IReadOnlyList<Assembly> assemblies)
    {
        services.AddScoped<ITypeTranspilerProvider, DefaultTypeTranspilerProvider>();

        services.AddScoped<ITranspilationRootAssembliesProvider>(
            serviceProvider => ActivatorUtilities.CreateInstance<TranspilationRootAssembliesProvider>(serviceProvider, assemblies)
        );
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
        var customTypeDefTranspilerTypes = GetCustomTypeDefinitionTranspilers(assembly);
        foreach (var (typeDefTranspiler, transpiledSourceType) in customTypeDefTranspilerTypes)
        {
            var serviceType = typeof(ITypeDefinitionTranspiler<>).MakeGenericType(transpiledSourceType);

            services.AddScoped(serviceType, typeDefTranspiler);
        }

        var customTypeRefTranspilerTypes = GetCustomTypeReferenceTranspilers(assembly);
        foreach (var (typeRefTranspilerType, transpiledSourceType) in customTypeRefTranspilerTypes)
        {
            var serviceType = typeof(ITypeReferenceTranspiler<>).MakeGenericType(transpiledSourceType);

            services.AddScoped(serviceType, typeRefTranspilerType);
        }

        return services;
    }

    public static IServiceCollection AddDefaultTypeTranspilers(this IServiceCollection services)
    {
        return services.AddTranspilers(_defaultTranspilersAssembly);
    }

    private static IReadOnlyList<(Type typeDefinitionTranspilerType, Type transpiledSourceType)> GetCustomTypeDefinitionTranspilers(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsGenericType)
            .Select(type => new
            {
                type,
                implementedInterfaceTypes = type.GetInterfaceImplementations(typeof(ITypeDefinitionTranspiler<>)),
            })
            .Where(x => x.implementedInterfaceTypes.Length == 1)
            .Select(x => (x.type, x.implementedInterfaceTypes[0].GetGenericArguments()[0]))
            .ToImmutableList();
    }

    private static IReadOnlyList<(Type typeReferenceTranspilerType, Type transpiledSourceType)> GetCustomTypeReferenceTranspilers(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsGenericType)
            .Select(type => new
            {
                type,
                implementedInterfaceTypes = type.GetInterfaceImplementations(typeof(ITypeReferenceTranspiler<>)),
            })
            .Where(x => x.implementedInterfaceTypes.Length == 1)
            .Select(x => (x.type, x.implementedInterfaceTypes[0].GetGenericArguments()[0]))
            .ToImmutableList();
    }

    private static IReadOnlyList<Type> GetTranspilationRootTypes(IReadOnlyList<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(assembly => GetTranspilationRootTypes(assembly))
            .ToImmutableList();
    }

    private static IEnumerable<Type> GetTranspilationRootTypes(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type => type.GetCustomAttribute<TranspilationRootAttribute>() is not null);
    }
}
