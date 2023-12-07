using System.Runtime.InteropServices;

namespace SSCamIQTool.LibComm;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct API
{
    public const short ChipSensorPadId = 36;

    public const short ChipApiId = 100;

    public const short ChipChannelId = 101;

    public const short ChipDeviceId = 104;

    public const short ImageResolutionId = 106;

    public const short RawCountApiId = 12803;

    public const short RawCompressId = 12804;

    public const short SclModInfoApiId = 12805;
}
