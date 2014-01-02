using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace FlvPlayer
{
    public class FlvPlayer : ContentControl
    {
        protected FlashPlayer Player { get; private set; }

        public FlvPlayer()
        {
            Player = new FlashPlayer();
            Content = new WindowsFormsHost { Child = Player};
        }

        #region DependencyProperty PlayerParameter
        public static readonly DependencyProperty PlayerParameterProperty =
          DependencyProperty.Register("PlayerParameter", typeof(PlayerParameters), typeof(FlvPlayer), new PropertyMetadata(null, ParaChanged));

        public DependencyProperty PlayerParameter
        {
            get { return (DependencyProperty)GetValue(PlayerParameterProperty); }
            set { SetValue(PlayerParameterProperty, value); }
        }

        private static void ParaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FlvPlayer;
            var para = e.NewValue as PlayerParameters;
            if (control == null || para == null) return;

            var flashVars = "url=" + para.FlvUrl + "&autoPlay=" + para.IsAutoPlay
                            + "&previewImageUrl=" + para.CaptureUrl + "&link=&playButton=1&showFullScreenButton=" +
                            para.IsAllowFullScreen;

            control.Player.Movie = Environment.CurrentDirectory + @"\ToobPlayer.swf?";
            control.Player.WMode = "transparent";
            control.Player.FlashVars = flashVars;
            control.Player.AllowFullScreen = para.IsAllowFullScreen.ToString();
            control.Player.Play();
        }
        #endregion
    }
}
