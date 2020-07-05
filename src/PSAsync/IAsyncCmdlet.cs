using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    public interface IAsyncCmdlet
    {
        Task BeginProcessingAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task ProcessRecordAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task EndProcessingAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
