namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System.Windows.Input;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.Vmc;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using System;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Model;

    public class ApplicationTreeViewItemViewModel : TreeViewItemViewModel
    {
        private CloudFoundryProvider provider;
        private Application application;
        public RelayCommand<MouseButtonEventArgs> OpenApplicationCommand { get; private set; }
        public RelayCommand StartApplicationCommand { get; private set; }
        public RelayCommand StopApplicationCommand { get; private set; }
        public RelayCommand RestartApplicationCommand { get; private set; }
        
        public ApplicationTreeViewItemViewModel(Application application, CloudTreeViewItemViewModel parentCloud) : base(parentCloud, true)
        {
            Messenger.Default.Send<NotificationMessageAction<CloudFoundryProvider>>(new NotificationMessageAction<CloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            OpenApplicationCommand = new RelayCommand<MouseButtonEventArgs>(OpenApplication);
            StartApplicationCommand = new RelayCommand(StartApplication, CanStart);
            StopApplicationCommand = new RelayCommand(StopApplication, CanStop);
            RestartApplicationCommand = new RelayCommand(RestartApplication, CanStop);

            this.application = application;
        }
        
        public Application Application
        {
            get { return this.application; }
            set { this.application = value; RaisePropertyChanged("Application"); }
        }

        private void OpenApplication(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
                Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.OpenApplication));
        }

        private bool CanStart()
        {
            return Application.CanStart;
        }

        private bool CanStop()
        {
            return Application.CanStop;
        }

        private void StartApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.StartApplication));
        }

        private void StopApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.StopApplication));
        }
        private void RestartApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.RestartApplication));
        }

        public override void LoadChildren()
        {
            Children.Clear();
            var statsResponse = provider.GetStats(this.application.Parent, this.application);        
            foreach (StatInfo statInfo in statsResponse.Response)
                base.Children.Add(new InstanceTreeViewItemViewModel(statInfo, this));
        }
    }
}