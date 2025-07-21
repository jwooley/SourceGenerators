using Generators.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generators;
public class PropertyInfo (string Name, string Type, bool IsStatic, bool IsPublic)
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public bool IsStatic { get; } = IsStatic;
    public bool IsPublic { get; } = IsPublic;
}

public class ClassTypeInfo : IEquatable<ClassTypeInfo>
{
    public string? Namespace { get; }
    public string Name { get; }
    public bool HasNameProperty { get; }
    public IReadOnlyList<PropertyInfo> Properties { get; }
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

    private static IReadOnlyList<PropertyInfo> GetProperties(ITypeSymbol type)
    {
        return type.GetMembers()
                   .Select(m =>
                   new PropertyInfo(
                       Name: m.Name,
                       Type: m is IPropertySymbol property ? property.Type.ToString() : string.Empty,
                        IsStatic: m.IsStatic,
                        IsPublic: m.DeclaredAccessibility == Accessibility.Public))
                   //{
                   //    if (!m.IsStatic || m.DeclaredAccessibility != Accessibility.Public || m is not IFieldSymbol field)
                   //        return null;

                   //    return SymbolEqualityComparer.Default.Equals(field.Type, type)
                   //        ? field.Name
                   //        : null;
                   //})
                   .Where(p => p?.Name is not null && !string.IsNullOrEmpty(p?.Type))
                   .ToList();
    }

    public override bool Equals(object? obj)
    {
        return obj is ClassTypeInfo other && Equals(other);
    }

    public bool Equals(ClassTypeInfo? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Namespace == other.Namespace
               && Name == other.Name
               && HasNameProperty == other.HasNameProperty
               && Properties.Select(p => p.Name).ToList().EqualsTo(other.Properties.Select(p => p.Name).ToList());
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ HasNameProperty.GetHashCode();
            hashCode = (hashCode * 397) ^ Properties.Select(p => p.Name).ToList().ComputeHashCode();

            return hashCode;
        }
    }
}