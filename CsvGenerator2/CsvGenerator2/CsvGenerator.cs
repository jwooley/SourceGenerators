using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;

namespace CsvGenerator2
{
    [Generator]
    public class CsvGenerator : ISourceGenerator
    {
        private const string csvSerializerAttributeText = @"
using System;
namespace CsvSerializer;

[AttributeUsage(AttributeTargets.Class)]
public class CsvSerializableAttribute : Attribute
{
    public CsvSerializableAttribute() {}
}";
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("CsvSerializableAttribute.g.cs", SourceText.From(csvSerializerAttributeText, Encoding.UTF8));

            if (!(context.SyntaxReceiver is CsvGeneratorSyntaxReceiver receiver))
            {
                return;
            }

            var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;

            var attributeSyntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(csvSerializerAttributeText, Encoding.UTF8), options);
            Compilation compilation = context.Compilation.AddSyntaxTrees(attributeSyntaxTree);

            var sb = new StringBuilder();
            // Initialize class
            sb.AppendLine(@"
namespace CsvSerializer;

public static class GeneratedSerializer
{");

            foreach (var classDeclaration in receiver.AttributeClasses)
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

                sb.AppendLine($@"    public static string ToCsv(this {classFullName} input) => ""{classFullName}"";");
            }
            // Close class
            sb.Append(@"
}");
            context.AddSource("CsvSerializer.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }


        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new CsvGeneratorSyntaxReceiver());
        }
    }
}
