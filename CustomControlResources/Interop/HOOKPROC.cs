using System;

namespace CustomControlResources.Interop
{
	// hook method called by system
	internal delegate IntPtr Hookproc(int code, IntPtr wParam, ref Kbdllhookstruct lParam);
}
