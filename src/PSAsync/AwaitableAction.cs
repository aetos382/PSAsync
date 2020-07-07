using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    internal class AwaitableAction<TCmdlet, TArgument, TResult> :
        IAction
        where TCmdlet :
            Cmdlet,
            IAsyncCmdlet
    {
        internal AwaitableAction(
            TCmdlet cmdlet,
            Func<TCmdlet, TArgument, TResult> action,
            TArgument argument,
            Action<object?>? postAction = null,
            object? postActionState = null,
            CancellationToken cancellationToken = default)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));
            Requires.ArgumentNotNull(action, nameof(action));

            this._cmdlet = cmdlet;
            this._action = action;
            this._argument = argument;
            this._postAction = postAction;
            this._postActionState = postActionState;
            this._cancellationToken = cancellationToken;

            cancellationToken.Register(
                () => this._tcs.TrySetCanceled(cancellationToken),
                false);
        }

        private readonly TCmdlet _cmdlet;

        private readonly Func<TCmdlet, TArgument, TResult> _action;

        private readonly TArgument _argument;

        private readonly Action<object?>? _postAction;

        private readonly object? _postActionState;

        private readonly CancellationToken _cancellationToken;

        private readonly TaskCompletionSource<TResult> _tcs = new TaskCompletionSource<TResult>();

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

                var result = this._action(this._cmdlet, this._argument);

                this._tcs.TrySetResult(result);
            }
            catch (OperationCanceledException ex)
            {
                this._tcs.TrySetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                this._tcs.TrySetException(ex);
            }
            finally
            {
                this._postAction?.Invoke(this._postActionState);
            }
        }

#pragma warning restore CA1031 // 一般的な例外の種類はキャッチしません

        public Task<TResult> Task
        {
            get
            {
                return this._tcs.Task;
            }
        }
    }
}
