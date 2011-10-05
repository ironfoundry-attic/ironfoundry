namespace CloudFoundry.Net.Types
{
    public static class VcapStates
    {
        public const string STARTING      = "STARTING";
        public const string STOPPED       = "STOPPED";
        public const string RUNNING       = "RUNNING";
        public const string STARTED       = "STARTED";
        public const string SHUTTING_DOWN = "SHUTTING_DOWN";
        public const string CRASHED       = "CRASHED";
        public const string DELETED       = "DELETED";

        public static bool IsValid(string argState)
        {
            return STARTING == argState ||
                   STOPPED == argState ||
                   RUNNING == argState ||
                   SHUTTING_DOWN == argState ||
                   CRASHED == argState ||
                   DELETED == argState;
        }
    }
}