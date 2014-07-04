using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelperLibrary
{
    class ShudownHelper
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int DoFlag, int rea);

        internal const int SePrivilegeEnabled = 0x00000002;
        internal const int TokenQuery = 0x00000008;
        internal const int TokenAdjustPrivileges = 0x00000020;
        internal const string SeShutdownName = "SeShutdownPrivilege";
        internal const int EwxLogoff = 0x00000000;
        internal const int EwxShutdown = 0x00000001;
        internal const int EwxReboot = 0x00000002;
        internal const int EwxForce = 0x00000004;
        internal const int EwxPoweroff = 0x00000008;
        internal const int EwxForceifhung = 0x00000010;

        private static void DoExitWin(int doFlag)
        {
            bool ok;
            TokPriv1Luid tp;
            var hproc = GetCurrentProcess();
            var htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TokenAdjustPrivileges | TokenQuery, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SePrivilegeEnabled;
            ok = LookupPrivilegeValue(null, SeShutdownName, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = ExitWindowsEx(doFlag, 0);
        }

        public static void Reboot()
        {
            DoExitWin(EwxForce | EwxReboot);
        }

        public static void PowerOff()
        {
            DoExitWin(EwxForce | EwxPoweroff);
        }

        public static void LogOff()
        {
            DoExitWin(EwxForce | EwxLogoff);
        }
    }
}
