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

[TypeTranslator(typeof(CustomGenericTranslatedType<>))]
public class CustomGenericTypeTranslator : ITypeTranslator
{
    public void Translate(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        codeWriter.Append("{ genericProperty?: T }");
    }
}

[TranspilationSource]
public class CustomGenericTranslatedType<T>
{
    public T? GenericProperty { get; set; }
}


[TypeTranslator(typeof(CustomInheritedTranslatedType))]
public class CustomInheritedTranslatedTypeTranslator : ITypeTranslator
{
    public void Translate(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        codeWriter.Append("{ test: string | number }");
    }
}

[TranspilationSource]
public class CustomInheritedTranslatedType : CustomGenericTranslatedType<string>
{
}
