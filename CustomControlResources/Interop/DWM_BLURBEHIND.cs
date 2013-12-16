using System;
using System.Runtime.InteropServices;

namespace CustomControlResources.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DwmBlurbehind
	{
        public uint dwFlags;
		[MarshalAs(UnmanagedType.Bool)]
        public bool fEnable;
        public IntPtr hRegionBlur;
		[MarshalAs(UnmanagedType.Bool)]
        public bool fTransitionOnMaximized;

        public const uint DwmBbEnable = 0x00000001;
        public const uint DwmBbBlurregion = 0x00000002;
        public const uint DwmBbTransitiononmaximized = 0x00000004;
	}
}
