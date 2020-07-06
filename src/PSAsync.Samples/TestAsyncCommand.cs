using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync.Samples
{
    [Cmdlet(VerbsDiagnostic.Test, "AsyncCommand")]
    public class TestAsyncCommand :
        Cmdlet,
        IAsyncCmdlet
    {
        #region standard methods

        protected override void BeginProcessing()
        {
            // this.DoBeginProcessingAsync();
        }

        protected override void ProcessRecord()
        {
            this.DoProcessRecordAsync();
        }

        protected override void EndProcessing()
        {
            // this.DoEndProcessingAsync();
        }

        protected override void StopProcessing()
        {
            this.CancelAsyncOperation();
        }

        #endregion

        #region async pipeline methods

        public async Task ProcessRecordAsync(
            CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        #endregion
    }
}
