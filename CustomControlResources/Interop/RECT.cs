using System.Runtime.InteropServices;

namespace CustomControlResources.Interop
{
	[StructLayout(LayoutKind.Sequential)]
    public struct Rect
	{
        public int left, top, right, bottom;

        public Rect(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}
	}
}
