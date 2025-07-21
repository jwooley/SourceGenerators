using Microsoft.CodeAnalysis;
using System.Linq;

namespace Generators.Helpers;
public record struct PropertyInfo (string Name, string Type, bool IsStatic, bool IsPublic)
{
}

public record struct ClassTypeInfo // : IEquatable<ClassTypeInfo>
{
    public string Namespace { get; }
    public string Name { get; }
    public bool HasNameProperty { get; }
    public EquatableArray<PropertyInfo> Properties { get; }
    public bool IsRecord { get; }

    public ClassTypeInfo(ITypeSymbol type)
    {
        Namespace = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToString();
        Name = type.Name;
        HasNameProperty = type.GetMembers().Any(m => m.Name == "Name"
                                                     && m is IPropertySymbol property
                                                     && property.Type.SpecialType == SpecialType.System_String);
        IsRecord = type.IsRecord;
        Properties = GetProperties(type);
    }

    private static EquatableArray<PropertyInfo> GetProperties(ITypeSymbol type)
    {
        return new EquatableArray<PropertyInfo>(type.GetMembers()
                   .Select(m =>
                   new PropertyInfo(
                       Name: m.Name,
                       Type: m is IPropertySymbol property ? property.Type.ToString() : string.Empty,
                        IsStatic: m.IsStatic,
                        IsPublic: m.DeclaredAccessibility == Accessibility.Public))
                   .Where(p => p.Name is not null && !string.IsNullOrEmpty(p.Type))
                   .ToArray());
    }

    //public override bool Equals(object? obj)
    //{
    //    return obj is ClassTypeInfo other && Equals(other);
    //}

    //public bool Equals(ClassTypeInfo other)
    //{
    //    if (ReferenceEquals(null, other))
    //        return false;
    //    if (ReferenceEquals(this, other))
    //        return true;

    //    return Namespace == other.Namespace
    //           && Name == other.Name
    //           && HasNameProperty == other.HasNameProperty
    //           && Properties.Select(p => p.Name).ToList().EqualsTo(other.Properties.Select(p => p.Name).ToList());
    //}

    //public override int GetHashCode()
    //{
    //    unchecked
    //    {
    //        var hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
    //        hashCode = (hashCode * 397) ^ Name.GetHashCode();
    //        hashCode = (hashCode * 397) ^ HasNameProperty.GetHashCode();
    //        hashCode = (hashCode * 397) ^ Properties.Select(p => p.Name).ToList().ComputeHashCode();

    //        return hashCode;
    //    }
    //}
}