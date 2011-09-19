using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using System.Collections.ObjectModel;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudViewModel : ViewModelBase
    {       
        public RelayCommand ChangePasswordCommand { get; private set; }
        public RelayCommand ValidateAccountCommand { get; private set; }
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }

        public RelayCommand StartCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }
        public RelayCommand RestartCommand { get; private set; }
        public RelayCommand UpdateAndRestartCommand { get; private set; }
        public Cloud Cloud { get; private set; }

        public CloudViewModel(Cloud cloud)
        {
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            ValidateAccountCommand = new RelayCommand(ValidateAccount, CanExecuteValidateAccount);
            ConnectCommand = new RelayCommand(Connect, () => !this.Connected);
            DisconnectCommand = new RelayCommand(Disconnect, () => this.Connected);
            StartCommand = new RelayCommand(Start, CanExecuteStart);
            StopCommand = new RelayCommand(Stop, CanExecuteStopActions);
            RestartCommand = new RelayCommand(Restart, CanExecuteStopActions);
            UpdateAndRestartCommand = new RelayCommand(UpdateAndRestart, CanExecuteStopActions);

            this.Cloud = cloud;
        }

        #region Overview

        private void ChangePassword()
        {
            // Register to initialize data in dialog
            Messenger.Default.Register<NotificationMessageAction<string>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ChangePasswordEmailAddress))
                        message.Execute(this.EMail);
                });

            // Fire message to open dialog
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ChangePassword,
                (confirmed) =>
                {
                    if (confirmed)
                    {

                    }
                }));
        }

        private void ValidateAccount()
        {

        }

        private bool CanExecuteValidateAccount()
        {
            return !String.IsNullOrEmpty(EMail) &&
                   !String.IsNullOrEmpty(Password) &&
                   !String.IsNullOrEmpty(Url);
        }

        private void Connect()
        {
            this.Connected = true;
        }

        private void Disconnect()
        {
            this.Connected = false;
        }

        public bool Connected
        {
            get { return this.Cloud.Connected; }
            set
            {
                this.Cloud.Connected = value;
                RaisePropertyChanged("Connected");
            }
        }
        
        public string ServerName
        {
            get { return this.Cloud.ServerName; }
            set
            {
                this.Cloud.ServerName = value;
                RaisePropertyChanged("ServerName");
            }
        }

        public string HostName
        {
            get { return this.Cloud.HostName; }
            set
            {
                this.Cloud.HostName = value;
                RaisePropertyChanged("HostName");
            }
        }

        public string EMail
        {
            get { return this.Cloud.Email; }
            set
            {
                this.Cloud.Email = value;
                RaisePropertyChanged("EMail");
            }
        }

        public string Password
        {
            get { return this.Cloud.Password; }
            set
            {
                this.Cloud.Password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string Url
        {
            get { return this.Cloud.Url; }
            set
            {
                this.Cloud.Url = value;
                RaisePropertyChanged("Url");
            }
        }

        public int TimeoutStart
        {
            get { return this.Cloud.TimeoutStart; }
            set
            {
                this.Cloud.TimeoutStart = value;
                RaisePropertyChanged("TimeoutStart");
            }
        }

        public int TimeoutStop
        {
            get { return this.Cloud.TimeoutStop; }
            set
            {
                this.Cloud.TimeoutStop = value;
                RaisePropertyChanged("TimeoutEnd");
            }
        }
        #endregion        

        #region Application
        
        public bool IsApplicationSelected
        {
            get { return this.SelectedApplication != null; }
        }

        private bool isApplicationViewSelected;
        public bool IsApplicationViewSelected
        {
            get { return this.isApplicationViewSelected; }
            set
            {
                this.isApplicationViewSelected = value;
                RaisePropertyChanged("IsApplicationViewSelected");
            }
        }

        public void Start()
        {
            this.State = CloudFoundry.Net.Types.Instance.InstanceState.RUNNING;
        }

        public bool CanExecuteStart()
        {
            return !this.State.Equals(CloudFoundry.Net.Types.Instance.InstanceState.RUNNING);
        }

        public bool CanExecuteStopActions()
        {
            return this.State.Equals(CloudFoundry.Net.Types.Instance.InstanceState.RUNNING);
        }

        public void Stop()
        {
            this.State = CloudFoundry.Net.Types.Instance.InstanceState.STOPPED;
        }

        public void Restart()
        {
            this.State = CloudFoundry.Net.Types.Instance.InstanceState.RUNNING;
        }

        public void UpdateAndRestart()
        {
            this.State = CloudFoundry.Net.Types.Instance.InstanceState.RUNNING;
        }

        public int[] MemoryLimits { get { return CloudFoundry.Net.VsExtension.Ui.Controls.Model.Constants.MemoryLimits; } }

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string state = CloudFoundry.Net.Types.Instance.InstanceState.STOPPED;
        public string State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
                RaisePropertyChanged("State");
            }
        }

        private int cpus;
        public int Cpus
        {
            get
            {
                return this.cpus;
            }
            set
            {
                this.cpus = value;
                RaisePropertyChanged("Cpus");
            }
        }

        private int diskLimit;
        public int DiskLimit
        {
            get
            {
                return this.diskLimit;
            }
            set
            {
                this.diskLimit = value;
                RaisePropertyChanged("DiskLimit");
            }
        }

        private ObservableCollection<string> mappedUrls;
        public ObservableCollection<string> MappedUrls
        {
            get
            {
                return this.mappedUrls;
            }
            set
            {
                this.mappedUrls = value;
                RaisePropertyChanged("MappedUrls");
            }
        }

        private int instanceCount;
        public int InstanceCount
        {
            get
            {
                return this.instanceCount;
            }
            set
            {
                this.instanceCount = value;
                RaisePropertyChanged("InstanceCount");
            }
        }

        private int memoryLimit;
        public int MemoryLimit
        {
            get
            {
                return this.memoryLimit;
            }
            set
            {
                this.memoryLimit = value;
                RaisePropertyChanged("MemoryLimit");
            }
        }

        public ObservableCollection<CloudFoundry.Net.VsExtension.Ui.Controls.Model.Application> Applications
        {
            get { return this.Cloud.Applications; }
        }

        private CloudFoundry.Net.VsExtension.Ui.Controls.Model.Application selectedApplication;
        public CloudFoundry.Net.VsExtension.Ui.Controls.Model.Application SelectedApplication
        {
            get { return this.selectedApplication; }
            set
            {
                
                this.selectedApplication = value;
                this.Instances = this.selectedApplication.Instances;
                this.ApplicationServices = this.selectedApplication.Services;
                this.Name = selectedApplication.Name;
                this.Cpus = selectedApplication.Cpus;
                this.DiskLimit = selectedApplication.DiskLimit;
                this.InstanceCount = selectedApplication.InstanceCount;
                this.MappedUrls = selectedApplication.MappedUrls;
                this.MemoryLimit = selectedApplication.MemoryLimit;
                this.State = selectedApplication.State;                
                RaisePropertyChanged("SelectedApplication");
                RaisePropertyChanged("IsApplicationSelected");
            }
        }

        public ObservableCollection<CloudFoundry.Net.VsExtension.Ui.Controls.Model.Service> CloudServices
        {
            get { return this.Cloud.Services; }
            set
            {
                this.Cloud.Services = value;
                RaisePropertyChanged("CloudServices");
            }
        }

        private ObservableCollection<Instance> instances;
        public ObservableCollection<Instance> Instances
        {
            get { return this.instances; }
            set
            {
                this.instances = value;
                RaisePropertyChanged("Instances");
            }
        }

        private ObservableCollection<Service> services;
        public ObservableCollection<Service> ApplicationServices
        {
            get { return this.services; }
            set
            {
                this.services = value;
                RaisePropertyChanged("ApplicationServices");
            }
        }

        #endregion

        
    }
}
