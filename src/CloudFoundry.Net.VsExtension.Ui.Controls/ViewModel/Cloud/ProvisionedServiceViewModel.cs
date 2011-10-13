using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using CloudFoundry.Net.Types;
using System.Collections.ObjectModel;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CreateServiceViewModel : DialogViewModel
    {
        private string name;
        private ObservableCollection<SystemService> systemServices = new ObservableCollection<SystemService>();
        private SystemService selectedSystemService;

        public CreateServiceViewModel() : base(Messages.CreateServiceDialogResult)
        {
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<CreateServiceViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetCreateServiceData))
                        message.Execute(this);
                    Cleanup();
                });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetCreateServiceData,
                (cloud) =>
                {
                    this.SystemServices.Synchronize(cloud.AvailableServices, new SystemServiceEqualityComparer());
                }));
        }

        public ObservableCollection<SystemService> SystemServices
        {
            get { return this.systemServices; }
        }

        public SystemService SelectedSystemService
        {
            get { return this.selectedSystemService; }
            set { this.selectedSystemService = value; RaisePropertyChanged("SelectedSystemService"); }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; RaisePropertyChanged("Name"); }
        }
    }
}
