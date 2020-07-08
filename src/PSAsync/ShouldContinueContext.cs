using System.Diagnostics;

namespace PSAsync
{
    internal readonly struct ShouldContinueContext
    {
        public ShouldContinueContext(
            bool yesToAll,
            bool noToAll)
        {
            this.YesToAll = yesToAll;
            this.NoToAll = noToAll;
        }

        // https://github.com/dotnet/roslyn-analyzers/issues/2834
        #pragma warning disable CA1822

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

        #pragma warning restore CA1822

        public void Deconstruct(
            out bool yesToAll,
            out bool noToAll)
        {
            yesToAll = this.YesToAll;
            noToAll = this.NoToAll;
        }
    }
}
