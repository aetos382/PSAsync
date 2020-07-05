using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    public static partial class AsyncCmdletExtensions
    {
        internal static Task QueueAction<TCmdlet>(
            this TCmdlet cmdlet,
            Action<TCmdlet> action,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            var context = AsyncCmdletContext.GetContext(cmdlet);

            var awaitableAction = context.QueueAction(
                cmdlet,
                action,
                cancellationToken);
            
            return awaitableAction.Task;
        }
    }
}
