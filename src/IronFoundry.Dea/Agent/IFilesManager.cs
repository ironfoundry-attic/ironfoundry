namespace IronFoundry.Dea
{
    using IronFoundry.Dea.Types;

    public interface IFilesManager
    {
        string ApplicationPath { get; }

        void BindServices(Droplet argDroplet, string argIIsName);

        void CleanupInstanceDirectory(Instance argInstance);

        string GetApplicationPathFor(Instance argInstance);

        Snapshot GetSnapshot();

        string SnapshotFile { get; }

        bool Stage(Droplet argDroplet, Instance argInstance);

        void TakeSnapshot(Snapshot argSnapshot);
    }
}