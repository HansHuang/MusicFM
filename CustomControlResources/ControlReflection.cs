using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomControlResources
{
    public class ControlReflection : ContentControl
    {

        #region DependencyProperty ReflectionPrecent
        //The height percent for reflection of the host control
        public static readonly DependencyProperty ReflectionPercentProperty =
            DependencyProperty.Register("ReflectionPercent", typeof(double), typeof(ControlReflection), new PropertyMetadata(0.5));
        public double ReflectionPercent
        {
            get { return (double)GetValue(ReflectionPercentProperty); }
            set { SetValue(ReflectionPercentProperty, value); }
        }
        #endregion

        #region DependencyProperty Host

        public static readonly DependencyProperty HostProperty =
            DependencyProperty.Register("Host", typeof(object), typeof(ControlReflection), new FrameworkPropertyMetadata(null, OnHostChanged));

        public object Host
        {
            get { return (object)GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }

        private static void OnHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reflection = d as ControlReflection;
            var host = e.NewValue as FrameworkElement;
            if (reflection == null || host == null) return;
            var pct = reflection.ReflectionPercent;

            var border = new Border
            {
                Width = host.Width,
                Height = host.Height
            };

            var visualBrush = new VisualBrush
            {
                Visual = host,
                Transform = new ScaleTransform
                {
                    ScaleX = 1,
                    ScaleY = -1 * pct,
                    CenterX = 0,
                    CenterY = host.Height * pct / (pct + 1)
                }
            };
            border.Background = visualBrush;

            var linearBursh = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            linearBursh.GradientStops.Add(new GradientStop { Offset = 0, Color = Colors.Black });
            linearBursh.GradientStops.Add(new GradientStop {Offset = 0.5*pct, Color = Colors.Transparent});
            border.OpacityMask = linearBursh;

            reflection.Content = border;
        }
        #endregion
    }
}
