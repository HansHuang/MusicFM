using System.Runtime.InteropServices;

namespace CustomControlResources.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct MARGINS
	{
		internal int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight;

		internal MARGINS(int left, int top, int right, int bottom)
		{
			cxLeftWidth = left;
			cyTopHeight = top;
			cxRightWidth = right;
			cyBottomHeight = bottom;
		}
	}
}
