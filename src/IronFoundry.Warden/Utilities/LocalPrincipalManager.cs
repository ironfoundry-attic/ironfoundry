namespace IronFoundry.Warden.Utilities
{
    using System;
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;

    public class LocalPrincipalManager
    {
        private readonly string directoryPath = String.Format("WinNT://{0}", Environment.MachineName);

        public string FindUser(string userName)
        {
            string rvUserName = null;

            using (var localDirectory = new DirectoryEntry(directoryPath))
            {
                DirectoryEntries users = localDirectory.Children;
                using (DirectoryEntry user = users.Find(userName))
                {
                    if (user != null)
                    {
                        rvUserName = user.Name;
                    }
                }
            }

            return rvUserName;
        }

        public string CreateUser(string userName)
        {
            string rvUserName;
            string password = Guid.NewGuid().ToString("N").Substring(0, 16);

            using (var context = new PrincipalContext(ContextType.Machine))
            {
                var user = new UserPrincipal(context, userName, password, true);
                user.DisplayName = "Warden User " + userName;
                user.Save();
                rvUserName = user.SamAccountName;
            }

            return rvUserName;
        }

        public void DeleteUser(string userName)
        {
            using (var localDirectory = new DirectoryEntry(directoryPath))
            {
                DirectoryEntries users = localDirectory.Children;
                using (DirectoryEntry user = users.Find(userName))
                {
                    users.Remove(user);
                }
            }
        }
    }
}
