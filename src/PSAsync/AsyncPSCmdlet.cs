using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    public abstract class AsyncPSCmdlet :
        PSCmdlet,
        IAsyncCmdlet
    {
        protected AsyncPSCmdlet()
        {
        }

        /// <inheritdoc />
        protected override void BeginProcessing()
        {
            this.DoBeginProcessingAsync();
        }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            this.DoProcessRecordAsync();
        }

        /// <inheritdoc />
        protected override void EndProcessing()
        {
            this.DoEndProcessingAsync();
        }

        /// <inheritdoc />
        protected override void StopProcessing()
        {
            this.CancelAsyncOperation();
        }

        [SkipImplementationCheck]
        public virtual Task BeginProcessingAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [SkipImplementationCheck]
        public virtual Task ProcessRecordAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        [SkipImplementationCheck]
        public virtual Task EndProcessingAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
