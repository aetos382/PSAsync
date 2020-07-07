using System.Diagnostics;

namespace PSAsync
{
    internal static class DiagnosticSourceExtensions
    {
        public static void Write(
            this DiagnosticSource diagnosticSource,
            string name)
        {
            Requires.ArgumentNotNull(diagnosticSource, nameof(diagnosticSource));

            diagnosticSource.Write(name, Unit.Instance);
        }

        public static Activity StartActivity(
            this DiagnosticSource diagnosticSource,
            Activity activity)
        {
            Requires.ArgumentNotNull(diagnosticSource, nameof(diagnosticSource));
            Requires.ArgumentNotNull(activity, nameof(activity));

            return diagnosticSource.StartActivity(activity, Unit.Instance);
        }

        public static void StopActivity(
            this DiagnosticSource diagnosticSource,
            Activity activity)
        {
            Requires.ArgumentNotNull(diagnosticSource, nameof(diagnosticSource));
            Requires.ArgumentNotNull(activity, nameof(activity));

            diagnosticSource.StopActivity(activity, Unit.Instance);
        }
    }
}
