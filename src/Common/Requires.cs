using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PSAsync
{
    internal static class Requires
    {
        [return: NotNull]
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
