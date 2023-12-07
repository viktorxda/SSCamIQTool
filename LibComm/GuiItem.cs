using System;
using System.Xml;

namespace SSCamIQTool.LibComm;

public class GuiItem
{
    public string Tag;

    public string Text;

    public string GuiType;

    public ITEM_TYPE ValueType;

    public string Paramters;

    public string Info;

    public int XSize;

    public int YSize;

    public long MinValue;

    public long MaxValue;

    public long[] DataValue;

    public bool openfile_flag;

    public const string METERING_WEIGHTING_TEXT = "WindowWeighting";

    public int ItemSize
    {
        get
        {
            switch (ValueType)
            {
                case ITEM_TYPE.U8:
                    return 1;
                case ITEM_TYPE.U16:
                case ITEM_TYPE.S16:
                    return 2;
                case ITEM_TYPE.U32:
                case ITEM_TYPE.S32:
                    return 4;
                default:
                    return 0;
            }
        }
    }

    public int ValueSize => ItemSize * XSize * YSize;

    public void Update(byte[] buffer)
    {
        int num = 0;
        for (int i = 0; i < buffer.Length; i += ItemSize)
        {
            switch (ValueType)
            {
                case ITEM_TYPE.U8:
                    DataValue[num++] = buffer[i];
                    break;
                case ITEM_TYPE.S16:
                    DataValue[num++] = BitConverter.ToInt16(buffer, i);
                    break;
                case ITEM_TYPE.U16:
                    DataValue[num++] = BitConverter.ToUInt16(buffer, i);
                    break;
                case ITEM_TYPE.S32:
                    DataValue[num++] = BitConverter.ToInt32(buffer, i);
                    break;
                case ITEM_TYPE.U32:
                    DataValue[num++] = BitConverter.ToUInt32(buffer, i);
                    break;
            }
        }
    }

    public void UpdateXSize(byte[] buffer, int index)
    {
        int num = index * XSize;
        for (int i = 0; i < buffer.Length; i += ItemSize)
        {
            switch (ValueType)
            {
                case ITEM_TYPE.U8:
                    DataValue[num++] = buffer[i];
                    break;
                case ITEM_TYPE.S16:
                    DataValue[num++] = BitConverter.ToInt16(buffer, i);
                    break;
                case ITEM_TYPE.U16:
                    DataValue[num++] = BitConverter.ToUInt16(buffer, i);
                    break;
                case ITEM_TYPE.S32:
                    DataValue[num++] = BitConverter.ToInt32(buffer, i);
                    break;
                case ITEM_TYPE.U32:
                    DataValue[num++] = BitConverter.ToUInt32(buffer, i);
                    break;
            }
        }
    }

    public byte[] GetBytes()
    {
        byte[] array = new byte[ValueSize];
        int num = 0;
        for (int i = 0; i < DataValue.Length; i++)
        {
            Buffer.BlockCopy(ValueType switch
            {
                ITEM_TYPE.U8 => BitConverter.GetBytes((byte)DataValue[i]),
                ITEM_TYPE.S16 => BitConverter.GetBytes((short)DataValue[i]),
                ITEM_TYPE.U16 => BitConverter.GetBytes((ushort)DataValue[i]),
                ITEM_TYPE.S32 => BitConverter.GetBytes((int)DataValue[i]),
                ITEM_TYPE.U32 => BitConverter.GetBytes((uint)DataValue[i]),
                _ => BitConverter.GetBytes((byte)DataValue[i]),
            }, 0, array, num, ItemSize);
            num += ItemSize;
        }
        return array;
    }

    public byte[] GetXSizeBytes(int index)
    {
        byte[] array = new byte[ItemSize * XSize];
        int num = 0;
        for (int i = index * XSize; i < (index + 1) * XSize; i++)
        {
            Buffer.BlockCopy(ValueType switch
            {
                ITEM_TYPE.U8 => BitConverter.GetBytes((byte)DataValue[i]),
                ITEM_TYPE.S16 => BitConverter.GetBytes((short)DataValue[i]),
                ITEM_TYPE.U16 => BitConverter.GetBytes((ushort)DataValue[i]),
                ITEM_TYPE.S32 => BitConverter.GetBytes((int)DataValue[i]),
                ITEM_TYPE.U32 => BitConverter.GetBytes((uint)DataValue[i]),
                _ => BitConverter.GetBytes((byte)DataValue[i]),
            }, 0, array, num, ItemSize);
            num += ItemSize;
        }
        return array;
    }

    public GuiItem(string tag, string text, string guitype, string param, string info, int xsize, int ysize, long min, long max, ITEM_TYPE itemType)
    {
        Tag = tag;
        Text = text;
        GuiType = guitype;
        Paramters = param;
        Info = info;
        XSize = xsize;
        YSize = ysize;
        MinValue = min;
        MaxValue = max;
        ValueType = itemType;
        DataValue = new long[xsize * ysize];
        for (int i = 0; i < DataValue.Length; i++)
        {
            DataValue[i] = MinValue;
        }
    }

    public string WriteByPass(IQComm comm, short apiID)
    {
        string result = "";
        int.Parse(Paramters);
        short[] src = new short[1] { apiID };
        short[] src2 = new short[1] { 2 };
        int[] src3 = new int[2] { 0, 4 };
        int num = 0;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        iQ_CMD_RESPONSE_S.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_OK;
        try
        {
            int[] src4 = new int[2]
            {
                int.Parse(Paramters),
                (int)DataValue[0]
            };
            byte[] array = new byte[20];
            Buffer.BlockCopy(src, 0, array, 0, 2);
            num += 2;
            Buffer.BlockCopy(src2, 0, array, num, 2);
            num += 2;
            Buffer.BlockCopy(src3, 0, array, num, 8);
            num += 8;
            Buffer.BlockCopy(src4, 0, array, num, 8);
            if (comm.ConnectMode == CONNECT_MODE.MODE_USB)
            {
                iQ_CMD_RESPONSE_S = comm.SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, array, array.Length);
            }
            else if (comm.ConnectMode == CONNECT_MODE.MODE_SOCKET)
            {
                iQ_CMD_RESPONSE_S = comm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, array, array.Length);
            }
            else if (comm.ConnectMode == CONNECT_MODE.MODE_UART)
            {
                iQ_CMD_RESPONSE_S = comm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, array, array.Length);
            }
            if (iQ_CMD_RESPONSE_S.ResCode != 0)
            {
                result = "send ByPass: " + Text + " error!!";
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
        }
        return result;
    }

    public void SaveValueToXml(XmlElement itemElement)
    {
        string text = "";
        long[] dataValue = DataValue;
        foreach (long num in dataValue)
        {
            text = text + num + ",";
        }
        itemElement.InnerText = text.Remove(text.Length - 1);
    }

    public void ReadValueFromXml(XmlElement itemElement)
    {
        string[] array = itemElement.InnerText.Split(',');
        try
        {
            for (int i = 0; i < DataValue.Length; i++)
            {
                DataValue[i] = long.Parse(array[i]);
            }
        }
        catch (IndexOutOfRangeException)
        {
        }
    }

    public string ToText()
    {
        char[] array = new char[DataValue.Length];
        for (int i = 0; i < DataValue.Length; i++)
        {
            array[i] = (char)DataValue[i];
        }
        return new string(array);
    }
}
