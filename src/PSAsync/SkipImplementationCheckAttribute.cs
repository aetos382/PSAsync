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

        public bool Skip
        {
            [DebuggerStepThrough]
            get;

            [DebuggerStepThrough]
            set;
        }
    }
}
