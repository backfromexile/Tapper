using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tapper.TypeMappers;

namespace Tapper;

public class TypeScriptCodeGenerator : ICodeGenerator
{
    private readonly string _newLineString;
    private readonly string _indent;
    private readonly INamedTypeSymbol[] _sourceTypes;
    private readonly INamedTypeSymbol _nullableStructTypeSymbol;
    private readonly ITranspilationOptions _transpilationOptions;

    public TypeScriptCodeGenerator(
        Compilation compilation,
        ITranspilationOptions options,
        ILogger _)
    {
        _transpilationOptions = options;

        _sourceTypes = compilation.GetSourceTypes(options.ReferencedAssembliesTranspilation);
        _nullableStructTypeSymbol = compilation.GetTypeByMetadataName("System.Nullable`1")!;

        _newLineString = options.NewLine.ToNewLineString();

        _indent = new string(' ', options.Indent);
    }

    public void AddHeader(IGrouping<INamespaceSymbol, INamedTypeSymbol> types, ref CodeWriter writer)
    {
        writer.Append($"/* THIS (.ts) FILE IS GENERATED BY Tapper */{_newLineString}");
        writer.Append($"/* eslint-disable */{_newLineString}");
        writer.Append($"/* tslint:disable */{_newLineString}");

        var memberTypes = types
            .SelectMany(static x => x.GetPublicFieldsAndProperties().IgnoreStatic())
            .SelectMany(RoslynExtensions.GetRelevantTypesFromMemberSymbol);

        var baseTypes = types
            .Where(static x => x.BaseType is not null
                && x.BaseType.IsType
                && x.BaseType.SpecialType != SpecialType.System_Object)
            .Select(static x => x.BaseType!);

        var differentNamespaceTypes = memberTypes
            .Concat(baseTypes)
            .OfType<INamedTypeSymbol>()
            .Where(x => !SymbolEqualityComparer.Default.Equals(x.ContainingNamespace, types.Key)
                && _sourceTypes.Contains(x, SymbolEqualityComparer.Default))
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .ToLookup<INamedTypeSymbol, INamespaceSymbol>(static x => x.ContainingNamespace, SymbolEqualityComparer.Default);

        foreach (var groupingType in differentNamespaceTypes)
        {
            writer.Append($"import {{ {string.Join(", ", groupingType.Select(x => x.Name))} }} from './{groupingType.Key.ToDisplayString()}';{_newLineString}");
        }

        writer.Append(_newLineString);
    }

