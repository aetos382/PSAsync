using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
#pragma warning disable CA1001 // DiangnosticSource は破棄しなくても問題ない

    internal sealed class AsyncCmdletAccessor
    {
        private AsyncCmdletAccessor(
            Type cmdletType)
        {
            this._beginProcessingAsync =
                new Lazy<AsyncPipelineMethod>(() => GetBeginProcessingAsync(cmdletType));

            this._processRecordAsync =
                new Lazy<AsyncPipelineMethod>(() => GetProcessRecordAsync(cmdletType));

            this._endProcessingAsync =
                new Lazy<AsyncPipelineMethod>(() => GetEndProcessingAsync(cmdletType));
        }

        private delegate Task AsyncOperationDelegate(
            Cmdlet cmdlet,
            CancellationToken cancellationToken);

        private readonly struct AsyncPipelineMethod
        {
            public AsyncPipelineMethod(
                AsyncOperationDelegate? @delegate,
                bool isImplemented)
            {
                this.Delegate = @delegate;
                this.IsImplemented = isImplemented;
            }

            public AsyncOperationDelegate? Delegate { get; }

            public bool IsImplemented { get; }

            public static readonly AsyncPipelineMethod NotImplemented =
                new AsyncPipelineMethod(null, false);
        }

        private readonly Lazy<AsyncPipelineMethod> _beginProcessingAsync;
        private readonly Lazy<AsyncPipelineMethod> _processRecordAsync;
        private readonly Lazy<AsyncPipelineMethod> _endProcessingAsync;

        private readonly SwitchingDiagnosticSource _diagnosticSource =
            new SwitchingDiagnosticSource(
                DiagnosticConstants.SourceName,
                DiagnosticConstants.TraceSwitchName);

        public Task DoBeginProcessingAsync<TCmdlet>(
            TCmdlet cmdlet,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            return this.InvokeWithTraceActivity(
                DiagnosticConstants.BeginProcessingAsyncActivity,
                cmdlet,
                this._beginProcessingAsync.Value,
                cancellationToken);
        }

        public Task DoProcessRecordAsync<TCmdlet>(
            TCmdlet cmdlet,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            return this.InvokeWithTraceActivity(
                DiagnosticConstants.ProcessRecordAsyncActivity,
                cmdlet,
                this._processRecordAsync.Value,
                cancellationToken);
        }

        public Task DoEndProcessingAsync<TCmdlet>(
            TCmdlet cmdlet,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            Requires.ArgumentNotNull(cmdlet, nameof(cmdlet));

            return this.InvokeWithTraceActivity(
                DiagnosticConstants.EndProcessingAsyncActivity,
                cmdlet,
                this._endProcessingAsync.Value,
                cancellationToken);
        }

        public bool IsBeginProcessingAsyncImplemented
        {
            get
            {
                return this._beginProcessingAsync.Value.IsImplemented;
            }
        }

        public bool IsProcessRecordAsyncImplemented
        {
            get
            {
                return this._processRecordAsync.Value.IsImplemented;
            }
        }

        public bool IsEndProcessingAsyncImplemented
        {
            get
            {
                return this._endProcessingAsync.Value.IsImplemented;
            }
        }

        private Task InvokeWithTraceActivity<TCmdlet>(
            string activityName,
            TCmdlet cmdlet,
            in AsyncPipelineMethod operation,
            CancellationToken cancellationToken)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            if (!operation.IsImplemented)
            {
                return Task.CompletedTask;
            }

            var cmdletType = cmdlet.GetType();
            Activity? activity = null;

            bool traceEnabled = this._diagnosticSource.IsSwitchEnabled;

            if (traceEnabled)
            {
                activity = new Activity(activityName)
                    .SetIdFormat(ActivityIdFormat.Hierarchical);

                this._diagnosticSource.StartActivity(
                    activity,
                    new
                    {
                        CmdletType = cmdletType
                    });
            }

            var task = operation.Delegate(cmdlet, cancellationToken);

            if (traceEnabled)
            {
                task.ContinueWith(
                    t => {

                        this._diagnosticSource.StopActivity(
                            activity!,
                            new
                            {
                                CmdletType = cmdletType
                            });

                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Current);
            }

            return task;
        }

        private static readonly Dictionary<Type, AsyncCmdletAccessor> _accessors =
            new Dictionary<Type, AsyncCmdletAccessor>();

        public static AsyncCmdletAccessor GetAccessor(
            Type cmdletType)
        {
            Requires.ArgumentNotNull(cmdletType, nameof(cmdletType));

            if (!_accessors.TryGetValue(cmdletType, out var accessor))
            {
                ValidateCmdletType(cmdletType);

                _accessors[cmdletType] = accessor = new AsyncCmdletAccessor(cmdletType);
            }

            return accessor!;
        }

        private static void ValidateCmdletType(
            Type cmdletType)
        {
            bool isCmdletType =
                cmdletType.IsSubclassOf(typeof(Cmdlet)) &&
                typeof(IAsyncCmdlet).IsAssignableFrom(cmdletType);

            if (!isCmdletType)
            {
                throw new ArgumentException();
            }
        }

        private static AsyncPipelineMethod GetBeginProcessingAsync(
            Type cmdletType)
        {
            return GetAsyncCmdletMethod(
                cmdletType,
                nameof(IAsyncCmdlet.BeginProcessingAsync));
        }

        private static AsyncPipelineMethod GetProcessRecordAsync(
            Type cmdletType)
        {
            return GetAsyncCmdletMethod(
                cmdletType,
                nameof(IAsyncCmdlet.ProcessRecordAsync));
        }

        private static AsyncPipelineMethod GetEndProcessingAsync(
            Type cmdletType)
        {
            return GetAsyncCmdletMethod(
                cmdletType,
                nameof(IAsyncCmdlet.EndProcessingAsync));
        }

        private static AsyncPipelineMethod GetAsyncCmdletMethod(
            Type cmdletType,
            string interfaceMethodName)
        {
            var map = cmdletType.GetInterfaceMap(typeof(IAsyncCmdlet));

            var targetMethod = map.InterfaceMethods
                .Zip(
                    map.TargetMethods,
                    (im, tm) =>
                        (InterfaceMethod: im, TargetMethod: tm))
                .FirstOrDefault(
                    x =>
                        x.InterfaceMethod.Name == interfaceMethodName)
                .TargetMethod;

            bool isImplemented = IsImplemented(targetMethod);

            if (!isImplemented)
            {
                return AsyncPipelineMethod.NotImplemented;
            }

            var cmdletParameter = Expression.Parameter(typeof(Cmdlet), "cmdlet");
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

            var @delegate = lambda.Compile();

            return new AsyncPipelineMethod(@delegate, isImplemented);
        }
        
        private static bool IsImplemented(
            MethodInfo methodInfo)
        {
            if (methodInfo.DeclaringType == typeof(IAsyncCmdlet))
            {
                return false;
            }

            var skipAttribute = methodInfo
                .GetCustomAttribute<SkipImplementationCheckAttribute>(false);

            if (skipAttribute is null)
            {
                return true;
            }

            return !skipAttribute.Skip;
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

#pragma warning restore
}
