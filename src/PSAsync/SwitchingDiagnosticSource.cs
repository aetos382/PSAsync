using System;
using System.Diagnostics;

namespace PSAsync
{
    internal class SwitchingDiagnosticSource :
        DiagnosticSource,
        IDisposable
    {
        public SwitchingDiagnosticSource(
            string sourceName,
            string switchName)
            : this(
                new DiagnosticListener(sourceName),
                switchName)
        {
        }

        public SwitchingDiagnosticSource(
            DiagnosticSource internalSource,
            string switchName)
        {
            Requires.ArgumentNotNull(internalSource, nameof(internalSource));
            Requires.ArgumentNotNull(switchName, nameof(switchName));

            this._internalSource = internalSource;
            this._switchName = switchName;
        }

        private readonly DiagnosticSource _internalSource;

        private readonly string _switchName;

        public bool AutoSuppress { get; set; } = true;

        public bool EnableDefault { get; set; } = false;

        public override bool IsEnabled(
            string name)
        {
            Requires.ArgumentNotNull(name, nameof(name));

            if (!this.IsSwitchEnabled)
            {
                return false;
            }

            return this._internalSource.IsEnabled(name);
        }

        public override void Write(
            string name,
            object value)
        {
            Requires.ArgumentNotNull(name, nameof(name));

            if (!this.AutoSuppress || this.IsEnabled(name))
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
