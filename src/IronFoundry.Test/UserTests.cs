namespace IronFoundry.Test
{
    using System;
    using IronFoundry.Types;
    using IronFoundry.Vcap;
    using Xunit;

    public class UserTests
    {
        [Fact(Skip="MANUAL")]
        public void Get_All_Users_And_Apps()
        {
            var client = new VcapClient("http://api.ironfoundry.me");
            client.Login("adminuser@email.com", "password");
            var users = client.GetUsers();
            Assert.NotEmpty(users);
            foreach (var user in users)
            {
                Console.WriteLine("User: {0}", user.Email);
                client.ProxyAs(user);
                var apps = client.GetApplications();
                foreach (var app in apps)
                {
                    Console.WriteLine("\t\tApp: {0}", app.Name);
                }
            }
        }

        [Fact(Skip="MANUAL")]
        public void Stop_App_As_User()
        {
            IVcapClient client = new VcapClient("http://api.ironfoundry.me");
            client.Login("adminuser@email.com", "password");
            VcapUser user = client.GetUser("otheruser");
            client.ProxyAs(user);
            client.Stop("appname");
        }
    }
}