﻿using System.Management.Automation;

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
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            var accessor = AsyncCmdletAccessor<TCmdlet>.Instance;

            AsyncMethodRunner.DoAsyncOperation(cmdlet, accessor.DoBeginProcessingAsync);
        }

        public static void DoProcessRecordAsync<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            var accessor = AsyncCmdletAccessor<TCmdlet>.Instance;

            AsyncMethodRunner.DoAsyncOperation(cmdlet, accessor.DoProcessRecordAsync);
        }

        public static void DoEndProcessingAsync<TCmdlet>(
            this TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            var accessor = AsyncCmdletAccessor<TCmdlet>.Instance;

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