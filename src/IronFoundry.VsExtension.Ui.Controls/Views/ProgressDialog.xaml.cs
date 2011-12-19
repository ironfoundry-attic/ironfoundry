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
using IronFoundry.VsExtension.Ui.Controls.ViewModel;
using IronFoundry.VsExtension.Ui.Controls.Utilities;

namespace IronFoundry.VsExtension.Ui.Controls.Views
{
    public partial class ProgressDialog : Window
    {
        public ProgressDialog()
        {            
            InitializeComponent();
            this.DataContext = new ProgressViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ProgressDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });
        }
    }
}
