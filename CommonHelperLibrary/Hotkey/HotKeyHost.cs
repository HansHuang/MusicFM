using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace CommonHelperLibrary.Hotkey
{
    /// <summary>
    /// The HotKeyHost needed for working with hotKeys.
    /// </summary>
    public sealed class HotKeyHost : IDisposable
    {
        /// <summary>
        /// Creates a new HotKeyHost
        /// </summary>
        /// <param name="hwndSource">The handle of the window. Must not be null.</param>
        public HotKeyHost(HwndSource hwndSource)
        {
            if (hwndSource == null)
                throw new ArgumentNullException("hwndSource");

            _hook = WndProc;
            _hwndSource = hwndSource;
            hwndSource.AddHook(_hook);
        }

        #region HotKey Interop

        private const int WmHotKey = 786;

        [DllImport("user32", CharSet = CharSet.Ansi,
          SetLastError = true, ExactSpelling = true)]
        private static extern int RegisterHotKey(IntPtr hwnd, int id, int modifiers, int key);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);

        #endregion

        #region Interop-Encapsulation

        private readonly HwndSourceHook _hook;
        private readonly HwndSource _hwndSource;

        private void RegisterHotKey(int id, HotKey hotKey)
        {
            if ((int)_hwndSource.Handle == 0) throw new InvalidOperationException("Handle is invalid");
            RegisterHotKey(_hwndSource.Handle, id, (int)hotKey.Modifiers, KeyInterop.VirtualKeyFromKey(hotKey.Key));
            var error = Marshal.GetLastWin32Error();
            if (error == 0) return;
            Exception e = new Win32Exception(error);

            if (error == 1409)
                throw new HotKeyAlreadyRegisteredException(e.Message, hotKey, e);
            throw e;
        }

        private void UnregisterHotKey(int id)
        {
            if ((int)_hwndSource.Handle == 0) return;
            UnregisterHotKey(_hwndSource.Handle, id);
            var error = Marshal.GetLastWin32Error();
            if (error != 0)
                throw new Win32Exception(error);
        }

        #endregion

        /// <summary>
        /// Will be raised if any registered hotKey is pressed
        /// </summary>
        public event EventHandler<HotKeyEventArgs> HotKeyPressed;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WmHotKey || !_hotKeys.ContainsKey((int)wParam)) return new IntPtr(0);

            var h = _hotKeys[(int)wParam];
            h.OnHotKeyPress();
            if (HotKeyPressed != null)
                HotKeyPressed(this, new HotKeyEventArgs(h));

            return new IntPtr(0);
        }

        private void HotKeyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var kvPair = _hotKeys.FirstOrDefault(h => Equals(h.Value, sender));
            if (kvPair.Value == null) return;
            switch (e.PropertyName)
            {
                case "Enabled":
                    if (kvPair.Value.Enabled)
                        RegisterHotKey(kvPair.Key, kvPair.Value);
                    else
                        UnregisterHotKey(kvPair.Key);
                    break;
                case "Modifiers":
                case "Key":
                    if (kvPair.Value.Enabled)
                    {
                        UnregisterHotKey(kvPair.Key);
                        RegisterHotKey(kvPair.Key, kvPair.Value);
                    }
                    break;
            }
        }

        private readonly Dictionary<int, HotKey> _hotKeys = new Dictionary<int, HotKey>();

        public class SerialCounter
        {
            public SerialCounter(int start)
            {
                Current = start;
            }

            public int Current { get; private set; }

            public int Next()
            {
                return ++Current;
            }
        }

        /// <summary>
        /// All registered hotKeys
        /// </summary>
        public IEnumerable<HotKey> HotKeys
        {
            get { return _hotKeys.Values; }
        }


        private static readonly SerialCounter IdGen = new SerialCounter(1);

        //Annotation: Can be replaced with "Random"-class
        /// <summary>
        /// Adds an hotKey.
        /// </summary>
        /// <param name="hotKey">The hotKey which will be added. Must not be null and can be registed only once.</param>
        public void AddHotKey(HotKey hotKey)
        {
            if (hotKey == null || hotKey.Key == 0)
                throw new ArgumentNullException("hotKey");
            if (_hotKeys.ContainsValue(hotKey))
                throw new HotKeyAlreadyRegisteredException("HotKey already registered!", hotKey);

            var id = IdGen.Next();
            if (hotKey.Enabled)
                RegisterHotKey(id, hotKey);
            hotKey.PropertyChanged += HotKeyPropertyChanged;
            _hotKeys[id] = hotKey;
        }

        /// <summary>
        /// Removes an hotKey
        /// </summary>
        /// <param name="hotKey">The hotKey to be removed</param>
        /// <returns>True if success, otherwise false</returns>
        public bool RemoveHotKey(HotKey hotKey)
        {
            var kvPair = _hotKeys.FirstOrDefault(h => Equals(h.Value, hotKey));
            if (kvPair.Value != null)
            {
                kvPair.Value.PropertyChanged -= HotKeyPropertyChanged;
                if (kvPair.Value.Enabled)
                    UnregisterHotKey(kvPair.Key);
                return _hotKeys.Remove(kvPair.Key);
            }
            return false;
        }

        #region Destructor

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) _hwndSource.RemoveHook(_hook);

            for (var i = _hotKeys.Count - 1; i >= 0; i--)
                RemoveHotKey(_hotKeys.Values.ElementAt(i));

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~HotKeyHost()
        {
            Dispose(false);
        }

        #endregion
    }
}
