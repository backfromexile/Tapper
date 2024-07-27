using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Tapper.Core;

internal class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);


        var assemblyPath = @"F:\github\Tapper\AspNetCoreSandbox\bin\Debug\net8.0\AspNetCoreSandbox.dll";

        var rootAssembly = Assembly.LoadFrom(assemblyPath);

        var transpilerOptions = new DingsOptions()
        {
            IncludeReferencedAssemblies = false,
        };
        var transpiler = new Dings(rootAssembly, transpilerOptions);

        transpiler.DoDings(builder.Services);
    }
}


public interface ITypeTranspiler
{
    string Transpile();
}
public interface ITypeTranspiler<T> : ITypeTranspiler
{

}

class DingsOptions
{
    public bool IncludeReferencedAssemblies { get; init; }
}

internal static class ReflectionExtensions
{
    public static Type[] GetInterfaceImplementations(this Type type, Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException("Must be an interface type");

        var implementedInterfaces = type.GetInterfaces();

        if (interfaceType.IsGenericType)
            return implementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                .ToArray();

        return implementedInterfaces
            .Where(i => i == interfaceType)
            .ToArray();
    }
}

internal class Dings
{
    private readonly Assembly _rootAssembly;
    private readonly DingsOptions _options;

    public Dings(Assembly rootAssembly, DingsOptions options)
    {
        _rootAssembly = rootAssembly;
        _options = options;
    }

    public void DoDings(IServiceCollection services)
    {
        var assemblies = GetAssemblies();

        //TODO: register default type transpilers

        //TODO: register custom type transpilers (TypeTranspiler<T>)
        var customTypeTranspilerTypes = GetCustomTypeTranspilers(assemblies);
        foreach (var (customTypeTranspilerType, targetType) in customTypeTranspilerTypes)
        {
            var serviceType = typeof(ITypeTranspiler<>).MakeGenericType(targetType);

            services.Replace(ServiceDescriptor.Scoped(serviceType, customTypeTranspilerType));
        }

        var transpilationRootTypes = GetTranspilationRootTypes(assemblies);
        //TODO: register default transpilers for marked types
    }

    private IReadOnlyList<(Type transpilerType, Type transpiledTargetType)> GetCustomTypeTranspilers(IReadOnlyList<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(assembly =>
                assembly
                    .GetTypes()
                    .Select(type => new
                    {
                        type,
                        implementedInterfaceTypes = type.GetInterfaceImplementations(typeof(ITypeTranspiler<>)),
                    })
            )
            .Where(x => x.implementedInterfaceTypes.Length == 1)
            .Select(x => (x.type, x.implementedInterfaceTypes[0].GetGenericArguments()[0]))
            .ToImmutableList();
    }

    private IReadOnlyList<Assembly> GetAssemblies()
    {
        if (!_options.IncludeReferencedAssemblies)
            return [_rootAssembly];

        return _rootAssembly.GetReferencedAssemblies()
            .Select(assemblyName => Assembly.Load(assemblyName))
            .Prepend(_rootAssembly)
            .ToImmutableList();
    }

    private IReadOnlyList<Type> GetTranspilationRootTypes(IReadOnlyList<Assembly> assemblies)
    {
        return assemblies.SelectMany(
            assembly => assembly.GetTypes().Where(
                type => type.GetCustomAttribute<TranspilationRootAttribute>() is not null
            )
        ).ToImmutableList();
    }
}
