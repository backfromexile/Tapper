using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Tapper.TypeMappers;

namespace Tapper;

internal class GenericTypeParameterMapper : ITypeMapper
{
    public ITypeSymbol Assign { get; } = default!;

    public string MapTo(ITypeSymbol typeSymbol, ITranspilationOptions options)
    {
        if (typeSymbol is not ITypeParameterSymbol typeParameterSymbol)
            throw new InvalidOperationException($"GenericTypeMapper does not support {typeSymbol.ToDisplayString()}.");

        return typeParameterSymbol.Name;
    }
}

public class DefaultTypeMapperProvider : ITypeMapperProvider
{
    private readonly ArrayTypeMapper _arrayTypeMapper;
    private readonly TupleTypeMapper _tupleTypeMapper;
    private readonly GenericTypeParameterMapper _genericTypeParameterMapper;

    private readonly IDictionary<ITypeSymbol, ITypeMapper> _mappers;

    public DefaultTypeMapperProvider(Compilation compilation, bool includeReferencedAssemblies)
    {
        _arrayTypeMapper = new ArrayTypeMapper(compilation);
        _tupleTypeMapper = new TupleTypeMapper();
        _genericTypeParameterMapper = new GenericTypeParameterMapper();

        var dateTimeTypeMapper = new DateTimeTypeMapper(compilation);
        var dateTimeOffsetTypeMapper = new DateTimeOffsetTypeMapper(compilation);
        var nullableStructTypeMapper = new NullableStructTypeMapper(compilation);

        var primitiveTypeMappers = PrimitiveTypeMappers.Create(compilation);
        var collectionTypeTypeMappers = CollectionTypeTypeMappers.Create(compilation);
        var dictionaryTypeMappers = DictionaryTypeMappers.Create(compilation);

        var sourceTypeMapper = compilation.GetSourceTypes(includeReferencedAssemblies)
            .Select(static x => new SourceTypeMapper(x.GetUnboundedType()));

        var typeMappers = sourceTypeMapper.Concat(primitiveTypeMappers)
            .Concat(collectionTypeTypeMappers)
            .Concat(dictionaryTypeMappers)
            .Concat(new ITypeMapper[] { dateTimeTypeMapper, dateTimeOffsetTypeMapper, nullableStructTypeMapper });

        _mappers = typeMappers.ToDictionary<ITypeMapper, ITypeSymbol>(x => x.Assign, SymbolEqualityComparer.Default);
    }

    public ITypeMapper GetTypeMapper(ITypeSymbol type)
    {
        if (type.IsTupleType)
        {
            return _tupleTypeMapper;
        }

        if (type is IArrayTypeSymbol)
        {
            return _arrayTypeMapper;
        }

        if (type is ITypeParameterSymbol)
        {
            return _genericTypeParameterMapper;
        }

        var sourceType = type.GetUnboundedType();

        if (_mappers.TryGetValue(sourceType, out var typeMapper))
        {
            return typeMapper;
        }

        throw new InvalidOperationException($"{type.ToDisplayString()} is not supported.");
    }

    public void AddTypeMapper(ITypeMapper typeMapper)
    {
        _mappers[typeMapper.Assign] = typeMapper;
    }
}
