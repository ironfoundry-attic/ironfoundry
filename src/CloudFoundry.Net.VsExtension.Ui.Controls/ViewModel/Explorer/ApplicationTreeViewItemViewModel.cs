using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.Types;
using CloudFoundry.Net.Vmc;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class ApplicationTreeViewItemViewModel : TreeViewItemViewModel
    {
        private Application application;
        public RelayCommand<MouseButtonEventArgs> OpenApplicationCommand { get; private set; }
        public RelayCommand StartApplicationCommand { get; private set; }
        public RelayCommand StopApplicationCommand { get; private set; }
        public RelayCommand RestartApplicationCommand { get; private set; }
        
        public ApplicationTreeViewItemViewModel(Application application, CloudTreeViewItemViewModel parentCloud) : base(parentCloud, false)
        {
            OpenApplicationCommand = new RelayCommand<MouseButtonEventArgs>(OpenApplication);
            StartApplicationCommand = new RelayCommand(StartApplication, CanStart);
            StopApplicationCommand = new RelayCommand(StopApplication, CanStop);
            RestartApplicationCommand = new RelayCommand(RestartApplication, CanStop);

            this.application = application;
            var manager = new VcapClient();
            var stats = manager.GetStats(this.application, this.application.Parent);            
            foreach (StatInfo statInfo in stats.Values)
                base.Children.Add(new InstanceTreeViewItemViewModel(statInfo, this));
        }

        public string Name
        {
            get { return this.application.Name; }
        }

        private void OpenApplication(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
                Messenger.Default.Send(new NotificationMessage<Application>(this, this.application, Messages.OpenApplication));
        }

        private bool CanStart()
        {
            return !(application.State.Equals(Types.Instance.InstanceState.RUNNING) ||
                     application.State.Equals(Types.Instance.InstanceState.STARTED) ||
                     application.State.Equals(Types.Instance.InstanceState.STARTING));
        }

        private bool CanStop()
        {
            return  (application.State.Equals(Types.Instance.InstanceState.RUNNING) ||
                     application.State.Equals(Types.Instance.InstanceState.STARTED) ||
                     application.State.Equals(Types.Instance.InstanceState.STARTING));

        }

        private void StartApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.application, Messages.StartApplication));
        }

        private void StopApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.application, Messages.StopApplication));
        }
        private void RestartApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.application, Messages.RestartApplication));
        }
    }
}
