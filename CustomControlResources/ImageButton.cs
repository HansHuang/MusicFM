﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomControlResources
{
    public class ImageButton : Button
    {
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        #region Image

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(object), typeof(ImageButton), new PropertyMetadata(null,OnImagePropertyChanged));

        private static void OnImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var iBtn = d as ImageButton;
            if (iBtn == null) return;
            var img = e.NewValue as ImageSource;
            if (img != null)
                iBtn.Image = new ColorlizeImage {Image = img, Width = iBtn.Width, Height = iBtn.Height};
        }

        public object Image
        {
            get { return GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        #endregion



        #region ImageHover

        public static readonly DependencyProperty ImageHoverProperty =
            DependencyProperty.Register("ImageHover", typeof(object), typeof(ImageButton), new PropertyMetadata(default(object)));

        public object ImageHover
        {
            get { return GetValue(ImageHoverProperty); }
            set { SetValue(ImageHoverProperty, value); }
        }

        #endregion

        #region ImagePressed

        public static readonly DependencyProperty ImagePressedProperty =
            DependencyProperty.Register("ImagePressed", typeof(object), typeof(ImageButton), new PropertyMetadata(default(object)));

        public object ImagePressed
        {
            get { return GetValue(ImagePressedProperty); }
            set { SetValue(ImagePressedProperty, value); }
        }
        #endregion

        #region ImageDisable

        public static readonly DependencyProperty ImageDisableProperty =
            DependencyProperty.Register("ImageDisable", typeof(object), typeof(ImageButton), new PropertyMetadata(default(object)));

        public object ImageDisable
        {
            get { return GetValue(ImageDisableProperty); }
            set { SetValue(ImageDisableProperty, value); }
        }

        #endregion
    }
}
