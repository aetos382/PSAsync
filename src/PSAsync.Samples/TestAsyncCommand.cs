using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync.Samples
{
    [Cmdlet(VerbsDiagnostic.Test, "AsyncCommand")]
    public class TestAsyncCommand :
        AsyncCmdlet
    {
        #region async pipeline methods

        public override async Task ProcessRecordAsync(
            CancellationToken cancellationToken)
        {
            await this.WriteObjectAsync("foo").ConfigureAwait(false);
        }

        #endregion
    }
}
