namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;

    public abstract class TaskCommand
    {
        protected readonly Container container;
        protected readonly string[] arguments;

        public TaskCommand(Container container, string[] arguments)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;
            this.arguments = arguments;
        }

        /*
         * Synchronous execution
         */
        public abstract TaskCommandResult Execute();

        /*
         * Asynchronous execution
         */
        public event EventHandler<TaskCommandResultEventArgs> ResultAvailable;

        public virtual Task ExecuteAsync()
        {
            return Task.Factory.StartNew(() =>
                {
                    TaskCommandResult result = Execute();
                    OnResultAvailable(result);
                });
        }

        protected void OnResultAvailable(TaskCommandResult result)
        {
            if (ResultAvailable != null)
            {
                ResultAvailable(this, new TaskCommandResultEventArgs(result));
            }
        }
    }
}
