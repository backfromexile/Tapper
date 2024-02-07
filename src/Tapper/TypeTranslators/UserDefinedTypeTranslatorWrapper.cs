using Microsoft.CodeAnalysis;

namespace Tapper.TypeTranslators;

internal class UserDefinedTypeTranslatorWrapper : ITypeTranslator
{
    private readonly ITypeTranslator _userDefinedTypeTranslator;

    public UserDefinedTypeTranslatorWrapper(ITypeTranslator userDefinedTypeTranslator)
    {
        _userDefinedTypeTranslator = userDefinedTypeTranslator;
    }

    public void Translate(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        var newLineString = options.NewLine.ToNewLineString();

        codeWriter.Append($"/** Transpiled from {typeSymbol.OriginalDefinition.ToDisplayString()} */{newLineString}");
        codeWriter.Append($"export type {TypeTranslatorHelper.GetGenericTypeName(typeSymbol)} = ");

        _userDefinedTypeTranslator.Translate(ref codeWriter, typeSymbol, options);

        codeWriter.Append(newLineString);
    }
}
