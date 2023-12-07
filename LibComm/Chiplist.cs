namespace SSCamIQTool.LibComm;

public class Chiplist
{
    public string name { get; set; }

    public string alias { get; set; }

    public ulong id { get; set; }

    public uint CalibrateChipID { get; set; }

    public int SupportSCL { get; set; }

    public Function[] Function { get; set; }
}
