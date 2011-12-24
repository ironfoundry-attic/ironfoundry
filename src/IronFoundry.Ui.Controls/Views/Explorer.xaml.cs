using System.Windows;
using System.Windows.Controls;
using IronFoundry.Ui.Controls.ViewModel;
using IronFoundry.Ui.Controls.Utilities;
using System.Windows.Interop;
using System;
using System.Windows.Media.Imaging;

namespace IronFoundry.Ui.Controls.Views
{
    using ViewModel;

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
