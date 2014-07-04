using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelperLibrary
{
    public class InternetHelper
    {
        //private const int INTERNET_CONNECTION_MODEM = 1;
        //private const int INTERNET_CONNECTION_LAN = 2;

        [DllImport("winInet.dll")]
        private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);

        public static bool IsConnected
        {
            get
            {
                var dwFlag = new int();
                return InternetGetConnectedState(ref dwFlag, 0);
            }
        }

    }
}
