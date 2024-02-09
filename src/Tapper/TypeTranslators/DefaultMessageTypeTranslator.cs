using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Tapper.TypeTranslators;

// The name "Message" is derived from the protobuf message.
// In other words, user defined type.
internal class DefaultMessageTypeTranslator : ITypeTranslator
{
    public void Translate(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        var newLineString = options.NewLine.ToNewLineString();

        codeWriter.Append($"/** Transpiled from {typeSymbol.OriginalDefinition.ToDisplayString()} */{newLineString}");
        codeWriter.Append($"export type {TypeTranslatorHelper.GetGenericTypeName(typeSymbol)} = ");

        var typeConverter = options.TypeConverterProvider.GetTypeConverter(typeSymbol);
        typeConverter.Convert(ref codeWriter, typeSymbol, options);

        if (typeSymbol.BaseType is not null)
        {
            if (MessageTypeTranslatorHelper.IsSourceType(typeSymbol.BaseType, options)
            || options.TypeConverterProvider.TryGetCustomTypeConverter(typeSymbol.BaseType, out _))
            {
                codeWriter.Append($" & {TypeTranslatorHelper.GetConcreteTypeName(typeSymbol.BaseType, options)}");
            }
        }

        codeWriter.Append(';');

        codeWriter.Append(newLineString);
    }
}

file static class MessageTypeTranslatorHelper
{
    public static bool IsSourceType([NotNullWhen(true)] INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        if (typeSymbol.SpecialType != SpecialType.System_Object)
        {
            if (options.SourceTypes.Contains(typeSymbol.ConstructedFrom, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        return false;
    }
}
