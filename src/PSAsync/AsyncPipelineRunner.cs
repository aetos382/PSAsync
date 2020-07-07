using System;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    internal static class AsyncPipelineRunner
    {
        public static void RunAsyncPipeline<TCmdlet>(
            TCmdlet cmdlet,
            Func<TCmdlet, CancellationToken, Task> asyncMethod,
            string pipelineStage)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            using var diagnosticSource = new SwitchingDiagnosticSource(
                DiagnosticConstants.SourceName,
                DiagnosticConstants.TraceSwitchName);

            using var pipelineStageActivity = ActivityScope.Start(
                diagnosticSource,
                new Activity(
                    DiagnosticConstants.AsyncPipelineStageActivity),
                new
                {
                    StageName = pipelineStage
                });

            using var context = AsyncCmdletContext.Start(cmdlet);

            var asyncTask = asyncMethod(cmdlet, context.GetCancellationToken());

            if (asyncTask is null)
            {
                throw new InvalidOperationException();
            }

            ExceptionDispatchInfo? exceptionDispatcher = null;

            try
            {
                asyncTask.ContinueWith(
                    (t, state) =>
                        ((AsyncCmdletContext?)state)!.Close(),
                    context,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Current);

                foreach (var action in context.GetActions())
                {
                    using var pipelineStepActivity = ActivityScope.Start(
                        diagnosticSource,
                        new Activity(
                            DiagnosticConstants.AsyncPipelineStepActivity));

                    action.Invoke();
                }
            }
            finally
            {
                try
                {
                    asyncTask.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                }
                catch (AggregateException ex)
                {
                    var exceptions = ex.Flatten().InnerExceptions;

                    var pse = exceptions.FirstOrDefault(e => e is PipelineStoppedException);

                    if (pse != null)
                    {
                        exceptionDispatcher = ExceptionDispatchInfo.Capture(pse);
                    }
                    else if (exceptions.Any(e => !(e is OperationCanceledException)))
                    {
                        exceptionDispatcher = ExceptionDispatchInfo.Capture(ex);
                    }
                }
            }

            exceptionDispatcher?.Throw();
        }

        private static class DiagnosticConstants
        {
            public const string SourceName = "PSAsync.AsyncMethodRunner";

            public const string TraceSwitchName = SourceName + ".TraceEnabled";

            public const string AsyncPipelineStageActivity = "AsyncPipelineStage";

            public const string AsyncPipelineStepActivity = "AsyncPipelineStep";
        }
    }
}
