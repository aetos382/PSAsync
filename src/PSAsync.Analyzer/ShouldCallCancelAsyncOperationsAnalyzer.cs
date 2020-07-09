using System.Collections.Immutable;
using System.Linq;

using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PSAsync.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ShouldCallCancelAsyncOperationsAnalyzer :
        DiagnosticAnalyzer
    {
        public static class DiagnosticIds
        {
            public const string OverrideStopProcessingAndCallCancelAsyncOperations =
                nameof(OverrideStopProcessingAndCallCancelAsyncOperations);

            public const string CallCancelAsyncOperationsInStopProcessing =
                nameof(CallCancelAsyncOperationsInStopProcessing);
        }

        private static readonly DiagnosticDescriptor _overrideStopProcessingAndCallCancelAsyncOperations = new DiagnosticDescriptor(
            DiagnosticIds.OverrideStopProcessingAndCallCancelAsyncOperations,
            "Hoge",
            "Hoge",
            "Hoge",
            DiagnosticSeverity.Info,
            true);
        
        private static readonly DiagnosticDescriptor _callCancelAsyncOperationsInStopProcessing = new DiagnosticDescriptor(
            DiagnosticIds.CallCancelAsyncOperationsInStopProcessing,
            "Hoge",
            "Hoge",
            "Hoge",
            DiagnosticSeverity.Info,
            true);

        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
            ImmutableArray.Create(
                _overrideStopProcessingAndCallCancelAsyncOperations,
                _callCancelAsyncOperationsInStopProcessing);

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

            var syntax = (ClassDeclarationSyntax) context.Node;

            // TODO: symbol の取得前に syntax でも絞り込んでおく？
            var symbol = context.SemanticModel.GetDeclaredSymbol(syntax, cancellationToken);

            if (!symbol.IsAsyncCmdletClass())
            {
                return;
            }

            if (!symbol.HasCmdletAttribute())
            {
                return;
            }

            var stopProcessing = symbol.GetCmdletStopProcessing();

            if (stopProcessing is null)
            {
                var diagnostic = Diagnostic.Create(
                    _overrideStopProcessingAndCallCancelAsyncOperations,
                    symbol.Locations[0],
                    symbol.Locations.Skip(1),
                    null,
                    null);

                context.ReportDiagnostic(diagnostic);
            }
            else
            {

            }
        }
    }
}
