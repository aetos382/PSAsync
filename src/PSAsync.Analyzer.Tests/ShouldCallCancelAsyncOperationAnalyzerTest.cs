using System.Threading.Tasks;

using Xunit;

using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<PSAsync.Analyzer.ShouldCallCancelAsyncOperationsAnalyzer>;

namespace PSAsync.Analyzer.Tests
{
    public class ShouldCallCancelAsyncOperationAnalyzerTest
    {
        [Fact]
        public async Task Test()
        {
            const string source = @"
using System;
using System.Management.Automation;

namespace Hoge
{
    [Cmdlet(VerbsDiagnostics.Test, ""Hoge"")]
    public class TestCmdlet : Cmdlet, IAsyncCmdlet
    {
        protected override void ProcessRecord()
        {
            this.WriteObject(1);
        }
    }
}";

            await Verifier.VerifyAnalyzerAsync(source);
        }
    }
}
