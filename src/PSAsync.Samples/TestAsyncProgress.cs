using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync.Samples
{
    [Cmdlet(VerbsDiagnostic.Test, "AsyncProgress")]
    public class TestAsyncProgress :
        Cmdlet,
        IAsyncCmdlet
    {
        #region standard methods

        protected override void BeginProcessing()
        {
            // this.DoBeginProcessingAsync();
        }

        protected override void ProcessRecord()
        {
            this.DoProcessRecordAsync();
        }

        protected override void EndProcessing()
        {
            // this.DoEndProcessingAsync();
        }

        protected override void StopProcessing()
        {
            this.CancelAsyncOperation();
        }

        #endregion

        #region parameters

        [Parameter]
        [ValidateRange(1, 10)]
        public int ThreadCount { get; set; } = 3;

        #endregion

        #region async pipeline methods

        public async Task ProcessRecordAsync(
            CancellationToken cancellationToken)
        {
            var parentProgress = new ProgressRecord(
                0,
                "いろいろしています",
                $"{this.ThreadCount} スレッドで処理中");

            this
                .WriteProgressAsync(parentProgress)
                .ConfigureAwait(false);

            var random = new Random();

            var tasks = new List<Task>(this.ThreadCount);

            for (int i = 0; i < this.ThreadCount; ++i)
            {
                int c = random.Next(20) + 50;

                var task = this.WorkAsync(
                    i + 1,
                    parentProgress.ActivityId,
                    c,
                    cancellationToken);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            parentProgress.RecordType = ProgressRecordType.Completed;

            await this
                .WriteProgressAsync(parentProgress)
                .ConfigureAwait(false);
        }

        #endregion

        #region implementation

        private async Task WorkAsync(
            int activityId,
            int parentActivityId,
            int count,
            CancellationToken cancellationToken)
        {
            var childProgess = new ProgressRecord(
                activityId,
                $"タスク {activityId} を処理しています",
                "初期化中…")
            {
                ParentActivityId = parentActivityId
            };

            try
            {
                for (int i = 0; i < count; ++i)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task
                        .Delay(TimeSpan.FromMilliseconds(100))
                        .ConfigureAwait(false);

                    childProgess.PercentComplete = (int)((double)i * 100 / count);
                    childProgess.StatusDescription = $"Thread Id: {Thread.CurrentThread.ManagedThreadId}";

                    await this
                        .WriteProgressAsync(childProgess, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            finally
            {
                childProgess.RecordType = ProgressRecordType.Completed;

                await this
                    .WriteProgressAsync(childProgess)
                    .ConfigureAwait(false);
            }
        }

        #endregion
    }
}
