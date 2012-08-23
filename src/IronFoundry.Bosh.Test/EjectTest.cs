namespace IronFoundry.Bosh.Test
{
    using IronFoundry.Bosh.Agent;
    using Xunit;

    public class EjectTest
    {
        [Fact(Skip="MANUAL")]
        public void Can_Eject_CD()
        {
            EjectMedia.Eject("G:");
        }
    }
}