namespace IronFoundry.Warden.Utilities
{
    using System;
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using System.Security.Principal;

    public class LocalPrincipalManager
    {
        private const string IIS_IUSRS_SID = "S-1-5-32-568";
        private const string IIS_IUSRS_NAME = "IIS_IUSRS";
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

        public string CreateUser(string userName, string password)
        {
            string rvUserName;

            using (var context = new PrincipalContext(ContextType.Machine))
            {
                var user = new UserPrincipal(context, userName, password, true);
                user.DisplayName = "Warden User " + userName;
                user.Save();
                rvUserName = user.SamAccountName;

                var groupQuery = new GroupPrincipal(context, IIS_IUSRS_NAME);
                var searcher = new PrincipalSearcher(groupQuery);
                var group = searcher.FindOne() as GroupPrincipal;
                group.Members.Add(user);
                group.Save();
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
