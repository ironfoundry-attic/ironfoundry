namespace IronFoundry.Ui.Controls.Model
{
    using Models;

    public interface IPreferencesProvider
    {
        PreferencesV2 Load();
        void Save(PreferencesV2 preferences);
    }
}
