using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Threading;

using Microsoft.Extensions.DiagnosticAdapter;

namespace PSAsync.Samples
{
#pragma warning disable CA1822 // メンバーを static に設定します
#pragma warning disable CA1801 // 使用されていないパラメーターの確認
#pragma warning disable IDE0051 // 使用されていないプライベート メンバーを削除する
#pragma warning disable IDE0060 // 未使用のパラメーターを削除します

    public class ModuleDiagnostics :
        IModuleAssemblyInitializer
    {
        public void OnImport()
        {
            this.SetupDiagnosticsListening();
        }

        [Conditional("DEBUG")]
        private void SetupDiagnosticsListening()
        {
            AppContext.SetSwitch("PSAsync.AsyncCmdletAccessor.TraceEnabled", true);
            AppContext.SetSwitch("PSAsync.AsyncMethodRunner.TraceEnabled", true);
            AppContext.SetSwitch("PSAsync.AsyncCmdletContext.TraceEnabled", true);
            AppContext.SetSwitch("PSAsync.PowerShellSynchronizationContext.TraceEnabled", true);

            DiagnosticListener.AllListeners.Subscribe(
                listener => {
                    switch (listener.Name)
                    {
                        case "PSAsync.AsyncCmdletAccessor":
                            listener.SubscribeWithAdapter(new AsyncCmdletAccessorListener());
                            break;

                        case "PSAsync.AsyncCmdletContext":
                            listener.SubscribeWithAdapter(new AsyncCmdletContextListener());
                            break;

                        case "PSAsync.AsyncMethodRunner":
                            listener.SubscribeWithAdapter(new AsyncMethodRunnerListener());
                            break;

                        case "PSAsync.PowerShellSynchronizationContext":
                            listener.SubscribeWithAdapter(new PowerShellSynchronizationContextListener());
                            break;
                    }
                });
        }

        private class AsyncCmdletAccessorListener
        {
            [DiagnosticName("BeginProcessingAsync.Start")]
            private void OnStartBeginProcessingAsync(
                Type cmdletType)
            {
                var activity = Activity.Current;

                Console.WriteLine($"AsyncCmdletAccessor.BeginProcessingAsync.Start(id: {activity.Id})");
            }

            [DiagnosticName("BeginProcessingAsync.Stop")]
            private void OnStopBeginProcessingAsync(
                Type cmdletType)
            {
                var activity = Activity.Current;

                Console.WriteLine($"AsyncCmdletAccessor.BeginProcessingAsync.Stop(id: {activity.Id})");
            }

            [DiagnosticName("ProcessRecordAsync.Start")]
            private void OnStartProcessRecordAsync(
                Type cmdletType)
            {
                var activity = Activity.Current;

                Console.WriteLine($"AsyncCmdletAccessor.ProcessRecordAsync.Start(id: {activity.Id})");
            }

            [DiagnosticName("ProcessRecordAsync.Stop")]
            private void OnStopProcessRecordAsync(
                Type cmdletType)
            {
                var activity = Activity.Current;

                Console.WriteLine($"AsyncCmdletAccessor.ProcessRecordAsync.Stop(id: {activity.Id})");
            }

            [DiagnosticName("EndProcessingAsync.Start")]
            private void OnStartEndProcessingAsync(
                Type cmdletType)
            {
                var activity = Activity.Current;

                Console.WriteLine($"AsyncCmdletAccessor.EndProcessingAsync.Start(id: {activity.Id})");
            }

            [DiagnosticName("EndProcessingAsync.Stop")]
            private void OnStopEndProcessingAsync(
                Type cmdletType)
            {
                var activity = Activity.Current;

                Console.WriteLine($"AsyncCmdletAccessor.EndProcessingAsync.Stop(id: {activity.Id})");
            }
        }

        private class AsyncMethodRunnerListener
        {
            [DiagnosticName("AsyncAction.Start")]
            private void OnStartAsyncAction()
            {
                var activity = Activity.Current;

                Console.WriteLine($"AsyncMethodRunner.AsyncAction.Start(id: {activity.Id})");
            }

            [DiagnosticName("AsyncAction.Stop")]
            private void OnStopAsyncAction()
            {
                var activity = Activity.Current;

                Console.WriteLine($"AsyncMethodRunner.AsyncAction.Stop(id: {activity.Id}, duration: {activity.Duration})");
            }
        }

        private class AsyncCmdletContextListener
        {
            [DiagnosticName("Construct")]
            private void OnConstruct()
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                Console.WriteLine($"AsyncCmdletContext.ctor(threadId: {threadId})");
            }

            [DiagnosticName("QueueAction")]
            private void OnQueueAction(
                bool canceled,
                int actionHashCode)
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                Console.WriteLine($"AsyncCmdletContext.QueueAction(canceled: {canceled}, action: {actionHashCode}, threadId: {threadId})");
            }
            
            [DiagnosticName("CancelAsyncOperations")]
            private void OnCancelAsyncOperations()
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                Console.WriteLine($"AsyncCmdletContext.CancelAsyncOperations(threadId: {threadId})");
            }

            [DiagnosticName("Close")]
            private void OnClose()
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                Console.WriteLine($"AsyncCmdletContext.Close(threadId: {threadId})");
            }

            [DiagnosticName("Dispose")]
            private void OnDispose()
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                Console.WriteLine($"AsyncCmdletContext.Dispose(threadId: {threadId})");
            }
        }

        private class PowerShellSynchronizationContextListener
        {
            [DiagnosticName("Post")]
            private void OnPost()
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                Console.WriteLine($"PowerShellSynchronizationContext.Post(threadId: {threadId})");
            }
        }
    }

#pragma warning restore IDE0060 // 未使用のパラメーターを削除します
#pragma warning restore IDE0051 // 使用されていないプライベート メンバーを削除する
#pragma warning restore CA1801 // 使用されていないパラメーターの確認
#pragma warning restore CA1822 // メンバーを static に設定します
}
