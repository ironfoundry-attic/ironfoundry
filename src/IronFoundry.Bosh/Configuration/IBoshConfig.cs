namespace IronFoundry.Bosh.Configuration
{
    using System;

    public interface IBoshConfig
    {
        string BaseDir { get; }
        string BoshBaseDir { get; }
        string SettingsFilePath { get; }
        string StateFilePath { get; }

        Uri Mbus { get; set; }
        string AgentID { get; set; }
    }
}