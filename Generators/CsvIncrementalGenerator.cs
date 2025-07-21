using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace Generators;

[Generator]
public class CsvIncrementalGenerator : IIncrementalGenerator
{
    private const string csvSerializerAttributeText = """
            using System;
            namespace CsvIncrementalSerializer;

            [AttributeUsage(AttributeTargets.Class)]
            public class CsvIncrementalSerializableAttribute : Attribute
            {
                public CsvIncrementalSerializableAttribute() {}
            }
            """;


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add attribute to compilation once after the generator is initialized
        context.RegisterPostInitializationOutput(c => c.AddSource("CsvIncrementalSerializableAttribute.g.cs", SourceText.From(csvSerializerAttributeText, Encoding.UTF8)));

        IncrementalValuesProvider<ClassTypeInfo> classDeclaration = context.SyntaxProvider
            // .NET 6 version
            //.CreateSyntaxProvider<ClassDeclarationSyntax>(
            //    predicate: static (classDeclaration, _) => IsSyntaxTargetForGeneration(classDeclaration),
            //    transform: static (classDeclaration, model) => GetSemanticTargetForGeneration(classDeclaration)
            //)
            // .NET 7 version
            .ForAttributeWithMetadataName("CsvIncrementalSerializer.CsvIncrementalSerializableAttribute",
                 predicate: static (s, _) => true,
                 transform: static (ctx, _) => GetClassInfo(ctx))
            .Where(static c => c is not null)
            .Collect()
            .SelectMany((classInfos, _) => classInfos.Distinct());

        context.RegisterSourceOutput(classDeclaration,
            static (spc, source) => Execute(spc, source));
    }

    private static ClassTypeInfo GetClassInfo(GeneratorAttributeSyntaxContext ctx)
    {
        var type = (INamedTypeSymbol)ctx.TargetSymbol;
        var classInfo = new ClassTypeInfo(type);
        return classInfo;
    }

    public static void Execute(SourceProductionContext context, ClassTypeInfo classInfo)
    {
        var sb = new StringBuilder();
        // Initialize class
        sb.AppendLine("using System.Linq;");
        if (!string.IsNullOrEmpty(classInfo.Namespace))
        {
            sb.AppendLine("");
            sb.AppendLine($"namespace {classInfo.Namespace};");
            sb.AppendLine("");
        }

        sb.Append($$"""
                public partial class {{classInfo.Name}}
                {
                """);

        var classFullName = !string.IsNullOrEmpty(classInfo.Namespace) ?
            $"{classInfo.Namespace}.{classInfo.Name}"
            : classInfo.Name;

        sb.Append($@"
    public string ToCsv() =>
        $""");

        var header = new StringBuilder();
        var propertyCount = 0;
        foreach (var propertyDeclaration in classInfo.Properties)
        {
            if (!propertyDeclaration.IsPublic || propertyDeclaration.IsStatic)
            {
                continue; // Skip non-public or static properties
            }
            if (propertyCount > 0)
            {
                sb.Append(",");
                header.Append(",");
            }
            if (propertyDeclaration.Type.Equals("string"))
            {
                sb.Append($"\\\"{{{propertyDeclaration.Name}}}\\\"");
            }
            else
            {
                sb.Append($"{{{propertyDeclaration.Name}}}");
            }
            header.Append(propertyDeclaration.Name);
            propertyCount++;
        }
        sb.Append(@""";
");

        sb.AppendLine($$"""

                public string ToCsvHeader() =>
                    "{{header}}";

                public static string ToCsv(System.Collections.Generic.IEnumerable<{{classInfo.Name}}> input)
                {
                    var sb = new System.Text.StringBuilder();
                    if (input.Any())
                    {
                        sb.Append(input.First().ToCsvHeader());
                        foreach (var item in input)
                        {
                            sb.Append("\r\n");
                            sb.Append(item.ToCsv());
                        }
                    }
                    return sb.ToString();
                }
            }
            """);

        context.AddSource($"CsvIncrementalSerializer.{classInfo.Name}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }


}