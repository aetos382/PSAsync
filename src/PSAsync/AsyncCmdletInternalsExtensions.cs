using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    public static class AsyncCmdletInternalsExtensions
    {
        internal static Task<TResult> QueueAction<TCmdlet, TArgument, TResult>(
            this TCmdlet cmdlet,
            Func<TCmdlet, TArgument, AsyncCmdletContext, TResult> action,
            TArgument argument,
            bool runSynchronouslyIfOnTheMainThread,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            var context = AsyncCmdletContext.GetContext(cmdlet);

            if (runSynchronouslyIfOnTheMainThread && context.IsMainThread)
            {
                var result = action(cmdlet, argument, context);
                return Task.FromResult(result);
            }

            var awaitableAction = context.QueueAction(
                cmdlet,
                action,
                argument,
                cancellationToken);
            
            return awaitableAction.Task;
        }

        internal static Task<TResult> QueueAction<TCmdlet, TArgument, TResult>(
            this TCmdlet cmdlet,
            Func<TCmdlet, TArgument, TResult> action,
            TArgument argument,
            bool runSynchronouslyIfOnTheMainThread,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            return QueueAction(
                cmdlet,
                (c, a, _) => action(c, a),
                argument,
                runSynchronouslyIfOnTheMainThread,
                cancellationToken);
        }

        internal static Task QueueAction<TCmdlet, TArgument>(
            this TCmdlet cmdlet,
            Action<TCmdlet, TArgument, AsyncCmdletContext> action,
            TArgument argument,
            bool runSynchronouslyIfOnTheMainThread,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            return QueueAction(
                cmdlet,
                (c, a, x) => {
                    action(c, a, x);
                    return Unit.Instance;
                },
                argument,
                runSynchronouslyIfOnTheMainThread,
                cancellationToken);
        }

        internal static Task QueueAction<TCmdlet, TArgument>(
            this TCmdlet cmdlet,
            Action<TCmdlet, TArgument> action,
            TArgument argument,
            bool runSynchronouslyIfOnTheMainThread,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            return QueueAction(
                cmdlet,
                (c, a, _) => action(c, a),
                argument,
                runSynchronouslyIfOnTheMainThread,
                cancellationToken);
        }
    }
}
