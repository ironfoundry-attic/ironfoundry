namespace IronFoundry.Warden.Tasks
{
    using System;

    public class TaskCommandResultEventArgs : EventArgs
    {
        private readonly TaskCommandResult result;

        public TaskCommandResultEventArgs(TaskCommandResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }
            this.result = result;
        }

        public TaskCommandResult Result
        {
            get { return result; }
        }
    }
}
