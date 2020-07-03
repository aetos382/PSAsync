using System;
using System.Runtime.CompilerServices;

namespace PSAsync
{
    internal static class Requires
    {
        public static T ArgumentNotNull<T>(
            T obj,
            [CallerArgumentExpression("obj")] string parameterName = "")
            where T : class
        {
            if (obj is null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return obj;
        }
    }
}
