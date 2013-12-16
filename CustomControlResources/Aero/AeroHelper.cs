using System;
using System.Windows;
using System.Windows.Interop;
using CustomControlResources.Interop;

namespace CustomControlResources.Aero
{
    /// <summary>
    /// Window Aero Effect Helper
    /// </summary>
    public class AeroHelper
    {
        /// <summary>
        /// Extend Aero Glass Effect to custom aera
        /// </summary>
        /// <param name="window">Target Window</param>
        /// <param name="margin">Margin</param>
        /// <returns>Succeed or not</returns>
        public static bool ExtendGlassFrame(Window window, Thickness margin)
        {
            if (!AeroGlassCompositionEnabled)
                return false;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException("Window must be display before enable aero effect");

            var margins = new MARGINS((int)margin.Left, (int)margin.Top, (int)margin.Right, (int)margin.Bottom);
            NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);

            return true;
        }

        /// <summary>
        /// Enable aero effect behind of window
        /// </summary>
        /// <param name="window">Target Window</param>
        /// <returns>Succeed or not</returns>
        public static bool EnableBlurBehindWindow(Window window)
        {
            if (!AeroGlassCompositionEnabled)
                return false;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException("Window must be display before enable aero effect");

            var bb = new DwmBlurbehind
                {
                    dwFlags = DwmBlurbehind.DwmBbEnable | DwmBlurbehind.DwmBbBlurregion,
                    fEnable = true,
                    hRegionBlur = NativeMethods.CreateRectRgn(0, 0, (int) window.ActualWidth, (int) window.ActualHeight)
                };

            NativeMethods.DwmEnableBlurBehindWindow(hwnd, ref bb);

            NativeMethods.DeleteObject(bb.hRegionBlur);

            return true;
        }

        /// <summary>
        /// Check system support aero effect or not
        /// </summary>
        public static bool AeroGlassCompositionEnabled
        {
            get
            {
                //Only Vista+ version support aero
                if (Environment.OSVersion.Version.Major < 6)
                    return false;
                try
                {
                    return NativeMethods.DwmIsCompositionEnabled();
                }
                catch
                {
                    return false;
                }
            }
        }

    }
}