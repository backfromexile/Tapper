using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Tapper.Test.SourceTypes;

[TypeTranslator(typeof(CustomTranslatedType))]
public class CustomTypeTranslator : ITypeTranslator
{
    public void Translate(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        codeWriter.Append("{ customProperty: any }");
    }
}

[TranspilationSource]
public class CustomTranslatedType
{

}
