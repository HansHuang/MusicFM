using System;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace CustomControlResources
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    public class AeroGlassEffect
    {
        // Enables the glass effect
        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

        // Checks whether the glass effect is enabled in the system
        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern bool DwmIsCompositionEnabled();

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(Boolean), typeof(AeroGlassEffect), new FrameworkPropertyMetadata(OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, Boolean value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static Boolean GetIsEnabled(DependencyObject element)
        {
            return (Boolean)element.GetValue(IsEnabledProperty);
        }

        public static void OnIsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue.Equals(false) || !DwmIsCompositionEnabled()) return;
            var wnd = (Window)obj;
            wnd.Loaded += wnd_Loaded;
        }

        static void wnd_Loaded(object sender, RoutedEventArgs e)
        {
            var wnd = (Window)sender;
            var originalBg = wnd.Background;
            // Set the background to transparent from both the WPF and Win32 perspectives
            wnd.Background = Brushes.Transparent;
            try
            {
                var hwnd = new WindowInteropHelper(wnd).Handle;
                HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;
                var margins = new Margins { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
            catch (DllNotFoundException)
            {
                wnd.Background = originalBg;
            }
        }

        
    }
}
