namespace IronFoundry.Ui.Controls.Model
{
    using IronFoundry.Types;

    public interface IPreferencesProvider
    {
        Preferences Load();
        void Save(Preferences preferences);
    }
}