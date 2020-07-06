using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    internal static class AsyncMethodRunner
    {
        private static class DiagnosticConstants
        {
            public const string SourceName = "PSAsync.AsyncMethodRunner";

            public const string TraceSwitchName = SourceName + ".TraceEnabled";

            public const string AsyncActionActivity = "AsyncAction";

            public const string OperationCancelled = "OperationCancelled";

            public const string Exception = "Exception";
        }

        public static void DoAsyncOperation<TCmdlet>(
            TCmdlet cmdlet,
            Func<TCmdlet, CancellationToken, Task> asyncMethod)
            where TCmdlet :
                Cmdlet,
                IAsyncCmdlet
        {
            var diagnosticSource = new SwitchingDiagnosticSource(
                DiagnosticConstants.SourceName,
                DiagnosticConstants.TraceSwitchName);

            using var context = AsyncCmdletContext.Start(cmdlet);

            var task = asyncMethod(cmdlet, context.GetCancellationToken());

            try
            {
                task.ContinueWith(
                    (t, state) =>
                        ((AsyncCmdletContext<TCmdlet>)state)!.Close(),
                    context,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Current);

                foreach (var action in context.GetActions())
                {
                    Activity? activity = null;

                    bool traceEnabled = diagnosticSource.IsSwitchEnabled;

                    if (traceEnabled)
                    {
                        activity = new Activity(DiagnosticConstants.AsyncActionActivity)
                            .SetIdFormat(ActivityIdFormat.Hierarchical);

                        diagnosticSource.StartActivity(activity, Unit.Instance);
                    }

                    try
                    {
                        action.Invoke();
                    }
                    catch (OperationCanceledException)
                    {
                        diagnosticSource.Write(
                            DiagnosticConstants.OperationCancelled,
                            Unit.Instance);
                    }
                    catch (Exception ex)
                    {
                        diagnosticSource.Write(
                            DiagnosticConstants.Exception,
                                new {
                                    Exception = ex
                                });

                        context.CancelAsyncOperations();
                        throw;
                    }
                    finally
                    {
                        if (traceEnabled)
                        {
                            diagnosticSource.StopActivity(activity!, Unit.Instance);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    task.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
    }
}
