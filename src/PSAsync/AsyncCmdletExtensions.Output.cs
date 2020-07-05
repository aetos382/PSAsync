using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    public static partial class AsyncCmdletExtensions
    {
        public static Task WriteObjectAsync<TCmdlet>(
            this TCmdlet cmdlet,
            object obj,
            bool enumerateCollection = false,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                c => c.WriteObject(obj, enumerateCollection),
                cancellationToken);

            return task;
        }
    }
}
