namespace IronFoundry.Dea.Providers
{
    using System;
    using IronFoundry.Dea.Types;

    public class StandaloneProvider : IStandaloneProvider
    {
        public void InstallApp(string localDirectory, string applicationInstanceName)
        {
            throw new NotImplementedException();
        }

        public ApplicationInstanceStatus GetApplicationStatus(Instance applicationInstance)
        {
            throw new NotImplementedException();
        }
    }
}