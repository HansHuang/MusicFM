using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommonHelperLibrary.Hotkey;

namespace CustomControlResources
{
    public class HotkeySettingBox : TextBox, INotifyPropertyChanged
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

        #region DependencyProperty Command

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof (ICommand), typeof (HotkeySettingBox),
                new PropertyMetadata(default(ICommand), OnCmdChanged));

        private static void OnCmdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as HotkeySettingBox;
            if (box == null) return;

            box.Hotkey = box.Hotkey ?? new HotKey(Key.None, ModifierKeys.None);
            box.Hotkey.RemoveAllHotkeyPressedHandle();
            box.Hotkey.HotKeyPressed += (s, arg) => box.Command.Execute(s);
        }

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion

        #region Hotkey (INotifyPropertyChanged Property)

        private HotKey _hotkey;

        public HotKey Hotkey
        {
            get { return _hotkey; }
            set
            {
                if (_hotkey != null && _hotkey.Equals(value)) return;
                _hotkey = value;
                RaisePropertyChanged("Hotkey");
                Text = value == null ? string.Empty : value.ToString();
            }
        }

        #endregion

        #region ClearHotkey

        public delegate void ClearHotkeyHandler();

        public event ClearHotkeyHandler ClearHotkey;

        protected void OnClearHotkey()
        {
            if (ClearHotkey != null)
            {
                ClearHotkey();
            }
        }

        #endregion

        #region RelayCommand ClearCmd

        private RelayCommand _clearCmd;

        public ICommand ClearCmd
        {
            get { return _clearCmd ?? (_clearCmd = new RelayCommand(s => ClearExecute())); }
        }

        private void ClearExecute()
        {
            Hotkey = null;
            OnClearHotkey();
        }

        #endregion

        #region Readonly Fields

        private static readonly HashSet<Key> IgnoredKeys = new HashSet<Key>
        {
            Key.None,
            Key.LineFeed,
            Key.KanaMode,
            Key.HangulMode,
            Key.JunjaMode,
            Key.FinalMode,
            Key.HanjaMode,
            Key.KanjiMode,
            Key.ImeConvert,
            Key.ImeNonConvert,
            Key.ImeAccept,
            Key.ImeModeChange,
            Key.ImeProcessed,
            Key.System,
            Key.NoName,
            Key.DeadCharProcessed,
            Key.Back
        };

        private static readonly Dictionary<Key, ModifierKeys> KeyMap = new Dictionary<Key, ModifierKeys>
        {
            {Key.LeftCtrl, ModifierKeys.Control},
            {Key.RightCtrl, ModifierKeys.Control},
            {Key.LeftAlt, ModifierKeys.Alt},
            {Key.RightAlt, ModifierKeys.Alt},
            {Key.LeftShift, ModifierKeys.Shift},
            {Key.RightShift, ModifierKeys.Shift},
            {Key.LWin, ModifierKeys.Windows},
            {Key.RWin, ModifierKeys.Windows}
        };

        #endregion

        //static HotkeySettingBox()
        //{
        //  DefaultStyleKeyProperty.OverrideMetadata(typeof(HotkeySettingBox), new FrameworkPropertyMetadata(typeof(HotkeySettingBox)));
        //}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            IsReadOnly = true;

            PreviewKeyDown += (s, e) =>
            {
                var key = Key.None;
                var modifider = ModifierKeys.None;
                foreach (Key k in Enum.GetValues(typeof (Key)))
                {
                    if (IgnoredKeys.Contains(k) || !Keyboard.IsKeyDown(k)) continue;
                    if (KeyMap.ContainsKey(k))
                        modifider |= KeyMap[k];
                    else
                        key = k;
                }
                if (Hotkey == null)
                    Hotkey = new HotKey(key, modifider);
                else
                {
                    Hotkey.Key = key;
                    Hotkey.Modifiers = modifider;
                    Text = Hotkey.ToString();
                }
                e.Handled = true;
            };
        }

    }
}