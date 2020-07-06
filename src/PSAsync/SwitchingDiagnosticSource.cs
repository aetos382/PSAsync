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
        {
            Requires.ArgumentNotNull(sourceName, nameof(sourceName));
            Requires.ArgumentNotNull(switchName, nameof(switchName));

            this._internalSource = new DiagnosticListener(sourceName);
            this._switchName = switchName;
        }

        private readonly DiagnosticSource _internalSource;

        private readonly string _switchName;

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
