﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft;

namespace PSAsync
{
    internal class AsyncCmdletContext :
        IActionConsumer,
        IDisposable
    {
        private readonly Cmdlet _cmdlet;

        private readonly int _mainThreadId;

        private readonly BlockingCollection<IAction> _queue;

        private readonly SynchronizationContext? _originalSynchronizationContext;

        private readonly CancellationTokenSource _cts;

        private readonly DiagnosticSource _diagnosticSource;

        public static AsyncCmdletContext Start<TCmdlet>(
            TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));

            var context = new AsyncCmdletContext(cmdlet);
            return context;
        }

        private AsyncCmdletContext(
            Cmdlet cmdlet)
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

            if (!this.IsMainThread)
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

        public AwaitableAction<TCmdlet, TArgument, TResult> CreateAction<TCmdlet, TArgument, TResult>(
            TCmdlet cmdlet,
            Func<TCmdlet, TArgument, AsyncCmdletContext, TResult> action,
            TArgument argument,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));
            Requires.NotNull(action, nameof(action));

            this.CheckDisposed();

            var linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this._cts.Token);

            return new AwaitableAction<TCmdlet, TArgument, TResult>(
                cmdlet,
                (c, a) => action(c, a, this),
                argument,
                state => ((CancellationTokenSource?)state)!.Dispose(),
                linkedTokenSource,
                linkedTokenSource.Token);
        }

        public void QueueAction(
            IAction action)
        {
            Requires.NotNull(action, nameof(action));

            this.CheckDisposed();

            this._queue.Add(action);

            this._diagnosticSource.Write(
                DiagnosticConstants.QueueAction,
                Unit.Instance);
        }
        
        public Task<TResult> QueueAction<TCmdlet, TArgument, TResult>(
            TCmdlet cmdlet,
            Func<TCmdlet, TArgument, AsyncCmdletContext, TResult> action,
            TArgument argument,
            bool runSynchronouslyOnMainThread,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));
            Requires.NotNull(action, nameof(action));

            this.CheckDisposed();

            var awaitableAction = this.CreateAction(cmdlet, action, argument, cancellationToken);

            awaitableAction.Task.ContinueWith(
                t => {
                    bool pipelineStopped = t.Exception!
                        .Flatten()
                        .InnerExceptions
                        .Any(
                            e => e is PipelineStoppedException);

                    if (!pipelineStopped)
                    {
                        return;
                    }

                    this._cts.Cancel();
                },
                this._cts.Token,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);

            if (runSynchronouslyOnMainThread && this.IsMainThread)
            {
                awaitableAction.Invoke();
            }
            else
            {
                this.QueueAction(awaitableAction);
            }

            return awaitableAction.Task;
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

        // https://github.com/dotnet/roslyn-analyzers/issues/2834
        #pragma warning disable CA1822

        internal ShouldContinueContext ShouldContinueContext
        {
            [DebuggerStepThrough]
            get;

            [DebuggerStepThrough]
            set;
        }

        #pragma warning restore CA1822 

        public static AsyncCmdletContext GetContext<TCmdlet>(
            TCmdlet cmdlet)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));

            if (!TryGetContext(cmdlet, out var context))
            {
                throw new InvalidOperationException();
            }

            return context;
        }

        public static bool TryGetContext<TCmdlet>(
            TCmdlet cmdlet,
            [MaybeNullWhen(false)]
            out AsyncCmdletContext context)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.NotNull(cmdlet, nameof(cmdlet));

            context = null!;

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

        private static readonly Dictionary<Cmdlet, AsyncCmdletContext> _contexts =
            new Dictionary<Cmdlet, AsyncCmdletContext>();

        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(AsyncCmdletContext));
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
