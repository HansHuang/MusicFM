using System;
using System.Windows.Forms;

namespace FlvPlayer
{
    public class FlashPlayer : AxShockwaveFlashObjects.AxShockwaveFlash
    {
        const int WmRbuttondown = 0x0204;

        //Forbid mouse right button click
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmRbuttondown)
            {
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }
    }
}