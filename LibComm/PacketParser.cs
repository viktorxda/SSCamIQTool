using System;
using System.Linq;

namespace SSCamIQTool.LibComm;

public class PacketParser
{
    private const int startIndex = 8;

    public static void ConvertBufferToType(SensorInfo sensorInfo, byte[] sensorBuffer)
    {
        sensorInfo.Width = BitConverter.ToInt32(sensorBuffer, 8);
        sensorInfo.Height = BitConverter.ToInt32(sensorBuffer, 12);
        sensorInfo.BayerID = (SensorInfo.SensorBayer)BitConverter.ToInt32(sensorBuffer, 16);
        sensorInfo.Precision = (SensorInfo.SensorPrecision)BitConverter.ToInt32(sensorBuffer, 20);
        if (sensorInfo.Precision != SensorInfo.SensorPrecision.bit16)
        {
            sensorInfo.Precision = SensorInfo.SensorPrecision.bit16;
        }
    }

    public static void ConvertBufferToType(ImageInfo imageInfo, ChipID chipid, byte[] aeBuffer, byte[] awbBuffer)
    {
        imageInfo.LongFNumber = BitConverter.ToInt32(aeBuffer, 16);
        imageInfo.LongAGain = BitConverter.ToInt32(aeBuffer, 20);
        imageInfo.LongDGain = BitConverter.ToInt32(aeBuffer, 24);
        imageInfo.LongExpoTime = BitConverter.ToInt32(aeBuffer, 28);
        imageInfo.ShortFNumber = BitConverter.ToInt32(aeBuffer, 32);
        imageInfo.ShortAGain = BitConverter.ToInt32(aeBuffer, 36);
        imageInfo.ShortDGain = BitConverter.ToInt32(aeBuffer, 40);
        imageInfo.ShortExpoTime = BitConverter.ToInt32(aeBuffer, 44);
        imageInfo.BVx16384 = BitConverter.ToInt32(aeBuffer, 572);
        if (chipid == ChipID.I1 || chipid == ChipID.I3 || chipid == ChipID.I2 || chipid == ChipID.M5 || chipid == ChipID.M5U)
        {
            imageInfo.RGain = BitConverter.ToInt16(awbBuffer, 8);
            imageInfo.GGain = BitConverter.ToInt16(awbBuffer, 10);
            imageInfo.BGain = BitConverter.ToInt16(awbBuffer, 14);
        }
        else
        {
            imageInfo.RGain = BitConverter.ToInt16(awbBuffer, 12);
            imageInfo.GGain = BitConverter.ToInt16(awbBuffer, 14);
            imageInfo.BGain = BitConverter.ToInt16(awbBuffer, 18);
        }
    }

    public static void ConvertBufferToType(I2CParam i2cParam, byte[] i2cBuffer)
    {
        i2cParam.Value = BitConverter.ToInt32(i2cBuffer, 8);
    }

    public static void ConvertBufferToType(ref ushort apiVerMajor, ref ushort apiVerMinor, byte[] versionBuffer)
    {
        apiVerMajor = BitConverter.ToUInt16(versionBuffer, 12);
        apiVerMinor = BitConverter.ToUInt16(versionBuffer, 16);
    }

    public static void ConvertBufferToType(ref ApiVersion apiVersion, byte[] versionBuffer)
    {
        apiVersion = (ApiVersion)BitConverter.ToInt32(versionBuffer, 8);
    }

    public static void ConvertBufferToTypeForI3(ref ushort apiVerMajor, ref ushort apiVerMinor, byte[] versionBuffer)
    {
        int num = 0;
        int num2 = 4;
        num = BitConverter.ToUInt16(versionBuffer, 2);
        if (num <= 0)
        {
            return;
        }
        int[] array = new int[num];
        ushort[] array2 = new ushort[num];
        for (int i = 0; i < num; i++)
        {
            array[i] = BitConverter.ToInt32(versionBuffer, num2 + i * 4);
        }
        versionBuffer = versionBuffer.Skip(num2 + num * 4).ToArray();
        for (int j = 0; j < num; j++)
        {
            array2[j] = BitConverter.ToUInt16(Enumerable.Take(count: j >= num - 1 ? versionBuffer.Length - array[j] : array[j + 1] - array[j], source: versionBuffer.Skip(array[j])).ToArray(), 0);
            switch (j)
            {
                case 0:
                    apiVerMajor = array2[j];
                    break;
                case 1:
                    apiVerMinor = array2[j];
                    break;
            }
        }
    }

    public static void ConvertBufferToType(ref ChipID chipID, byte[] idBuffer)
    {
        int value = BitConverter.ToInt32(idBuffer, 8);
        chipID = (ChipID)Enum.ToObject(typeof(ChipID), value);
    }

    public static void ConvertChannelBufferToType(ref ushort channelID, byte[] idBuffer)
    {
        channelID = BitConverter.ToUInt16(idBuffer, 8);
    }

    public static void ConvertDeviceBufferToType(ref ushort DeviceID, byte[] idBuffer)
    {
        DeviceID = BitConverter.ToUInt16(idBuffer, 8);
    }

    public static void ConvertBufferToID(ref ushort sensorPadID, byte[] idBuffer)
    {
        sensorPadID = BitConverter.ToUInt16(idBuffer, 8);
    }

    public static void ConvertBufferToIQIndex(ref byte index, byte[] indexBuffer)
    {
        index = indexBuffer[8];
    }

    public static byte[] GetSendBufferByDataArray(short apiId, int[] data)
    {
        byte[] array = null;
        int value = 0;
        int num = 0;
        short value2 = 1;
        int num2 = data.Length;
        byte[] array2 = null;
        try
        {
            array = new byte[8 + num2 * 4];
            array2 = BitConverter.GetBytes(apiId);
            Buffer.BlockCopy(array2, 0, array, 0, array2.Length);
            num += array2.Length;
            array2 = BitConverter.GetBytes(value2);
            Buffer.BlockCopy(array2, 0, array, num, array2.Length);
            num += array2.Length;
            array2 = BitConverter.GetBytes(value);
            Buffer.BlockCopy(array2, 0, array, num, array2.Length);
            num += array2.Length;
            for (int i = 0; i < num2; i++)
            {
                array2 = BitConverter.GetBytes(data[i]);
                Buffer.BlockCopy(array2, 0, array, num, array2.Length);
                num += array2.Length;
            }
        }
        catch (Exception ex)
        {
            ex.ToString();
        }
        return array;
    }

    public static byte[] GetApiBufferByDataArray(short apiId, int[] data)
    {
        byte[] array = null;
        int num = 0;
        int num2 = 0;
        short num3 = (short)data.Length;
        byte[] array2 = null;
        try
        {
            array = new byte[4 + num3 * 4 + num3 * 4];
            array2 = BitConverter.GetBytes(apiId);
            Buffer.BlockCopy(array2, 0, array, 0, array2.Length);
            num2 += array2.Length;
            array2 = BitConverter.GetBytes(num3);
            Buffer.BlockCopy(array2, 0, array, num2, array2.Length);
            num2 += array2.Length;
            for (int i = 0; i < num3; i++)
            {
                array2 = BitConverter.GetBytes(num);
                Buffer.BlockCopy(array2, 0, array, num2, array2.Length);
                num += 4;
                num2 += array2.Length;
            }
            for (int j = 0; j < num3; j++)
            {
                array2 = BitConverter.GetBytes(data[j]);
                Buffer.BlockCopy(array2, 0, array, num2, array2.Length);
                num2 += array2.Length;
            }
        }
        catch (Exception ex)
        {
            ex.ToString();
        }
        return array;
    }
}
