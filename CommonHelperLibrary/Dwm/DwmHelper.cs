using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace CommonHelperLibrary.Dwm
{
    /// <summary>
    /// Encapsulated some DWM functions.
    /// </summary>
    public class DwmHelper
    {
        /// <summary>
        /// Gets a value indicating whether DWM is supported.
        /// </summary>
        /// <value>
        ///   <c>true</c> if DWM is supported; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDwmSupported
        {
            get
            {
                //Only support vista, 7; not win8+
                var version = Environment.OSVersion.Version;
                return version.Major >= 6 && version.Minor < 2;
            }
        }

        /// <summary>
        /// Gets a value indicating whether composition is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if composition is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsCompositionEnabled
        {
            get
            {
                ValidateDwmIsSupported();
                bool enabled;
                NativeMethods.DwmIsCompositionEnabled(out enabled);
                return enabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Aero glass effect is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if Aero glass effect is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsAeroGlassEffectEnabled
        {
            get
            {
                ValidateDwmIsSupported();
                return IsCompositionEnabled && ColorizationColor.Opaque == false;
            }
        }

        /// <summary>
        /// Gets the colorization color.
        /// </summary>
        /// <value>
        /// The colorization color.
        /// </value>
        public static ColorizationColor ColorizationColor
        {
            get
            {
                ValidateDwmIsSupported();
                uint color;
                bool opaque;
                NativeMethods.DwmGetColorizationColor(out color, out opaque);
                return new ColorizationColor { Color = ColorizationColor.UInt32ToColor(color), Opaque = opaque };
            }
        }

        private readonly Window _window;

        /// <summary>
        /// Gets the the window asso associated with this DwmHelper instance.
        /// </summary>
        /// <value>
        /// The window asso associated with this DwmHelper instance。
        /// </value>
        public Window Window
        {
            get
            {
                return _window;
            }
        }

        /// <summary>
        /// Validates the DWM is supported.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">DWM is not supported by this operating system.</exception>
        protected static void ValidateDwmIsSupported()
        {
            if (!IsDwmSupported)
            {
                throw new InvalidOperationException("DWM is not supported by this operating system.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DwmHelper"/> class based on a window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <exception cref="System.ArgumentNullException">window</exception>
        public DwmHelper(Window window)
        {
            ValidateDwmIsSupported();
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            _window = window;

            if (new WindowInteropHelper(Window).Handle != IntPtr.Zero)
            {
                AddWindowMessageHook();
            }
            else
            {
                window.SourceInitialized += delegate
                {
                    AddWindowMessageHook();
                };
            }
        }

        /// <summary>
        /// Adds the window message hook.
        /// </summary>
        private void AddWindowMessageHook()
        {
            var interopHelper = new WindowInteropHelper(Window);
            HwndSource source = HwndSource.FromHwnd(interopHelper.EnsureHandle());
            if (source != null) source.AddHook(WndProc);
        }

        /// <summary>
        /// Occurs when composition changed.
        /// </summary>
        public event EventHandler CompositionChanged;
        /// <summary>
        /// Occurs when colorization color changed.
        /// </summary>
        public event EventHandler ColorizationColorChanged;
        /// <summary>
        /// Occurs when Aero glass effect changed.
        /// </summary>
        public event EventHandler AeroGlassEffectChanged;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WindowMessage.WmDwmcompositionchanged)
            {
                if (CompositionChanged != null)
                {
                    CompositionChanged(this, EventArgs.Empty);
                }
                if (AeroGlassEffectChanged != null)
                {
                    AeroGlassEffectChanged(this, EventArgs.Empty);
                }
                handled = true;
            }
            if (msg == WindowMessage.WmDwmcolorizationcolorchanged)
            {
                if (ColorizationColorChanged != null)
                {
                    ColorizationColorChanged(this, EventArgs.Empty);
                }
                if (AeroGlassEffectChanged != null)
                {
                    AeroGlassEffectChanged(this, EventArgs.Empty);
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Enables blur behind window.
        /// </summary>
        /// <param name="enable">Set to <c>true</c> to enable blur behind window. Set to <c>false</c> to disable blur behind window.</param>
        /// <returns><c>true</c> if succeeds; otherwise, <c>false</c>.</returns>
        public bool EnableBlurBehindWindow(bool enable = true)
        {
            if (!IsCompositionEnabled)
                return false;

            IntPtr hwnd = new WindowInteropHelper(Window).EnsureHandle();

            var bb = new DwmBlurbehind
            {
                dwFlags = DwmBlurbehind.DwmBbEnable | DwmBlurbehind.DwmBbBlurregion,
                fEnable = enable,
                hRegionBlur = IntPtr.Zero
            };

            try
            {
                NativeMethods.DwmEnableBlurBehindWindow(hwnd, ref bb);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            finally
            {
                NativeMethods.DeleteObject(bb.hRegionBlur);
            }
        }
    }

    internal static class WindowMessage
    {
        internal const int WmDwmcompositionchanged = 0x031E;
        internal const int WmDwmcolorizationcolorchanged = 0x0320;
    }
}
