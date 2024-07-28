using Microsoft.Extensions.DependencyInjection;
using Tapper.Core.Transpilers;

namespace Tapper.Core;

internal class DefaultTypeTranspilerProvider : ITypeTranspilerProvider
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultTypeTranspilerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ITypeDefinitionTranspiler<T> GetTypeDefinitionTranspiler<T>()
    {
        var registeredTranspiler = _serviceProvider.GetService<ITypeDefinitionTranspiler<T>>();

        if (registeredTranspiler is not null)
            return registeredTranspiler;

        var type = typeof(T);
        if (type.IsEnum)
        {
            var enumTranspiler = typeof(EnumTypeTranspiler<>).MakeGenericType(type);
            return (ITypeDefinitionTranspiler<T>)ActivatorUtilities.CreateInstance(_serviceProvider, enumTranspiler);
        }

        throw new InvalidOperationException();
    }

    public ITypeReferenceTranspiler<T> GetTypeReferenceTranspiler<T>()
    {
        var registeredTranspiler = _serviceProvider.GetService<ITypeReferenceTranspiler<T>>();

        if (registeredTranspiler is not null)
            return registeredTranspiler;

        var type = typeof(T);
        if (type.IsEnum)
        {
            var enumTranspiler = typeof(EnumTypeTranspiler<>).MakeGenericType(type);
            return (ITypeReferenceTranspiler<T>)ActivatorUtilities.CreateInstance(_serviceProvider, enumTranspiler);
        }

        throw new InvalidOperationException();
    }
}
