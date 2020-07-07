﻿using System.Management.Automation;
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
                (c, a) => c.WriteObject(a.obj, a.enumerateCollection),
                (obj, enumerateCollection),
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
                (c, a) => c.WriteError(a),
                error,
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
                (c, a) => c.WriteWarning(a),
                message,
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
                (c, a) => c.WriteVerbose(a),
                message,
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
                (c, a) => c.WriteInformation(a),
                information,
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
                (c, a) => c.WriteInformation(a.messageData, a.tags),
                (messageData, tags),
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
                (c, a) => c.WriteProgress(a),
                progress,
                true,
                cancellationToken);

            return task;
        }

        public static Task<bool> ShouldProcessAsync<TCmdlet>(
            this TCmdlet cmdlet,
            string target,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                (c, a) => c.ShouldProcess(a),
                target,
                true,
                cancellationToken);

            return task;
        }

        public static Task<bool> ShouldProcessAsync<TCmdlet>(
            this TCmdlet cmdlet,
            string target,
            string action,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                (c, a) => c.ShouldProcess(a.target, a.action),
                (target, action),
                true,
                cancellationToken);

            return task;
        }

        public struct ShouldProcessResult
        {
            public ShouldProcessResult(
                bool result,
                ShouldProcessReason reason)
            {
                this.Result = result;
                this.Reason = reason;
            }

            public bool Result { get; }

            public ShouldProcessReason Reason { get; }

            public void Deconstruct(
                out bool result,
                out ShouldProcessReason reason)
            {
                result = this.Result;
                reason = this.Reason;
            }
        }

        public static Task<ShouldProcessResult> ShouldProcessAsync<TCmdlet>(
            this TCmdlet cmdlet,
            string verboseDescription,
            string verboseWarning,
            string caption,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                (c, a, x) => {
                    bool result = c.ShouldProcess(a.verboseDescription, a.verboseWarning, a.caption, out var reason);
                    return new ShouldProcessResult(result, reason);
                },
                (verboseDescription, verboseWarning, caption),
                true,
                cancellationToken);

            return task;
        }

        public static Task<bool> ShouldContinueAsync<TCmdlet>(
            this TCmdlet cmdlet,
            string query,
            string caption,
            bool hasSecurityImpact = false,
            bool saveContext = false,
            CancellationToken cancellationToken = default)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            cancellationToken.ThrowIfCancellationRequested();

            var task = cmdlet.QueueAction(
                (c, a, x) => {

                    bool yesToAll = false;
                    bool noToAll = false;

                    if (saveContext)
                    {
                        (yesToAll, noToAll) = x.ShouldContinueContext;
                    }

                    bool result =
                        c.ShouldContinue(
                            a.query,
                            a.caption,
                            a.hasSecurityImpact,
                            ref yesToAll,
                            ref noToAll);

                    if (saveContext)
                    {
                        x.ShouldContinueContext = new ShouldContinueContext(yesToAll, noToAll);
                    }

                    return result;
                },
                (query, caption, hasSecurityImpact),
                true,
                cancellationToken);

            return task;
        }
    }
}
