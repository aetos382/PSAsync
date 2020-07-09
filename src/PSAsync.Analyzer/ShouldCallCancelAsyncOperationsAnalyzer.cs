using System.Collections.Immutable;

using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using PSAsync.Analyzer.Properties;

namespace PSAsync.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ShouldCallCancelAsyncOperationsAnalyzer :
        DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _psAsync001 = new DiagnosticDescriptor(
            DiagnosticIdentifiers.PSASYNC001,
            ResourceString.GetResurceString(nameof(Resources.PSASYNC001_Title)), 
            ResourceString.GetResurceString(nameof(Resources.PSASYNC001_Message)), 
            DiagnosticCategories.General,
            DiagnosticSeverity.Info,
            true);
        
        private static readonly DiagnosticDescriptor _psAsync002 = new DiagnosticDescriptor(
            DiagnosticIdentifiers.PSASYNC002,
            ResourceString.GetResurceString(nameof(Resources.PSASYNC002_Title)), 
            ResourceString.GetResurceString(nameof(Resources.PSASYNC002_Message)),
            DiagnosticCategories.General,
            DiagnosticSeverity.Info,
            true);

        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
            ImmutableArray.Create(
                _psAsync001,
                _psAsync002);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return _supportedDiagnostics;
            }
        }

        public override void Initialize(
            AnalysisContext context)
        {
            Requires.NotNull(context, nameof(context));

            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(
                this.SyntaxNodeAction,
                SyntaxKind.ClassDeclaration);
        }

        private void SyntaxNodeAction(
            SyntaxNodeAnalysisContext context)
        {
            var cancellationToken = context.CancellationToken;

            var classDeclarationSyntax = (ClassDeclarationSyntax) context.Node;

            var classDeclarationSymbol =
                context.SemanticModel.GetDeclaredSymbol(
                    classDeclarationSyntax, cancellationToken);

            if (!classDeclarationSymbol.IsAsyncCmdletClass())
            {
                return;
            }

            if (!classDeclarationSymbol.HasCmdletAttribute())
            {
                return;
            }

            var stopProcessing = classDeclarationSymbol.GetCmdletStopProcessing();

            if (stopProcessing is null)
            {
                var locations = classDeclarationSymbol.Locations;

                var diagnostic = Diagnostic.Create(
                    _psAsync001,
                    locations[0]);

                context.ReportDiagnostic(diagnostic);
                return;
            }

            bool foundCancelAsyncOperationsCall = false;

            foreach (var syntaxRef in stopProcessing.DeclaringSyntaxReferences)
            {
                if (foundCancelAsyncOperationsCall)
                {
                    break;
                }

                var body = syntaxRef.GetSyntax(cancellationToken);

                if (!(body is MethodDeclarationSyntax methodDeclartaionSyntax))
                {
                    continue;
                }

                var block = methodDeclartaionSyntax.Body;

                foreach (var statementSyntax in block.Statements)
                {
                    if (!statementSyntax.IsKind(SyntaxKind.ExpressionStatement))
                    {
                        continue;
                    }

                    var expression = (ExpressionStatementSyntax) statementSyntax;

                    if (!(expression.Expression is InvocationExpressionSyntax invocation))
                    {
                        continue;
                    }

                    var invocationSymbolInfo = context.SemanticModel.GetSymbolInfo(invocation, cancellationToken);

                    if (!(invocationSymbolInfo.Symbol is IMethodSymbol methodSymbol))
                    {
                        continue;
                    }

                    if (methodSymbol.Name != "CancelAsyncOperations" ||
                        !methodSymbol.IsExtensionMethod ||
                        methodSymbol.DeclaredAccessibility != Accessibility.Public ||
                        methodSymbol.Parameters.Length != 0 ||
                        methodSymbol.ContainingType.GetFullName() != "PSAsync.AsyncCmdletInvocationExtensions")
                    {
                        continue;
                    }

                    foundCancelAsyncOperationsCall = true;
                    break;
                }
            }

            if (!foundCancelAsyncOperationsCall)
            {
                var locations = stopProcessing.Locations;

                var diagnostic = Diagnostic.Create(
                    _psAsync002,
                    locations[0]);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
