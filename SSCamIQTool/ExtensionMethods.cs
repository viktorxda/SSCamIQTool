using System.Reflection;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public static class ExtensionMethods
{
    public static void DoubleBuffered(this DataGridView dgv, bool setting)
    {
        dgv.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dgv, setting, null);
    }
}
