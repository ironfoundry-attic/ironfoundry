namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using Types;
    using CloudFoundry.Net.Vmc;
    using GalaSoft.MvvmLight.Messaging;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using System.Windows.Threading;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel;

    public class InstanceTreeViewItemViewModel : TreeViewItemViewModel
    {
        private const string Host_Default = "Loading...";
        private StatInfo statInfo;
        private Application app;
        private ICloudFoundryProvider provider;
        private string host = Host_Default;

        public InstanceTreeViewItemViewModel(StatInfo statInfo, ApplicationTreeViewItemViewModel parentApplication) : base(parentApplication, true)
        {
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            this.app = parentApplication.Application;
            this.statInfo = statInfo;

            if (statInfo.Stats == null || String.IsNullOrWhiteSpace(statInfo.Stats.Host))
                SetHostName();
            else
                this.Host = statInfo.Stats.Host;
            

            var instanceTimer = new DispatcherTimer();
            instanceTimer.Interval = TimeSpan.FromSeconds(10);
            instanceTimer.Tick += (s, e) => SetHostName();
            instanceTimer.Start();
        }

        private void SetHostName()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                if (host.Equals(Host_Default))
                {
                    var result = provider.GetStats(app, app.Parent);
                    if (result.Response != null)
                    {
                        var newStatInfo =
                            result.Response.SingleOrDefault((st) => st.ID == this.statInfo.ID);
                        if (newStatInfo != null && newStatInfo.Stats != null)
                            this.Host = newStatInfo.Stats.Host;
                    }
                }
            };
            worker.RunWorkerAsync();
        }

        public string Host
        {
            get { return this.host; }
            set { this.host = value; RaisePropertyChanged("Host"); }
        }

        public override void LoadChildren()
        {
            Children.Clear();
            ushort id = (ushort)statInfo.ID;
            var result = provider.GetFiles(app.Parent, app, "/", id);
            if (result.Response == null)
            {
                Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
                return;
            }

            foreach (var dir in result.Response.Directories)
                base.Children.Add(new FolderTreeViewItemViewModel(dir.Name, dir.Name, app, id));
            foreach (var file in result.Response.Files)
                base.Children.Add(new FileTreeViewItemViewModel(file.Name, file.Name, app, id));
        }
    }
}