using Tapper.Test.SourceTypes;
using Xunit;
using Xunit.Abstractions;

namespace Tapper.Tests;

public class CustomTypeConvertersTest
{
    private readonly ITestOutputHelper _output;

    public CustomTypeConvertersTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Test_CustomTranslatedType()
    {
        var compilation = CompilationSingleton.Compilation;

        var options = new TranspilationOptions(
            compilation,
            SerializerOption.Json,
            NamingStyle.None,
            EnumStyle.Value,
            NewLineOption.Lf,
            4,
            false,
            true
        );

        var codeGenerator = new TypeScriptCodeGenerator(compilation, options);

        var type = typeof(CustomConvertedType);
        var typeSymbol = compilation.GetTypeByMetadataName(type.FullName!)!;

        var writer = new CodeWriter();

        codeGenerator.AddType(typeSymbol, ref writer);

        var code = writer.ToString();
        var gt = @"/** Transpiled from Tapper.Test.SourceTypes.CustomConvertedType */
export type CustomConvertedType = { customProperty: any };
";

        _output.WriteLine(code);
        _output.WriteLine(gt);

        Assert.Equal(gt, code, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Test_CustomGenericTranslatedType()
    {
        var compilation = CompilationSingleton.Compilation;

        var options = new TranspilationOptions(
            compilation,
            SerializerOption.Json,
            NamingStyle.None,
            EnumStyle.Value,
            NewLineOption.Lf,
            4,
            false,
            true
        );

        var codeGenerator = new TypeScriptCodeGenerator(compilation, options);

        var type = typeof(CustomGenericConvertedType<>);
        var typeSymbol = compilation.GetTypeByMetadataName(type.FullName!)!;

        var writer = new CodeWriter();

        codeGenerator.AddType(typeSymbol, ref writer);

        var code = writer.ToString();
        var gt = @"/** Transpiled from Tapper.Test.SourceTypes.CustomGenericConvertedType<T> */
export type CustomGenericConvertedType<T> = { genericProperty?: T };
";

        _output.WriteLine(code);
        _output.WriteLine(gt);

        Assert.Equal(gt, code, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Test_CustomInheritedTranslatedType()
    {
        var compilation = CompilationSingleton.Compilation;

        var options = new TranspilationOptions(
            compilation,
            SerializerOption.Json,
            NamingStyle.None,
            EnumStyle.Value,
            NewLineOption.Lf,
            4,
            false,
            true
        );

        var codeGenerator = new TypeScriptCodeGenerator(compilation, options);

        var type = typeof(CustomInheritedConvertedType);
        var typeSymbol = compilation.GetTypeByMetadataName(type.FullName!)!;

        var writer = new CodeWriter();

        codeGenerator.AddType(typeSymbol, ref writer);

        var code = writer.ToString();
        var gt = @"/** Transpiled from Tapper.Test.SourceTypes.CustomInheritedConvertedType */
export type CustomInheritedConvertedType = { test: string | number } & CustomGenericConvertedType<string>;
";

        _output.WriteLine(code);
        _output.WriteLine(gt);

        Assert.Equal(gt, code, ignoreLineEndingDifferences: true);
    }
}
