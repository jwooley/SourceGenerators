//HintName: CsvIncrementalSerializableAttribute.g.cs
using System;
namespace CsvIncrementalSerializer;

[AttributeUsage(AttributeTargets.Class)]
public class CsvIncrementalSerializableAttribute : Attribute
{
    public CsvIncrementalSerializableAttribute() {}
}