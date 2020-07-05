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
            try
            {
                this.DoProcessRecordAsync();
            }
            catch (Exception e)
            {
                throw;
            }
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

        #region async methods

        public async Task ProcessRecordAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                await Task.Yield();

                await this
                    .WriteObjectAsync(1, false, cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                await this.WriteObjectAsync(2, false, cancellationToken);

                await Task.Yield();

                await this.WriteObjectAsync(3, false, cancellationToken);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion
    }
}
