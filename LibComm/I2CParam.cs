namespace SSCamIQTool.LibComm;

public class I2CParam
{
    public int SlaveAddr { get; set; }

    public int AddrLength { get; set; }

    public int DataLength { get; set; }

    public int I2CSpeed { get; set; }

    public int Address { get; set; }

    public int Value { get; set; }

    public I2CParam()
    {
        SlaveAddr = 108;
        AddrLength = 16;
        DataLength = 8;
        I2CSpeed = 200000;
        Address = 0;
        Value = 0;
    }

    public int[] ToDataArray()
    {
        return new int[6] { SlaveAddr, AddrLength, DataLength, I2CSpeed, Address, Value };
    }
}
