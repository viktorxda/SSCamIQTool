namespace SSCamIQTool.LibComm;

public class SensorInfo
{
    public enum SensorBayer
    {
        RG,
        GR,
        BG,
        GB,
        R0,
        G0,
        B0,
        G1,
        G2,
        I0,
        G3,
        I1
    }

    public enum SensorPrecision
    {
        bit8,
        bit10,
        bit12,
        bit14,
        bit16
    }

    public int Width { get; set; }

    public int Height { get; set; }

    public SensorBayer BayerID { get; set; }

    public SensorPrecision Precision { get; set; }

    public override string ToString()
    {
        return Width + "x" + Height + "_" + Precision.ToString().Substring(3) + "_" + BayerID.ToString() + "_";
    }

    public string ScOutToString()
    {
        int num = Width;
        int num2 = Height;
        if (Width % 32 != 0)
        {
            num = (Width / 32 + 1) * 32;
        }
        if (Height % 32 != 0)
        {
            num2 = (Height / 32 + 1) * 32;
        }
        if (Width != num || Height != num2)
        {
            return Width + "x" + Height + "[" + num + "x" + num2 + "]_" + Precision.ToString().Substring(3) + "_" + BayerID.ToString() + "_";
        }
        return Width + "x" + Height + "_" + Precision.ToString().Substring(3) + "_" + BayerID.ToString() + "_";
    }
}
