namespace IronFoundry.Warden.Containers
{
    using System;
    using System.Net;
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
                this.userName = CreateUserName(uniqueId);
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

            AddDesktopPermission(this.userName);
        }

        public static void CleanUp(string uniqueId)
        {
            try
            {
                string userName = CreateUserName(uniqueId);
                DeleteUser(userName);
                RemoveDesktopPermission(userName);
            }
            catch { }
        }

        public void Delete()
        {
            DeleteUser(userName);
        }

        public static implicit operator string(ContainerUser containerUser)
        {
            return containerUser.userName;
        }

        public static bool operator ==(ContainerUser x, ContainerUser y)
        {
            if (Object.ReferenceEquals(x, null))
            {
                return Object.ReferenceEquals(y, null);
            }
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

        private static void AddDesktopPermission(string userName)
        {
            if (Environment.UserInteractive == false)
            {
                var desktopPermissionManager = new DesktopPermissionManager(userName);
                desktopPermissionManager.AddDesktopPermission();
            }
        }

        private static void DeleteUser(string userName)
        {
            var principalManager = new LocalPrincipalManager();
            principalManager.DeleteUser(userName);
        }

        private static void RemoveDesktopPermission(string userName)
        {
            if (Environment.UserInteractive == false)
            {
                var desktopPermissionManager = new DesktopPermissionManager(userName);
                desktopPermissionManager.RemoveDesktopPermission();
            }
        }

        private static string CreateUserName(string uniqueId)
        {
            return String.Concat(userPrefix, uniqueId);
        }
    }
}
