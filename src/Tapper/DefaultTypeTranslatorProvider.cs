using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Tapper.TypeTranslators;

namespace Tapper;

internal class DefaultTypeTranslatorProvider : ITypeTranslatorProvider
{
    private readonly ITypeTranslator _messageTypeTranslator;
    private readonly ITypeTranslator _enumTypeTranslator;
    private readonly IReadOnlyDictionary<INamedTypeSymbol, ITypeTranslator>? _customTypeTranslators;

    public DefaultTypeTranslatorProvider(ITypeTranslator messageTypeTranslator, ITypeTranslator enumTypeTranslator, IReadOnlyDictionary<INamedTypeSymbol, ITypeTranslator>? customTypeTranslators = null)
    {
        _messageTypeTranslator = messageTypeTranslator;
        _enumTypeTranslator = enumTypeTranslator;
        _customTypeTranslators = customTypeTranslators;
    }

    public ITypeTranslator GetTypeTranslator(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            return _enumTypeTranslator;
        }

        if (_customTypeTranslators?.TryGetValue(typeSymbol, out var userDefinedTypeTranslator) ?? false)
        {
            return new UserDefinedTypeTranslatorWrapper(userDefinedTypeTranslator);
        }

        return _messageTypeTranslator;
    }
}
