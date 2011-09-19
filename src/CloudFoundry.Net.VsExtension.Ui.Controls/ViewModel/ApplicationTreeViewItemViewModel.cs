using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class ApplicationTreeViewItemViewModel : TreeViewItemViewModel
    {
        private Application application;
        public RelayCommand<MouseButtonEventArgs> OpenApplicationCommand { get; private set; }
        
        public ApplicationTreeViewItemViewModel(Application application, CloudTreeViewItemViewModel parentCloud) : base(parentCloud, false)
        {
            OpenApplicationCommand = new RelayCommand<MouseButtonEventArgs>(OpenApplication);

            this.application = application;
            foreach (Instance instance in application.Instances)
                base.Children.Add(new InstanceTreeViewItemViewModel(instance, this));
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
    }
}
