using System;
using System.Collections.Immutable;

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
        public const string DiagnosticId = "Hoge";

        private static readonly DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            "Hoge",
            "Hoge",
            "Hoge",
            DiagnosticSeverity.Info,
            true);

        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
            ImmutableArray.Create(_descriptor);

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

            context.RegisterCompilationStartAction(
                this.CompilationStartAction);

            context.RegisterSyntaxNodeAction(
                this.SyntaxNodeAction,
                SyntaxKind.ClassDeclaration);
        }

        private void CompilationStartAction(
            CompilationStartAnalysisContext context)
        {
        }

        private void SyntaxNodeAction(
            SyntaxNodeAnalysisContext context)
        {
            var cancellationToken = context.CancellationToken;

            var syntax = (ClassDeclarationSyntax) context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(syntax, cancellationToken);
            var type = context.SemanticModel.GetTypeInfo(syntax, cancellationToken);

            var attributes = symbol.GetAttributes();
        }
    }
}
