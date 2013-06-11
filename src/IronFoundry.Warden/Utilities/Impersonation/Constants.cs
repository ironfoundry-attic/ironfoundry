using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronFoundry.Warden.Utilities.Impersonation
{
    public static class Constants
    {
        public const int SECURITY_IMPERSONATION = 2;

        /// <summary>
        /// Use the standard logon provider for the system. 
        /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name 
        /// is not in UPN format. In this case, the default provider is NTLM. 
        /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
        /// </summary>
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_WINNT50 = 3; // T3N impersonation uses this one
        public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
    }
}
