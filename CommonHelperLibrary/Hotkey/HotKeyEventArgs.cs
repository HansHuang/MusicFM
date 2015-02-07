using System;

namespace CommonHelperLibrary.Hotkey
{
    /// <summary>
    /// Event arg mode for hotkey event
    /// </summary>
    public class HotKeyEventArgs : EventArgs
    {
        public HotKey HotKey { get; private set; }

        public HotKeyEventArgs(HotKey hotKey)
        {
            HotKey = hotKey;
        }
    }
}
