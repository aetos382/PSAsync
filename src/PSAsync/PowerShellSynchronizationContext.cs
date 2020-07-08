using System.Diagnostics;
using System.Threading;

using Microsoft;

namespace PSAsync
{
    internal sealed class PowerShellSynchronizationContext :
        SynchronizationContext
    {
        public PowerShellSynchronizationContext(
            IActionConsumer queue)
        {
            Requires.NotNull(queue, nameof(queue));

            this._queue = queue;
        }

        private readonly IActionConsumer _queue;

        public override void Post(
            SendOrPostCallback d,
            object? state)
        {
            Requires.NotNull(d, nameof(d));

            this._queue.QueueAction(new Callback(d, state));

            this._diagnosticSource.Write(
                DiagnosticConstants.Post,
                Unit.Instance);
        }

        private class Callback :
            IAction
        {
            public Callback(
                SendOrPostCallback callback,
                object? state)
            {
                this._callback = callback;
                this._state = state;
            }

            private readonly SendOrPostCallback _callback;

            private readonly object? _state;

            public void Invoke()
            {
                this._callback(this._state);
            }
        }

        private static class DiagnosticConstants
        {
            public const string SourceName = "PSAsync.PowerShellSynchronizationContext";

            public const string TraceSwitchName = SourceName + ".TraceEnabled";

            public const string Post = "Post";
        }

        private readonly DiagnosticSource _diagnosticSource =
            new SwitchingDiagnosticSource(
                DiagnosticConstants.SourceName,
                DiagnosticConstants.TraceSwitchName);
    }
}
