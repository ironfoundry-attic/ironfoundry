namespace IronFoundry.VsExtension.Ui.Controls.ViewModel
{
    using System.Text.RegularExpressions;
    using IronFoundry.Types;
    using IronFoundry.VsExtension.Ui.Controls.Mvvm;
    using IronFoundry.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Messaging;

    public class CreateMicrocloudTargetViewModel : DialogViewModel
    {
        private string replacementText;
        private string name;
        private CloudUrl cloudUrl;

        public CreateMicrocloudTargetViewModel() : base(Messages.CreateMicrocloudTargetDialogResult)
        {
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<CloudUrl>(Messages.SetAddCloudUrlData,
                (cloudUrl) =>
                {
                    this.cloudUrl = cloudUrl;
                    this.replacementText = Regex.Match(this.cloudUrl.Url, @"\{(\w+)\}").Groups[1].Value;
                    this.name = string.Format("Microcloud ({0})", this.replacementText);
                }));
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<CreateMicrocloudTargetViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetMicrocloudTargetData))
                    {
                        this.cloudUrl.ServerType = this.name;
                        var toReplace = "{" + Regex.Match(this.cloudUrl.Url, @"\{(\w+)\}").Groups[1].Value + "}";
                        this.cloudUrl.Url = this.cloudUrl.Url.Replace(toReplace,this.replacementText);
                        message.Execute(this);
                    }
                    Cleanup();
                });
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; RaisePropertyChanged("Name"); }
        }

        public string ReplacementText
        {
            get { return this.replacementText; }
            set { this.replacementText = value; RaisePropertyChanged("ReplacementText"); this.Name = string.Format("Microcloud ({0})", this.replacementText); }
        }

        public CloudUrl CloudUrl
        {
            get { return this.cloudUrl; }
        }
    }
}