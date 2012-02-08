namespace IronFoundry.Ui.Controls.ViewModel.AddCloud
{
    using System.Text.RegularExpressions;
    using GalaSoft.MvvmLight.Messaging;
    using Mvvm;
    using Types;
    using Utilities;

    public class CreateMicrocloudTargetViewModel : DialogViewModel
    {
        private CloudUrl cloudUrl;
        private string name;
        private string replacementText;

        public CreateMicrocloudTargetViewModel() : base(Messages.CreateMicrocloudTargetDialogResult)
        {
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string ReplacementText
        {
            get { return replacementText; }
            set
            {
                replacementText = value;
                RaisePropertyChanged("ReplacementText");
                Name = string.Format("Microcloud ({0})", replacementText);
            }
        }

        public CloudUrl CloudUrl
        {
            get { return cloudUrl; }
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<CloudUrl>(Messages.SetAddCloudUrlData,
                                                                           (cloudUrl) =>
                                                                           {
                                                                               this.cloudUrl = cloudUrl;
                                                                               replacementText =
                                                                                   Regex.Match(this.cloudUrl.Url,
                                                                                               @"\{(\w+)\}").Groups[1].
                                                                                       Value;
                                                                               name = string.Format("Microcloud ({0})",
                                                                                                    replacementText);
                                                                           }));
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<CreateMicrocloudTargetViewModel>>(this,
                                                                                                   message =>
                                                                                                   {
                                                                                                       if (
                                                                                                           message.
                                                                                                               Notification
                                                                                                               .Equals(
                                                                                                                   Messages
                                                                                                                       .
                                                                                                                       GetMicrocloudTargetData))
                                                                                                       {
                                                                                                           cloudUrl.
                                                                                                               ServerName
                                                                                                               = name;
                                                                                                           string
                                                                                                               toReplace
                                                                                                                   =
                                                                                                                   "{" +
                                                                                                                   Regex
                                                                                                                       .
                                                                                                                       Match
                                                                                                                       (cloudUrl
                                                                                                                            .
                                                                                                                            Url,
                                                                                                                        @"\{(\w+)\}")
                                                                                                                       .
                                                                                                                       Groups
                                                                                                                       [
                                                                                                                           1
                                                                                                                       ]
                                                                                                                       .
                                                                                                                       Value +
                                                                                                                   "}";
                                                                                                           cloudUrl.Url
                                                                                                               =
                                                                                                               cloudUrl.
                                                                                                                   Url.
                                                                                                                   Replace
                                                                                                                   (toReplace,
                                                                                                                    replacementText);
                                                                                                           message.
                                                                                                               Execute(
                                                                                                                   this);
                                                                                                       }
                                                                                                       Cleanup();
                                                                                                   });
        }
    }
}