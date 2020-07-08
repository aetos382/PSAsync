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

        /// <inheritdoc cref="Cmdlet.BeginProcessing"/>
        protected override void BeginProcessing()
        {
            this.DoBeginProcessingAsync();
        }

        /// <inheritdoc cref="Cmdlet.ProcessRecord"/>
        protected override void ProcessRecord()
        {
            this.DoProcessRecordAsync();
        }

        /// <inheritdoc cref="Cmdlet.EndProcessing"/>
        protected override void EndProcessing()
        {
            this.DoEndProcessingAsync();
        }

        /// <inheritdoc cref="Cmdlet.StopProcessing"/>
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
