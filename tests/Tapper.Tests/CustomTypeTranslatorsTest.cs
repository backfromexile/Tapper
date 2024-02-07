using Tapper.Test.SourceTypes;
using Xunit;
using Xunit.Abstractions;

namespace Tapper.Tests;

public class CustomTypeTranslatorsTest
{
    private readonly ITestOutputHelper _output;

    public CustomTypeTranslatorsTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Test_CustomMappedType()
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

        var type = typeof(CustomTranslatedType);
        var typeSymbol = compilation.GetTypeByMetadataName(type.FullName!)!;

        var writer = new CodeWriter();

        codeGenerator.AddType(typeSymbol, ref writer);

        var code = writer.ToString();
        var gt = @"/** Transpiled from Tapper.Test.SourceTypes.CustomTranslatedType */
export type CustomTranslatedType = { customProperty: any }
";

        _output.WriteLine(code);
        _output.WriteLine(gt);

        Assert.Equal(gt, code, ignoreLineEndingDifferences: true);
    }
}
