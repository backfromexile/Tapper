using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Tapper.Test.SourceTypes;

[TypeConverter(typeof(CustomConvertedType))]
public class CustomTypeConverter : ITypeConverter
{
    public void Convert(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        codeWriter.Append("{ customProperty: any }");
    }
}

[TranspilationSource]
public class CustomConvertedType
{

}

[TypeConverter(typeof(CustomGenericConvertedType<>))]
public class CustomGenericTypeConverter : ITypeConverter
{
    public void Convert(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        codeWriter.Append("{ genericProperty?: T }");
    }
}

[TranspilationSource]
public class CustomGenericConvertedType<T>
{
    public T? GenericProperty { get; set; }
}


[TypeConverter(typeof(CustomInheritedConvertedType))]
public class CustomInheritedConvertedTypeTranslator : ITypeConverter
{
    public void Convert(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        codeWriter.Append("{ test: string | number }");
    }
}

[TranspilationSource]
public class CustomInheritedConvertedType : CustomGenericConvertedType<string>
{
}
