namespace IronFoundry.Ui.Controls.Views
{
    using System.Windows;
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