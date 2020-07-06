using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    internal class AwaitableAction<TCmdlet> :
        IAction
        where TCmdlet :
            Cmdlet,
            IAsyncCmdlet
    {
        public AwaitableAction(
            TCmdlet cmdlet,
            Action<TCmdlet> action,
            CancellationToken cancellationToken)
            : this(
                cmdlet,
                action,
                null,
                null,
                cancellationToken)
        {
        }

        internal AwaitableAction(
            TCmdlet cmdlet,
            Action<TCmdlet> action,
            Action<object?>? postAction,
            object? postActionState,
            CancellationToken cancellationToken)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            this._cmdlet = cmdlet;
            this._action = action;
            this._cancellationToken = cancellationToken;
            this._postAction = postAction;
            this._postActionState = postActionState;

            cancellationToken.Register(
                () => this._tcs.TrySetCanceled(cancellationToken),
                false);
        }

        private readonly TCmdlet _cmdlet;

        private readonly Action<TCmdlet> _action;

        private readonly Action<object?>? _postAction;

        private readonly object? _postActionState;

        private readonly CancellationToken _cancellationToken;

        private readonly TaskCompletionSource<Unit> _tcs = new TaskCompletionSource<Unit>();

#pragma warning disable CA1031 // 一般的な例外の種類はキャッチしません

        public void Invoke()
        {
            try
            {
                if (this._cancellationToken.IsCancellationRequested)
                {
                    this._tcs.TrySetCanceled(this._cancellationToken);
                    return;
                }

                this._action(this._cmdlet);

                this._tcs.SetResult(Unit.Instance);
            }
            catch (OperationCanceledException)
            {
                this._tcs.SetCanceled();
            }
            catch (Exception ex)
            {
                this._tcs.SetException(ex);
            }
            finally
            {
                this._postAction?.Invoke(this._postActionState);
            }
        }

#pragma warning restore CA1031 // 一般的な例外の種類はキャッチしません

        public Task Task
        {
            get
            {
                return this._tcs.Task;
            }
        }
    }
}
