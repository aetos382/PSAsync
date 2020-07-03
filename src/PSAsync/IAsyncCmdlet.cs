using System.Threading.Tasks;

namespace PSAsync
{
    public interface IAsyncCmdlet
    {
        protected Task BeginProcessingAsync()
        {
            return Task.CompletedTask;
        }

        protected Task ProcessRecordAsync()
        {
            return Task.CompletedTask;
        }

        protected Task EndProcessingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
