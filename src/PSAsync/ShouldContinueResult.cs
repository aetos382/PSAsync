using System;
using System.Diagnostics;

namespace PSAsync
{
    public readonly struct ShouldContinueResult :
        IEquatable<ShouldContinueResult>
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

        // https://github.com/dotnet/roslyn-analyzers/issues/2834
        #pragma warning disable CA1822

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

        #pragma warning restore CA1822

        public void Deconstruct(
            out bool result,
            out bool yesToAll,
            out bool noToAll)
        {
            result = this.Result;
            yesToAll = this.YesToAll;
            noToAll = this.NoToAll;
        }

        public bool Equals(
            ShouldContinueResult other)
        {
            return
                this.Result == other.Result &&
                this.YesToAll == other.YesToAll &&
                this.NoToAll == other.NoToAll;
        }

        /// <inheritdoc />
        public override bool Equals(
            object? obj)
        {
            return (obj is ShouldContinueResult other) && this.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Result, this.YesToAll, this.NoToAll);
        }

        public static bool operator ==(
            ShouldContinueResult left,
            ShouldContinueResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            ShouldContinueResult left,
            ShouldContinueResult right)
        {
            return !(left == right);
        }
    }
}
