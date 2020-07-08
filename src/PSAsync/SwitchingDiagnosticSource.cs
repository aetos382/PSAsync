using System;
using System.Diagnostics;

using Microsoft;

namespace PSAsync
{
    internal class SwitchingDiagnosticSource :
        DiagnosticSource,
        IDisposable
    {
        public SwitchingDiagnosticSource(
            string sourceName,
            string switchName)
        {
            Requires.NotNull(sourceName, nameof(sourceName));
            Requires.NotNull(switchName, nameof(switchName));

            this._internalSource = new DiagnosticListener(sourceName);
            this._switchName = switchName;

            this.EnableDefault = true;
        }

        private readonly DiagnosticSource _internalSource;

        private readonly string _switchName;

        // https://github.com/dotnet/roslyn-analyzers/issues/2834
        #pragma warning disable CA1822

        public bool EnableDefault
        {
            [DebuggerStepThrough]
            get;

            [DebuggerStepThrough]
            set;
        }

        #pragma warning restore CA1822

        public override bool IsEnabled(
            string name)
        {
            Requires.NotNull(name, nameof(name));

            if (!this.IsSwitchEnabled)
            {
                return false;
            }

            return this._internalSource.IsEnabled(name);
        }

        public override bool IsEnabled(
            string name,
            object arg1,
            object? arg2 = null)
        {
            Requires.NotNull(name, nameof(name));

            if (!this.IsSwitchEnabled)
            {
                return false;
            }

            return this._internalSource.IsEnabled(name, arg1, arg2);
        }

        public override void Write(
            string name,
            object value)
        {
            Requires.NotNull(name, nameof(name));

            if (this.IsEnabled(name))
            {
                this._internalSource.Write(name, value);
            }
        }

        public bool IsSwitchEnabled
        {
            get
            {
                if (!AppContext.TryGetSwitch(this._switchName, out var enabled))
                {
                    return this.EnableDefault;
                }

                return enabled;
            }
        }

        void IDisposable.Dispose()
        {
            if (this._internalSource is IDisposable d)
            {
                d.Dispose();
            }
        }
    }
}
