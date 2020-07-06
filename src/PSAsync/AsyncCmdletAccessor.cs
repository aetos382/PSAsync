using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    internal sealed class AsyncCmdletAccessor<TCmdlet>
        where TCmdlet :
            Cmdlet,
            IAsyncCmdlet
    {
        private AsyncCmdletAccessor()
        {
            this._beginProcessingAsync =
                new Lazy<AsyncOperationDelegate>(() => GetBeginProcessingAsync());

            this._processRecordAsync =
                new Lazy<AsyncOperationDelegate>(() => GetProcessRecordAsync());

            this._endProcessingAsync =
                new Lazy<AsyncOperationDelegate>(() => GetEndProcessingAsync());
        }

        private delegate Task AsyncOperationDelegate(
            TCmdlet cmdlet,
            CancellationToken cancellationToken);

        private readonly Lazy<AsyncOperationDelegate> _beginProcessingAsync;
        private readonly Lazy<AsyncOperationDelegate> _processRecordAsync;
        private readonly Lazy<AsyncOperationDelegate> _endProcessingAsync;

        private readonly SwitchingDiagnosticSource _diagnosticSource =
            new SwitchingDiagnosticSource(
                DiagnosticConstants.SourceName,
                DiagnosticConstants.TraceSwitchName);

        public Task DoBeginProcessingAsync(
            TCmdlet cmdlet,
            CancellationToken cancellationToken)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            return this.InvokeWithTraceActivity(
                DiagnosticConstants.BeginProcessingAsyncActivity,
                cmdlet,
                this._beginProcessingAsync.Value,
                cancellationToken);
        }

        public Task DoProcessRecordAsync(
            TCmdlet cmdlet,
            CancellationToken cancellationToken)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            return this.InvokeWithTraceActivity(
                DiagnosticConstants.ProcessRecordAsyncActivity,
                cmdlet,
                this._processRecordAsync.Value,
                cancellationToken);
        }

        public Task DoEndProcessingAsync(
            TCmdlet cmdlet,
            CancellationToken cancellationToken)
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            return this.InvokeWithTraceActivity(
                DiagnosticConstants.EndProcessingAsyncActivity,
                cmdlet,
                this._endProcessingAsync.Value,
                cancellationToken);
        }

        private Task InvokeWithTraceActivity(
            string activityName,
            TCmdlet cmdlet,
            AsyncOperationDelegate operation,
            CancellationToken cancellationToken)
        {
            Activity? activity = null;

            bool traceEnabled = this._diagnosticSource.IsSwitchEnabled;

            if (traceEnabled)
            {
                activity = new Activity(activityName)
                    .SetIdFormat(ActivityIdFormat.Hierarchical);

                this._diagnosticSource.StartActivity(
                    activity,
                    new {
                        CmdletType = typeof(TCmdlet)
                    });
            }

            var task = operation(cmdlet, cancellationToken);

            if (traceEnabled)
            {
                task.ContinueWith(
                    t => {

                        this._diagnosticSource.StopActivity(
                            activity!,
                            new {
                                CmdletType = typeof(TCmdlet)
                            });

                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Current);
            }

            return task;
        }

        public static readonly AsyncCmdletAccessor<TCmdlet> Instance = new AsyncCmdletAccessor<TCmdlet>();

        private static AsyncOperationDelegate GetBeginProcessingAsync()
        {
            return GetAsyncCmdletMethod(nameof(IAsyncCmdlet.BeginProcessingAsync));
        }

        private static AsyncOperationDelegate GetProcessRecordAsync()
        {
            return GetAsyncCmdletMethod(nameof(IAsyncCmdlet.ProcessRecordAsync));
        }

        private static AsyncOperationDelegate GetEndProcessingAsync()
        {
            return GetAsyncCmdletMethod(nameof(IAsyncCmdlet.EndProcessingAsync));
        }

        private static AsyncOperationDelegate GetAsyncCmdletMethod(
            string interfaceMethodName)
        {
            var cmdletType = typeof(TCmdlet);
            var map = cmdletType.GetInterfaceMap(typeof(IAsyncCmdlet));

            var targetMethod = map.InterfaceMethods
                .Zip(
                    map.TargetMethods,
                    (im, tm) => (InterfaceMethod: im, TargetMethod: tm))
                .FirstOrDefault(
                    x => x.InterfaceMethod.Name == interfaceMethodName)
                .TargetMethod;

            var cmdletParameter = Expression.Parameter(typeof(TCmdlet), "cmdlet");
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            var lambda = Expression.Lambda<AsyncOperationDelegate>(
                Expression.Call(
                    Expression.Convert(
                        cmdletParameter,
                        cmdletType),
                    targetMethod,
                    cancellationTokenParameter),
                cmdletParameter,
                cancellationTokenParameter);

            return lambda.Compile();
        }

        private static class DiagnosticConstants
        {
            public const string SourceName = "PSAsync.AsyncCmdletAccessor";

            public const string TraceSwitchName = SourceName + ".TraceEnabled";

            public const string BeginProcessingAsyncActivity = "BeginProcessingAsync";

            public const string ProcessRecordAsyncActivity = "ProcessRecordAsync";

            public const string EndProcessingAsyncActivity = "EndProcessingAsync";
        }
    }
}
