using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxShockwaveFlashObjects;

namespace FlvPlayer
{
  public class FlashPlay : AxShockwaveFlash
  {
    const int WM_RBUTTONDOWN = 0x0204;

    protected override void WndProc(ref Forms.Message m)
    {
      if (m.Msg == WM_RBUTTONDOWN)
      {
        m.Result = IntPtr.Zero;
        return;
      }
      base.WndProc(ref m);
    }
  }
}
