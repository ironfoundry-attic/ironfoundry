namespace IronFoundry.Ui.Controls.ViewModel.Explorer
{
    using System;
    using System.ComponentModel;
    using System.Windows.Threading;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Models;
    using Mvvm;
    using Utilities;
    using Vcap;

    public class InstanceTreeViewItemViewModel : TreeViewItemViewModel
    {
        private readonly Application app;
        public Dispatcher dispatcher;
        private Instance instance;
        private ICloudFoundryProvider provider;

        public InstanceTreeViewItemViewModel(Instance instance, ApplicationTreeViewItemViewModel parentApplication)
            : base(parentApplication, true)
        {
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(
                                       Messages.GetCloudFoundryProvider, p => provider = p));
            app = parentApplication.Application;
            this.instance = instance;
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public Instance Instance
        {
            get { return instance; }
            set
            {
                instance = value;
                RaisePropertyChanged("Instance");
            }
        }

        public override void LoadChildren()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                dispatcher.BeginInvoke((Action) (() => Children.Clear()));
                var id = (ushort) instance.Id;
                ProviderResponse<VcapFilesResult> result = provider.GetFiles(app.Parent, app, "/", id);
                if (result.Response == null)
                {
                    Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
                    return;
                }

                dispatcher.BeginInvoke((Action) (() =>
                {
                    foreach (VcapFilesResult.FilesResultData dir in result.Response.Directories)
                        base.Children.Add(new FolderTreeViewItemViewModel(dir.Name, dir.Name, app, id));
                    foreach (VcapFilesResult.FilesResultData file in result.Response.Files)
                        base.Children.Add(new FileTreeViewItemViewModel(file.Name, file.Name, app, id));
                }));
            };
            worker.RunWorkerAsync();
        }
    }
}