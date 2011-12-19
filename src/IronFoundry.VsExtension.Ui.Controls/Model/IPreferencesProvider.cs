namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    using CloudFoundry.Net.Types;

    public interface IPreferencesProvider
    {
        Preferences Load();
        void Save(Preferences preferences);
    }
}