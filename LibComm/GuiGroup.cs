using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SSCamIQTool.LibComm;

public class GuiGroup
{
    public string Name;

    public short ID;

    public int[] paramIndex;

    public List<GuiItem> ItemList;

    public PAGE_ACTION PageAction;

    public API_ACTION Action;

    public int[] autoMode;

    public bool InFile;

    public File_Mode FileMode;

    public int recvLengthForCaliSpeed;

    public string[] mv5log_str = new string[5000];

    public string mv5setlength_str = "";

    public int ValueSize
    {
        get
        {
            int num = 0;
            foreach (GuiItem item in ItemList)
            {
                num += item.ValueSize;
            }
            return num;
        }
    }

    public GuiGroup(string name, short id, PAGE_ACTION pgAction, API_ACTION action, int[] index, int[] mode, bool inFile, File_Mode fileMode)
    {
        Name = name;
        ID = id;
        PageAction = pgAction;
        Action = action;
        paramIndex = index;
        autoMode = mode;
        InFile = inFile;
        FileMode = fileMode;
        ItemList = new List<GuiItem>();
    }

    public string ReadGroup(IQComm comm)
    {
        byte[] pbyRcvData = null;
        byte[] bytes = GetBytes();
        string result = "";
        int num = 0;
        CONNECT_MODE connectMode = comm.ConnectMode;
        if (Action is API_ACTION.RW or API_ACTION.R)
        {
            switch (connectMode)
            {
                case CONNECT_MODE.MODE_USB:
                    num = comm.ReceiveUSBInitialApiPacket(ID, bytes, out pbyRcvData);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    num = comm.ReceiveInitialApiPacket(ID, bytes, out pbyRcvData);
                    break;
                case CONNECT_MODE.MODE_UART:
                    num = comm.ReceiveUartInitialApiPacket(ID, bytes, out pbyRcvData);
                    break;
            }
            recvLengthForCaliSpeed = num;
            result = num <= 0 ? "Api: " + Name + " receive packet fail!!" : UpdateGroup(pbyRcvData);
        }
        return result;
    }

    public string ReadNirIQGroup(IQComm comm)
    {
        byte[] pbyRcvData = null;
        byte[] bytes = GetBytes();
        string result = "";
        int num = 0;
        CONNECT_MODE connectMode = comm.ConnectMode;
        if (Action is API_ACTION.RW or API_ACTION.R)
        {
            switch (connectMode)
            {
                case CONNECT_MODE.MODE_USB:
                    num = comm.ReceiveUSBInitialApiPacketByType(CAMERA_CMD_TYPE.CAMERA_CMD_GET_NIR_API, ID, bytes, out pbyRcvData);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    num = comm.ReceiveInitialApiPacketByType(CAMERA_CMD_TYPE.CAMERA_CMD_GET_NIR_API, ID, bytes, out pbyRcvData);
                    break;
                case CONNECT_MODE.MODE_UART:
                    num = comm.ReceiveUartInitialApiPacketByType(CAMERA_CMD_TYPE.CAMERA_CMD_GET_NIR_API, ID, bytes, out pbyRcvData);
                    break;
            }
            recvLengthForCaliSpeed = num;
            result = num <= 0 ? "Api: " + Name + " receive packet fail!!" : UpdateGroup(pbyRcvData);
        }
        return result;
    }

    public string[] mv5log(IQComm comm)
    {
        return mv5log_str;
    }

    public string mv5lengthlog(IQComm comm)
    {
        return mv5setlength_str;
    }

    public string ReadUartGroup(IQComm comm)
    {
        string result = "";
        _ = GetBytes().Length;
        if (Action is API_ACTION.RW or API_ACTION.R)
        {
            result = comm.ReceiveUartApiPacket(ID, out byte[] pbyRcvData) <= 0 ? "Api: " + Name + " receive data fail!!" : UpdateGroup(pbyRcvData);
        }
        return result;
    }

    public string ReadGroupBySending(IQComm comm)
    {
        byte[] pbyRcvData = null;
        int num = 0;
        CONNECT_MODE connectMode = comm.ConnectMode;
        byte[] array = GetBytes();
        switch (connectMode)
        {
            case CONNECT_MODE.MODE_USB:
                num = comm.ReceiveUSBApiPacket(array, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = comm.ReceiveApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, array, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                num = comm.ReceiveUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, array, out pbyRcvData);
                break;
        }
        return num > 0 ? UpdateCaliPathGroup(pbyRcvData) : "Api: " + Name + " receive packet fail!!";
    }

