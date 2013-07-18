namespace IronFoundry.Warden.PInvoke
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal partial class NativeMethods
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint LsaOpenPolicy(
            LsaUnicodeString[] SystemName,
            LsaObjectAttributes ObjectAttributes,
            int AccessMask,
            out IntPtr PolicyHandle
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint LsaAddAccountRights(
            IntPtr PolicyHandle,
            IntPtr pSID,
            LsaUnicodeString[] UserRights,
            int CountOfRights
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint LsaRemoveAccountRights(
            IntPtr PolicyHandle,
            IntPtr pSID,
            LsaUnicodeString[] UserRights,
            int CountOfRights
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int LsaLookupNames2(
            IntPtr PolicyHandle,
            uint Flags,
            uint Count,
            LsaUnicodeString[] Names,
            ref IntPtr ReferencedDomains,
            ref IntPtr Sids
        );

        [DllImport("advapi32.dll")]
        public static extern int LsaNtStatusToWinError(int NTStatus);

        [DllImport("advapi32.dll")]
        public static extern int LsaClose(IntPtr PolicyHandle);

        [DllImport("advapi32.dll")]
        public static extern int LsaFreeMemory(IntPtr Buffer);

        [StructLayout(LayoutKind.Sequential)]
        public class LsaObjectAttributes
        {
            public int Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public int Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;

            public LsaObjectAttributes()
            {
                this.RootDirectory            = IntPtr.Zero;
                this.ObjectName               = IntPtr.Zero;
                this.Attributes               = 0;
                this.SecurityDescriptor       = IntPtr.Zero;
                this.SecurityQualityOfService = IntPtr.Zero;
                this.Length                   = Marshal.SizeOf(typeof(LsaObjectAttributes));
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LsaUnicodeString
        {
            public ushort Length;
            public ushort MaximumLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Buffer;
        }

        /// <summary>
        /// This class is used to grant "Log on as a service", "Log on as a batchjob", "Log on localy" etc.
        /// to a user.
        /// </summary>
        public sealed class LsaWrapper : IDisposable
        {
            [StructLayout(LayoutKind.Sequential)]
            struct LsaTrustInformation
            {
                public LsaUnicodeString Name;
                public IntPtr Sid;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct LsaTranslatedSid2
            {
                public SidNameUse Use;
                public IntPtr Sid;
                public int DomainIndex;
                public uint Flags;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct LsaReferencedDomainList
            {
                public uint Entries;
                public LsaTrustInformation Domains;
            }

            enum SidNameUse : int
            {
                User = 1,
                Group = 2,
                Domain = 3,
                Alias = 4,
                KnownGroup = 5,
                DeletedAccount = 6,
                Invalid = 7,
                Unknown = 8,
                Computer = 9
            }

            enum Access : int
            {
                POLICY_READ = 0x20006,
                POLICY_ALL_ACCESS = 0x00F0FFF,
                POLICY_EXECUTE = 0X20801,
                POLICY_WRITE = 0X207F8
            }

            const uint STATUS_ACCESS_DENIED = 0xc0000022;
            const uint STATUS_INSUFFICIENT_RESOURCES = 0xc000009a;
            const uint STATUS_NO_MEMORY = 0xc0000017;

            private IntPtr lsaHandle;

            private IntPtr tsids = IntPtr.Zero;
            private IntPtr tdom = IntPtr.Zero;

            public LsaWrapper() : this(null)
            {
            }

            // local system if systemName is null
            public LsaWrapper(string systemName)
            {
                var lsaAttr = new LsaObjectAttributes();

                lsaHandle = IntPtr.Zero;
                LsaUnicodeString[] system = null;
                if (systemName != null)
                {
                    system = new LsaUnicodeString[1];
                    system[0] = InitLsaString(systemName);
                }

                uint ret = LsaOpenPolicy(system, lsaAttr, (int)Access.POLICY_ALL_ACCESS, out lsaHandle);
                if (ret == 0)
                {
                    return;
                }

                if (ret == STATUS_ACCESS_DENIED)
                {
                    throw new UnauthorizedAccessException();
                }

                if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY))
                {
                    throw new OutOfMemoryException();
                }

                throw new Win32Exception(LsaNtStatusToWinError((int)ret));
            }

            public void AddPrivileges(string account, string privilege)
            {
                try
                {
                    IntPtr pSid = GetSIDInformation(account);

                    var privileges = new LsaUnicodeString[] { InitLsaString(privilege) };

                    uint ret = LsaAddAccountRights(lsaHandle, pSid, privileges, 1);
                    if (ret == 0)
                    {
                        return;
                    }

                    if (ret == STATUS_ACCESS_DENIED)
                    {
                        throw new UnauthorizedAccessException();
                    }

                    if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY))
                    {
                        throw new OutOfMemoryException();
                    }

                    throw new Win32Exception(LsaNtStatusToWinError((int)ret));
                }
                finally
                {
                    if (tsids != IntPtr.Zero)
                    {
                        LsaFreeMemory(tsids);
                        tsids = IntPtr.Zero;
                    }
                    if (tdom != IntPtr.Zero)
                    {
                        LsaFreeMemory(tdom);
                        tdom = IntPtr.Zero;
                    }
                }
            }

            public void RemovePrivileges(string account, string privilege)
            {
                try
                {
                    IntPtr pSid = GetSIDInformation(account);

                    var privileges = new LsaUnicodeString[] { InitLsaString(privilege) };

                    uint ret = LsaRemoveAccountRights(lsaHandle, pSid, privileges, 1);
                    if (ret == 0)
                    {
                        return;
                    }

                    if (ret == STATUS_ACCESS_DENIED)
                    {
                        throw new UnauthorizedAccessException();
                    }

                    if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY))
                    {
                        throw new OutOfMemoryException();
                    }

                    throw new Win32Exception(LsaNtStatusToWinError((int)ret));
                }
                finally
                {
                    if (tsids != IntPtr.Zero)
                    {
                        LsaFreeMemory(tsids);
                        tsids = IntPtr.Zero;
                    }
                    if (tdom != IntPtr.Zero)
                    {
                        LsaFreeMemory(tdom);
                        tdom = IntPtr.Zero;
                    }
                }
            }

            public void Dispose()
            {
                if (lsaHandle != IntPtr.Zero)
                {
                    LsaClose(lsaHandle);
                    lsaHandle = IntPtr.Zero;
                }
                GC.SuppressFinalize(this);
            }

            ~LsaWrapper()
            {
                Dispose();
            }

            private IntPtr GetSIDInformation(string account)
            {
                var names = new LsaUnicodeString[] { InitLsaString(account) };

                LsaTranslatedSid2 lts;
                lts.Sid = IntPtr.Zero;

                int ret = LsaLookupNames2(lsaHandle, 0, 1, names, ref tdom, ref tsids);
                if (ret != 0)
                {
                    throw new Win32Exception(LsaNtStatusToWinError(ret));
                }

                lts = (LsaTranslatedSid2)Marshal.PtrToStructure(tsids, typeof(LsaTranslatedSid2));
                return lts.Sid;
            }

            private static LsaUnicodeString InitLsaString(string s)
            {
                // Unicode strings max. 32KB
                if (s.Length > 0x7ffe)
                {
                    throw new ArgumentException("String too long");
                }

                var lus = new LsaUnicodeString();
                lus.Buffer = s;
                lus.Length = (ushort)(s.Length * sizeof(char));
                lus.MaximumLength = (ushort)(lus.Length + sizeof(char));
                return lus;
            }
        }
    }
}
