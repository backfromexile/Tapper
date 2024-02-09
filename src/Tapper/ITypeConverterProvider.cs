using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Tapper;

public interface ITypeConverterProvider
{
    bool TryGetCustomTypeConverter(INamedTypeSymbol typeSymbol, [NotNullWhen(true)] out ITypeConverter? typeConverter);
    ITypeConverter GetTypeConverter(INamedTypeSymbol typeSymbol);
}

