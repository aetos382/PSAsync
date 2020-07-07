using System.Diagnostics;

namespace PSAsync
{
    public readonly struct ShouldContinueResult
    {
        public ShouldContinueResult(
            bool result,
            bool yesToAll,
            bool noToAll)
        {
            this.Result = result;
            this.YesToAll = yesToAll;
            this.NoToAll = noToAll;
        }

        public bool Result
        {
            [DebuggerStepThrough]
            get;
        }

        public bool YesToAll
        {
            [DebuggerStepThrough]
            get;
        }

        public bool NoToAll
        {
            [DebuggerStepThrough]
            get;
        }

        public void Deconstruct(
            out bool result,
            out bool yesToAll,
            out bool noToAll)
        {
            result = this.Result;
            yesToAll = this.YesToAll;
            noToAll = this.NoToAll;
        }

        public static implicit operator bool(
            ShouldContinueResult result)
        {
            return result.Result;
        }
    }
}
