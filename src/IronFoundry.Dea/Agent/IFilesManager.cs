namespace IronFoundry.Dea.Agent
{
    using IronFoundry.Dea.Types;

    public interface IFilesManager
    {
        string ApplicationPath { get; }

        void BindServices(Droplet droplet, string IIsName);

        void CleanupInstanceDirectory(Instance instance);

        string GetApplicationPathFor(Instance instance);

        Snapshot GetSnapshot();

        string SnapshotFile { get; }

        bool Stage(Droplet droplet, Instance instance);

        void TakeSnapshot(Snapshot snapshot);

        void RemoveStaged(Droplet droplet, Instance instance);
    }
}
