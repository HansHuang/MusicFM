using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomControlResources
{
    public class ColourfulImage : Image
    {

        //public ImageSource Source
        //{
        //    get { return (ImageSource)GetValue(SourceProperty); }
        //    set { SetValue(SourceProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SourceProperty =
        //    DependencyProperty.Register("Source", typeof(ImageSource), typeof(ColourfulImage));



        //public Stretch Stretch
        //{
        //    get { return (Stretch)GetValue(StretchProperty); }
        //    set { SetValue(StretchProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Stretch.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty StretchProperty =
        //    DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ColourfulImage));



        //public StretchDirection StretchDirection
        //{
        //    get { return (StretchDirection)GetValue(StretchDirectionProperty); }
        //    set { SetValue(StretchDirectionProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for StretchDirection.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty StretchDirectionProperty =
        //    DependencyProperty.Register("StretchDirection", typeof(StretchDirection), typeof(ColourfulImage));


        public Brush ImageBrush
        {
            get { return (Brush)GetValue(ImageBrushProperty); }
            set { SetValue(ImageBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageBrushProperty =
            DependencyProperty.Register("ImageBrush", typeof (Brush), typeof (ColourfulImage)
                                        , new FrameworkPropertyMetadata(OnImageBrushChanged));

        private static void OnImageBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisImage = (ColourfulImage)d;
            thisImage.IsMasked = e.NewValue != null;
        }

        public Color ImageColor
        {
            get { return (Color)GetValue(ImageColorProperty); }
            set { SetValue(ImageColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageColorProperty =
            DependencyProperty.Register("ImageColor", typeof (Color), typeof (ColourfulImage),
                                        new FrameworkPropertyMetadata(OnImageColorChanged));

        private static void OnImageColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisImage = (ColourfulImage)d;
            var b = new SolidColorBrush { Color = (Color)e.NewValue };
            d.SetCurrentValue(ImageBrushProperty, b);
            //thisImage.ImageBrush = b;

            if (e.NewValue == null)
            {
                thisImage.IsMasked = false;
            }

        }

        public bool IsMasked
        {
            get { return (bool)GetValue(IsMaskedProperty); }
            set { SetValue(IsMaskedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMasked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaskedProperty =
            DependencyProperty.Register("IsMasked", typeof(bool), typeof(ColourfulImage), new FrameworkPropertyMetadata(false));


        static ColourfulImage()
        {

        }
    }
}
