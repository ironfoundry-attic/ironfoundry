namespace IronFoundry.Dea.Services
{
    using System;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Properties;

    public class FilesServiceValidator : UserNamePasswordValidator
    {
        private readonly FilesServiceCredentials credentials;

        public FilesServiceValidator(IConfig config)
        {
            this.credentials = config.FilesCredentials;
        }

        public override void Validate(string userName, string password)
        {
            if (userName.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace())
            {
                throw new ArgumentException();
            }
            if (userName != credentials.Username)
            {
                throw new FaultException(String.Format(Resources.FilesServiceValidator_InvalidUser_Fmt, userName));
            }
            if (password != credentials.Password)
            {
                throw new FaultException(String.Format(Resources.FilesServiceValidator_InvalidPassword_Fmt, password));
            }
        }
    }
}