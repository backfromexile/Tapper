using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Tapper.TypeMappers;

namespace Tapper;

internal class DefaultTypeConverter : ITypeConverter
{
    public void Convert(ref CodeWriter codeWriter, INamedTypeSymbol typeSymbol, ITranspilationOptions options)
    {
        var newLineString = options.NewLine.ToNewLineString();

        var indent = options.GetIndentString();
        var members = typeSymbol.GetPublicFieldsAndProperties()
            .IgnoreStatic()
            .ToArray();

        codeWriter.Append('{');
        codeWriter.Append(newLineString);

        foreach (var member in members)
        {
            var (memberTypeSymbol, isNullable) = DefaultTypeConverterHelper.GetMemberTypeSymbol(member, options);

            var (isValid, name) = DefaultTypeConverterHelper.GetMemberName(member, options);

            if (!isValid)
            {
                continue;
            }

            // Add jsdoc comment
            codeWriter.Append($"{indent}/** Transpiled from {memberTypeSymbol.ToDisplayString()} */{newLineString}");
            codeWriter.Append($"{indent}{name}{(isNullable ? "?" : string.Empty)}: {TypeMapper.MapTo(memberTypeSymbol, options)};{newLineString}");
        }

        codeWriter.Append('}');
    }
}

file static class DefaultTypeConverterHelper
{
    public static (ITypeSymbol TypeSymbol, bool IsNullable) GetMemberTypeSymbol(ISymbol symbol, ITranspilationOptions options)
    {
        if (symbol is IPropertySymbol propertySymbol)
        {
            var typeSymbol = propertySymbol.Type;

            if (typeSymbol.IsValueType)
            {
                if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    if (namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
                    {
                        return (namedTypeSymbol.TypeArguments[0], true);
                    }

                    return (typeSymbol, false);
                }
            }

            var isNullable = propertySymbol.NullableAnnotation is not NullableAnnotation.NotAnnotated;
            return (typeSymbol, isNullable);
        }
        else if (symbol is IFieldSymbol fieldSymbol)
        {
            var typeSymbol = fieldSymbol.Type;

            if (typeSymbol.IsValueType)
            {
                if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    if (namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
                    {
                        return (namedTypeSymbol.TypeArguments[0], true);
                    }

                    return (typeSymbol, false);
                }
            }

            var isNullable = fieldSymbol.NullableAnnotation is not NullableAnnotation.NotAnnotated;
            return (typeSymbol, isNullable);
        }

        throw new UnreachableException($"{nameof(symbol)} must be IPropertySymbol or IFieldSymbol");
    }

    public static (bool IsValid, string Name) GetMemberName(ISymbol memberSymbol, ITranspilationOptions options)
    {
        if (options.SerializerOption == SerializerOption.Json)
        {
            foreach (var attr in memberSymbol.GetAttributes())
            {
                if (options.SpecialSymbols.JsonIgnoreAttributes.Any(x => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, x)))
                {
                    return (false, string.Empty);
                }

                if (options.SpecialSymbols.JsonPropertyNameAttributes.Any(x => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, x)))
                {
                    var name = attr.ConstructorArguments[0].Value!.ToString()!;
                    return (true, name);
                }
            }
        }
        else if (options.SerializerOption == SerializerOption.MessagePack)
        {
            foreach (var attr in memberSymbol.GetAttributes())
            {
                if (options.SpecialSymbols.MessagePackIgnoreMemberAttributes.Any(x => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, x)))
                {
                    return (false, string.Empty);
                }

                if (options.SpecialSymbols.MessagePackKeyAttributes.Any(x => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, x)))
                {
                    if (attr.ConstructorArguments[0].Type?.SpecialType == SpecialType.System_String)
                    {
                        var name = attr.ConstructorArguments[0].Value!.ToString()!;
                        return (true, name);
                    }
                }
            }
        }

        return (true, options.NamingStyle.Transform(memberSymbol.Name));
    }
}
