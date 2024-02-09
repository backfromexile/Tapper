using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Tapper;

internal class DefaultTypeConverterProvider : ITypeConverterProvider
{
    private readonly ITypeConverter _defaultConverter;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, ITypeConverter> _converters;

    public DefaultTypeConverterProvider(IReadOnlyDictionary<INamedTypeSymbol, ITypeConverter> converters)
    {
        _defaultConverter = new DefaultTypeConverter();
        _converters = converters;
    }

    public ITypeConverter GetTypeConverter(INamedTypeSymbol typeSymbol)
    {
        return TryGetCustomTypeConverter(typeSymbol, out var typeConverter)
            ? typeConverter
            : _defaultConverter;
    }

    public bool TryGetCustomTypeConverter(INamedTypeSymbol typeSymbol, [NotNullWhen(true)] out ITypeConverter? typeConverter)
    {
        if (_converters.TryGetValue(typeSymbol, out typeConverter)
            || _converters.TryGetValue(typeSymbol.ConstructedFrom, out typeConverter))
        {
            return true;
        }

        return false;
    }
}

