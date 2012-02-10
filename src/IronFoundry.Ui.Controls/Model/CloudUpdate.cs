namespace IronFoundry.Ui.Controls.Model
{
    using System;

    public class CloudUpdate
    {
        private readonly Uri apiUri;
        private readonly string serverName;
        private readonly string email;
        private readonly string password;

        public CloudUpdate(Uri apiUri, string serverName, string email, string password)
        {
            this.apiUri     = apiUri;
            this.serverName = serverName;
            this.email      = email;
            this.password   = password;
        }

        public Uri ApiUri { get { return apiUri; } }
        public string ServerName { get { return serverName; } }
        public string Email { get { return email; } }
        public string Password { get { return password; } }
    }
}