namespace IronFoundry.Ui.Controls.Model
{
    using IronFoundry.Types;

    public interface IPreferencesProvider
    {
        PreferencesV2 Load();
        void Save(PreferencesV2 preferences);
    }
}
