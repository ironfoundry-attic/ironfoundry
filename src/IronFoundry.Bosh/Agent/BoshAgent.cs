namespace IronFoundry.Bosh.Agent
{
    using IronFoundry.Misc.Agent;

    public sealed class BoshAgent : IAgent
    {
        public string Name
        {
            get { return "BOSH"; }
        }

        public bool Error
        {
            get { throw new System.NotImplementedException(); }
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}