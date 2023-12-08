using System;
using System.Collections.Generic;
using System.Xml;

namespace SSCamIQTool.LibComm;

public class GuiPage
{
    public string Name;

    public string PageType;

    public int GroupIndex;

    public List<GuiGroup> GroupList;

    public bool bRead;

    public PAGE_ACTION Action;

    public bool AutoWrite;

    public int recvLengthForCaliSpeed;

    public string[] mv5log;

    public string mv5setlength = "0";

    public GuiPage(string name, string type, int gIndex, PAGE_ACTION action, bool aw)
    {
        Name = name;
        PageType = type;
        GroupIndex = gIndex;
        GroupList = new List<GuiGroup>();
        bRead = false;
        Action = action;
        AutoWrite = aw;
    }

    public string ReadPage(IQComm comm)
    {
        string text = "";
        recvLengthForCaliSpeed = 0;
        if (Action is PAGE_ACTION.R or PAGE_ACTION.RW)
        {
            if (GroupIndex == -1)
            {
                for (int i = 0; i < GroupList.Count; i++)
                {
                    string text2 = GroupList[i].Name is "CaliDBPath" or "CaliPath" ? GroupList[i].ReadGroupBySending(comm) : GroupList[i].Name == "BypassItem" ? GroupList[i].ReadByPass(comm) : !GroupList[i].Name.StartsWith("NIR_IQ") ? GroupList[i].ReadGroup(comm) : GroupList[i].ReadNirIQGroup(comm);
                    recvLengthForCaliSpeed += GroupList[i].recvLengthForCaliSpeed;
                    if (text2 != "")
                    {
                        text = text + text2 + Environment.NewLine;
                    }
                }
            }
            else
            {
                text = ReadUnionGroup(comm);
            }
        }
        return text;
    }

    public string[] getmv5log(IQComm comm)
    {
        return mv5log;
    }

    public string getmv5length(IQComm comm)
    {
        return mv5setlength;
    }

    public string ReadUartPage(IQComm comm)
    {
        string text = "";
        recvLengthForCaliSpeed = 0;
        if (Action is PAGE_ACTION.R or PAGE_ACTION.RW)
        {
            if (GroupIndex == -1)
            {
                for (int i = 0; i < GroupList.Count; i++)
                {
                    string text2 = GroupList[i].Name is "CaliDBPath" or "CaliPath" ? GroupList[i].ReadGroupBySending(comm) : !(GroupList[i].Name == "BypassItem") ? GroupList[i].ReadUartGroup(comm) : GroupList[i].ReadByPass(comm);
                    recvLengthForCaliSpeed += GroupList[i].recvLengthForCaliSpeed;
                    if (text2 != "")
                    {
                        text = text + text2 + Environment.NewLine;
                    }
                }
            }
            else
            {
                text = ReadUnionGroup(comm);
            }
        }
        return text;
    }

    public string ReadUnionGroup(IQComm comm)
    {
        byte[] apiBuffer = null;
        string text = GroupList[0].ReadGroup(comm, ref apiBuffer);
        recvLengthForCaliSpeed += GroupList[0].recvLengthForCaliSpeed;
        foreach (GuiItem item in GroupList[0].ItemList)
        {
            if (item.GuiType != "")
            {
                GroupIndex = (int)item.DataValue[0];
                break;
            }
        }
        if (text == "" && apiBuffer != null)
        {
            text = GroupList[GroupIndex].UpdateGroup(apiBuffer);
        }
        return text;
    }

    public void UpdatePage(GuiPage page)
    {
        if (Name == page.Name)
        {
            for (int i = 0; i < GroupList.Count; i++)
            {
                GroupList[i].UpdateGroup(page.GroupList[i]);
            }
        }
    }

    public string WritePage(IQComm comm)
    {
        string text = "";
        string text2 = "";
        if (Action is PAGE_ACTION.RW or PAGE_ACTION.W)
        {
            if (GroupIndex == -1)
            {
                for (int i = 0; i < GroupList.Count; i++)
                {
                    if (GroupList[i].Action == API_ACTION.RW)
                    {
                        text2 = !GroupList[i].Name.StartsWith("NIR_IQ") ? GroupList[i].WriteGroup(comm) : GroupList[i].WriteNirIQGroup(comm);
                    }
                    if (text2 != "")
                    {
                        text = text + text2 + Environment.NewLine;
                    }
                }
            }
            else if (GroupList[0].Action == API_ACTION.RW)
            {
                text = GroupList[GroupIndex].WriteGroup(comm);
            }
        }
        return text;
    }

