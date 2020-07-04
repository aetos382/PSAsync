/*
    PSAsync.Analyzer は .NET Standard 2.0 だが、CallerArgumentExpressionAttribute は .NET Standard には存在しないので、
    コードの互換性のためにダミークラスを入れておく。
*/

#if NETSTANDARD2_0

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute :
        Attribute
    {
        public CallerArgumentExpressionAttribute(
            string parameterName)
        {
            this.ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}

#endif
