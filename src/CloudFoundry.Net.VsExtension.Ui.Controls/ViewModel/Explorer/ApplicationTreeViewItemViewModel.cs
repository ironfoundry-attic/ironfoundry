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
    using System.ComponentModel;

    public class ApplicationTreeViewItemViewModel : TreeViewItemViewModel
    {
        private ICloudFoundryProvider provider;
        private Application application;
        public RelayCommand<MouseButtonEventArgs> OpenApplicationCommand { get; private set; }
        public RelayCommand StartApplicationCommand { get; private set; }
        public RelayCommand StopApplicationCommand { get; private set; }
        public RelayCommand RestartApplicationCommand { get; private set; }
        public RelayCommand DeleteApplicationCommand { get; private set; }
        
        public ApplicationTreeViewItemViewModel(Application application, CloudTreeViewItemViewModel parentCloud) : base(parentCloud, true)
        {
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            OpenApplicationCommand = new RelayCommand<MouseButtonEventArgs>(OpenApplication);
            StartApplicationCommand = new RelayCommand(StartApplication, CanStart);
            StopApplicationCommand = new RelayCommand(StopApplication, CanStop);
            RestartApplicationCommand = new RelayCommand(RestartApplication, CanStop);
            DeleteApplicationCommand = new RelayCommand(DeleteApplication);

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
        
        private void DeleteApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.DeleteApplication));
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
            if (statsResponse.Response == null)
            {
                Messenger.Default.Send(new NotificationMessage<string>(statsResponse.Message, Messages.ErrorMessage));
                return;
            }
            foreach (StatInfo statInfo in statsResponse.Response)
                base.Children.Add(new InstanceTreeViewItemViewModel(statInfo, this));
        }
    }
}