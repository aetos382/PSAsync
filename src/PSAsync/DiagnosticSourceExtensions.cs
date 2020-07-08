using System.Diagnostics;

using Microsoft;

namespace PSAsync
{
    internal static class DiagnosticSourceExtensions
    {
        public static void Write(
            this DiagnosticSource diagnosticSource,
            string name)
        {
            Requires.NotNull(diagnosticSource, nameof(diagnosticSource));

            diagnosticSource.Write(name, Unit.Instance);
        }

        public static Activity StartActivity(
            this DiagnosticSource diagnosticSource,
            Activity activity)
        {
            Requires.NotNull(diagnosticSource, nameof(diagnosticSource));
            Requires.NotNull(activity, nameof(activity));

            return diagnosticSource.StartActivity(activity, Unit.Instance);
        }

        public static void StopActivity(
            this DiagnosticSource diagnosticSource,
            Activity activity)
        {
            Requires.NotNull(diagnosticSource, nameof(diagnosticSource));
            Requires.NotNull(activity, nameof(activity));

            diagnosticSource.StopActivity(activity, Unit.Instance);
        }
    }
}
