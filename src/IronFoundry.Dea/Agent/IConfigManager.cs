namespace IronFoundry.Dea.Agent
{
    using IronFoundry.Dea.Types;

    public interface IConfigManager
    {
        void BindServices(Droplet droplet, Instance instance);
        void SetupEnvironment(Droplet droplet, Instance instance);
    }
}