using System.Diagnostics;
using System.Management.Automation;

namespace PSAsync
{
    public readonly struct ShouldProcessResult
    {
        public ShouldProcessResult(
            bool result,
            ShouldProcessReason reason)
        {
            this.Result = result;
            this.Reason = reason;
        }

        public bool Result
        {
            [DebuggerStepThrough]
            get;
        }

        public ShouldProcessReason Reason
        {
            [DebuggerStepThrough]
            get;
        }

        public void Deconstruct(
            out bool result,
            out ShouldProcessReason reason)
        {
            result = this.Result;
            reason = this.Reason;
        }

        public static implicit operator bool(
            ShouldProcessResult result)
        {
            return result.Result;
        }
    }
}
