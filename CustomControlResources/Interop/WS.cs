namespace CustomControlResources.Interop
{
	internal static class WS
	{
		internal const int Overlapped = 0x00000000;
		internal const int Popup = (int)-0x80000000;
		internal const int Child = 0x40000000;
		internal const int Minimize = 0x20000000;
		internal const int Visible = 0x10000000;
		internal const int Disabled = 0x08000000;
		internal const int Clipsiblings = 0x04000000;
		internal const int Clipchildren = 0x02000000;
		internal const int Maximize = 0x01000000;
		internal const int Caption = Border | Dlgframe;
		internal const int Border = 0x00800000;
		internal const int Dlgframe = 0x00400000;
		internal const int Vscroll = 0x00200000;
		internal const int Hscroll = 0x00100000;
		internal const int Sysmenu = 0x00080000;
		internal const int Thickframe = 0x00040000;
		internal const int Group = 0x00020000;
		internal const int Tabstop = 0x00010000;

		internal const int Minimizebox = 0x00020000;
		internal const int Maximizebox = 0x00010000;

		internal const int Tiled = Overlapped;
		internal const int Iconic = Minimize;
		internal const int Sizebox = Thickframe;
		internal const int Tiledwindow = Overlappedwindow;

		// Common Window Styles
		internal const int Overlappedwindow = Overlapped | Caption | Sysmenu | Thickframe | Minimizebox | Maximizebox;
		internal const int Popupwindow = Popup | Border | Sysmenu;
		internal const int Childwindow = Child;

		#region WS_EX
		internal static class EX
		{
			internal const int Dlgmodalframe = 0x00000001;
			internal const int Noparentnotify = 0x00000004;
			internal const int Topmost = 0x00000008;
			internal const int Acceptfiles = 0x00000010;
			internal const int Transparent = 0x00000020;

			//#if(WINVER >= 0x0400)
			internal const int Mdichild = 0x00000040;
			internal const int Toolwindow = 0x00000080;
			internal const int Windowedge = 0x00000100;
			internal const int Clientedge = 0x00000200;
			internal const int Contexthelp = 0x00000400;

			internal const int Right = 0x00001000;
			internal const int Left = 0x00000000;
			internal const int Rtlreading = 0x00002000;
			internal const int Ltrreading = 0x00000000;
			internal const int Leftscrollbar = 0x00004000;
			internal const int Rightscrollbar = 0x00000000;

			internal const int Controlparent = 0x00010000;
			internal const int Staticedge = 0x00020000;
			internal const int Appwindow = 0x00040000;

			internal const int OVERLAPPEDWINDOW = (Windowedge | Clientedge);
			internal const int Palettewindow = (Windowedge | Toolwindow | Topmost);
			//#endif /* WINVER >= 0x0400 */

			//#if(_WIN32_WINNT >= 0x0500)
			internal const int Layered = 0x00080000;
			//#endif /* _WIN32_WINNT >= 0x0500 */

			//#if(WINVER >= 0x0500)
			internal const int Noinheritlayout = 0x00100000; // Disable inheritence of mirroring by children
			internal const int Layoutrtl = 0x00400000; // Right to left mirroring
			//#endif /* WINVER >= 0x0500 */

			//#if(_WIN32_WINNT >= 0x0500)
			internal const int Composited = 0x02000000;
			internal const int Noactivate = 0x08000000;
			//#endif /* _WIN32_WINNT >= 0x0500 */
		}
		#endregion
	}
}