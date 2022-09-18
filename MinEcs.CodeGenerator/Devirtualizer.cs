using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator
{
    [Generator]
    public class HelloSourceGenerator : ISourceGenerator
    {

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Find the main method
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            // Build up the source code
            string source = $@"
// Auto-generated code
using System;

namespace Generated;

// {mainMethod.ToString()}

public static partial class Functions
{{
    public static void Print(string name) =>
        Console.WriteLine($""Generator v2 says: Hi from '{{name}}'"");
}}

";
            var typeName = mainMethod.ContainingType.Name;

            // Add the source code to the compilation
            context.AddSource($"{typeName}.MinEcsGenerated.cs", source);
            //Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Generated"));
            //File.WriteAllText(Path.Combine("Generated", $"{typeName}.MinEcs.cs"), source);



            (context.SyntaxReceiver as MySyntaxReceiver)?.GenerateClass(context);



        }

    }

    class MySyntaxReceiver : ISyntaxReceiver
    {
        string? _genericArgument;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MemberAccessExpressionSyntax expression)
            {
                var memberCalled = expression.Name.Identifier.Value;
                var genericArgument = ((expression.Name as GenericNameSyntax)?.TypeArgumentList.Arguments.First() as IdentifierNameSyntax)?.Identifier.Value;
                var containinigClass = (expression.Expression as IdentifierNameSyntax)?.Identifier.Value;

                _genericArgument = genericArgument as string;
            }
        }

        public void GenerateClass(GeneratorExecutionContext context)
        {
            if (_genericArgument is not null)
            {
                context.AddSource($"{_genericArgument}.MinEcsGenerated.cs", $"public class {_genericArgument} {{ }}");
            }
        }
    }
}