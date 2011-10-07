namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using Types;

    public class InstanceTreeViewItemViewModel : TreeViewItemViewModel
    {
        readonly StatInfo statInfo;

        public InstanceTreeViewItemViewModel(StatInfo statInfo, ApplicationTreeViewItemViewModel parentApplication) : base(parentApplication,false)
        {
            this.statInfo = statInfo;
        }

        public string Host
        {
            get
            {
                string host = null;
                if (null != statInfo.Stats) // TODO probably not a null check we want to do
                {
                    host = statInfo.Stats.Host;
                }
                return host;
            }
        }
    }
}