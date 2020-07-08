using System;
using System.Diagnostics;
using System.Management.Automation;

namespace PSAsync
{
    public readonly struct ShouldProcessResult :
        IEquatable<ShouldProcessResult>
    {
        public ShouldProcessResult(
            bool result,
            ShouldProcessReason reason)
        {
            this.Result = result;
            this.Reason = reason;
        }

        // https://github.com/dotnet/roslyn-analyzers/issues/2834
        #pragma warning disable CA1822

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

        #pragma warning restore CA1822

        public void Deconstruct(
            out bool result,
            out ShouldProcessReason reason)
        {
            result = this.Result;
            reason = this.Reason;
        }

        public bool Equals(
            ShouldProcessResult other)
        {
            return
                this.Result == other.Result &&
                this.Reason == other.Reason;
        }

        /// <inheritdoc />
        public override bool Equals(
            object? obj)
        {
            return (obj is ShouldProcessResult other) && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Result, (int) this.Reason);
        }

        public static bool operator ==(
            ShouldProcessResult left,
            ShouldProcessResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            ShouldProcessResult left,
            ShouldProcessResult right)
        {
            return !(left == right);
        }
    }
}
