using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tapper.TypeMappers;

namespace Tapper;

public class TypeScriptCodeGenerator : ICodeGenerator
{
    private readonly string _newLine;
    private readonly string _indent;
    private readonly INamedTypeSymbol[] _sourceTypes;
    private readonly INamedTypeSymbol _nullableStructTypeSymbol;
    private readonly ITranspilationOptions _transpilationOptions;

    public TypeScriptCodeGenerator(Compilation compilation, string newLine, int indent, SerializerOption serializerOption, NamingStyle namingStyle, ILogger _)
    {
        _transpilationOptions = new TranspilationOptions(new DefaultTypeMapperProvider(compilation), serializerOption, namingStyle);

        _sourceTypes = compilation.GetSourceTypes();
        _nullableStructTypeSymbol = compilation.GetTypeByMetadataName("System.Nullable`1")!;
        _newLine = newLine;
        _indent = new string(' ', indent);
    }

    public void AddHeader(IGrouping<INamespaceSymbol, INamedTypeSymbol> types, ref CodeWriter writer)
    {
        writer.Append($"/* eslint-disable */{_newLine}");

        var diffrentNamespaceTypes = types
            .SelectMany(static x => x.GetPublicFieldsAndProperties().IgnoreStatic())
            .SelectMany(RoslynExtensions.GetRelevantTypesFromMemberSymbol)
            .OfType<INamedTypeSymbol>()
            .Where(x => !SymbolEqualityComparer.Default.Equals(x.ContainingNamespace, types.Key)
                && _sourceTypes.Contains(x, SymbolEqualityComparer.Default))
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .ToLookup<INamedTypeSymbol, INamespaceSymbol>(static x => x.ContainingNamespace, SymbolEqualityComparer.Default);

        foreach (var groupingType in diffrentNamespaceTypes)
        {
            writer.Append($"import {{ {string.Join(", ", groupingType.Select(x => x.Name))} }} from './{groupingType.Key.ToDisplayString()}';{_newLine}");
        }

        writer.Append(_newLine);
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
        var members = typeSymbol.GetPublicFieldsAndProperties().IgnoreStatic().ToArray();

        writer.Append($"/** Transpied from {typeSymbol.ToDisplayString()} */{_newLine}");
        writer.Append($"export enum {typeSymbol.Name} {{{_newLine}");

        foreach (var member in members.OfType<IFieldSymbol>())
        {
            writer.Append($"{_indent}{Transform(member.Name, _transpilationOptions.NamingStyle)} = {member.ConstantValue},{_newLine}");
        }

        writer.Append('}');
        writer.Append(_newLine);
    }

    private void AddClassOrStruct(INamedTypeSymbol typeSymbol, ref CodeWriter writer)
    {
        var members = typeSymbol.GetPublicFieldsAndProperties().IgnoreStatic().ToArray();

        writer.Append($"/** Transpied from {typeSymbol.ToDisplayString()} */{_newLine}");
        writer.Append($"export type {typeSymbol.Name} = {{{_newLine}");

        foreach (var member in members)
        {
            var (memberTypeSymbol, isNullable) = GetMemberTypeSymbol(member);

            if (memberTypeSymbol is null)
            {
                throw new InvalidOperationException();
            }

            // Add jdoc comment
            writer.Append($"{_indent}/** Transpied from {memberTypeSymbol.ToDisplayString()} */{_newLine}");
            writer.Append($"{_indent}{Transform(member.Name, _transpilationOptions.NamingStyle)}{(isNullable ? "?" : string.Empty)}: {TypeMapper.MapTo(memberTypeSymbol, _transpilationOptions)};{_newLine}");
        }

        writer.Append('}');
        writer.Append(_newLine);
    }

    private (ITypeSymbol? TypeSymbol, bool IsNullable) GetMemberTypeSymbol(ISymbol symbol)
    {
        if (symbol is IPropertySymbol propertySymbol)
        {
            if (propertySymbol.Type is ITypeSymbol typeSymbol)
            {
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
        }
        else if (symbol is IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.Type is ITypeSymbol typeSymbol)
            {
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
}