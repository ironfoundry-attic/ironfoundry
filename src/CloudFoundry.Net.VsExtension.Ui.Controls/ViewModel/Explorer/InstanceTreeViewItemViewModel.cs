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
        private Instance instance;
        private Application app;
        private ICloudFoundryProvider provider;
        public Dispatcher dispatcher;

        public InstanceTreeViewItemViewModel(Instance instance, ApplicationTreeViewItemViewModel parentApplication) : base(parentApplication, true)
        {
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            this.app = parentApplication.Application;
            this.instance = instance;
            this.dispatcher = Dispatcher.CurrentDispatcher;                       
        }

        public Instance Instance
        {
            get { return this.instance; }
            set { this.instance = value; RaisePropertyChanged("Instance"); }
        }       

        public override void LoadChildren()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                dispatcher.BeginInvoke((Action)(() => Children.Clear()));
                ushort id = (ushort)this.instance.ID;
                var result = provider.GetFiles(app.Parent, app, "/", id);
                if (result.Response == null)
                {
                    Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
                    return;
                }

                dispatcher.BeginInvoke((Action)(()=> {
                    foreach (var dir in result.Response.Directories)
                        base.Children.Add(new FolderTreeViewItemViewModel(dir.Name, dir.Name, app, id));
                    foreach (var file in result.Response.Files)
                        base.Children.Add(new FileTreeViewItemViewModel(file.Name, file.Name, app, id));
                }));
            };
            worker.RunWorkerAsync();
        }
    }
}