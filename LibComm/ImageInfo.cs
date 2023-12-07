namespace SSCamIQTool.LibComm;

public class ImageInfo
{
    public int LongFNumber { get; set; }

    public int LongExpoTime { get; set; }

    public int LongAGain { get; set; }

    public int LongDGain { get; set; }

    public int ShortFNumber { get; set; }

    public int ShortExpoTime { get; set; }

    public int ShortAGain { get; set; }

    public int ShortDGain { get; set; }

    public int BVx16384 { get; set; }

    public int RGain { get; set; }

    public int GGain { get; set; }

    public int BGain { get; set; }

    public int HdrEnable { get; set; }

    public EXPOSURE_TYPE_E ExposureType { get; set; }

    public CAMERA_MODE_TYPE CaptureType { get; set; }

    public override string ToString()
    {
        string result = "";
        switch (CaptureType)
        {
            case CAMERA_MODE_TYPE.CAMERA_MODE_SCL_OUT:
            case CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM:
            case CAMERA_MODE_TYPE.CAMERA_MODE_ISP_OUT:
            case CAMERA_MODE_TYPE.CAMERA_MODE_ENC_JPEG:
                result = HdrEnable != 0 ? "FN=" + LongFNumber + "_" + ShortFNumber + ",US=" + LongExpoTime + "_" + ShortExpoTime + ",AG=" + LongAGain + "_" + ShortAGain + ",DG=" + LongDGain + "_" + ShortDGain + ",BV=" + BVx16384 + ",R=" + RGain + ",G=" + GGain + ",B=" + BGain : "FN=" + LongFNumber + ",US=" + LongExpoTime + ",AG=" + LongAGain + ",DG=" + LongDGain + ",BV=" + BVx16384 + ",R=" + RGain + ",G=" + GGain + ",B=" + BGain;
                break;
            case CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW:
                result = ExposureType != EXPOSURE_TYPE_E.EXPOSURE_LONG_E ? "FN=" + ShortFNumber + ",US=" + ShortExpoTime + ",AG=" + ShortAGain + ",DG=" + ShortDGain + ",BV=" + BVx16384 + ",R=" + RGain + ",G=" + GGain + ",B=" + BGain : "FN=" + LongFNumber + ",US=" + LongExpoTime + ",AG=" + LongAGain + ",DG=" + LongDGain + ",BV=" + BVx16384 + ",R=" + RGain + ",G=" + GGain + ",B=" + BGain;
                break;
        }
        return result;
    }
}
