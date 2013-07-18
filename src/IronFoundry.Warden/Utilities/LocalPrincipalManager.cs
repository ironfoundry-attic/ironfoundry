namespace IronFoundry.Warden.Utilities
{
    using System;
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using IronFoundry.Warden.PInvoke;

    public class LocalPrincipalManager
    {
        // IIS_IUSRS_SID = "S-1-5-32-568";
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

        // TODO:
        // Grant "Log on as a service"
        // http://weblogs.asp.net/avnerk/archive/2007/05/10/granting-user-rights-in-c.aspx
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
                var iisUsersGroup = searcher.FindOne() as GroupPrincipal;
                iisUsersGroup.Members.Add(user);
                iisUsersGroup.Save();
            }

            using (var lsaWrapper = new NativeMethods.LsaWrapper())
            {
                lsaWrapper.AddPrivileges(userName, "SeServiceLogonRight");
            }

            return rvUserName;
        }

        public void DeleteUser(string userName)
        {
            /*
             * TODO: errors with "the array bounds are invalid"
             *
            using (var lsaWrapper = new NativeMethods.LsaWrapper())
            {
                lsaWrapper.RemovePrivileges(userName, "SeServiceLogonRight");
            }
             */

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
