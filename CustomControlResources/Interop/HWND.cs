using System;

namespace CustomControlResources.Interop
{
	internal static class Hwnd
	{
		internal static readonly IntPtr Notopmost = new IntPtr(-2);
		internal static readonly IntPtr Topmost = new IntPtr(-1);
		internal static readonly IntPtr Top = new IntPtr(0);
		internal static readonly IntPtr Bottom = new IntPtr(1);
	}
}