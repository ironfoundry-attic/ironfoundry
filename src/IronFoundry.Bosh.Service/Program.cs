namespace IronFoundry.Bosh.Service
{
    using IronFoundry.Misc.WinService;

    static class Program
    {
        static void Main(string[] args)
        {
            ServiceMain.Start(args);
        }
    }
}