using System.Management.Automation;
using System.Threading.Tasks;

namespace PSAsync.Samples
{
    public class TestAsyncCommand :
        Cmdlet,
        IAsyncCmdlet
    {
        #region standard methods

        protected override void BeginProcessing()
        {
            this.DoBeginProcessingAsync();
        }

        protected override void ProcessRecord()
        {
            this.DoBeginProcessingAsync();
        }

        protected override void EndProcessing()
        {
            this.DoEndProcessingAsync();
        }

        #endregion

        #region async methods

        protected async Task BeginProcessingAsync()
        {
        }

        #endregion
    }
}
