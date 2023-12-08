using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public static class ExtensionMethods
{
    public static void DoubleBuffered(this DataGridView dgv, bool setting)
    {
        dgv.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dgv, setting, null);
    }

    private static Stream GetStream(string file)
    {
        var asm = Assembly.GetExecutingAssembly();
        foreach (string image in asm.GetManifestResourceNames())
        {
            if (image.Contains(file))
            {
                return asm.GetManifestResourceStream(image);
            }
        }
        return null;
    }

    public static Bitmap GetImageByName(string file)
    {
        return new(GetStream(file));
    }

    public static Icon GetIconByName(string file)
    {
        return new(GetStream(file));
    }
}
