using System;
using System.Windows;

namespace CloudFoundry.Net.VsExtension
{
    public partial class ProgressDialog : Window
    {
        public ProgressDialog(string title, string initialActivity)
        {
            InitializeComponent();
            this.Title = title;
            LogInfoTextBlock.Text = initialActivity;
        }

        public string LogInfo
        {
            set
            {
                LogInfoTextBlock.Text = value;
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
