using System.Windows;
using System.Windows.Controls;
using CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using System.Windows.Interop;
using System;
using System.Windows.Media.Imaging;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Explorer : Window
    {
        public Explorer()
        {
            InitializeComponent();
            this.DataContext = new ExplorerViewModel();            
        }
    }
}
