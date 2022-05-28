using System.IO;
using Microsoft.CodeAnalysis;

namespace SourceGenerator
{
    [Generator]
    public class HelloSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Find the main method
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            // Build up the source code
            string source = $@"
// Auto-generated code
using System;

namespace Generated;

public static partial class Functions
{{
    public static void Print(string name) =>
        Console.WriteLine($""Generator v2 says: Hi from '{{name}}'"");
}}

";
            var typeName = mainMethod.ContainingType.Name;

            // Add the source code to the compilation
            // context.AddSource($"{typeName}.MinEcsGenerated.cs", source);
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Generated"));
            File.WriteAllText(Path.Combine("Generated", $"{typeName}.MinEcs.cs"), source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}