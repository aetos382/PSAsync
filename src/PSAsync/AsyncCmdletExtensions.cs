namespace PSAsync
{
    public static class AsyncCmdletExtensions
    {
        public static void DoBeginProcessingAsync(
            this IAsyncCmdlet asyncCmdlet)
        {
            Requires.ArgumentNotNull(asyncCmdlet, nameof(asyncCmdlet));
        }

        public static void DoProcessRecordAsync(
            this IAsyncCmdlet asyncCmdlet)
        {
            Requires.ArgumentNotNull(asyncCmdlet, nameof(asyncCmdlet));
        }

        public static void DoEndProcessingAsync(
            this IAsyncCmdlet asyncCmdlet)
        {
            Requires.ArgumentNotNull(asyncCmdlet, nameof(asyncCmdlet));
        }

        private static void DoAsyncOperation()
        {

        }
    }
}