    public string ReadByPass(IQComm comm)
    {
        byte[] pbyRcvData = null;
        short[] src = new short[1] { ID };
        short[] src2 = new short[1] { 1 };
        int[] src3 = new int[1];
        string result = "";
        int num = 0;
        int num2 = 0;
        CONNECT_MODE connectMode = comm.ConnectMode;
        byte[] array = new byte[12];
        Buffer.BlockCopy(src, 0, array, num2, 2);
        num2 += 2;
        Buffer.BlockCopy(src2, 0, array, num2, 2);
        num2 += 2;
        Buffer.BlockCopy(src3, 0, array, num2, 4);
        num2 += 4;
        foreach (GuiItem item in ItemList)
        {
            Buffer.BlockCopy(new int[1] { int.Parse(item.Paramters) }, 0, array, num2, 4);
            switch (connectMode)
            {
                case CONNECT_MODE.MODE_USB:
                    num = comm.ReceiveUSBApiPacket(array, out pbyRcvData);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    num = comm.ReceiveApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, array, out pbyRcvData);
                    break;
                case CONNECT_MODE.MODE_UART:
                    num = comm.ReceiveUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, array, out pbyRcvData);
                    break;
            }
            if (num > 0)
            {
                int num3 = BitConverter.ToInt32(pbyRcvData.Skip(8).ToArray(), 0);
                SetItemValue(item.Tag, new long[1] { num3 });
            }
            else
            {
                result = "Api: " + Name + " receive packet fail!!";
            }
        }
        return result;
    }

    public string UpdateCaliPathGroup(byte[] buffer)
    {
        int num2 = 4;
        string result = "";
        int num = BitConverter.ToUInt16(buffer, 2);
        if (num > 0)
        {
            int[] array = new int[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = BitConverter.ToInt32(buffer, num2 + (i * 4));
            }
            buffer = buffer.Skip(num2 + (num * 4)).ToArray();
            for (int j = 0; j < num; j++)
            {
                int count;
                int endIndex;
                if (j < num - 1)
                {
                    count = array[j + 1] - array[j];
                    endIndex = paramIndex[j + 1];
                }
                else
                {
                    count = buffer.Length - array[j];
                    endIndex = ItemList.Count;
                }
                byte[] paramBytes = buffer.Skip(array[j]).Take(count).ToArray();
                try
                {
                    ParseParamBytes(paramBytes, paramIndex[j], endIndex);
                }
                catch (Exception)
                {
                    result = "";
                }
            }
        }
        else
        {
            result = "parameter num error!!";
        }
        return result;
    }

    public string ReadCaliAWBGroup(IQComm comm, FILE_TRANSFER_ID type)
    {
        short apiId = (short)type;
        byte[] bytes = GetBytes();
        int num = comm.ReceiveCaliAWBApiPacket(apiId, out byte[] pbyRcvData);
        for (int i = 37; i < 52; i++)
        {
            bytes[i - 16] = pbyRcvData[i];
        }
        return num > 0 ? UpdateGroup(bytes) : "Api: " + Name + " receive packet fail!!";
    }

    public string ReadCaliDPCGroup(IQComm comm, FILE_TRANSFER_ID type)
    {
        short apiId = (short)type;
        byte[] bytes = GetBytes();
        int num = comm.ReceiveCaliDPCApiPacket(apiId, out byte[] pbyRcvData);
        for (int i = 53; i < 73; i++)
        {
            bytes[i - 20] = pbyRcvData[i];
        }
        return num > 0 ? UpdateGroup(bytes) : "Api: " + Name + " receive packet fail!!";
    }

    public string ReadGroup(IQComm comm, ref byte[] apiBuffer)
    {
        string result = "";
        if (Action is API_ACTION.RW or API_ACTION.R)
        {
            if (comm.ReceiveApiPacket(ID, out byte[] pbyRcvData) > 0)
            {
                apiBuffer = pbyRcvData;
                result = UpdateGroup(pbyRcvData);
            }
            else
            {
                result = "Api: " + Name + " receive data fail!!";
            }
        }
        return result;
    }

    public void StoreRollback(GuiGroup group)
    {
        if (!(Name == group.Name))
        {
            return;
        }
        for (int i = 0; i < ItemList.Count; i++)
        {
            for (int j = 0; j < ItemList[i].DataValue.Length; j++)
            {
                group.ItemList[i].DataValue[j] = ItemList[i].DataValue[j];
            }
        }
    }

    public void UpdateGroup(GuiGroup group)
    {
        if (!(Name == group.Name))
        {
            return;
        }
        for (int i = 0; i < ItemList.Count; i++)
        {
            for (int j = 0; j < ItemList[i].DataValue.Length; j++)
            {
                ItemList[i].DataValue[j] = group.ItemList[i].DataValue[j];
            }
        }
    }

    public void UpdateGroup(GuiGroup group, int CurrPageIndex, ActionList actionList)
    {
        if (!(Name == group.Name))
        {
            return;
        }
        for (int i = 0; i < ItemList.Count; i++)
        {
            for (int j = 0; j < ItemList[i].DataValue.Length; j++)
            {
                ItemList[i].DataValue[j] = group.ItemList[i].DataValue[j];
            }
            actionList.Add(new ActionStep(CurrPageIndex, isReset: false, ItemList[i].Tag, 0, ItemList[i].DataValue));
        }
    }

    public string UpdateGroup(byte[] buffer)
    {
        int num2 = 4;
        string result = "";
        int num = BitConverter.ToUInt16(buffer, 2);
        if (num > 0)
        {
            int[] array = new int[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = BitConverter.ToInt32(buffer, num2 + (i * 4));
            }
            buffer = buffer.Skip(num2 + (num * 4)).ToArray();
            for (int j = 0; j < num; j++)
            {
                int count;
                int endIndex;
                if (j < num - 1)
                {
                    count = array[j + 1] - array[j];
                    endIndex = paramIndex[j + 1];
                }
                else
                {
                    count = buffer.Length - array[j];
                    endIndex = ItemList.Count;
                }
                byte[] paramBytes = buffer.Skip(array[j]).Take(count).ToArray();
                try
                {
                    ParseParamBytes(paramBytes, paramIndex[j], endIndex);
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }
        }
        else
        {
            result = "parameter num error!!";
        }
        return result;
    }

    public string WriteNirIQGroup(IQComm comm)
    {
        string result = "";
        byte[] bytes = GetBytes();
        CONNECT_MODE connectMode = comm.ConnectMode;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        iQ_CMD_RESPONSE_S.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        iQ_CMD_RESPONSE_S.DataLen = 0;
        if (Action is API_ACTION.RW or API_ACTION.W)
        {
            switch (connectMode)
            {
                case CONNECT_MODE.MODE_USB:
                    iQ_CMD_RESPONSE_S = comm.SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_NIR_API, bytes, bytes.Length);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    iQ_CMD_RESPONSE_S = comm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_NIR_API, bytes, bytes.Length);
                    break;
                case CONNECT_MODE.MODE_UART:
                    iQ_CMD_RESPONSE_S = comm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_NIR_API, bytes, bytes.Length);
                    break;
            }
            if (iQ_CMD_RESPONSE_S.ResCode != 0)
            {
                result = "send Api: " + Name + " error!!";
            }
        }
        return result;
    }

    public string WriteGroup(IQComm comm)
    {
        string result = "";
        byte[] bytes = GetBytes();
        CONNECT_MODE connectMode = comm.ConnectMode;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        iQ_CMD_RESPONSE_S.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        iQ_CMD_RESPONSE_S.DataLen = 0;
        if (Action is API_ACTION.RW or API_ACTION.W)
        {
            switch (connectMode)
            {
                case CONNECT_MODE.MODE_USB:
                    iQ_CMD_RESPONSE_S = comm.SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, bytes, bytes.Length);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    iQ_CMD_RESPONSE_S = comm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, bytes, bytes.Length);
                    break;
                case CONNECT_MODE.MODE_UART:
                    iQ_CMD_RESPONSE_S = comm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, bytes, bytes.Length);
                    break;
            }
            if (iQ_CMD_RESPONSE_S.ResCode != 0)
            {
                result = "send Api: " + Name + " error!!";
            }
        }
        return result;
    }

    public string WriteUartGroup(IQComm comm)
    {
        byte[] bytes = GetBytes();
        if (Action is API_ACTION.RW or API_ACTION.W)
        {
            _ = comm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, bytes, bytes.Length);
            mv5setlength_str = bytes.Length.ToString();
        }
        return "";
    }

    public string WriteGroupByPageAction(IQComm comm)
    {
        string result = "";
        byte[] bytes = GetBytes();
        if ((PageAction == PAGE_ACTION.RW || PageAction == PAGE_ACTION.W) && (Action == API_ACTION.RW || Action == API_ACTION.W) && comm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, bytes, bytes.Length).ResCode != 0)
        {
            result = "send Api: " + Name + " error!!";
        }
        return result;
    }

    public byte[] GetBytes()
    {
        byte[] array = new byte[ValueSize];
        int[] array2 = new int[paramIndex.Length];
        short[] src = new short[1] { ID };
        short[] src2 = new short[1] { (short)paramIndex.Length };
        int num2 = 0;
        int num3 = 0;
        for (int i = 0; i < paramIndex.Length; i++)
        {
            array2[i] = num3;
            byte[] array3 = GetParamBytes(endIndex: i != paramIndex.Length - 1 ? paramIndex[i + 1] : ItemList.Count, startIndex: paramIndex[i]);
            Buffer.BlockCopy(array3, 0, array, num3, array3.Length);
            num3 += array3.Length;
        }
        byte[] array4 = new byte[4 + (paramIndex.Length * 4) + ValueSize];
        Buffer.BlockCopy(src, 0, array4, num2, 2);
        num2 += 2;
        Buffer.BlockCopy(src2, 0, array4, num2, 2);
        num2 += 2;
        for (int j = 0; j < paramIndex.Length; j++)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(array2[j]), 0, array4, num2, 4);
            num2 += 4;
        }
        Buffer.BlockCopy(array, 0, array4, num2, array.Length);
        return array4;
    }

    private void ParseParamBytes(byte[] paramBytes, int startIndex, int endIndex)
    {
        int offset = 0;
        if (autoMode.Length > 1 && autoMode[0] >= startIndex && autoMode[1] < endIndex)
        {
            if (autoMode.Length > 2)
            {
                ParseBytesByIndex(paramBytes, startIndex, autoMode[0], ref offset);
                ParseAutoModeBytesByIndex(paramBytes, autoMode[0], autoMode[1] + 1, ref offset);
                ParseBytesByIndex(paramBytes, autoMode[1] + 1, autoMode[2], ref offset);
                ParseAutoModeBytesByIndex(paramBytes, autoMode[2], autoMode[3] + 1, ref offset);
                ParseBytesByIndex(paramBytes, autoMode[3] + 1, endIndex, ref offset);
            }
            else
            {
                ParseBytesByIndex(paramBytes, startIndex, autoMode[0], ref offset);
                ParseAutoModeBytesByIndex(paramBytes, autoMode[0], autoMode[1] + 1, ref offset);
                ParseBytesByIndex(paramBytes, autoMode[1] + 1, endIndex, ref offset);
            }
        }
        else
        {
            ParseBytesByIndex(paramBytes, startIndex, endIndex, ref offset);
        }
    }

    public void ParseBytesByIndex(byte[] buffer, int start, int end, ref int offset)
    {
        for (int i = start; i < end; i++)
        {
            byte[] buffer2 = buffer.Skip(offset).Take(ItemList[i].ValueSize).ToArray();
            ItemList[i].Update(buffer2);
            offset += ItemList[i].ValueSize;
        }
    }

    public void ParseAutoModeBytesByIndex(byte[] buffer, int start, int end, ref int offset)
    {
        int ySize = ItemList[start].YSize;
        for (int i = 0; i < ySize; i++)
        {
            for (int j = start; j < end; j++)
            {
                int num = ItemList[j].ItemSize * ItemList[j].XSize;
                byte[] buffer2 = buffer.Skip(offset).Take(num).ToArray();
                ItemList[j].UpdateXSize(buffer2, i);
                offset += num;
            }
        }
    }

    private byte[] GetParamBytes(int startIndex, int endIndex)
    {
        int offset = 0;
        int num = 0;
        for (int i = startIndex; i < endIndex; i++)
        {
            num += ItemList[i].ValueSize;
        }
        byte[] array = new byte[num];
        if (autoMode.Length > 1 && autoMode[0] >= startIndex && autoMode[1] < endIndex)
        {
            if (autoMode.Length > 2)
            {
                GetBytesByIndex(array, startIndex, autoMode[0], ref offset);
                GetAutoModeBytesByIndex(array, autoMode[0], autoMode[1] + 1, ref offset);
                GetBytesByIndex(array, autoMode[1] + 1, autoMode[2], ref offset);
                GetAutoModeBytesByIndex(array, autoMode[2], autoMode[3] + 1, ref offset);
                GetBytesByIndex(array, autoMode[3] + 1, endIndex, ref offset);
            }
            else
            {
                GetBytesByIndex(array, startIndex, autoMode[0], ref offset);
                GetAutoModeBytesByIndex(array, autoMode[0], autoMode[1] + 1, ref offset);
                GetBytesByIndex(array, autoMode[1] + 1, endIndex, ref offset);
            }
        }
        else
        {
            GetBytesByIndex(array, startIndex, endIndex, ref offset);
        }
        return array;
    }

    public void GetBytesByIndex(byte[] buffer, int start, int end, ref int offset)
    {
        for (int i = start; i < end; i++)
        {
            ItemList[i].GetBytes().CopyTo(buffer, offset);
            offset += ItemList[i].ValueSize;
        }
    }

    public void GetAutoModeBytesByIndex(byte[] buffer, int start, int end, ref int offset)
    {
        int ySize = ItemList[start].YSize;
        for (int i = 0; i < ySize; i++)
        {
            for (int j = start; j < end; j++)
            {
                byte[] xSizeBytes = ItemList[j].GetXSizeBytes(i);
                xSizeBytes.CopyTo(buffer, offset);
                offset += xSizeBytes.Length;
            }
        }
    }

    public GuiItem FindItemByName(string name)
    {
        foreach (GuiItem item in ItemList)
        {
            if (item.Tag == name)
            {
                return item;
            }
        }
        return null;
    }

    public void SetItemValue(string tag, long[] data)
    {
        FindItemByName(tag).DataValue = data;
    }

    public void SaveValueToXml(XmlNode xmlNode)
    {
        foreach (GuiItem item in ItemList)
        {
            foreach (XmlElement item2 in xmlNode)
            {
                if (item.Tag == item2.GetAttribute("Name"))
                {
                    item.SaveValueToXml(item2);
                    break;
                }
            }
        }
    }

    public byte[] GetBinBytes(uint magicKey)
    {
        byte[] bytes = GetBytes();
        byte[] array = new byte[12 + bytes.Length];
        BitConverter.GetBytes(magicKey).CopyTo(array, 0);
        BitConverter.GetBytes(5).CopyTo(array, 4);
        BitConverter.GetBytes(bytes.Length).CopyTo(array, 8);
        bytes.CopyTo(array, 12);
        return array;
    }

    public byte[] GetBinBytesByType(CAMERA_CMD_TYPE type, uint magicKey)
    {
        byte[] bytes = GetBytes();
        byte[] array = new byte[12 + bytes.Length];
        BitConverter.GetBytes(magicKey).CopyTo(array, 0);
        BitConverter.GetBytes((int)type).CopyTo(array, 4);
        BitConverter.GetBytes(bytes.Length).CopyTo(array, 8);
        bytes.CopyTo(array, 12);
        return array;
    }

    public byte[] GetBinBytes(uint magicKey, uint videomagicKey)
    {
        byte[] bytes = GetBytes();
        byte[] array = new byte[12 + bytes.Length];
        BitConverter.GetBytes(magicKey).CopyTo(array, 0);
        ushort[] array2 = new ushort[2] { 4132, 4133 };
        ushort num = (ushort)((bytes[1] << 8) | bytes[0]);
        for (int i = 0; i < array2.Length; i++)
        {
            if (num == array2[i])
            {
                BitConverter.GetBytes(videomagicKey).CopyTo(array, 0);
            }
        }
        BitConverter.GetBytes(5).CopyTo(array, 4);
        BitConverter.GetBytes(bytes.Length).CopyTo(array, 8);
        bytes.CopyTo(array, 12);
        return array;
    }
}
