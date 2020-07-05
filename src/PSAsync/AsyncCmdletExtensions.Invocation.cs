using System.Management.Automation;
using System.Threading;

namespace PSAsync
{
    public static partial class AsyncCmdletExtensions
    {
        public static void DoBeginProcessingAsync<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            var accessor = AsyncCmdletAccessor<TCmdlet>.GetAccessor();

            AsyncMethodRunner.DoAsyncOperation(cmdlet, accessor.DoBeginProcessingAsync);
        }

        public static void DoProcessRecordAsync<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            var accessor = AsyncCmdletAccessor<TCmdlet>.GetAccessor();

            AsyncMethodRunner.DoAsyncOperation(cmdlet, accessor.DoProcessRecordAsync);
        }

        public static void DoEndProcessingAsync<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            
            var accessor = AsyncCmdletAccessor<TCmdlet>.GetAccessor();

            AsyncMethodRunner.DoAsyncOperation(cmdlet, accessor.DoEndProcessingAsync);
        }

        public static void CancelAsyncOperation<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            if (AsyncCmdletContext<TCmdlet>.TryGetContext(cmdlet, out var context))
            {
                context.CancelAsyncOperations();
            }
        }
    }
}
