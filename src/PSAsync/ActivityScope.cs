using System;
using System.Diagnostics;

using Microsoft;

namespace PSAsync
{
    internal class ActivityScope :
        IDisposable
    {
        private ActivityScope(
            DiagnosticSource diagnosticSource,
            Activity activity,
            object? args)
        {
            Requires.NotNull(diagnosticSource, nameof(diagnosticSource));
            Requires.NotNull(activity, nameof(activity));

            this._diagnosticSource = diagnosticSource;
            this._activity = activity;
            this._activityArgs = args;
        }

        private readonly DiagnosticSource _diagnosticSource;

        private readonly Activity _activity;

        private readonly object? _activityArgs;

        public static ActivityScope Start(
            DiagnosticSource diagnosticSource,
            Activity activity,
            object? args = null)
        {
            var scope = new ActivityScope(diagnosticSource, activity, args);

            diagnosticSource.StartActivity(activity, args ?? Unit.Instance);

            return scope;
        }

        public void Dispose()
        {
            this._diagnosticSource.StopActivity(this._activity, this._activityArgs);
        }
    }
}
