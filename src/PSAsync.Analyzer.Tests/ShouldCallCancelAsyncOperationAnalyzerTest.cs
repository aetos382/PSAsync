using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

using Xunit;

namespace PSAsync.Analyzer.Tests
{
    public class ShouldCallCancelAsyncOperationAnalyzerTest
    {
        private static Task VerifyAnalyzerAsync(
            string source,
            params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<ShouldCallCancelAsyncOperationsAnalyzer, XUnitVerifier>();

            test.TestCode = source;
            test.TestState.AdditionalReferences.Add(typeof(IAsyncCmdlet).Assembly);
            test.TestState.AdditionalReferences.Add(typeof(Cmdlet).Assembly);

            test.ExpectedDiagnostics.AddRange(expected);

            return test.RunAsync(CancellationToken.None);
        }

        [Fact]
        public async Task IAsyncCmdlet実装クラスでStopProcessingをオーバーライドしていない場合は実装をサジェストする()
        {
            const string source = @"
using System;
using System.Management.Automation;

using PSAsync;

namespace Hoge
{
    [Cmdlet(VerbsDiagnostic.Test, ""Hoge"")]
    public class TestCmdlet :
        Cmdlet,
        IAsyncCmdlet
    {
    }
}";
            var expectedResult =
                new DiagnosticResult(
                    ShouldCallCancelAsyncOperationsAnalyzer.DiagnosticIds.OverrideStopProcessingAndCallCancelAsyncOperations,
                    DiagnosticSeverity.Info)
                .WithLocation(9, 18);

            await VerifyAnalyzerAsync(source.Trim(), expectedResult);
        }

        [Fact]
        public async Task IAsyncCmdlet実装クラスにStopProcessingがあるがCancelAsyncOperationsを呼んでいない場合は実装をサジェストする()
        {
            const string source = @"
using System;
using System.Management.Automation;

using PSAsync;

namespace Hoge
{
    [Cmdlet(VerbsDiagnostic.Test, ""Hoge"")]
    public class TestCmdlet :
        Cmdlet,
        IAsyncCmdlet
    {
        protected override void StopProcessing()
        {
        }
    }
}";
            var expectedResult =
                new DiagnosticResult(
                    ShouldCallCancelAsyncOperationsAnalyzer.DiagnosticIds.CallCancelAsyncOperationsInStopProcessing,
                    DiagnosticSeverity.Info)
                .WithLocation(13, 33);

            await VerifyAnalyzerAsync(source.Trim(), expectedResult);
        }
    }
}
