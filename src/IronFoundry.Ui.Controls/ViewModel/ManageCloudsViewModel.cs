namespace IronFoundry.Ui.Controls.ViewModel
{
    using System.Collections.ObjectModel;
    using IronFoundry.Types;

    public class ManageCloudsViewModel
    {
        public SafeObservableCollection<CloudUrl> Clouds
        {
            get
            {
                return CloudUrl.DefaultCloudUrls;
            }
        }
    }
}