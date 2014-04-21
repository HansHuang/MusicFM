using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CustomControlResources
{
    public class ImageToggleButton:ToggleButton
    {
        static ImageToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageToggleButton), new FrameworkPropertyMetadata(typeof(ImageToggleButton)));
        }

        #region Image

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(object), typeof(ImageToggleButton), new PropertyMetadata(null, OnImagePropertyChanged));

        private static void OnImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var iBtn = d as ImageToggleButton;
            if (iBtn == null) return;
            var img = e.NewValue as ImageSource;
            if (img != null)
                iBtn.Image = new ColorlizeImage { Image = img };
        }

        public object Image
        {
            get { return GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        #endregion
    }
}
