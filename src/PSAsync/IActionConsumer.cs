namespace PSAsync
{
    internal interface IActionConsumer
    {
        void QueueAction(
            IAction action);
    }
}
