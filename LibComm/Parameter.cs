namespace SSCamIQTool.LibComm;

public class Parameter
{
    public string chip { get; set; }

    public int QueryExposureInfo { get; set; }

    public int QueryWBInfo { get; set; }

    public int GetGamma { get; set; }

    public int I2CAccess { get; set; }

    public int QuerySensorInfo { get; set; }

    public int GetLinearityLUT { get; set; }

    public int GetOBCCALIB { get; set; }

    public int ApiVersion { get; set; }

    public int GetGammaEx { get; set; }

    public int GetIQIndex { get; set; }

    public int WBCT { get; set; }

    public int AWBCTStats { get; set; }

    public int AWBHWStats { get; set; }

    public int AWBHWStatsShort { get; set; }

    public int OBbits { get; set; }

    public int AFWindow { get; set; }

    public int GetOBC { get; set; }

    public int GetALSC { get; set; }

    public int GetLSC { get; set; }

    public int GetApplyALSC { get; set; }

    public int GetApplyLSC { get; set; }

    public int GetApplyOBC { get; set; }

    public int AEHisto0HwStats { get; set; }

    public int AFStats { get; set; }

    public int AFRoiWindow { get; set; }

    public int AFRoiMode { get; set; }

    public int GetNirIQDevice { get; set; }

    public int GetNirIQChannel { get; set; }
}
