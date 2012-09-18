namespace IronFoundry.Ui.Controls.Model
{
    using System;
    using Models;

    public class CloudEventArgs : EventArgs
    {
        private readonly Cloud cloud;

        public CloudEventArgs(Cloud cloud)
        {
            this.cloud = cloud;
        }

        public Cloud Cloud { get { return cloud; } }
    }
}