using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    public static class AsyncCmdletOutputExtensions
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
                true,
                cancellationToken);

            return task;
        }

        public static Task WriteErrorAsync<TCmdlet>(
            this TCmdlet cmdlet,
            ErrorRecord error,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                c => c.WriteError(error),
                true,
                cancellationToken);

            return task;
        }

        public static Task WriteWarningAsync<TCmdlet>(
            this TCmdlet cmdlet,
            string message,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                c => c.WriteWarning(message),
                true,
                cancellationToken);

            return task;
        }

        public static Task WriteVerboseAsync<TCmdlet>(
            this TCmdlet cmdlet,
            string message,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                c => c.WriteVerbose(message),
                true,
                cancellationToken);

            return task;
        }

        public static Task WriteInformationAsync<TCmdlet>(
            this TCmdlet cmdlet,
            InformationRecord information,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                c => c.WriteInformation(information),
                true,
                cancellationToken);

            return task;
        }

        public static Task WriteInformationAsync<TCmdlet>(
            this TCmdlet cmdlet,
            object messageData,
            string[] tags,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                c => c.WriteInformation(messageData, tags),
                true,
                cancellationToken);

            return task;
        }

        public static Task WriteProgressAsync<TCmdlet>(
            this TCmdlet cmdlet,
            ProgressRecord progress,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                c => c.WriteProgress(progress),
                true,
                cancellationToken);

            return task;
        }
    }
}
