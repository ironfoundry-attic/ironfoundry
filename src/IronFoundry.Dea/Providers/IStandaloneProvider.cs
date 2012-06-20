namespace IronFoundry.Dea.Providers
{
    using IronFoundry.Dea.Types;

    public interface IStandaloneProvider
    {
        void InstallApp(string localDirectory, string applicationInstanceName);
        ApplicationInstanceStatus GetApplicationStatus(Instance applicationInstance);
    }
}