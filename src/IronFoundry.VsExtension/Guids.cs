namespace CloudFoundry.Net.VsExtension
{
    using System;

    public static class GuidList
    {
        public const string guidCloudFoundryPkgString = "D11F588D-A83B-40C1-9637-A5F44B65D110";
        public const string guidCloudFoundryCmdSetString = "05F6013A-8190-4680-9122-69C726FBA0D9";
        public static readonly Guid guidCloudFoundryCmdSet = new Guid(guidCloudFoundryCmdSetString);
    };
}