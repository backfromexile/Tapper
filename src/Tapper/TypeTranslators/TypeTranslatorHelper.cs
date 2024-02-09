using System.Linq;
using Microsoft.CodeAnalysis;

namespace Tapper.TypeTranslators;

internal static class TypeTranslatorHelper
{
    public static string GetConcreteTypeName(INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        var genericTypeArguments = "";
        if (typeSymbol.IsGenericType)
        {
            var mappedGenericTypeArguments = typeSymbol.TypeArguments.Select(typeArg =>
            {
                var mapper = options.TypeMapperProvider.GetTypeMapper(typeArg);
                return mapper.MapTo(typeArg, options);
            });
            genericTypeArguments = $"<{string.Join(", ", mappedGenericTypeArguments)}>";
        }

        return $"{typeSymbol.Name}{genericTypeArguments}";
    }

    public static string GetGenericTypeName(INamedTypeSymbol typeSymbol)
    {
        var genericTypeParameters = "";
        if (typeSymbol.IsGenericType)
        {
            genericTypeParameters = $"<{string.Join(", ", typeSymbol.TypeParameters.Select(param => param.Name))}>";
        }

        return $"{typeSymbol.Name}{genericTypeParameters}";
    }

}
