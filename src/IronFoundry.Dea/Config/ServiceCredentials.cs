namespace IronFoundry.Dea.Config
{
    using System;

    public class ServiceCredentials
    {
        private readonly string username;
        private readonly string password;

        public ServiceCredentials()
        {
            this.username = getRandomCredential();
            this.password = getRandomCredential();
        }

        public string Username { get { return username; } }

        public string Password { get { return password; } }

        private string getRandomCredential()
        {
            return Guid.NewGuid().ToString().ToLowerInvariant().Replace("-", String.Empty);
        }

        public string[] ToArray()
        {
            return new[] { username, password };
        }
    }
}