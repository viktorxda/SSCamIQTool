using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class SstarHScrollBar : HScrollBar
{
    private const int WM_MOUSEWHEEL = 522;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg != 522)
        {
            base.WndProc(ref m);
        }
    }
}
