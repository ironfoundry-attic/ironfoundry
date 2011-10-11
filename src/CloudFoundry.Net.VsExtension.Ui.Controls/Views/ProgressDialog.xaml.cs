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

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Views
{
    public partial class ProgressDialog : Window
    {
        public ProgressDialog(string title, string initialActivity)
        {
            InitializeComponent();
            this.Title = title;
            this.TitleLabel.Content = title;
            LogInfoTextBlock.Content = initialActivity;
        }

        public string LogInfo
        {
            set
            {
                LogInfoTextBlock.Content = value;
            }
        }

        public string Response
        {
            set
            {
                ResponseTextBox.Text = value;
            }
        }
        
        public int ProgressValue
        {
            set
            {
                this.progressBar1.Value = value;
            }
        }

        public bool OkButtonEnabled
        {
            set
            {
                this.OkButton.IsEnabled = value;
            }
        }

        public bool CancelButtonEnabled
        {
            set
            {
                this.CancelButton.IsEnabled = value;
            }
        }

        public event EventHandler Cancel = delegate { };

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel(sender, e);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
