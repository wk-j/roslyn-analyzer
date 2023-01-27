using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace RemoveUnnecessaryUsing;

public static class Program {

    static async Task Main(string[] args) {
        if (args.Length != 1) {
            Console.WriteLine("Usage: UnusedUsingAnalyzer.exe <path to solution or project file>");
            return;
        }

        MSBuildLocator.RegisterDefaults();

        var unusedUsingRule = new DiagnosticDescriptor(
            id: "UnusedUsing",
            title: "Unused using statement",
            messageFormat: "Using statement for '{0}' is never used",
            category: "Performance",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        var code = args[0];
        var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(code);


        foreach (var project in solution.Projects) {
            var compilation = await project.GetCompilationAsync();

            if (compilation.SyntaxTrees.Any() == false) {
                Console.WriteLine("Cannot compile project {0}", project.Name);
                continue;
            }

            foreach (var tree in compilation.SyntaxTrees) {
                var model = compilation.GetSemanticModel(tree);

                var root = tree.GetRoot();
                var usingStatements = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

                Console.WriteLine("Number of using statement in {0}: {1}", root.GetLocation(), usingStatements.Count());

                foreach (var usingStatement in usingStatements) {
                    // var symbolInfo = model.GetSymbolInfo(usingStatement.Name);
                    // if (symbolInfo.Symbol == null) {
                    //     var location = usingStatement.GetLocation();
                    //     var diagnostic = Diagnostic.Create(unusedUsingRule, location, usingStatement.Name);
                    //     Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()} at line {location.GetLineSpan().StartLinePosition.Line + 1}");
                    // }
                }
            }
        }
    }
}