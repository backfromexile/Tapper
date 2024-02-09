using Microsoft.CodeAnalysis;

namespace Tapper;

public interface ITypeConverter
{
    void Convert(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options);
}