    public string WriteUartPage(IQComm comm)
    {
        string text = "";
        string text2 = "";
        if (Action is PAGE_ACTION.RW or PAGE_ACTION.W)
        {
            if (GroupIndex == -1)
            {
                for (int i = 0; i < GroupList.Count; i++)
                {
                    if (GroupList[i].Action == API_ACTION.RW)
                    {
                        text2 = GroupList[i].WriteUartGroup(comm);
                        mv5setlength = GroupList[i].mv5lengthlog(comm);
                    }
                    if (text2 != "")
                    {
                        text = text + text2 + Environment.NewLine;
                    }
                }
            }
            else if (GroupList[0].Action == API_ACTION.RW)
            {
                text = GroupList[GroupIndex].WriteUartGroup(comm);
                mv5setlength = GroupList[GroupIndex].mv5lengthlog(comm);
            }
        }
        return text;
    }

    public GuiGroup FindGroupByName(string name)
    {
        foreach (GuiGroup group in GroupList)
        {
            if (group.Name == name)
            {
                return group;
            }
        }
        return null;
    }

    public void SaveValueToXml(XmlDocument xmlDoc)
    {
        foreach (GuiGroup group in GroupList)
        {
            string xpath = "/ISP_ITEM/PAGE/GROUP[@Id = '" + group.ID + "']";
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes(xpath);
            if (xmlNodeList.Count == 0)
            {
                xpath = "/API_XML/ISP_ITEM/PAGE/GROUP[@Id = '" + group.ID + "']";
                xmlNodeList = xmlDoc.SelectNodes(xpath);
            }
            foreach (XmlNode item in xmlNodeList)
            {
                group.SaveValueToXml(item);
            }
        }
    }

    public byte[] GetBinBytesByType(CAMERA_CMD_TYPE type, uint magicKey)
    {
        int num = 0;
        int num2 = 0;
        byte[][] array = new byte[GroupList.Count][];
        for (int i = 0; i < GroupList.Count; i++)
        {
            if (GroupList[i].InFile)
            {
                array[i] = GroupList[i].GetBinBytesByType(type, magicKey);
                num += array[i].Length;
            }
        }
        byte[] array2 = new byte[num];
        for (int j = 0; j < GroupList.Count; j++)
        {
            if (GroupList[j].InFile)
            {
                array[j].CopyTo(array2, num2);
                num2 += array[j].Length;
            }
        }
        return array2;
    }

    public byte[] GetBinBytes(uint magicKey)
    {
        int num = 0;
        int num2 = 0;
        byte[][] array = new byte[GroupList.Count][];
        for (int i = 0; i < GroupList.Count; i++)
        {
            if (GroupList[i].InFile)
            {
                array[i] = GroupList[i].GetBinBytes(magicKey);
                num += array[i].Length;
            }
        }
        byte[] array2 = new byte[num];
        for (int j = 0; j < GroupList.Count; j++)
        {
            if (GroupList[j].InFile)
            {
                array[j].CopyTo(array2, num2);
                num2 += array[j].Length;
            }
        }
        return array2;
    }

    public byte[] GetBinBytes(uint magicKey, uint videomagicKey)
    {
        int num = 0;
        int num2 = 0;
        byte[][] array = new byte[GroupList.Count][];
        for (int i = 0; i < GroupList.Count; i++)
        {
            if (GroupList[i].InFile)
            {
                array[i] = GroupList[i].GetBinBytes(magicKey, videomagicKey);
                num += array[i].Length;
            }
        }
        byte[] array2 = new byte[num];
        for (int j = 0; j < GroupList.Count; j++)
        {
            if (GroupList[j].InFile)
            {
                array[j].CopyTo(array2, num2);
                num2 += array[j].Length;
            }
        }
        return array2;
    }
}
