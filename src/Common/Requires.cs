using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PSAsync
{
    // https://github.com/dotnet/roslyn-analyzers/issues/3451
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class ValidatedNotNullAttribute :
        Attribute
    {
    }

    public static class Requires
    {
        [return: NotNull]
        public static T ArgumentNotNull<T>(
            [ValidatedNotNull]
            T obj,

            [CallerArgumentExpression("obj")]
            string parameterName = "")
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
