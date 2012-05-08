namespace IronFoundry.Test
{
    using IronFoundry.Vcap;
    using Xunit;

    public class UserTests
    {
        [Fact(Skip="MANUAL")]
        public void Get_All_Users()
        {
            var client = new VcapClient("http://foo.com");
            VcapClientResult rslt = client.Login("email", "password");
            Assert.True(rslt.Success);
            var users = client.GetUsers();
            Assert.NotEmpty(users);
            foreach (var user in users)
            {
                var apps = client.GetApplications(user);
            }
        }
    }
}