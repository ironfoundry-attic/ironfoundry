namespace IronFoundry.Warden.Containers
{
    using System;
    using System.Net;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using IronFoundry.Warden.Utilities;

    public class ContainerUser : IEquatable<ContainerUser>
    {
        private const string userPrefix = "warden_";
        private static readonly Regex uniqueIdValidator = new Regex(@"^\w{8,}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private readonly string uniqueId;
        private readonly string userName;

        public NetworkCredential GetCredential()
        {
            return new NetworkCredential(userName, uniqueId);
        }

        public ContainerUser(string uniqueId, bool shouldCreate = false)
        {
            if (uniqueId.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("uniqueId");
            }
            this.uniqueId = uniqueId;

            if (uniqueIdValidator.IsMatch(uniqueId))
            {
                this.userName = userPrefix + uniqueId;
            }
            else
            {
                throw new ArgumentException("uniqueId must be 8 or more word characters.");
            }

            var principalManager = new LocalPrincipalManager();
            if (shouldCreate)
            {
                principalManager.CreateUser(this.userName, this.uniqueId);
            }
            else
            {
                string foundUser = principalManager.FindUser(this.userName);
                if (foundUser == null)
                {
                    throw new ArgumentException(String.Format("Could not find user '{0}'", this.userName));
                }
            }
        }

        public void Delete()
        {
            var principalManager = new LocalPrincipalManager();
            principalManager.DeleteUser(this.userName);
        }

        public IdentityReference Identity
        {
            get { return new NTAccount(userName); }
        }

        public static bool operator ==(ContainerUser x, ContainerUser y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ContainerUser x, ContainerUser y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return userName;
        }

        public override int GetHashCode()
        {
            return userName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContainerUser);
        }

        public bool Equals(ContainerUser other)
        {
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.GetHashCode() == other.GetHashCode();
        }
    }
}
