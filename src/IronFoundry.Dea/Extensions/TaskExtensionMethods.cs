namespace System.Threading.Tasks
{
    public static class TaskExtensionMethods
    {
        public static bool IsRunning(this Task argThis)
        {
            return TaskStatus.Running == argThis.Status;
        }
    }
}