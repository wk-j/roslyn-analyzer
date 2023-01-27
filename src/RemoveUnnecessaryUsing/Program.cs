using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace RemoveUnnecessaryUsing;

public static class Program {

    static void Main(string[] args) {
        if (args.Length != 1) {
            Console.WriteLine("Usage: UnusedUsingAnalyzer.exe <path to solution or project file>");
            return;
        }

        var unusedUsingRule = new DiagnosticDescriptor(
            id: "UnusedUsing",
            title: "Unused using statement",
            messageFormat: "Using statement for '{0}' is never used",
            category: "Performance",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        var code = args[0];
        var workspace = MSBuildWorkspace.Create();

        var solution = workspace.OpenSolutionAsync(code).Result;

        foreach (var project in solution.Projects) {
            var compilation = project.GetCompilationAsync().Result;
            var model = compilation.GetSemanticModel(compilation.SyntaxTrees.First());

            var root = compilation.SyntaxTrees.First().GetRoot();
            var usingStatements = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

            foreach (var usingStatement in usingStatements) {
                var symbolInfo = model.GetSymbolInfo(usingStatement.Name);
                if (symbolInfo.Symbol == null) {
                    var location = usingStatement.GetLocation();
                    var diagnostic = Diagnostic.Create(unusedUsingRule, location, usingStatement.Name);
                    Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()} at line {location.GetLineSpan().StartLinePosition.Line + 1}");
                }
            }
        }
    }
}