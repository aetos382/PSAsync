using System;
using System.Diagnostics;

namespace PSAsync
{
    [AttributeUsage(
        AttributeTargets.Method,
        Inherited = false)]
    public class SkipImplementationCheckAttribute :
        Attribute
    {
        public SkipImplementationCheckAttribute()
        {
            this.Skip = true;
        }

        // https://github.com/dotnet/roslyn-analyzers/issues/2834
        #pragma warning disable CA1822

        public bool Skip
        {
            [DebuggerStepThrough]
            get;

            [DebuggerStepThrough]
            set;
        }

        #pragma warning restore CA1822
    }
}