    public void AddType(INamedTypeSymbol typeSymbol, ref CodeWriter writer)
    {
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            AddEnum(typeSymbol, ref writer);
        }
        else
        {
            AddClassOrStruct(typeSymbol, ref writer);
        }
    }

    private void AddEnum(INamedTypeSymbol typeSymbol, ref CodeWriter writer)
    {
        if (_transpilationOptions.EnumStyle
            is EnumStyle.Value
            or EnumStyle.NameString
            or EnumStyle.NameStringCamel
            or EnumStyle.NameStringPascal)
        {
            AddEnumAsEnum(typeSymbol, ref writer);
        }
        else if (_transpilationOptions.EnumStyle
                 is EnumStyle.Union
                 or EnumStyle.UnionCamel
                 or EnumStyle.UnionPascal)
        {
            AddEnumAsUnion(typeSymbol, ref writer);
        }
    }

    private void AddEnumAsEnum(INamedTypeSymbol typeSymbol, ref CodeWriter writer)
    {
        var enumStyle = _transpilationOptions.EnumStyle;

        var members = typeSymbol.GetPublicFieldsAndProperties().IgnoreStatic().ToArray();

        writer.Append($"/** Transpiled from {typeSymbol.ToDisplayString()} */{_newLineString}");
        writer.Append($"export enum {typeSymbol.Name} {{{_newLineString}");

        foreach (var member in members.OfType<IFieldSymbol>())
        {
            var value = enumStyle is EnumStyle.Value
                ? member.ConstantValue
                : $"\"{Transform(member.Name, enumStyle)}\"";

            writer.Append($"{_indent}{Transform(member.Name, enumStyle)} = {value},{_newLineString}");
        }

        writer.Append('}');
        writer.Append(_newLineString);
    }

    private void AddEnumAsUnion(INamedTypeSymbol typeSymbol, ref CodeWriter writer)
    {
        var enumStyle = _transpilationOptions.EnumStyle;

        var members = typeSymbol.GetPublicFieldsAndProperties().IgnoreStatic().ToArray();

        writer.Append($"/** Transpiled from {typeSymbol.ToDisplayString()} */{_newLineString}");
        writer.Append($"export type {typeSymbol.Name} = ");

        var memberNames = members.OfType<IFieldSymbol>()
            .Select(x => $"\"{Transform(x.Name, enumStyle)}\"");

        writer.Append(string.Join(" | ", memberNames));

        writer.Append(';');
        writer.Append(_newLineString);
    }

    private void AddClassOrStruct(INamedTypeSymbol typeSymbol, ref CodeWriter writer)
    {
        var members = typeSymbol.GetPublicFieldsAndProperties().IgnoreStatic().ToArray();

        writer.Append($"/** Transpiled from {typeSymbol.ToDisplayString()} */{_newLineString}");
        writer.Append($"export type {typeSymbol.Name} = {{{_newLineString}");

        foreach (var member in members)
        {
            var (memberTypeSymbol, isNullable) = GetMemberTypeSymbol(member);

            if (memberTypeSymbol is null)
            {
                throw new InvalidOperationException();
            }

            // Add jsdoc comment
            writer.Append($"{_indent}/** Transpiled from {memberTypeSymbol.ToDisplayString()} */{_newLineString}");
            writer.Append($"{_indent}{Transform(member.Name, _transpilationOptions.NamingStyle)}{(isNullable ? "?" : string.Empty)}: {TypeMapper.MapTo(memberTypeSymbol, _transpilationOptions)};{_newLineString}");
        }

        writer.Append('}');

        if (typeSymbol.BaseType is not null &&
            typeSymbol.BaseType.IsType &&
            typeSymbol.BaseType.SpecialType != SpecialType.System_Object)
        {
            if (_sourceTypes.Contains(typeSymbol.BaseType, SymbolEqualityComparer.Default))
            {
                writer.Append($" & {typeSymbol.BaseType.Name};");
            }
        }

        writer.Append(_newLineString);
    }

    private (ITypeSymbol? TypeSymbol, bool IsNullable) GetMemberTypeSymbol(ISymbol symbol)
    {
        if (symbol is IPropertySymbol propertySymbol)
        {
            var typeSymbol = propertySymbol.Type;

            if (typeSymbol.IsValueType)
            {
                if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    if (!namedTypeSymbol.IsGenericType)
                    {
                        return (typeSymbol, false);
                    }

                    if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, _nullableStructTypeSymbol))
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
                    if (!namedTypeSymbol.IsGenericType)
                    {
                        return (typeSymbol, false);
                    }

                    if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, _nullableStructTypeSymbol))
                    {
                        return (namedTypeSymbol.TypeArguments[0], true);
                    }

                    return (typeSymbol, false);
                }
            }

            var isNullable = fieldSymbol.NullableAnnotation is not NullableAnnotation.NotAnnotated;
            return (typeSymbol, isNullable);
        }

        return (null, false);
    }

    private static string Transform(string text, NamingStyle namingStyle)
    {
        return namingStyle switch
        {
            NamingStyle.PascalCase => $"{char.ToUpper(text[0])}{text[1..]}",
            NamingStyle.CamelCase => $"{char.ToLower(text[0])}{text[1..]}",
            _ => text,
        };
    }

    private static string Transform(string text, EnumStyle enumStyle)
    {
        return enumStyle switch
        {
            EnumStyle.NameStringPascal or EnumStyle.UnionPascal => $"{char.ToUpper(text[0])}{text[1..]}",
            EnumStyle.NameStringCamel or EnumStyle.UnionCamel => $"{char.ToLower(text[0])}{text[1..]}",
            _ => text,
        };
    }
}
