using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PSAsync
{
    public interface IAsyncCmdlet
    {
        public Task BeginProcessingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
