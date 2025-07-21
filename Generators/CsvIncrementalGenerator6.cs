using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Generators;

/// <summary>
/// CSV Incremental Generator for classes marked with the CsvIncrementalSerializableAttribute6.
/// This is the older version of the incremental generator that works with .NET 6 and doesn't use
/// the .Net 7 ForAttributeWithMetadataName method.
/// </summary>
[Generator]
public class CsvIncrementalGenerator6 : IIncrementalGenerator
{
    private const string csvSerializerAttributeText = """
            using System;
            namespace CsvIncrementalSerializer;

            [AttributeUsage(AttributeTargets.Class)]
            public class CsvIncrementalSerializableAttribute6 : Attribute
            {
                public CsvIncrementalSerializableAttribute6() {}
            }
            """;


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add attribute to compilation once after the generator is initialized
        context.RegisterPostInitializationOutput(c => c.AddSource("CsvIncrementalSerializableAttribute6.g.cs", SourceText.From(csvSerializerAttributeText, Encoding.UTF8)));

        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations =
            context.SyntaxProvider.CreateSyntaxProvider<ClassDeclarationSyntax>(
                predicate: static (classDeclaration, _) => IsSyntaxTargetForGeneration(classDeclaration),
                transform: static (classDeclaration, model) => GetSemanticTargetForGeneration(classDeclaration)
            )
            .Where(static c => c is not null);/// from previous snippet

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(spc, source.Item1, source.Item2));
    }

    // Lightweight check for class with attributes
    // This will be called on every generation pass and should be as fast as possible
    // with no allocations
    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0;

    // This is called on any class that has attributes and can do a bit more work to 
    // decide if the attribute is the one we care about. This is only called if the
    // previous method returned true.
    // Don't do the actual generation work here, this is just a quick check to see if
    // the attribute is the one we care about.
    static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a ClassDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
                if (attributeSymbol == null)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [LoggerMessage] attribute?
                if (fullName == "CsvIncrementalSerializer.CsvIncrementalSerializableAttribute6")
                {
                    // return the parent class of the method
                    return classDeclarationSyntax;
                }
            }
        }
        return null;
    }

    public static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes)
    {
        // Move to initialize
        //context.AddSource("CsvSerializableAttribute.g.cs", SourceText.From(csvSerializerAttributeText, Encoding.UTF8));

        //if (!(context.SyntaxReceiver is CsvGeneratorSyntaxReceiver receiver))
        //{
        //    return;
        //}

        //var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;

        //var attributeSyntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(csvSerializerAttributeText, Encoding.UTF8), options);
        //Compilation compilation = context.Compilation.AddSyntaxTrees(attributeSyntaxTree);

        INamedTypeSymbol stringSymbol = compilation.GetTypeByMetadataName("System.String");

        var sb = new StringBuilder();
        // Initialize class
        sb.Append("""
                using System.Linq;
                namespace CsvIncrementalSerializer6;

                public static class GeneratedIncrementalSerializer6
                {
                """);

        foreach (var classDeclaration in classes)
        {
            if (classDeclaration == null)
            {
                continue;
            }
            SemanticModel model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            ITypeSymbol classSymbol = model.GetDeclaredSymbol(classDeclaration);

            var classFullName = !string.IsNullOrEmpty(classSymbol.ContainingNamespace?.Name) ?
                $"{classSymbol.ContainingNamespace.Name}.{classDeclaration.Identifier.Text}"
                : classDeclaration.Identifier.Text;

            sb.Append($@"
    #region {classFullName}
    public static string ToCsv(this {classFullName} input) =>
        $""");

            var header = new StringBuilder();
            var propertyCount = 0;
            foreach (var propertyDeclaration in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (propertyCount > 0)
                {
                    sb.Append(", ");
                    header.Append(", ");
                }
                IPropertySymbol propertySymbol = model.GetDeclaredSymbol(propertyDeclaration);
                if (propertySymbol.Type.Equals(stringSymbol, SymbolEqualityComparer.Default))
                {
                    sb.Append($"\\\"{{input.{propertyDeclaration.Identifier.Text}}}\\\"");
                }
                else
                {
                    sb.Append($"{{input.{propertyDeclaration.Identifier.Text}}}");
                }
                header.Append(propertyDeclaration.Identifier.Text);
                propertyCount++;
            }
            sb.Append(@""";
");

            sb.Append($@"
    public static string ToCsvHeader(this {classFullName} input) =>
        ""{header}"";
");
            sb.Append($@"
    public static string ToCsv(this System.Collections.Generic.IEnumerable<{classFullName}> input)
    {{
        var sb = new System.Text.StringBuilder();
        if (input.Any())
        {{
            sb.AppendLine(input.First().ToCsvHeader());
            foreach (var item in input)
            {{
                sb.AppendLine(item.ToCsv());
            }}
        }}
        return sb.ToString();
    }}

    #endregion
");
        }
        // Close class
        sb.Append(@"
}");
        context.AddSource("CsvIncrementalSerializer6.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }


}