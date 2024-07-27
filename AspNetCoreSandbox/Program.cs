using Tapper;
using Tapper.Core;

namespace AspNetCoreSandbox;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}

public class TypeWithBuiltinTypePropertiesTranspiler : ITypeTranspiler<TypeWithBuiltinTypeProperties>
{
    public string Transpile()
    {
        return nameof(TypeWithBuiltinTypeProperties);
    }
}

[TranspilationRoot]
public class TypeWithBuiltinTypeProperties
{
    public byte ByteProperty { get; set; }
    public sbyte SByteProperty { get; set; }

    public ushort UInt16Property { get; set; }
    public short Int16Property { get; set; }

    public uint UInt32Property { get; set; }
    public int Int32Property { get; set; }

    public ulong UInt64Property { get; set; }
    public long Int64Property { get; set; }

    public UInt128 UInt128Property { get; set; }
    public Int128 Int128Property { get; set; }

    public Half HalfProperty { get; set; }
    public float SingleProperty { get; set; }
    public double DoubleProperty { get; set; }
    public decimal DecimalProperty { get; set; }

    public bool BoolProperty { get; set; }

    public char CharProperty { get; set; }
    public string StringProperty { get; set; }

    public object ObjectProperty { get; set; }
}


[TranspilationRoot]
public class TypeWithNullableBuiltinTypeProperties
{
    public byte? NullableByteProperty { get; set; }
    public sbyte? NullableSByteProperty { get; set; }

    public ushort? NullableUInt16Property { get; set; }
    public short? NullableInt16Property { get; set; }

    public uint? NullableUInt32Property { get; set; }
    public int? NullableInt32Property { get; set; }

    public ulong? NullableUInt64Property { get; set; }
    public long? NullableInt64Property { get; set; }

    public UInt128? NullableUInt128Property { get; set; }
    public Int128? NullableInt128Property { get; set; }

    public Half? NullableHalfProperty { get; set; }
    public float? NullableSingleProperty { get; set; }
    public double? NullableDoubleProperty { get; set; }
    public decimal? NullableDecimalProperty { get; set; }

    public bool? NullableBoolProperty { get; set; }

    public char? NullableCharProperty { get; set; }
    public string? NullableStringProperty { get; set; }

    public object? NullableObjectProperty { get; set; }
}


[TranspilationRoot]
public class TypeWithDateAndTimeTypeProperties
{
    public DateTime DateTimeProperty { get; set; }
    public DateTimeOffset DateTimeOffsetProperty { get; set; }
    public DateOnly DateOnlyProperty { get; set; }
    public TimeOnly TimeOnlyProperty { get; set; }
    public TimeSpan TimeSpanProperty { get; set; }
}


[TranspilationRoot]
public class TypeWithNullableDateTimeTypeProperties
{
    public DateTime? NullableDateTimeProperty { get; set; }
    public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }
    public DateOnly? NullableDateOnlyProperty { get; set; }
    public TimeOnly? NullableTimeOnlyProperty { get; set; }
    public TimeSpan? NullableTimeSpanProperty { get; set; }
}


[TranspilationRoot]
public class TypeWithOtherStandardTypeProperties
{
    public Guid GuidProperty { get; set; }
    public Guid? NullableGuidProperty { get; set; }
}
