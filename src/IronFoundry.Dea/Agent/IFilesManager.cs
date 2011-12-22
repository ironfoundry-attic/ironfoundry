namespace IronFoundry.Dea.Agent
{
    using IronFoundry.Dea.Types;

    public interface IFilesManager
    {
        void BindServices(Droplet droplet, string IIsName);

        void CleanupInstanceDirectory(Instance instance);
        void CleanupInstanceDirectory(Instance instance, bool force);

        string GetApplicationPathFor(Instance instance);

        Snapshot GetSnapshot();

        string SnapshotFile { get; }

        bool Stage(Droplet droplet, Instance instance);

        void TakeSnapshot(Snapshot snapshot);
    }
}