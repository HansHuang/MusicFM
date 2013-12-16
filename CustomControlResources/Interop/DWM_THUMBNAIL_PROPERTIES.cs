using System.Runtime.InteropServices;

namespace CustomControlResources.Interop
{
	[StructLayout(LayoutKind.Sequential)]
    public struct DwmThumbnailProperties
	{
        public uint dwFlags;
        public Rect rcDestination;
        public Rect rcSource;
        public byte opacity;
		[MarshalAs(UnmanagedType.Bool)]
        public bool fVisible;
		[MarshalAs(UnmanagedType.Bool)]
        public bool fSourceClientAreaOnly;

        public const uint DwmTnpRectdestination = 0x00000001;
        public const uint DwmTnpRectsource = 0x00000002;
        public const uint DwmTnpOpacity = 0x00000004;
        public const uint DwmTnpVisible = 0x00000008;
        public const uint DwmTnpSourceclientareaonly = 0x00000010;
	}
}
