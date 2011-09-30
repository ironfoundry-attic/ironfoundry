using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;

namespace CloudFoundry.Net.VsExtension.Ui.Controls
{
    /// <summary>
    /// Interaction logic for FoundryProperties.xaml
    /// </summary>
    public partial class CloudView : UserControl
    {
        public CloudView()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessageAction<bool>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ChangePassword))
                    {
                        var view = new Views.ChangePassword();
                        Window parentWindow = Window.GetWindow(this);
                        view.Owner = parentWindow;
                        var result = view.ShowDialog();
                        message.Execute(result.GetValueOrDefault());
                    }
                });

            Messenger.Default.Register<NotificationMessageAction<bool>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ManageApplicationUrls))
                    {
                        var view = new Views.ManageApplicationUrls();
                        Window parentWindow = Window.GetWindow(this);
                        view.Owner = parentWindow;
                        var result = view.ShowDialog();
                        message.Execute(result.GetValueOrDefault());
                    }
                });
        }
    }
}
