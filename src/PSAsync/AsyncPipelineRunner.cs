using System;
using System.Diagnostics;
using System.Management.Automation;
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

            bool traceEnabled = diagnosticSource.IsSwitchEnabled;

            Activity? pipelineStageActivity = null;

            if (traceEnabled)
            {
                pipelineStageActivity =
                    new Activity(DiagnosticConstants.AsyncPipelineStageActivity)
                        .SetIdFormat(ActivityIdFormat.Hierarchical);

                diagnosticSource.StartActivity(
                    pipelineStageActivity,
                    new
                    {
                        StageName = pipelineStage
                    });
            }

            using var context = AsyncCmdletContext.Start(cmdlet);

            var task = asyncMethod(cmdlet, context.GetCancellationToken());

            try
            {
                task.ContinueWith(
                    (t, state) =>
                        ((AsyncCmdletContext?)state)!.Close(),
                    context,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Current);

                foreach (var action in context.GetActions())
                {
                    Activity? pipelineStepActivity = null;

                    if (traceEnabled)
                    {
                        pipelineStepActivity =
                            new Activity(DiagnosticConstants.AsyncPipelineStepActivity)
                                .SetIdFormat(ActivityIdFormat.Hierarchical);

                        diagnosticSource.StartActivity(pipelineStepActivity);
                    }

                    action.Invoke();

                    if (traceEnabled)
                    {
                        diagnosticSource.StopActivity(pipelineStepActivity!);
                    }
                }
            }
            finally
            {
                try
                {
                    task.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                }

                if (traceEnabled)
                {
                    diagnosticSource.StopActivity(
                        pipelineStageActivity!,
                        new
                        {
                            StageName = pipelineStage
                        });
                }
            }
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
