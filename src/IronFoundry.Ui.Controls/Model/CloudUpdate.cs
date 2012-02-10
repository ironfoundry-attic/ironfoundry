namespace IronFoundry.Ui.Controls.Model
{

    using System;
    public class CloudUpdate
    {
        private readonly Guid id;
        private readonly string serverUrl;
        private readonly string serverName;
        private readonly string email;
        private readonly string password;

        public CloudUpdate(Guid id, string serverUrl, string serverName, string email, string password)
        {
            this.id         = id;
            this.serverUrl  = serverUrl;
            this.serverName = serverName;
            this.email      = email;
            this.password   = password;
        }

        public Guid ID { get { return id; } }
        public string ServerUrl { get { return serverUrl; } }
        public string ServerName { get { return serverName; } }
        public string Email { get { return email; } }
        public string Password { get { return password; } }
    }
}