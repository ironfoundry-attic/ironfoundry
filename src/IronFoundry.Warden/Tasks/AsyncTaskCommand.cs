namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;

    public abstract class AsyncTaskCommand : TaskCommand
    {
        public AsyncTaskCommand(Container container, string[] arguments)
            : base(container, arguments)
        {
        }

        public override bool CanExecuteAsync
        {
            get { return true; }
        }

        /*
         * Asynchronous execution
         */
        public event EventHandler<TaskCommandStatusEventArgs> StatusAvailable;

        public abstract Task<TaskCommandResult> ExecuteAsync();

        protected void OnStatusAvailable(TaskCommandStatus status)
        {
            if (StatusAvailable != null)
            {
                StatusAvailable(this, new TaskCommandStatusEventArgs(status));
            }
        }
    }
}
