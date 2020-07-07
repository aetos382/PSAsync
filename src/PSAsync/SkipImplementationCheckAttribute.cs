using System;

namespace PSAsync
{
    [AttributeUsage(
        AttributeTargets.Method,
        Inherited = false)]
    public class SkipImplementationCheckAttribute :
        Attribute
    {
        public bool Skip { get; set; } = true;
    }
}
