using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Threading;

namespace PSAsync
{
    internal static class AsyncCmdletContext
    {
        public static AsyncCmdletContext<TCmdlet> Start<TCmdlet>(
            TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            return AsyncCmdletContext<TCmdlet>.Start(cmdlet);
        }

        public static AsyncCmdletContext<TCmdlet> GetContext<TCmdlet>(
            TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            return AsyncCmdletContext<TCmdlet>.GetContext(cmdlet);
        }
    }

    internal class AsyncCmdletContext<TCmdlet> :
        IActionConsumer,
        IDisposable
        where TCmdlet :
            Cmdlet,
            IAsyncCmdlet
    {
        private readonly TCmdlet _cmdlet;

        private readonly int _mainThreadId;

        private readonly BlockingCollection<IAction> _queue;

        private readonly SynchronizationContext? _originalSynchronizationContext;

        private readonly CancellationTokenSource _cts;

        private readonly DiagnosticSource _diagnosticSource;

        public static AsyncCmdletContext<TCmdlet> Start(
            TCmdlet cmdlet)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            var context = new AsyncCmdletContext<TCmdlet>(cmdlet);
            return context;
        }

        private AsyncCmdletContext(
            TCmdlet cmdlet)
        {
            if (!_contexts.TryAdd(cmdlet, this))
            {
                throw new InvalidOperationException();
            }

            this._originalSynchronizationContext = SynchronizationContext.Current;

            this._cmdlet = cmdlet;

            this._mainThreadId = Thread.CurrentThread.ManagedThreadId;

            this._queue = new BlockingCollection<IAction>();

            this._cts = new CancellationTokenSource();

            this._diagnosticSource = new SwitchingDiagnosticSource(
                DiagnosticConstants.SourceName,
                DiagnosticConstants.TraceSwitchName);

            var syncCtx = new PowerShellSynchronizationContext(this);
            SynchronizationContext.SetSynchronizationContext(syncCtx);

            this._diagnosticSource.Write(
                DiagnosticConstants.Construct,
                Unit.Instance);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(
            bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            if (this.IsMainThread)
            {
                throw new InvalidOperationException();
            }

            this._disposed = true;

            if (disposing)
            {
                SynchronizationContext.SetSynchronizationContext(this._originalSynchronizationContext);

                this._cts.Dispose();
                this._queue.Dispose();

                this._diagnosticSource.Write(
                    DiagnosticConstants.Dispose,
                    Unit.Instance);

                _contexts.Remove(this._cmdlet, out _);
            }
        }

        public IEnumerable<IAction> GetActions()
        {
            this.CheckDisposed();

            return this._queue.GetConsumingEnumerable();
        }

        public void Close()
        {
            if (this._disposed)
            {
                return;
            }

            this._queue.CompleteAdding();

            this._diagnosticSource.Write(
                DiagnosticConstants.Close,
                Unit.Instance);
        }

        public AwaitableAction<TCmdlet> CreateAction(
            TCmdlet cmdlet,
            Action<TCmdlet> action,
            CancellationToken cancellationToken)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            this.CheckDisposed();

            var linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this._cts.Token);

            return new AwaitableAction<TCmdlet>(
                cmdlet,
                action,
                state => ((CancellationTokenSource)state)!.Dispose(),
                linkedTokenSource,
                linkedTokenSource.Token);
        }
        
        public void QueueAction(
            IAction action)
        {
            Requires.ArgumentNotNull(action, nameof(action));

            this.CheckDisposed();

            this._queue.Add(action);

            this._diagnosticSource.Write(
                DiagnosticConstants.QueueAction,
                Unit.Instance);
        }
        
        public AwaitableAction<TCmdlet> QueueAction(
            TCmdlet cmdlet,
            Action<TCmdlet> action,
            CancellationToken cancellationToken)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            this.CheckDisposed();

            var awaitableAction = this.CreateAction(cmdlet, action, cancellationToken);

            this.QueueAction(awaitableAction);

            return awaitableAction;
        }

        public void CancelAsyncOperations()
        {
            if (this._disposed)
            {
                return;
            }

            this._cts.Cancel();

            this._diagnosticSource.Write(
                DiagnosticConstants.CancelAsyncOperations,
                Unit.Instance);
        }

        public CancellationToken GetCancellationToken()
        {
            this.CheckDisposed();

            return this._cts.Token;
        }

        internal bool IsMainThread
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId == this._mainThreadId;
            }
        }

        public static AsyncCmdletContext<TCmdlet> GetContext(
            TCmdlet cmdlet)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            if (!TryGetContext(cmdlet, out var context))
            {
                throw new InvalidOperationException();
            }

            return context;
        }

        public static bool TryGetContext(
            TCmdlet cmdlet,

            [NotNullWhen(true)]
            out AsyncCmdletContext<TCmdlet>? context)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            context = null;

            if (!_contexts.TryGetValue(cmdlet, out var ctx))
            {
                return false;
            }

            if (ctx._disposed)
            {
                return false;
            }

            context = ctx;
            return true;
        }

        private static readonly Dictionary<TCmdlet, AsyncCmdletContext<TCmdlet>> _contexts =
            new Dictionary<TCmdlet, AsyncCmdletContext<TCmdlet>>();

        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(AsyncCmdletContext<TCmdlet>));
            }
        }

        private static class DiagnosticConstants
        {
            public const string SourceName = "PSAsync.AsyncCmdletContext";

            public const string TraceSwitchName = SourceName + ".TraceEnabled";

            public const string Construct = "Construct";

            public const string Dispose = "Dispose";

            public const string Close = "Close";

            public const string QueueAction = "QueueAction";

            public const string CancelAsyncOperations = "CancelAsyncOperations";
        }
    }
}
