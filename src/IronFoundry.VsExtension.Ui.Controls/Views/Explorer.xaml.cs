using System.Windows;
using System.Windows.Controls;
using IronFoundry.VsExtension.Ui.Controls.ViewModel;
using IronFoundry.VsExtension.Ui.Controls.Utilities;
using System.Windows.Interop;
using System;
using System.Windows.Media.Imaging;

namespace IronFoundry.VsExtension.Ui.Controls.Views
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
