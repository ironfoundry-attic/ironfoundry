using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public interface IPreferencesProvider
    {
        Preferences Load();
        void Save(Preferences preferences);
    }
}