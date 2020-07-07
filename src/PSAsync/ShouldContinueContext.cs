namespace PSAsync
{
    internal readonly struct ShouldContinueContext
    {
        public ShouldContinueContext(
            bool yesToAll,
            bool noToAll)
        {
            this.YesToAll = yesToAll;
            this.NoToAll = noToAll;
        }

        public bool YesToAll { get; }

        public bool NoToAll { get; }

        public void Deconstruct(
            out bool yesToAll,
            out bool noToAll)
        {
            yesToAll = this.YesToAll;
            noToAll = this.NoToAll;
        }
    }
}
