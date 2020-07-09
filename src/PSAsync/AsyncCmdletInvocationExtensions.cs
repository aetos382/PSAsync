using System.Management.Automation;

using Microsoft;

namespace PSAsync
{
    public static class AsyncCmdletInvocationExtensions
    {
        public static void DoBeginProcessingAsync<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));

            var accessor = AsyncCmdletAccessor.GetAccessor(cmdlet.GetType());

            if (!accessor.IsBeginProcessingAsyncImplemented)
            {
                return;
            }

            AsyncPipelineRunner.RunAsyncPipeline(
                cmdlet,
                accessor.DoBeginProcessingAsync,
                nameof(IAsyncCmdlet.BeginProcessingAsync));
        }

        public static void DoProcessRecordAsync<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));

            var accessor = AsyncCmdletAccessor.GetAccessor(cmdlet.GetType());

            if (!accessor.IsProcessRecordAsyncImplemented)
            {
                return;
            }

            AsyncPipelineRunner.RunAsyncPipeline(
                cmdlet,
                accessor.DoProcessRecordAsync,
                nameof(IAsyncCmdlet.ProcessRecordAsync));
        }

        public static void DoEndProcessingAsync<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));

            var accessor = AsyncCmdletAccessor.GetAccessor(cmdlet.GetType());

            if (!accessor.IsEndProcessingAsyncImplemented)
            {
                return;
            }

            AsyncPipelineRunner.RunAsyncPipeline(
                cmdlet,
                accessor.DoEndProcessingAsync,
                nameof(IAsyncCmdlet.EndProcessingAsync));
        }

        public static void CancelAsyncOperations<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));

            if (AsyncCmdletContext.TryGetContext(cmdlet, out var context))
            {
                context.CancelAsyncOperations();
            }
        }
    }
}
