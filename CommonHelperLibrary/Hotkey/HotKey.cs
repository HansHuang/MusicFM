using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;
using System.Windows.Interop;

namespace CommonHelperLibrary.Hotkey
{
    /// <summary>
    /// Represents an hotKey
    /// </summary>
    [Serializable]
    public class HotKey : INotifyPropertyChanged, ISerializable, IEquatable<HotKey>
    {
        #region INotifyPropertyChanged RaisePropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Key (INotifyPropertyChanged Property)

        private Key _key;

        /// <summary>
        /// The Key. Must not be null when registering to an HotKeyHost.
        /// </summary>
        public Key Key
        {
            get { return _key; }
            set
            {
                if (_key.Equals(value)) return;
                _key = value;
                RaisePropertyChanged("Key");
            }
        }

        #endregion

        #region Modifiers (INotifyPropertyChanged Property)

        private ModifierKeys _modifiers;

        /// <summary>
        /// The modifier. Multiple modifiers can be combined with or.
        /// </summary>
        public ModifierKeys Modifiers
        {
            get { return _modifiers; }
            set
            {
                if (_modifiers.Equals(value)) return;
                _modifiers = value;
                RaisePropertyChanged("Modifiers");
            }
        }

        #endregion

        #region Enabled (INotifyPropertyChanged Property)

        private bool _enabled;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled.Equals(value)) return;
                _enabled = value;
                RaisePropertyChanged("Enabled");
            }
        }

        #endregion

        #region Construction methods

        /// <summary>
        /// Creates an HotKey object. This instance has to be registered in an HotKeyHost.
        /// </summary>
        public HotKey()
        {
        }

        /// <summary>
        /// Creates an HotKey object. This instance has to be registered in an HotKeyHost.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="modifiers">The modifier. Multiple modifiers can be combined with or.</param>
        /// <param name="enabled">Specifies whether the HotKey will be enabled when registered to an HotKeyHost</param>
        public HotKey(Key key, ModifierKeys modifiers, bool enabled = true)
        {
            Key = key;
            Modifiers = modifiers;
            Enabled = enabled;
        }

        protected HotKey(SerializationInfo info, StreamingContext context)
        {
            Key = (Key)info.GetValue("Key", typeof(Key));
            Modifiers = (ModifierKeys)info.GetValue("Modifiers", typeof(ModifierKeys));
            Enabled = info.GetBoolean("Enabled");
        }

        #endregion

        #region Override methods of Object

        public override bool Equals(object obj)
        {
            var hotKey = obj as HotKey;
            return hotKey != null && Equals(hotKey);
        }

        public bool Equals(HotKey other)
        {
            return (Key == other.Key && Modifiers == other.Modifiers);
        }

        public override int GetHashCode()
        {
            return (int)Modifiers + 10 * (int)Key;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Modifiers != ModifierKeys.None) sb.Append(Modifiers);
            if (Key != Key.None)
            {
                if (sb.Length > 0) sb.Append(" + ");
                sb.Append(Key);
            }
            return sb.ToString();
        }

        #endregion

        #region Event: HotKeyPressed

        /// <summary>
        /// Will be raised if the hotkey is pressed (works only if registed in HotKeyHost)
        /// </summary>
        public event EventHandler<HotKeyEventArgs> HotKeyPressed;

        internal virtual void OnHotKeyPress()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this, new HotKeyEventArgs(this));
        }

        public void RemoveAllHotkeyPressedHandle()
        {
            if (HotKeyPressed == null) return;
            foreach (var dlg in HotKeyPressed.GetInvocationList())
            {
                HotKeyPressed -= (EventHandler<HotKeyEventArgs>)dlg;
            }
        }


        #endregion

        #region GetObjectData

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", Key, typeof(Key));
            info.AddValue("Modifiers", Modifiers, typeof(ModifierKeys));
            info.AddValue("Enabled", Enabled);
        }

        #endregion


    }
}
