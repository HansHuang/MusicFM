using System;

namespace CustomControlResources.Interop
{
	internal struct Kbdllhookstruct
	{
		internal UInt32 VkCode;
		internal UInt32 ScanCode;
		internal UInt32 Flags;
		internal UInt32 time;
		internal IntPtr ExtraInfo;
	}
}
