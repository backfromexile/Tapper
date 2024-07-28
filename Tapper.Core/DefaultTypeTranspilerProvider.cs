using Microsoft.Extensions.DependencyInjection;
using Tapper.Core.Transpilers;

namespace Tapper.Core;

internal class DefaultTypeTranspilerProvider : ITypeTranspilerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITranspilationRootTypesProvider _typesProvider;

    public DefaultTypeTranspilerProvider(IServiceProvider serviceProvider, ITranspilationRootTypesProvider typesProvider)
    {
        _serviceProvider = serviceProvider;
        _typesProvider = typesProvider;
    }

    public ITypeTranspiler<T> GetTypeTranspiler<T>()
    {
        var registeredTranspiler = _serviceProvider.GetService<ITypeTranspiler<T>>();

        if (registeredTranspiler is not null)
            return registeredTranspiler;

        var type = typeof(T);
        if (type.IsEnum)
        {
            var enumTranspiler = typeof(EnumTypeTranspiler<>).MakeGenericType(type);
            return (ITypeTranspiler<T>)ActivatorUtilities.CreateInstance(_serviceProvider, enumTranspiler);
        }

        var objectTranspilerType = typeof(ObjectTypeTranspiler<>).MakeGenericType(type);
        var transpiler = (ITypeTranspiler<T>)ActivatorUtilities.CreateInstance(_serviceProvider, objectTranspilerType);

        return transpiler;
    }
}
