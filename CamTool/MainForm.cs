using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Xml;

namespace SSCamIQTool.CamTool;

public class MainForm : Form
{
    public class CALLBACK_DATA_STRUCT
    {
        public double nReceiveByte;

        public IQComm lpIQcomm;

        public Control control;

        public Stopwatch sw;
    }

    public delegate uint callBackHandler(ushort precent, CALLBACK_DATA_STRUCT lpCallbackData);

    private delegate void UpdateDisconnectHandler();

    private delegate void UpdateConnectFinishHandler(string strMsg);

    private delegate void ChangeApiVersionHandler(ChipID Id, ApiVersion version);

    private delegate void SuccessGetXmlHander();

    private delegate void FailGetXmlHander(string msg);

    private delegate void ShowMessageHandler(string strMsg);

    private static readonly GuiParser m_GuiContainer = new();

    private readonly IQComm m_BoardComm = new();

    private readonly IQUart m_BoardUart = new();

    private readonly int m_GuiBorderWidth = Settings.Default.GuiBorderWidth;

    private readonly int m_GuiRwButtonSpace = Settings.Default.GuiRWButtonSpace;

    private ApiVersion apiVersion;

    private ChipID m_chipID = ChipID.I3;

    private string chipname;

    private ushort channelID;

    private ushort sensorPadID;

    private ushort DeviceID;

    private bool m_bInitStatus;

    private byte IQIndex;

    private uint magicKey = 1234u;

    private readonly ushort m_BinChecksumVer = 1;

    private readonly uint videomagicKey = 2036624993u;

    private uint SaveBinFile_Interval = 5u;

    private uint SaveBinFile_Interval_sec;

    private List<string> m_PluginList = new();

    private FormLiveView m_fmLiveView;

    private readonly ConnectSetting connSetting = new();

    private readonly SensorInfo sensorInfo = new();

    private ImageCapture imageCapture;

    private ushort EvbAPIVerMajor;

    private ushort EvbAPIVerMinor;

    private bool IsLoadBinXml;

    private string loadbinxml;

    private bool m_bSaveBinXml = true;

    private System.Timers.Timer mTimersTimer;

    private string Auto_DUMP_BIN_FILENAME;

    private const uint persec_ms = 1000u;

    private const uint permin_s = 60u;

    private uint m_RawCount = 20u;

    private uint m_RawLoopCount = 1u;

    private int m_SclDevice;

    private int m_SclChannel;

    private int m_SclPort;

    private IQConfig m_json_config = new();

    private readonly Dictionary<ulong, string> m_chipdef = new();

    private readonly IContainer components = null;

    private MenuStrip menuStripMain;

    private ToolStripMenuItem fileToolStripMenuItem;

    private ToolStripMenuItem viewToolStripMenuItem;

    private TextBox txbMsgLog;

    private StatusStrip statusStripMain;

    private ToolStripStatusLabel statusLabelInfo;

    private ToolStripStatusLabel statusLabelConnection;

    private SplitContainer splitConItemPage;

    private TreeView treePageItem;

    private Button btnPageItemCollapse;

    private SplitContainer splitConItemMain;

    private ToolStrip toolStripMain;

    private ToolStripButton toolBtnConnect;

    private ToolStripLabel toolLabelClientHostName;

    private ToolStripTextBox toolTxbClientHostName;

    private Button btnPageItemExpand;

    private Panel panelMainContent;

    private Panel panelMainTitle;

    private Label labelMainTitle;

    private Panel panelLogAction;

    private ToolStripButton toolBtnReadAll;

    private ToolStripButton toolBtnWriteAll;

    private ToolStripComboBox toolCbxPlugIn;

    private ToolStripButton toolBtnGetRawImage;

    private ToolStripButton toolBtnGetIspYuv;

    private ToolStripButton toolBtnGetJpeg;

    private ToolStripButton toolGetScalarOutImage;

    private ToolStripComboBox toolCbxApiVersion;

    private Button btnReadPage;

    private ToolStripLabel toolLabelPort;

    private ToolStripTextBox toolTxbClientPort;

    private ToolStripLabel toolLabelID;

    private ToolStripTextBox toolTxbClientID;

    private ToolStripButton toolBtnStepBack;

    private ToolStripButton toolBtnStepNext;

    private CheckBox chkAutoWrite;

    private Button btnWritePage;

    private ToolStripMenuItem settingToolStripMenuItem;

    private ToolStripMenuItem helpToolStripMenuItem;

    private ToolStripMenuItem setMagicKeyToolStripMenuItem;

    private ToolStripMenuItem aboutToolStripMenuItem;

    private ToolStripMenuItem newFileToolStripMenuItem;

    private ToolStripMenuItem newParamFileToolStripMenuItem;

    private ToolStripMenuItem openFileToolStripMenuItem;

    private ToolStripMenuItem openParamFileToolStripMenuItem;

    private ToolStripMenuItem openBinFileToolStripMenuItem;

    private ToolStripMenuItem saveFileToolStripMenuItem;

    private ToolStripMenuItem saveParamFileToolStripMenuItem;

    private ToolStripMenuItem saveBinFileToolStripMenuItem;

    private ToolStripMenuItem liveViewToolStripMenuItem;

    private Button buttonIQIndex;

    private TextBox textBoxIQIndex;

    private Label labelIQIndex;

    private ToolStripMenuItem openBinXmlFileToolStripMenuItem;

    private ToolStripMenuItem convertToolStripMenuItem;

    private Button btnUartConn;

    private Label labelUart;

    private TextBox textBoxUartPort;

    private ToolStripMenuItem uARTLogToolStripMenuItem;

    private ToolStripMenuItem xUCommandToolStripMenuItem;

    private ToolStripMenuItem burnFWToolStripMenuItem;

    private ToolStripMenuItem sendIQDataToolStripMenuItem;

    private ToolStripMenuItem getIQDataToolStripMenuItem;

    private ToolStripMenuItem saveBinXmlFileToolStripMenuItem;

    private ToolStripLabel toolLabelChannelID;

    private ToolStripComboBox ChannelIDComboBox;

    private ToolStripButton toolBtnGetRawStream;

    private ToolStripMenuItem autoSaveBinFileToolStripMenuItem;

    private ToolStripStatusLabel StatuslabelTimer;

    private ToolStripLabel toolLabelSensorPadID;

    private ToolStripComboBox SensorPadIDComboBox;

    private ToolStripLabel toolStripLabel1;

    private ToolStripComboBox DeviceIDComboBox;

    private Button button_Two;

    private Button button_One;

    [DllImport("AitGUI.dll")]
    private static extern int ShowVideoDevDlg(byte[] msg, int buf_len);

    [DllImport("AitUVCExtApi.dll")]
    private static extern unsafe int AITAPI_OpenDeviceByPath(byte[] msg, IntPtr* pDevHandle);

    [DllImport("AitUVCExtApi.dll")]
    private static extern int AITAPI_GetFWVersion(IntPtr pDevHandle, byte[] msg);

    [DllImport("AitUVCExtApi.dll", EntryPoint = "AITAPI_UpdateFW_842x")]
    private static extern unsafe int AITAPI_UpdateFW(IntPtr pDevHandle, byte[] data, int len, void* ProgressCB, void* callbackData, byte mode);

    [DllImport("AitUVCExtApi.dll")]
    private static extern unsafe int AITAPI_SendData(IntPtr pDevHandle, byte[] data, int len, void* ProgressCB, void* callbackData, byte StorageType, byte PackageType);

    [DllImport("AitUVCExtApi.dll")]
    private static extern int AITAPI_ReadPartialFlashData(IntPtr pDevHandle, uint FlashAddr, uint Len, byte[] buf, ushort[] byteRet, callBackHandler callBackFunc, CALLBACK_DATA_STRUCT lpCallBackData);

    public MainForm()
    {
        InitializeComponent();
    }

    private unsafe void burnFWToolStripMenuItem_Click(object sender, EventArgs e)
    {
        IntPtr in_ait_handel = (IntPtr)(void*)null;
        byte[] msg = new byte[32767];
        _ = new byte[260];
        _ = new byte[100];
        _ = ShowVideoDevDlg(msg, 260);
        if (AITAPI_OpenDeviceByPath(msg, &in_ait_handel) != 0)
        {
            _ = MessageBox.Show("Open device failed!");
        }
        _ = ToBurnFW(in_ait_handel) != 0 ? MessageBox.Show("FW update error!") : MessageBox.Show("FW Update OK!");
    }

    private unsafe void sendIQDataToolStripMenuItem_Click(object sender, EventArgs e)
    {
        IntPtr in_ait_handel = (IntPtr)(void*)null;
        byte[] msg = new byte[1024];
        _ = new byte[1024].Length;
        _ = ShowVideoDevDlg(msg, 260);
        if (AITAPI_OpenDeviceByPath(msg, &in_ait_handel) != 0)
        {
            _ = MessageBox.Show("Open device failed!");
        }
        byte[] array = File.ReadAllBytes(btOpenFwFile_Click());
        int num = array.Length;
        _ = Send_IQData(in_ait_handel, array, num) != 0 ? MessageBox.Show("Send IQ Data error!") : MessageBox.Show("Send IQ Data OK!");
    }

    private unsafe void getIQDataToolStripMenuItem_Click(object sender, EventArgs e)
    {
        IntPtr in_ait_handel = (IntPtr)(void*)null;
        byte[] msg = new byte[1024];
        _ = new byte[1000];
        ushort cmdData_Size = 1000;
        byte[] array2 = new byte[2000];
        ushort out_IQ_Size = 2000;
        _ = ShowVideoDevDlg(msg, 260);
        if (AITAPI_OpenDeviceByPath(msg, &in_ait_handel) != 0)
        {
            _ = MessageBox.Show("Open device failed!");
        }
        byte[] array = BitConverter.GetBytes(4103);
        if (Get_IQData(in_ait_handel, cmdData_Size, array, out_IQ_Size, array2) != 0)
        {
            _ = MessageBox.Show("Get IQ Data error!");
            return;
        }
        using (FileStream output = File.Create("Out_IQ_Data.dat"))
        {
            new BinaryWriter(output).Write(array2);
        }
        _ = MessageBox.Show("Get IQ Data OK!");
    }

    public unsafe int Send_IQData(IntPtr in_ait_handel, byte[] Data, int DataSize)
    {
        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
        if (MessageBox.Show("Send IQ Data?", "Check", buttons) == DialogResult.Yes)
        {
            Console.WriteLine("[Send_IQData] - IQ Data Size = {0}\n", DataSize);
            return AITAPI_SendData(in_ait_handel, Data, DataSize, null, null, 3, 6);
        }
        return 0;
    }

    public unsafe int Get_IQData(IntPtr in_ait_handel, ushort CmdData_Size, byte[] CmdData_Data, ushort Out_IQ_Size, byte[] Out_IQData)
    {
        ushort[] byteRet = new ushort[1];
        ushort num2 = ushort.MaxValue;
        if (CmdData_Size > num2 || Out_IQ_Size > num2)
        {
            return -1;
        }
        int num = AITAPI_SendData(in_ait_handel, CmdData_Data, CmdData_Size, null, null, 3, 7);
        return num != 0 ? num : AITAPI_ReadPartialFlashData(in_ait_handel, 0u, Out_IQ_Size, Out_IQData, byteRet, null, null);
    }

    public unsafe int ToBurnFW(IntPtr in_ait_handel)
    {
        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
        string text = btOpenFwFile_Click();
        if (MessageBox.Show(text.ToString(), "Burn Fw?", buttons) == DialogResult.Yes)
        {
            byte[] array = File.ReadAllBytes(text);
            return AITAPI_UpdateFW(in_ait_handel, array, array.Length, null, null, 0);
        }
        return 0;
    }

    private string btOpenFwFile_Click()
    {
        OpenFileDialog openFileDialog = new()
        {
            Title = "Select FW bin file",
            InitialDirectory = "./",
            Filter = "bin files (*.*)|*.bin"
        };
        return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : null;
    }

    private void StringToByte(string inString, byte[] OutByte)
    {
        byte[] bytes = Encoding.Default.GetBytes(inString);
        int length = inString.Length;
        int num = 0;
        for (int i = 0; i < length; i++)
        {
            OutByte[num] = bytes[i];
            num++;
            OutByte[num] = 0;
            num++;
        }
    }

    private void ChangeStringFromDLL(byte[] inString, byte[] OutString, int MaxSize)
    {
        bool flag = false;
        int num = 0;
        for (int i = 0; i < MaxSize; i++)
        {
            if (inString[i] == 0)
            {
                if (flag)
                {
                    OutString[num] = inString[i];
                    break;
                }
                flag = true;
            }
            else
            {
                OutString[num] = inString[i];
                flag = false;
                num++;
            }
        }
    }

    private T JsonfileObject<T>(string path)
    {
        try
        {
            return (T)Deserialize(typeof(T), File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return default;
        }
    }

    public object Deserialize(Type type, string json)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
        DataContractJsonSerializer serializer = new(type);
        return serializer.ReadObject(stream);
    }

    private int LoadConfig()
    {
        string text = Application.StartupPath + "/config.json";
        if (!File.Exists(text))
        {
            _ = MessageBox.Show("Config not found: " + text);
            return -1;
        }
        m_json_config = JsonfileObject<IQConfig>(text);
        Chiplist[] chipList = m_json_config.ChipList;
        foreach (Chiplist chiplist in chipList)
        {
            m_chipdef.Add(chiplist.id, chiplist.name);
        }
        m_BoardComm.m_config = m_json_config;
        m_BoardComm.m_chiplist = m_chipdef;
        return 0;
    }

    private string GetApiIDXmlName(ChipID chipId)
    {
        string text = "";
        return string.IsNullOrEmpty(m_chipdef[(ulong)chipId]) ? text + "isp_api.xml" : text + m_chipdef[(ulong)chipId] + "_isp_api.xml";
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        toolTxbClientHostName.Text = Settings.Default.ClientHostName;
        toolTxbClientPort.Text = Settings.Default.ClientPort.ToString();
        btnPageItemExpand.Enabled = false;
        btnPageItemExpand.Visible = false;
        toolBtnGetRawStream.Enabled = false;
        toolBtnGetRawStream.Visible = false;
        m_GuiContainer.SetLayout(treePageItem, panelMainContent, labelMainTitle, statusLabelInfo);
        m_GuiContainer.SetDebugLog(txbMsgLog);
        m_GuiContainer.SetDebugParser(m_BoardComm);
        m_GuiContainer.SetCheckConnectionFunc(CheckConnection);
        m_GuiContainer.SetPageGuiMode(SetPageActionMode, SetAutoWriteMode);
        m_GuiContainer.SetUndoRedoMode(SetBackStepMode, SetNextStepMode);
        chkAutoWrite.Checked = m_GuiContainer.AutoWrite;
        GuiPageItemCollapse(bCollapse: false);
        Text = "SigmaStar IPCam IQ Tool v" + Settings.Default.ApVersion;
        ChannelIDComboBox.SelectedIndex = channelID;
        SensorPadIDComboBox.SelectedIndex = sensorPadID;
        DeviceIDComboBox.SelectedIndex = DeviceID;
        ListPluginFile();
        _ = LoadConfig();
        imageCapture = new ImageCapture(m_BoardComm, sensorInfo, this);
        m_GuiContainer.isFWAPIParmChange = false;
        IsLoadBinXml = false;
        string path = Application.StartupPath + "/CvtXml";
        if (!Directory.Exists(path))
        {
            _ = Directory.CreateDirectory(path);
        }
        m_GuiContainer.chooseProduct(out string product_style);
        mTimersTimer = new System.Timers.Timer();
        mTimersTimer.Elapsed += _TimersTimer_Elapsed;
        mTimersTimer.SynchronizingObject = this;
        switch (product_style)
        {
            case "Ethernet Protocol":
                m_BoardComm.ConnectMode = CONNECT_MODE.MODE_SOCKET;
                m_BoardComm.ChipIDName = m_chipID;
                btnUartConn.Visible = false;
                labelUart.Visible = false;
                textBoxUartPort.Visible = false;
                uARTLogToolStripMenuItem.Visible = false;
                break;
            case "USB Protocol":
                m_BoardComm.ConnectMode = CONNECT_MODE.MODE_USB;
                m_BoardComm.ChipIDName = m_chipID;
                toolBtnConnect.Visible = false;
                toolLabelClientHostName.Visible = false;
                toolTxbClientHostName.Visible = false;
                toolLabelPort.Visible = false;
                toolTxbClientPort.Visible = false;
                toolLabelID.Visible = false;
                toolTxbClientID.Visible = false;
                btnUartConn.Location = new Point(toolCbxApiVersion.Bounds.X + toolCbxApiVersion.Bounds.Width + 4, menuStripMain.Height + ((toolStripMain.Height - btnUartConn.Height) / 2));
                break;
            case "UART Protocol":
                m_BoardComm.ConnectMode = CONNECT_MODE.MODE_UART;
                m_BoardComm.ChipIDName = m_chipID;
                toolBtnConnect.Visible = false;
                toolLabelClientHostName.Visible = false;
                toolTxbClientHostName.Visible = false;
                toolLabelPort.Visible = false;
                toolTxbClientPort.Visible = false;
                toolLabelID.Visible = false;
                toolTxbClientID.Visible = false;
                btnUartConn.Location = new Point(toolCbxApiVersion.Bounds.X + toolCbxApiVersion.Bounds.Width + 4, menuStripMain.Height + ((toolStripMain.Height - btnUartConn.Height) / 2));
                break;
        }
    }

    private string ClientConnect(string clientHostName, int Port)
    {
        string text = m_BoardComm.Connect(clientHostName, Port, Settings.Default.ConnectionTimeout);
        if (!text.Equals(""))
        {
            text = Settings.Default.ErrConnectionFail;
        }
        return text;
    }

    private void ClientDisconnect()
    {
        btnUartConn.Enabled = true;
        statusLabelConnection.Text = "Client Port Status - Disconnect";
        toolBtnConnect.Image = ExtensionMethods.GetImageByName("connect.png");
        toolBtnConnect.Text = "Connect";
        connSetting.Clear();
        m_BoardComm.disconnectUartPage();
        m_BoardComm.Disconnect();
        m_BoardUart.DisConnected();
    }

    public void MsgLog(string strMsg)
    {
        txbMsgLog.Text = strMsg + Environment.NewLine + txbMsgLog.Text;
    }

    public void CheckConnection()
    {
        if (!m_BoardComm.IsConnected())
        {
            _ = Invoke(new UpdateDisconnectHandler(ClientDisconnect));
        }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        ClientDisconnect();
        Settings.Default.Save();
    }

    private void SetCommonConfig()
    {
        channelID = (ushort)ChannelIDComboBox.SelectedIndex;
        SetChannelIDByAPI(101);
        sensorPadID = (ushort)SensorPadIDComboBox.SelectedIndex;
        _ = SetSensorPadIDByAPI(sensorPadID);
        DeviceID = (ushort)DeviceIDComboBox.SelectedIndex;
        SetDeviceIDByAPI(104);
    }

    private void ConnectToBoardFinish(string strMsg)
    {
        if (!m_bInitStatus)
        {
            m_bInitStatus = true;
        }
        SetUartStart();
        GetChipIDByAPI(100);
        SetCommonConfig();
        GetChannelIDByAPI(101);
        GeSensorPadIDByAPI(36);
        GetDeviceIDByAPI(104);
        if (m_BoardComm.ChipIsSupport(m_chipID) != 1)
        {
            _ = Invoke(new ShowMessageHandler(GUIShowMessage), "Cannot find chip id by API. Load default isp_api.xml file\n");
            GetChipIDByXml(Application.StartupPath + "/isp_api.xml");
        }
        m_BoardComm.ChipIDName = m_chipID;
        if (!strMsg.Equals(""))
        {
            btnUartConn.Enabled = true;
            Function chipFunction = new();
            _ = GetChipFunction(m_chipID, m_BoardComm.ConnectMode, ref chipFunction);
            toolBtnGetRawImage.Enabled = chipFunction.GetRaw == 1;
            toolBtnGetRawStream.Enabled = chipFunction.GetRawStream == 1;
            toolBtnGetRawStream.Visible = chipFunction.GetRawStream == 1;
            toolBtnGetIspYuv.Enabled = chipFunction.GetYuv == 1;
            toolGetScalarOutImage.Enabled = chipFunction.GetScl == 1;
            toolBtnGetJpeg.Enabled = chipFunction.GetJpeg == 1;
            if (m_chipID is not ChipID.M5 and not ChipID.M5U and not ChipID.I2 and not ChipID.I6)
            {
                GetSensorInfo(m_BoardComm.GetApiIDValue(m_chipID, "QuerySensorInfo"));
            }
            GetApiVersionForUSBMode();
            if (m_BoardComm.ConnectMode == CONNECT_MODE.MODE_UART)
            {
                ChangeUartXmlType(m_chipID, apiVersion);
            }
            else
            {
                ChangeXmlType(m_chipID, apiVersion);
            }
            if (MessageBox.Show("Do you want to read all parameters?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                m_GuiContainer.ReadAllPage();
            }
        }
        else
        {
            btnUartConn.Enabled = true;
            toolBtnConnect.Image = ExtensionMethods.GetImageByName("disconnect.png");
            toolBtnConnect.Text = "Disconnect";
            statusLabelConnection.Text = "Client Port Status - Connect";
            Settings.Default.ClientHostName = connSetting.HostName;
            Settings.Default.ClientPort = connSetting.Port;
            Update();
            Function chipFunction2 = new();
            _ = GetChipFunction(m_chipID, m_BoardComm.ConnectMode, ref chipFunction2);
            toolBtnGetRawImage.Enabled = chipFunction2.GetRaw == 1;
            toolBtnGetRawStream.Enabled = chipFunction2.GetRawStream == 1;
            toolBtnGetRawStream.Visible = chipFunction2.GetRawStream == 1;
            toolBtnGetIspYuv.Enabled = chipFunction2.GetYuv == 1;
            toolGetScalarOutImage.Enabled = chipFunction2.GetScl == 1;
            toolBtnGetJpeg.Enabled = chipFunction2.GetJpeg == 1;
            GetApiVersionForInfinitySeries(m_BoardComm.GetApiIDValue(m_chipID, "ApiVersion"));
            GetSensorInfo(m_BoardComm.GetApiIDValue(m_chipID, "QuerySensorInfo"));
            switch (m_chipID)
            {
                case ChipID.I1:
                    toolCbxApiVersion.Visible = true;
                    toolCbxApiVersion.SelectedIndex = (int)apiVersion;
                    ChangeXmlType(m_chipID, apiVersion);
                    if (apiVersion == ApiVersion.IQ_APIVERSION_10)
                    {
                        m_GuiContainer.ReadAllPage();
                    }
                    break;
                default:
                    if (m_BoardComm.ChipIsSupport(m_chipID) != 1)
                    {
                        break;
                    }
                    if (IsLoadBinXml)
                    {
                        if (m_GuiContainer.isFWAPIParmChange)
                        {
                            if (MessageBox.Show("API Parameter has been changed. Please first save parameters.", Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                            {
                                return;
                            }
                            SaveTunningParmToBin(isSaveBinXml: true);
                        }
                        if (MessageBox.Show("Do you want to select loaded binxml? If no, it select EVB/IQtool xml.", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            ChangeLoadBinXmlType(m_chipID);
                            if (!m_GuiContainer.APIVerIsMatch)
                            {
                                ClientDisconnect();
                            }
                            else if (MessageBox.Show("Do you want to write all parameters?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                m_GuiContainer.WriteAllPage();
                            }
                        }
                        else
                        {
                            ChangeXmlType(m_chipID, apiVersion);
                            if (!m_GuiContainer.APIVerIsMatch)
                            {
                                ClientDisconnect();
                            }
                            else
                            {
                                m_GuiContainer.ReadAllPage();
                            }
                        }
                    }
                    else
                    {
                        ChangeXmlType(m_chipID, apiVersion);
                        if (!m_GuiContainer.APIVerIsMatch)
                        {
                            ClientDisconnect();
                        }
                        else if (MessageBox.Show("Do you want to read all parameters?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            m_GuiContainer.ReadAllPage();
                        }
                    }
                    break;
                case ChipID.M5:
                    break;
            }
        }
        toolBtnConnect.Enabled = true;
        toolTxbClientHostName.Enabled = true;
        toolTxbClientPort.Enabled = true;
    }

    public string ReadXMLGroup(IQComm comm, FILE_TRANSFER_ID type, string srcpath)
    {
        string result = "";
        short apiId = (short)type;
        if (comm.ReceiveXMLApiPacket(this, apiId, out byte[] pbyRcvData) > 0)
        {
            byte[] array = new byte[pbyRcvData.Length - 8];
            for (int i = 8; i < pbyRcvData.Length; i++)
            {
                array[i - 8] = pbyRcvData[i];
            }
            BinaryWriter binaryWriter = new(File.Open(srcpath, FileMode.Create));
            binaryWriter.Write(array);
            binaryWriter.Close();
        }
        else
        {
            result = "Warning: " + chipname + " ISP API XML on board not found. " + Name + " receive packet failed! ";
        }
        return result;
    }

    public string ReadXMLGroup(IQComm comm, FILE_TRANSFER_ID type, string srcpath, ushort APIVerMajor, ushort APIVerMinor)
    {
        string result = "";
        short apiId = (short)type;
        if (comm.ReceiveXMLApiPacket(apiId, out byte[] pbyRcvData, APIVerMajor, APIVerMinor) > 0)
        {
            byte[] array = new byte[pbyRcvData.Length - 8];
            for (int i = 8; i < pbyRcvData.Length; i++)
            {
                array[i - 8] = pbyRcvData[i];
            }
            BinaryWriter binaryWriter = new(File.Open(srcpath, FileMode.Create));
            binaryWriter.Write(array);
            binaryWriter.Close();
        }
        else
        {
            result = "Warning: " + chipname + " ISP API XML on board not found. " + Name + " receive packet failed! ";
        }
        return result;
    }

    private void ConnectToBoardTask(object objList)
    {
        ConnectSetting connectSetting = (ConnectSetting)objList;
        string text = ClientConnect(connectSetting.HostName, connectSetting.Port);
        if (text.Equals(""))
        {
            if (connectSetting.RemoteID != 0)
            {
                text = m_BoardComm.MatchServer(connectSetting.RemoteID);
            }
            if (text.Equals(""))
            {
                connSetting.CopyFrom(connectSetting);
            }
            else
            {
                m_BoardComm.Disconnect();
            }
        }
        _ = Invoke(new UpdateConnectFinishHandler(ConnectToBoardFinish), text);
    }

    private void GetBoardXMLTask(object objList)
    {
        ConnectSetting connectSetting = (ConnectSetting)objList;
        string text = ClientConnect(connectSetting.HostName, connectSetting.Port);
        if (text.Equals(""))
        {
            if (connectSetting.RemoteID != 0)
            {
                text = m_BoardComm.MatchServer(connectSetting.RemoteID);
            }
            if (text.Equals(""))
            {
                connSetting.CopyFrom(connectSetting);
            }
            else
            {
                m_BoardComm.Disconnect();
            }
        }
        if (!text.Equals(""))
        {
            _ = MessageBox.Show(text, "Connection", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
        else
        {
            _ = GetXML(Application.StartupPath + "/I3_isp_api.xml");
        }
        if (m_BoardComm.IsConnected())
        {
            m_BoardComm.Disconnect();
        }
    }

    private void toolBtnConnect_Click(object sender, EventArgs e)
    {
        string text = "";
        string text2 = "";
        string text3 = "";
        int num = 0;
        string pattern = "^((\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])\\.){3}(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])$";
        string pattern2 = "^((([a-z0-9]+-*)*[a-z0-9]+)\\.)+[a-z]+$";
        string pattern3 = "^[1-9][0-9]{3}$";
        text = toolTxbClientHostName.Text;
        text2 = toolTxbClientPort.Text;
        text3 = toolTxbClientID.Text;
        if (!m_BoardComm.IsConnected())
        {
            if (Regex.IsMatch(text, pattern) || Regex.IsMatch(text, pattern2))
            {
                btnUartConn.Enabled = false;
                if (!text2.Equals(""))
                {
                    num = Convert.ToInt32(text2);
                    if (num is >= 0 and <= 65535)
                    {
                        ConnectSetting connectSetting = new(text, num);
                        Thread thread = new(ConnectToBoardTask);
                        if (Regex.IsMatch(text3, pattern3) || text3.Equals(""))
                        {
                            connectSetting.RemoteID = !text3.Equals("") ? int.Parse(text3) : 0;
                            toolBtnConnect.Enabled = false;
                            toolTxbClientHostName.Enabled = false;
                            toolTxbClientPort.Enabled = false;
                            toolStripMain.Update();
                            thread.Start(connectSetting);
                            SaveBinFile_Interval = 5u;
                            RunTimer();
                        }
                        else
                        {
                            btnUartConn.Enabled = true;
                            _ = MessageBox.Show("ID should be 1000 ~ 9999", "Status", MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        btnUartConn.Enabled = true;
                        _ = MessageBox.Show("Port Number should be 0 ~ 65535", "Status", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    btnUartConn.Enabled = true;
                    _ = MessageBox.Show("Port Number should not be empty", "Status", MessageBoxButtons.OK);
                }
            }
            else
            {
                btnUartConn.Enabled = true;
                _ = MessageBox.Show("Host Name should be legal IP address or Domain name", "Status", MessageBoxButtons.OK);
            }
        }
        else
        {
            btnUartConn.Enabled = true;
            ClientDisconnect();
            StopTimer();
            mTimersTimer.Close();
        }
    }

    private void GuiPageItemCollapse(bool bCollapse)
    {
        if (bCollapse)
        {
            splitConItemPage.Panel1Collapsed = true;
            btnPageItemExpand.Enabled = true;
            btnPageItemExpand.Visible = true;
        }
        else
        {
            splitConItemPage.Panel1Collapsed = false;
            btnPageItemExpand.Enabled = false;
            btnPageItemExpand.Visible = false;
        }
        GuiSetLayout();
    }

    private void btnPageItemCollapse_Click(object sender, EventArgs e)
    {
        GuiPageItemCollapse(bCollapse: true);
    }

    private void btnPageItemExpand_Click(object sender, EventArgs e)
    {
        GuiPageItemCollapse(bCollapse: false);
    }

    private void splitConItemMain_Moved(object sender, SplitterEventArgs e)
    {
        panelMainContent.Height = splitConItemMain.SplitterDistance - panelMainTitle.Height;
    }

    private void GuiSetLayout()
    {
        if (ClientSize.Width != 0 && ClientSize.Height != 0)
        {
            splitConItemPage.Width = ClientSize.Width - m_GuiBorderWidth;
            splitConItemPage.Height = ClientSize.Height - (menuStripMain.Height + toolStripMain.Height + statusStripMain.Height + m_GuiBorderWidth);
            splitConItemMain.Width = splitConItemPage.Panel2.Width - m_GuiBorderWidth;
            splitConItemMain.Height = splitConItemPage.Panel2.Height - m_GuiBorderWidth;
            splitConItemMain.SplitterDistance = splitConItemPage.Height - (panelLogAction.Height + txbMsgLog.Height + splitConItemMain.SplitterWidth + m_GuiBorderWidth) + 60;
            panelMainTitle.Width = splitConItemMain.Panel1.Width - m_GuiBorderWidth;
            panelMainContent.Width = splitConItemMain.Panel1.Width - m_GuiBorderWidth;
            panelMainContent.Height = splitConItemMain.Panel1.Height - panelMainTitle.Height - m_GuiBorderWidth;
            txbMsgLog.Width = splitConItemPage.Panel2.Width - m_GuiBorderWidth;
            panelLogAction.Width = txbMsgLog.Width;
            treePageItem.Height = splitConItemPage.Panel1.Height - m_GuiBorderWidth;
            if (splitConItemPage.Panel1Collapsed)
            {
                panelMainContent.Width = splitConItemMain.Panel1.Width - (btnPageItemExpand.Width + m_GuiBorderWidth);
                panelMainContent.Location = new Point(btnPageItemExpand.Left + btnPageItemExpand.Width + m_GuiBorderWidth, panelMainContent.Location.Y);
            }
            else
            {
                panelMainContent.Location = new Point(0, panelMainContent.Location.Y);
                panelMainContent.Width = splitConItemMain.Panel1.Width - m_GuiBorderWidth;
            }
            chkAutoWrite.Location = new Point(splitConItemMain.Panel1.Width - chkAutoWrite.Width - m_GuiRwButtonSpace, chkAutoWrite.Location.Y);
            btnWritePage.Location = new Point(chkAutoWrite.Location.X - btnWritePage.Width - m_GuiRwButtonSpace, btnWritePage.Location.Y);
            btnReadPage.Location = new Point(btnWritePage.Location.X - btnReadPage.Width - m_GuiRwButtonSpace, btnReadPage.Location.Y);
            button_Two.Location = new Point(btnReadPage.Location.X - button_Two.Width - m_GuiRwButtonSpace, button_Two.Location.Y);
            button_One.Location = new Point(button_Two.Location.X - button_One.Width - m_GuiRwButtonSpace, button_One.Location.Y);
            m_GuiContainer.RearrangeLayout();
        }
    }

    private void MainForm_SizeChanged(object sender, EventArgs e)
    {
        GuiSetLayout();
    }

    private void toolTxbClientPort_KeyPress(object sender, KeyPressEventArgs e)
    {
        e.Handled = e.KeyChar is (< '0' or > '9') and not '\b';
    }

    private void toolTxbClientID_KeyPress(object sender, KeyPressEventArgs e)
    {
        e.Handled = e.KeyChar is (< '0' or > '9') and not '\b';
    }

    private void toolTxbClientHostName_KeyPress(object sender, KeyPressEventArgs e)
    {
        char keyChar = e.KeyChar;
        e.Handled = keyChar is (< '0' or > '9') and (< 'a' or > 'z') and not '.' and not '-' and not '\b';
    }

    private void checkAutoWrite_CheckedChanged(object sender, EventArgs e)
    {
        m_GuiContainer.AutoWrite = chkAutoWrite.Checked;
    }

    private void btnReadPage_Click(object sender, EventArgs e)
    {
        if (m_BoardComm.IsConnected())
        {
            m_GuiContainer.ReadPage();
        }
        else if (m_BoardUart.IsConnected())
        {
            m_GuiContainer.ReadUartPage();
        }
        else
        {
            _ = MessageBox.Show("need connect!");
        }
    }

    private void btnWritePage_Click(object sender, EventArgs e)
    {
        if (m_BoardComm.IsConnected())
        {
            m_GuiContainer.WritePage();
        }
        else if (m_BoardUart.IsConnected())
        {
            m_GuiContainer.WriteUartPage();
        }
        else
        {
            _ = MessageBox.Show("need connect!");
        }
    }

    public void btnRecordParameter(string itemTag, int dataIndex, long[] dataValue)
    {
        m_GuiContainer.buttonUpDown_KeyPress(itemTag, dataIndex, dataValue);
    }

    private void GetSensorInfo(short sensorApiID)
    {
        byte[] pbyRcvData = null;
        if (m_BoardComm.ConnectMode switch
        {
            CONNECT_MODE.MODE_SOCKET => m_BoardComm.ReceiveApiPacket(sensorApiID, out pbyRcvData),
            CONNECT_MODE.MODE_USB => m_BoardComm.ReceiveUsbApiPacket(sensorApiID, out pbyRcvData),
            _ => m_BoardComm.ReceiveUartApiPacket(sensorApiID, out pbyRcvData),
        } > 0)
        {
            PacketParser.ConvertBufferToType(sensorInfo, pbyRcvData);
        }
    }

    private void GetSensorInfoByChipID()
    {
        byte[] pbyRcvData = null;
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        short num2 = m_BoardComm.GetApiIDValue(m_chipID, "QuerySensorInfo");
        if (connectMode switch
        {
            CONNECT_MODE.MODE_SOCKET => m_BoardComm.ReceiveApiPacket(num2, out pbyRcvData),
            CONNECT_MODE.MODE_USB => m_BoardComm.ReceiveUsbApiPacket(num2, out pbyRcvData),
            _ => m_BoardComm.ReceiveUartApiPacket(num2, out pbyRcvData),
        } > 0)
        {
            PacketParser.ConvertBufferToType(sensorInfo, pbyRcvData);
        }
    }

    private void SetUartStart()
    {
        string text = "";
        if (m_BoardComm.ConnectMode == CONNECT_MODE.MODE_UART)
        {
            text = m_BoardComm.SendUartStart();
        }
        if (text != "")
        {
            m_GuiContainer.ShowMessage("", text);
        }
    }

    private void GetChipIDByAPI(short chipApiID)
    {
        byte[] pbyRcvData = null;
        byte[] bufferInitial = new byte[100];
        int num = 0;
        switch (m_BoardComm.ConnectMode)
        {
            case CONNECT_MODE.MODE_USB:
                num = m_BoardComm.ReceiveUSBInitialApiPacket(chipApiID, bufferInitial, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = m_BoardComm.ReceiveApiPacket(chipApiID, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                num = m_BoardComm.ReceiveUartApiPacket(chipApiID, out pbyRcvData);
                break;
        }
        if (num > 0)
        {
            PacketParser.ConvertBufferToType(ref m_chipID, pbyRcvData);
        }
    }

    private void GetChannelIDByAPI(short ChannelApiID)
    {
        byte[] pbyRcvData = null;
        byte[] bufferInitial = new byte[100];
        int num = 0;
        switch (m_BoardComm.ConnectMode)
        {
            case CONNECT_MODE.MODE_USB:
                num = m_BoardComm.ReceiveUSBInitialApiPacket(ChannelApiID, bufferInitial, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = m_BoardComm.ReceiveApiPacket(ChannelApiID, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                num = m_BoardComm.ReceiveUartApiPacket(ChannelApiID, out pbyRcvData);
                break;
        }
        if (num > 0)
        {
            PacketParser.ConvertChannelBufferToType(ref channelID, pbyRcvData);
            if (ChannelIDComboBox.SelectedIndex != channelID)
            {
                ChannelIDComboBox.SelectedIndex = channelID;
            }
        }
    }

    private void SetChannelIDByAPI(short ChannelApiID)
    {
        int[] data = new int[1] { channelID };
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        byte[] apiBufferByDataArray = PacketParser.GetApiBufferByDataArray(101, data);
        if (m_BoardComm.IsConnected())
        {
            switch (connectMode)
            {
                case CONNECT_MODE.MODE_USB:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_UART:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
            }
            if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
            {
                string msgText = "Set channel success, current channel information = " + channelID;
                m_GuiContainer.ShowMessage("", msgText);
            }
        }
        else if (m_bInitStatus)
        {
            m_GuiContainer.ShowMessage("", "Current status: not connected.");
        }
    }

    private void GetDeviceIDByAPI(short DeviceApiID)
    {
        byte[] pbyRcvData = null;
        byte[] bufferInitial = new byte[100];
        int num = 0;
        switch (m_BoardComm.ConnectMode)
        {
            case CONNECT_MODE.MODE_USB:
                num = m_BoardComm.ReceiveUSBInitialApiPacket(DeviceApiID, bufferInitial, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = m_BoardComm.ReceiveApiPacket(DeviceApiID, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                num = m_BoardComm.ReceiveUartApiPacket(DeviceApiID, out pbyRcvData);
                break;
        }
        if (num > 0)
        {
            PacketParser.ConvertDeviceBufferToType(ref DeviceID, pbyRcvData);
            if (DeviceIDComboBox.SelectedIndex != DeviceID)
            {
                DeviceIDComboBox.SelectedIndex = DeviceID;
            }
        }
    }

    private void SetDeviceIDByAPI(short DeviceApiID)
    {
        int[] data = new int[1] { DeviceID };
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        byte[] apiBufferByDataArray = PacketParser.GetApiBufferByDataArray(104, data);
        if (m_BoardComm.IsConnected())
        {
            switch (connectMode)
            {
                case CONNECT_MODE.MODE_USB:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_UART:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
            }
            if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
            {
                string msgText = "Set device success, current device information = " + DeviceID;
                m_GuiContainer.ShowMessage("", msgText);
            }
        }
        else if (m_bInitStatus)
        {
            m_GuiContainer.ShowMessage("", "Current status: not connected.");
        }
    }

    private void GeSensorPadIDByAPI(short SensorPadApiID)
    {
        byte[] pbyRcvData = null;
        byte[] bufferInitial = new byte[100];
        int num = 0;
        switch (m_BoardComm.ConnectMode)
        {
            case CONNECT_MODE.MODE_USB:
                num = m_BoardComm.ReceiveUSBInitialApiPacket(SensorPadApiID, bufferInitial, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = m_BoardComm.ReceiveApiPacket(SensorPadApiID, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                num = m_BoardComm.ReceiveUartApiPacket(SensorPadApiID, out pbyRcvData);
                break;
        }
        if (num > 0)
        {
            PacketParser.ConvertBufferToID(ref sensorPadID, pbyRcvData);
            if (SensorPadIDComboBox.SelectedIndex != sensorPadID)
            {
                SensorPadIDComboBox.SelectedIndex = sensorPadID;
            }
        }
    }

    private int SetSensorPadIDByAPI(ushort nSensorPadID)
    {
        int result = -1;
        int[] data = new int[1] { nSensorPadID };
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        byte[] apiBufferByDataArray = PacketParser.GetApiBufferByDataArray(36, data);
        if (m_BoardComm.IsConnected())
        {
            switch (connectMode)
            {
                case CONNECT_MODE.MODE_USB:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_UART:
                    iQ_CMD_RESPONSE_S = m_BoardComm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
            }
            if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
            {
                string msgText = "Set sensor id success, current sensor id information = " + nSensorPadID;
                m_GuiContainer.ShowMessage("", msgText);
                result = 0;
            }
        }
        else if (m_bInitStatus)
        {
            m_GuiContainer.ShowMessage("", "Current status : It's not connected.");
        }
        return result;
    }

    private void GetChipIDByXml(string xmlPath)
    {
        if (File.Exists(xmlPath))
        {
            XmlDocument xmlDocument = new();
            xmlDocument.Load(xmlPath);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("/ISP_ITEM");
            xmlNode ??= xmlDocument.SelectSingleNode("/API_XML");
            string value = xmlNode.Attributes["Name"].Value;
            m_chipID = (ChipID)m_BoardComm.GetChipIDByName(value);
        }
        else
        {
            string text = "Xml : " + xmlPath + " ==> cannot be found";
            _ = Invoke(new ShowMessageHandler(GUIShowMessage), text);
        }
    }

    private void GetApiVersion()
    {
        toolCbxApiVersion.Visible = true;
        short apiIDValue = m_BoardComm.GetApiIDValue(ChipID.I1, "ApiVersion");
        if (m_BoardComm.ReceiveApiPacket(apiIDValue, out byte[] pbyRcvData) > 0)
        {
            PacketParser.ConvertBufferToType(ref apiVersion, pbyRcvData);
        }
        else
        {
            toolCbxApiVersion.Visible = false;
        }
    }

    private void GetApiVersionForInfinitySeries(short APIVersion)
    {
        if (m_BoardComm.ReceiveApiPacket(APIVersion, out byte[] pbyRcvData) > 0)
        {
            short apiIDValue = m_BoardComm.GetApiIDValue(ChipID.I1, "ApiVersion");
            short apiIDValue2 = m_BoardComm.GetApiIDValue(ChipID.I3, "ApiVersion");
            if (APIVersion == apiIDValue)
            {
                toolCbxApiVersion.Visible = true;
                PacketParser.ConvertBufferToType(ref apiVersion, pbyRcvData);
            }
            else if (APIVersion == apiIDValue2)
            {
                toolCbxApiVersion.Visible = false;
                PacketParser.ConvertBufferToTypeForI3(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
            }
            else
            {
                toolCbxApiVersion.Visible = false;
                PacketParser.ConvertBufferToType(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
            }
        }
        else
        {
            toolCbxApiVersion.Visible = false;
        }
    }

    private void GetApiVersionForM5()
    {
        byte[] pbyRcvData = null;
        byte[] bufferInitial = new byte[100];
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        int num = 0;
        toolCbxApiVersion.Visible = false;
        short apiIDValue = m_BoardComm.GetApiIDValue(ChipID.M5, "ApiVersion");
        switch (connectMode)
        {
            case CONNECT_MODE.MODE_USB:
                num = m_BoardComm.ReceiveUSBInitialApiPacket(apiIDValue, bufferInitial, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = m_BoardComm.ReceiveApiPacket(apiIDValue, out pbyRcvData);
                break;
        }
        if (num > 0)
        {
            PacketParser.ConvertBufferToTypeForI3(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
        }
        else
        {
            toolCbxApiVersion.Visible = false;
        }
    }

    private void GetApiVersionForUSBMode()
    {
        byte[] pbyRcvData = null;
        byte[] bufferInitial = new byte[100];
        ChipID chipIDName = m_BoardComm.ChipIDName;
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        int num = 0;
        toolCbxApiVersion.Visible = false;
        short apiIDValue = m_BoardComm.GetApiIDValue(m_chipID, "ApiVersion");
        switch (connectMode)
        {
            case CONNECT_MODE.MODE_USB:
                num = m_BoardComm.ReceiveUSBInitialApiPacket(apiIDValue, bufferInitial, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = m_BoardComm.ReceiveApiPacket(apiIDValue, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                num = m_BoardComm.ReceiveUartApiPacket(apiIDValue, out pbyRcvData);
                break;
        }
        if (num > 0)
        {
            if (chipIDName is ChipID.M5 or ChipID.M5U)
            {
                PacketParser.ConvertBufferToTypeForI3(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
            }
            else
            {
                PacketParser.ConvertBufferToType(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
            }
        }
        else
        {
            toolCbxApiVersion.Visible = false;
        }
    }

    private bool GetXML(string SrcPath)
    {
        string text = ReadXMLGroup(m_BoardComm, FILE_TRANSFER_ID.ISP_XML_DATA, SrcPath);
        if (!text.Equals(""))
        {
            _ = Invoke(new ShowMessageHandler(GUIShowMessage), text);
            return false;
        }
        return true;
    }

    private void GetXML(ushort APIVerMajor, ushort APIVerMinor)
    {
        string srcpath = Application.StartupPath + "/I3_isp_api_" + APIVerMajor + "_" + APIVerMinor + ".xml";
        string text = ReadXMLGroup(m_BoardComm, FILE_TRANSFER_ID.ISP_XML_DATA, srcpath, APIVerMajor, APIVerMinor);
        if (!text.Equals(""))
        {
            _ = Invoke(new ShowMessageHandler(GUIShowMessage), text);
        }
    }

    private void SetApiVersion(ApiVersion version)
    {
        int[] data = new int[1] { (int)version };
        byte[] apiBufferByDataArray = PacketParser.GetApiBufferByDataArray(m_BoardComm.GetApiIDValue(ChipID.I1, "ApiVersion"), data);
        if (m_BoardComm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length).ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            _ = Invoke(new ChangeApiVersionHandler(ChangeXmlType), m_chipID, version);
        }
    }

    private void ChangeLoadBinXmlType(ChipID chipId)
    {
        try
        {
            _ = Path.GetExtension(loadbinxml).Equals(".bin")
                ? m_GuiContainer.LoadBinXmlFile(loadbinxml)
                : m_GuiContainer.LoadXmlFile(loadbinxml);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void getChipXmlName(ref string srcxml, ref string desxml)
    {
        srcxml = Application.StartupPath + "/" + m_chipdef[(ulong)m_chipID] + "_isp_api.xml";
        desxml = Application.StartupPath + "/CvtXml/" + m_chipdef[(ulong)m_chipID] + "_isp_api_" + EvbAPIVerMajor + "_" + EvbAPIVerMinor + ".xml";
    }

    private void SuccessGetXml()
    {
        string srcxml = null;
        string desxml = null;
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        m_BoardComm.TransFinish("");
        getChipXmlName(ref srcxml, ref desxml);
        string text = Application.StartupPath + "/CvtXml";
        if (!Directory.Exists(text))
        {
            _ = Directory.CreateDirectory(text);
        }
        try
        {
            File.Copy(srcxml, desxml, overwrite: true);
        }
        catch (Exception)
        {
            string msgText = "Warning: " + chipname + " ISP API XML not found!";
            m_GuiContainer.ShowMessage("", msgText);
            return;
        }
        if (m_GuiContainer.checkChipID(m_chipID, desxml) != 1)
        {
            _ = m_GuiContainer.checkAPIVersion(desxml, m_chipID, connectMode);
            _ = m_GuiContainer.InitGUI(desxml);
        }
    }

    private void FailGetXml(string strmsg)
    {
        string srcxml = null;
        string desxml = null;
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        m_BoardComm.TransFinish("");
        getChipXmlName(ref srcxml, ref desxml);
        if (File.Exists(desxml))
        {
            if (m_GuiContainer.checkChipID(m_chipID, desxml) != 1)
            {
                _ = m_GuiContainer.checkAPIVersion(desxml, m_chipID, connectMode);
                _ = m_GuiContainer.InitGUI(desxml);
            }
        }
        else if (File.Exists(srcxml))
        {
            if (m_GuiContainer.checkChipID(m_chipID, srcxml) != 1)
            {
                _ = m_GuiContainer.checkAPIVersion(srcxml, m_chipID, connectMode);
                _ = m_GuiContainer.InitGUI(srcxml);
            }
        }
        else
        {
            _ = MessageBox.Show("Xml cannot be found.", Text, MessageBoxButtons.OK);
        }
    }

    private void GetXMLTask(object obj)
    {
        string srcpath = (string)obj;
        string text = ReadXMLGroup(m_BoardComm, FILE_TRANSFER_ID.ISP_XML_DATA, srcpath);
        _ = !text.Equals("") ? Invoke(new FailGetXmlHander(FailGetXml), text) : Invoke(new SuccessGetXmlHander(SuccessGetXml));
    }

    private void ChangeUartXmlType(ChipID chipId, ApiVersion version)
    {
        string text = null;
        string srcxml = null;
        string desxml = null;
        _ = m_BoardComm.ConnectMode;
        m_chipID = chipId;
        GetChipName(m_chipID, ref chipname);
        switch (chipId)
        {
            case ChipID.I1:
                switch (version)
                {
                    case ApiVersion.IQ_APIVERSION_10:
                        text = Application.StartupPath + "/" + Settings.Default.ApiModeXmlFile;
                        _ = m_GuiContainer.InitGUI(text);
                        break;
                    case ApiVersion.IQ_APIVERSION_20:
                        text = Application.StartupPath + "/I1_isp_api_v2.xml";
                        _ = m_GuiContainer.InitGUI(text);
                        break;
                    default:
                        text = Application.StartupPath + "/" + Settings.Default.ApiModeXmlFile;
                        _ = m_GuiContainer.InitGUI(text);
                        break;
                }
                break;
            case ChipID.M5:
            case ChipID.M5U:
                text = Application.StartupPath + "/M5_isp_api_" + EvbAPIVerMajor + "_" + EvbAPIVerMinor + ".xml";
                _ = m_GuiContainer.InitGUI(text);
                break;
            default:
                if (m_BoardComm.ChipIsSupport(chipId) == 0)
                {
                    _ = MessageBox.Show("[Error] Not support Chip Type!");
                    break;
                }
                getChipXmlName(ref srcxml, ref desxml);
                m_BoardComm.updateTaskQueue(GetXMLTask, srcxml);
                m_BoardComm.ShowProgressBar("Load XML...");
                break;
        }
    }

    private void ChangeXmlType(ChipID chipId, ApiVersion version)
    {
        m_chipID = chipId;
        GetChipName(m_chipID, ref chipname);
        CONNECT_MODE connectMode = m_BoardComm.ConnectMode;
        string text3;
        switch (chipId)
        {
            case ChipID.I1:
                switch (version)
                {
                    case ApiVersion.IQ_APIVERSION_10:
                        text3 = Application.StartupPath + "/" + Settings.Default.ApiModeXmlFile;
                        _ = m_GuiContainer.InitGUI(text3);
                        break;
                    case ApiVersion.IQ_APIVERSION_20:
                        text3 = Application.StartupPath + "/I1_isp_api_v2.xml";
                        _ = m_GuiContainer.InitGUI(text3);
                        break;
                    default:
                        text3 = Application.StartupPath + "/" + Settings.Default.ApiModeXmlFile;
                        _ = m_GuiContainer.InitGUI(text3);
                        break;
                }
                return;
            case ChipID.M5:
            case ChipID.M5U:
                text3 = Application.StartupPath + "/M5_isp_api_" + EvbAPIVerMajor + "_" + EvbAPIVerMinor + ".xml";
                _ = m_GuiContainer.InitGUI(text3);
                return;
        }
        if (m_BoardComm.ChipIsSupport(chipId) == 0)
        {
            _ = MessageBox.Show("[Error] Not support Chip Type!\n");
            return;
        }
        string text = Application.StartupPath + "/" + m_chipdef[(ulong)chipId] + "_isp_api.xml";
        string text2 = Application.StartupPath + "/CvtXml/" + m_chipdef[(ulong)chipId] + "_isp_api_" + EvbAPIVerMajor + "_" + EvbAPIVerMinor + ".xml";
        string text4 = Application.StartupPath + "/CvtXml";
        if (!Directory.Exists(text4))
        {
            _ = Directory.CreateDirectory(text4);
        }
        if (GetXML(text))
        {
            File.Copy(text, text2, overwrite: true);
            if (m_GuiContainer.checkChipID(m_chipID, text2) != 1)
            {
                _ = m_GuiContainer.checkAPIVersion(text2, m_chipID, connectMode);
                _ = m_GuiContainer.InitGUI(text2);
            }
        }
        else if (File.Exists(text2))
        {
            if (m_GuiContainer.checkChipID(m_chipID, text2) != 1)
            {
                _ = m_GuiContainer.checkAPIVersion(text2, m_chipID, connectMode);
                _ = m_GuiContainer.InitGUI(text2);
            }
        }
        else if (File.Exists(text))
        {
            if (m_GuiContainer.checkChipID(m_chipID, text) != 1)
            {
                _ = m_GuiContainer.checkAPIVersion(text, m_chipID, connectMode);
                _ = m_GuiContainer.InitGUI(text);
            }
        }
        else
        {
            _ = MessageBox.Show("Xml cannot be found.", Text, MessageBoxButtons.OK);
        }
    }

    public void GetImageTask(object obj)
    {
        CAMERA_MODE_TYPE type = (CAMERA_MODE_TYPE)obj;
        string text = "";
        imageCapture.SetChipID(m_chipID);
        imageCapture.SetConnectMode(m_BoardComm.ConnectMode);
        imageCapture.SetRawCaptureSetting(m_RawCount, m_RawLoopCount);
        imageCapture.SetSclSetting(m_SclDevice, m_SclChannel, m_SclPort);
        text = imageCapture.GetImageByMode(type);
        if (text.Equals(""))
        {
            _ = Invoke(new ShowMessageHandler(GUIShowMessage), "Get image success" + Environment.NewLine + AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "image");
        }
        else
        {
            _ = Invoke(new ShowMessageHandler(ShowMessageBox), "Get image failed");
            _ = Invoke(new ShowMessageHandler(GUIShowMessage), text);
        }
        CheckConnection();
    }

    private void SaveImageFromCamera(CAMERA_MODE_TYPE outputType)
    {
        if (m_BoardComm.IsConnected())
        {
            m_BoardComm.updateTaskQueue(GetImageTask, outputType);
            if (m_BoardComm.ConnectMode is CONNECT_MODE.MODE_SOCKET or CONNECT_MODE.MODE_USB or CONNECT_MODE.MODE_UART)
            {
                m_BoardComm.ShowProgressBar("Recv Data...");
            }
        }
        else
        {
            _ = MessageBox.Show(Settings.Default.MsgNotConnected, "Connection", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }

    private void toolBtnGetRawImage_Click(object sender, EventArgs e)
    {
        SaveImageFromCamera(CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW);
    }

    private bool getCameraHdrMode()
    {
        bool result = false;
        if (m_BoardComm.IsConnected() && m_BoardComm.ReceivePacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_MODE, out byte[] pbyRcvData) > 0 && pbyRcvData[0] == 1)
        {
            result = true;
        }
        return result;
    }

    private void SetRawCount(uint frame_count, uint loop_count)
    {
        if (getCameraHdrMode() && frame_count % 2 != 0)
        {
            _ = MessageBox.Show("HDR Mode Raw Data Numbers Must Be Even", Text, MessageBoxButtons.YesNo);
            return;
        }
        m_RawCount = frame_count;
        m_RawLoopCount = loop_count;
        SaveImageFromCamera(CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM);
    }

    private void toolBtnGetRawStream_Click(object sender, EventArgs e)
    {
        FormSetRawFrameCount formSetRawFrameCount = new(m_RawCount, m_RawLoopCount, SetRawCount)
        {
            StartPosition = FormStartPosition.CenterScreen
        };
        formSetRawFrameCount.Show();
    }

    private void toolBtnGetIspYuv_Click(object sender, EventArgs e)
    {
        SaveImageFromCamera(CAMERA_MODE_TYPE.CAMERA_MODE_ISP_OUT);
    }

    private void toolBtnGetJpeg_Click(object sender, EventArgs e)
    {
        SaveImageFromCamera(CAMERA_MODE_TYPE.CAMERA_MODE_ENC_JPEG);
    }

    private void SetSclSetting(int device, int channel, int port)
    {
        m_SclDevice = device;
        m_SclChannel = channel;
        m_SclPort = port;
        SaveImageFromCamera(CAMERA_MODE_TYPE.CAMERA_MODE_SCL_OUT);
    }

    private void toolGetScalarOutImage_Click(object sender, EventArgs e)
    {
        if (m_BoardComm.GetChipSupportScl(m_chipID) == 1)
        {
            FormSclSetting formSclSetting = new(m_SclDevice, m_SclChannel, m_SclPort, SetSclSetting)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            formSclSetting.Show();
        }
        else
        {
            SaveImageFromCamera(CAMERA_MODE_TYPE.CAMERA_MODE_SCL_OUT);
        }
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _ = new FormAbout().ShowDialog();
        _ = 1;
    }

    private void SaveTunningParamters()
    {
        string text = "";
        SaveFileDialog saveFileDialog = new()
        {
            Filter = "Parameters file (*.xml) | *.xml"
        };
        if (saveFileDialog.ShowDialog() == DialogResult.OK && saveFileDialog.FileName != "")
        {
            text = m_GuiContainer.SaveXml(saveFileDialog.FileName);
        }
        if (!saveFileDialog.FileName.Equals(""))
        {
            if (text.Equals(""))
            {
                text = "Save File " + saveFileDialog.FileName + " Success.";
                _ = MessageBox.Show(text, "Save Parameters File", MessageBoxButtons.OK);
                m_GuiContainer.isFWAPIParmChange = false;
            }
            else
            {
                text = "Save File " + saveFileDialog.FileName + " Error!";
                _ = MessageBox.Show(text, "Save Parameters File", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
    }

    private void newParamFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        DialogResult dialogResult = MessageBox.Show("Do you want to save current parameters?", Text, MessageBoxButtons.YesNoCancel);
        if (dialogResult == DialogResult.Yes)
        {
            SaveTunningParamters();
        }
        if (dialogResult is DialogResult.Yes or DialogResult.No)
        {
            string text = Application.StartupPath + "/" + Settings.Default.ApiModeXmlFile;
            _ = m_GuiContainer.InitGUI(text);
        }
    }

    private void openParamFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (m_GuiContainer.isFWAPIParmChange)
        {
            if (MessageBox.Show("API Parameter has been changed. Please first save parameters.", Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            SaveTunningParmToBin(isSaveBinXml: true);
        }
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Parameters file (*.xml)| *.xml| All files (*.*)|*.*",
            InitialDirectory = Application.StartupPath
        };
        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        if (m_GuiContainer.LoadXmlFile(openFileDialog.FileName) == 0)
        {
            IsLoadBinXml = true;
            loadbinxml = openFileDialog.FileName;
        }
        else
        {
            IsLoadBinXml = false;
            loadbinxml = "";
        }
        if (m_BoardComm.IsConnected())
        {
            if (!m_GuiContainer.APIVerIsMatch)
            {
                ClientDisconnect();
            }
            else if (MessageBox.Show("Do you want to write all parameters?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                m_GuiContainer.WriteAllPage();
            }
        }
    }

    private void openBinFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (m_GuiContainer.isFWAPIParmChange)
        {
            if (MessageBox.Show("API Parameter has been changed. Please first save parameters.", Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            m_bSaveBinXml = false;
            SaveTunningParmToBin(m_bSaveBinXml);
        }
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Bin file (*.bin)|*.bin",
            InitialDirectory = Application.StartupPath
        };
        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        if (m_GuiContainer.LoadBinFile(openFileDialog.FileName) == 0)
        {
            IsLoadBinXml = true;
            loadbinxml = openFileDialog.FileName;
        }
        else
        {
            IsLoadBinXml = false;
            loadbinxml = "";
        }
        if (m_BoardComm.IsConnected())
        {
            if (!m_GuiContainer.APIVerIsMatch)
            {
                ClientDisconnect();
            }
            else if (MessageBox.Show("Do you want to write all parameters?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                m_GuiContainer.WriteAllPage();
            }
        }
    }

    private void openBinXmlFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (m_GuiContainer.isFWAPIParmChange)
        {
            if (MessageBox.Show("API Parameter has been changed. Please first save parameters.", Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            m_bSaveBinXml = true;
            SaveTunningParmToBin(m_bSaveBinXml);
        }
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Bin file (*.bin)|*.bin",
            InitialDirectory = Application.StartupPath
        };
        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        if (m_GuiContainer.LoadBinXmlFile(openFileDialog.FileName) == 0)
        {
            IsLoadBinXml = true;
            loadbinxml = openFileDialog.FileName;
        }
        else
        {
            IsLoadBinXml = false;
            loadbinxml = "";
        }
        if (m_BoardComm.IsConnected())
        {
            if (!m_GuiContainer.APIVerIsMatch)
            {
                ClientDisconnect();
            }
            else if (MessageBox.Show("Do you want to write all parameters?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                m_GuiContainer.WriteAllPage();
            }
        }
    }

    private void saveParamFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        SaveTunningParamters();
    }

    private void SaveTunningParmToBin(bool isSaveBinXml)
    {
        SaveFileDialog saveFileDialog = new()
        {
            Filter = "Bin file (*.bin)|*.bin",
            InitialDirectory = Application.StartupPath
        };
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                string text = Application.StartupPath + "/XmlTmp.xml";
                string text2 = Application.StartupPath + "/XmlTmp.zip";
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
                if (File.Exists(text2))
                {
                    File.Delete(text2);
                }
                _ = m_GuiContainer.SaveXml(text);
                m_GuiContainer.GetChipIDByXml(text);
                CompressGZipfile(text, text2);
                byte[] array = !(m_GuiContainer.m_binVersion == "0.0") ? m_GuiContainer.GetBinBytesXmlCRC(m_GuiContainer.XmlChipID, magicKey, videomagicKey, text2, m_BinChecksumVer) : m_GuiContainer.GetBinBytes(m_GuiContainer.XmlChipID, magicKey, m_BinChecksumVer);
                byte[] array2 = File.ReadAllBytes(text2);
                BinaryWriter binaryWriter = new(File.Open(saveFileDialog.FileName, FileMode.Create));
                byte[] array3 = null;
                if (isSaveBinXml)
                {
                    array3 = new byte[array.Length + array2.Length];
                    array.CopyTo(array3, 0);
                    array2.CopyTo(array3, array.Length);
                }
                else
                {
                    array3 = new byte[array.Length];
                    array.CopyTo(array3, 0);
                }
                binaryWriter.Write(array3);
                binaryWriter.Close();
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
                if (File.Exists(text2))
                {
                    File.Delete(text2);
                }
                StopTimer();
                RunTimer();
            }
            catch (Exception)
            {
            }
        }
        if (!saveFileDialog.FileName.Equals(""))
        {
            _ = MessageBox.Show("Save File " + saveFileDialog.FileName + " Success.", "Save Parameters File", MessageBoxButtons.OK);
            m_GuiContainer.isFWAPIParmChange = false;
        }
    }

    private void AutoSaveTunningParmToBin(bool isSaveBinXml)
    {
        SaveFileDialog saveFileDialog = new()
        {
            Filter = "Bin file (*.bin)|*.bin"
        };
        string text = Application.StartupPath + "/XmlTmp.xml";
        string text2 = Application.StartupPath + "/XmlTmp.zip";
        if (File.Exists(text))
        {
            File.Delete(text);
        }
        if (File.Exists(text2))
        {
            File.Delete(text2);
        }
        _ = m_GuiContainer.SaveXml(text);
        m_GuiContainer.GetChipIDByXml(text);
        CompressGZipfile(text, text2);
        byte[] array = !(m_GuiContainer.m_binVersion == "0.0") ? m_GuiContainer.GetBinBytesXmlCRC(m_GuiContainer.XmlChipID, magicKey, videomagicKey, text2, m_BinChecksumVer) : m_GuiContainer.GetBinBytes(m_GuiContainer.XmlChipID, magicKey, m_BinChecksumVer);
        byte[] array2 = File.ReadAllBytes(text2);
        Auto_DUMP_BIN_FILENAME = Application.StartupPath + "./CvtXml/" + chipname + "_Device_" + DeviceIDComboBox.SelectedIndex + "_Channel_" + ChannelIDComboBox.SelectedIndex + "_isp_api_auto_save.bin";
        string path = Application.StartupPath + "./CvtXml";
        saveFileDialog.FileName = Auto_DUMP_BIN_FILENAME;
        if (!Directory.Exists(path))
        {
            _ = MessageBox.Show("Path not found");
        }
        else
        {
            BinaryWriter binaryWriter = new(File.Open(saveFileDialog.FileName, FileMode.Create));
            byte[] array3;
            if (isSaveBinXml)
            {
                array3 = new byte[array.Length + array2.Length];
                array.CopyTo(array3, 0);
                array2.CopyTo(array3, array.Length);
            }
            else
            {
                array3 = new byte[array.Length];
                array.CopyTo(array3, 0);
            }
            binaryWriter.Write(array3);
            binaryWriter.Close();
        }
        if (File.Exists(text))
        {
            File.Delete(text);
        }
        if (File.Exists(text2))
        {
            File.Delete(text2);
        }
        m_GuiContainer.isFWAPIParmChange = false;
    }

    private void saveBinFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        m_bSaveBinXml = false;
        SaveTunningParmToBin(m_bSaveBinXml);
    }

    private void saveBinXmlFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        m_bSaveBinXml = true;
        SaveTunningParmToBin(m_bSaveBinXml);
    }

    private void setMagicKeyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        FormSetMagicKey formSetMagicKey = new(magicKey, SetMagicKey)
        {
            StartPosition = FormStartPosition.CenterScreen
        };
        formSetMagicKey.Show();
    }

    private void SetMagicKey(uint key)
    {
        magicKey = key;
    }

    public void ReleaseLiveViewForm()
    {
        m_fmLiveView = null;
    }

    private void liveViewToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (m_fmLiveView == null)
        {
            m_fmLiveView = new FormLiveView();
            m_fmLiveView.SetReleaseFunc(ReleaseLiveViewForm);
            m_fmLiveView.setConnSetting(connSetting);
            m_fmLiveView.Show();
            _ = m_fmLiveView.Focus();
        }
        else
        {
            m_fmLiveView.Show();
            _ = m_fmLiveView.Focus();
        }
    }

    private void toolBtnStepBack_Click(object sender, EventArgs e)
    {
        toolBtnStepNext.Enabled = true;
        m_GuiContainer.StepBack();
        if (m_GuiContainer.IsBackStepOver())
        {
            toolBtnStepBack.Enabled = false;
        }
        m_GuiContainer.WritePage();
    }

    private void toolBtnStepNext_Click(object sender, EventArgs e)
    {
        toolBtnStepBack.Enabled = true;
        m_GuiContainer.StepNext();
        if (m_GuiContainer.IsNextStepOver())
        {
            toolBtnStepNext.Enabled = false;
        }
        m_GuiContainer.WritePage();
    }

    private void toolBtnReadAll_Click(object sender, EventArgs e)
    {
        if (m_BoardComm.IsConnected())
        {
            m_GuiContainer.ReadAllPage();
        }
    }

    private void toolBtnWriteAll_Click(object sender, EventArgs e)
    {
        if (m_BoardComm.IsConnected())
        {
            m_GuiContainer.WriteAllPage();
        }
    }

    public void ListFilesInFolder(ref List<string> fileList, string dirTarget, string searchFile)
    {
        string[] files = Directory.GetFiles(dirTarget, searchFile);
        for (int i = 0; i < files.Length; i++)
        {
            _ = new FileInfo(files[i]);
            fileList.Add(files[i].Trim());
        }
    }

    public void ListPluginFile()
    {
        string text = Application.StartupPath + "/Plugin";
        if (!Directory.Exists(text))
        {
            return;
        }
        ListFilesInFolder(ref m_PluginList, text, "*.dll");
        for (int i = 0; i < m_PluginList.Count; i++)
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(m_PluginList[i]);
            if (versionInfo.ProductName != null)
            {
                _ = toolCbxPlugIn.Items.Add(versionInfo.ProductName);
            }
            else
            {
                m_PluginList.RemoveAt(i--);
            }
        }
        toolCbxPlugIn.SelectedIndex = 0;
    }

    private void toolCbxPlugIn_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (toolCbxPlugIn.SelectedIndex > m_PluginList.Count || toolCbxPlugIn.SelectedIndex <= 0)
        {
            return;
        }
        Assembly assembly = Assembly.LoadFile(m_PluginList[toolCbxPlugIn.SelectedIndex - 1]);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(m_PluginList[toolCbxPlugIn.SelectedIndex - 1]);
        string text = fileNameWithoutExtension + ".Form" + fileNameWithoutExtension;
        Type type = assembly.GetType(text);
        Form form;
        try
        {
            if (text == "AwbAnalyzerCombo.FormAwbAnalyzerCombo")
            {
                form = (Form)Activator.CreateInstance(type, m_BoardComm, m_chipID, apiVersion);
                EventInfo @event = type.GetEvent("CallBack_GetOBC_RGB", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (@event != null)
                {
                    Type eventHandlerType = @event.EventHandlerType;
                    MethodInfo method = GetType().GetMethod("CallBack_GetOBC_RGB", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    Delegate @delegate = Delegate.CreateDelegate(eventHandlerType, method);
                    MethodInfo addMethod = @event.GetAddMethod();
                    object[] parameters = new object[1] { @delegate };
                    _ = addMethod.Invoke(form, parameters);
                    FieldInfo field = type.GetField("CallBack_GetOBC_RGB", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        object value = field.GetValue(form);
                        if (value is not null and not Delegate)
                        {
                        }
                    }
                }
            }
            else
            {
                form = (Form)Activator.CreateInstance(type, m_BoardComm, m_chipID, apiVersion);
            }
        }
        catch (Exception)
        {
            form = (Form)Activator.CreateInstance(type);
        }
        form.Show();
    }

    private void toolCbxApiVersion_SelectedIndexChanged(object sender, EventArgs e)
    {
        apiVersion = (ApiVersion)toolCbxApiVersion.SelectedIndex;
        if (m_BoardComm.IsConnected())
        {
            SetApiVersion(apiVersion);
        }
        else
        {
            ChangeXmlType(m_chipID, apiVersion);
        }
    }

    private void buttonIQIndex_Click(object sender, EventArgs e)
    {
        if (m_BoardComm.IsConnected())
        {
            short apiIDValue = m_BoardComm.GetApiIDValue(m_chipID, "GetIQIndex");
            if (apiIDValue > 0)
            {
                m_BoardComm.updateTaskQueue(GetIQIndexTask, apiIDValue);
            }
        }
    }

    public void GetIQIndexTask(object obj)
    {
        short apiId = (short)obj;
        byte[] pbyRcvData = null;
        byte[] bufferInitial = new byte[10];
        switch (m_BoardComm.ConnectMode)
        {
            case CONNECT_MODE.MODE_USB:
                _ = m_BoardComm.ReceiveUSBInitialApiPacket(apiId, bufferInitial, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                _ = m_BoardComm.ReceiveUartApiPacket(apiId, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                _ = m_BoardComm.ReceiveApiPacket(apiId, out pbyRcvData);
                break;
        }
        PacketParser.ConvertBufferToIQIndex(ref IQIndex, pbyRcvData);
        _ = Invoke(new ShowMessageHandler(ShowIQIndex), IQIndex.ToString());
    }

    private void panelMainContent_MouseClick(object sender, MouseEventArgs e)
    {
        _ = panelMainContent.Focus();
    }

    private void GUIShowMessage(string strMsg)
    {
        m_GuiContainer.ShowMessage("", strMsg);
    }

    private void ShowMessageBox(string strMsg)
    {
        _ = MessageBox.Show(strMsg, "Status", MessageBoxButtons.OK);
    }

    private void ShowIQIndex(string strMsg)
    {
        textBoxIQIndex.Text = strMsg;
    }

    public void SetPageActionMode(PAGE_ACTION action)
    {
        btnWritePage.Enabled = action is PAGE_ACTION.W or PAGE_ACTION.RW;
        btnReadPage.Enabled = action is PAGE_ACTION.R or PAGE_ACTION.RW;
    }

    public void SetBackStepMode(bool mode)
    {
        toolBtnStepBack.Enabled = mode;
    }

    public void SetNextStepMode(bool mode)
    {
        toolBtnStepNext.Enabled = mode;
    }

    public void SetAutoWriteMode(bool Mode)
    {
        chkAutoWrite.Enabled = Mode;
    }

    public void CompressGZipfile(string src, string zip)
    {
        FileStream fileStream = File.OpenRead(src);
        FileStream fileStream2 = File.Create(zip);
        GZipStream gZipStream = new(fileStream2, CompressionMode.Compress);
        byte[] array = new byte[2048];
        int count;
        while ((count = fileStream.Read(array, 0, array.Length)) != 0)
        {
            gZipStream.Write(array, 0, count);
        }
        fileStream.Close();
        gZipStream.Close();
        fileStream2.Close();
    }

    private void savebinXmlFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        toolBtnReadAll_Click(sender, e);
        SaveFileDialog saveFileDialog = new()
        {
            Filter = "Bin file (*.bin)|*.bin",
            InitialDirectory = Application.StartupPath
        };
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                byte[] binBytes = m_GuiContainer.GetBinBytes(m_chipID, magicKey, m_BinChecksumVer);
                string text = Application.StartupPath + "/XmlTmp.xml";
                string text2 = Application.StartupPath + "/XmlTmp.zip";
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
                if (File.Exists(text2))
                {
                    File.Delete(text2);
                }
                _ = m_GuiContainer.SaveXml(text);
                CompressGZipfile(text, text2);
                byte[] array = File.ReadAllBytes(text2);
                BinaryWriter binaryWriter = new(File.Open(saveFileDialog.FileName, FileMode.Create));
                byte[] array2 = new byte[binBytes.Length + array.Length];
                binBytes.CopyTo(array2, 0);
                array.CopyTo(array2, binBytes.Length);
                binaryWriter.Write(array2);
                binaryWriter.Close();
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
                if (File.Exists(text2))
                {
                    File.Delete(text2);
                }
            }
            catch (Exception)
            {
            }
        }
        if (!saveFileDialog.FileName.Equals(""))
        {
            _ = MessageBox.Show("Save File " + saveFileDialog.FileName + " Success.", "Save Parameters File", MessageBoxButtons.OK);
        }
    }

    private void convertToolStripMenuItem_Click(object sender, EventArgs e)
    {
        FormConvert formConvert = new();
        formConvert.SetDebugParser(m_BoardComm);
        formConvert.m_chipID = m_chipID;
        formConvert.apiVersion = apiVersion;
        _ = formConvert.ShowDialog();
    }

    private void btnUartConn_Click(object sender, EventArgs e)
    {
        if (m_BoardComm.ConnectMode == CONNECT_MODE.MODE_UART)
        {
            if (!m_BoardUart.IsConnected())
            {
                string text = m_BoardComm.connectUartPage(textBoxUartPort.Text);
                if (text != "")
                {
                    _ = MessageBox.Show(text);
                    return;
                }
                m_BoardUart.UartConnected();
                toolBtnConnect.Enabled = false;
                btnUartConn.Image = ExtensionMethods.GetImageByName("transfer.png");
                _ = Invoke(new UpdateConnectFinishHandler(ConnectToBoardFinish), "Connect UART Comm.!");
            }
            else
            {
                toolBtnConnect.Enabled = true;
                m_BoardComm.disconnectUartPage();
                m_BoardUart.DisConnected();
                btnUartConn.Image = ExtensionMethods.GetImageByName("refresh.png");
                _ = MessageBox.Show("DisConnect Uart!");
            }
            return;
        }
        string text2 = m_BoardComm.USBConnect();
        if (text2 == "")
        {
            if (m_BoardComm.IsConnected())
            {
                toolBtnConnect.Enabled = true;
                btnUartConn.Image = ExtensionMethods.GetImageByName("refresh.png");
                _ = Invoke(new UpdateConnectFinishHandler(ConnectToBoardFinish), "Connect USB Comm.!");
            }
            else
            {
                toolBtnConnect.Enabled = false;
                btnUartConn.Image = ExtensionMethods.GetImageByName("transfer.png");
                _ = MessageBox.Show("DisConnect USB Comm.!");
            }
        }
        else
        {
            _ = MessageBox.Show("[Error] USB Connect message : %s", text2);
        }
    }

    private void uARTLogToolStripMenuItem_Click(object sender, EventArgs e)
    {
        FormUART formUART = new();
        _ = formUART.ShowDialog();
        _ = formUART.Focus();
    }

    private void ChannelIDComboBox_Click(object sender, EventArgs e)
    {
    }

    private void ChannelIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        int num = channelID;
        channelID = (ushort)ChannelIDComboBox.SelectedIndex;
        SetChannelIDByAPI(101);
        if (m_BoardComm.IsConnected())
        {
            DialogResult dialogResult = MessageBox.Show("Channel = " + num + " - IQ API Parameter has been changed. Please save parameters.", Text, MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                m_bSaveBinXml = false;
                SaveTunningParmToBin(m_bSaveBinXml);
            }
            else
            {
                dialogResult = MessageBox.Show("Channel = " + num + " - IQ API parameter are not retained.", Text, MessageBoxButtons.YesNo);
            }
            if (dialogResult == DialogResult.Yes)
            {
                toolBtnReadAll_Click(sender, e);
            }
        }
    }

    private void DeviceIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        int deviceID = DeviceID;
        DeviceID = (ushort)DeviceIDComboBox.SelectedIndex;
        SetDeviceIDByAPI(104);
        if (m_BoardComm.IsConnected())
        {
            DialogResult dialogResult = MessageBox.Show("Device = " + deviceID + " - IQ API Parameter has been changed. Please save parameters.", Text, MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                m_bSaveBinXml = false;
                SaveTunningParmToBin(m_bSaveBinXml);
            }
            else
            {
                dialogResult = MessageBox.Show("Device = " + deviceID + " - IQ API parameter are not retained.", Text, MessageBoxButtons.YesNo);
            }
            if (dialogResult == DialogResult.Yes)
            {
                toolBtnReadAll_Click(sender, e);
            }
        }
    }

    private void autoSaveBinFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        FormAutoSaveBinFile formAutoSaveBinFile = new(SaveBinFile_Interval, autoSaveBin)
        {
            StartPosition = FormStartPosition.CenterScreen
        };
        formAutoSaveBinFile.Show();
    }

    private void autoSaveBin(uint Times)
    {
        SaveBinFile_Interval = Times;
        SaveBinFile_Interval_sec = Times * 60;
        if (SaveBinFile_Interval == 0)
        {
            StopTimer();
        }
        else
        {
            RunTimer();
        }
    }

    private void _TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        m_bSaveBinXml = false;
        StatuslabelTimer.Text = "Auto Save Bin File Remain(sec) - " + SaveBinFile_Interval_sec;
        SaveBinFile_Interval_sec--;
        if (SaveBinFile_Interval_sec == 0)
        {
            AutoSaveTunningParmToBin(m_bSaveBinXml);
            SaveBinFile_Interval_sec = SaveBinFile_Interval * 60;
        }
    }

    private void GetChipName(ChipID chipId, ref string chipName)
    {
        chipName = m_chipdef.ContainsKey((ulong)chipId) ? m_chipdef[(ulong)chipId] : "UNKNOW_" + chipId;
    }

    private string GetConnectModeString(CONNECT_MODE connect_mode)
    {
        string result = "";
        switch (connect_mode)
        {
            case CONNECT_MODE.MODE_SOCKET:
                result = "Network";
                break;
            case CONNECT_MODE.MODE_UART:
                result = "UART";
                break;
            case CONNECT_MODE.MODE_USB:
                result = "UVC";
                break;
        }
        return result;
    }

    public int GetChipFunction(ChipID chipId, CONNECT_MODE connect_mode, ref Function chipFunction)
    {
        int result = -1;
        string text = GetConnectModeString(connect_mode);
        Chiplist[] chipList = m_json_config.ChipList;
        foreach (Chiplist chiplist in chipList)
        {
            if ((ulong)chipId != chiplist.id)
            {
                continue;
            }
            Function[] function = chiplist.Function;
            foreach (Function function2 in function)
            {
                if (function2.Protocol == text)
                {
                    result = 0;
                    chipFunction = function2;
                    break;
                }
            }
            break;
        }
        return result;
    }

    private void RunTimer()
    {
        SaveBinFile_Interval_sec = SaveBinFile_Interval * 60;
        StatuslabelTimer.Text = "Auto Save Bin File Remain(sec) - " + SaveBinFile_Interval_sec;
        mTimersTimer.Interval = 1000.0;
        mTimersTimer.Start();
    }

    private void StopTimer()
    {
        mTimersTimer.Stop();
        StatuslabelTimer.Text = "Auto Save Bin File Remain(sec)";
    }

    private void SensorPadIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        sensorPadID = (ushort)SensorPadIDComboBox.SelectedIndex;
        _ = SetSensorPadIDByAPI(sensorPadID);
        GetSensorInfoByChipID();
    }

    private static void CallBack_GetOBC_RGB(ref int ret, ref int R, ref int Gr, ref int Gb, ref int B)
    {
        ret = m_GuiContainer.GetOBCRGB(ref R, ref Gr, ref Gb, ref B);
    }

    private void button_One_Click(object sender, EventArgs e)
    {
        m_GuiContainer.OneParam();
    }

    private void button_Two_Click(object sender, EventArgs e)
    {
        m_GuiContainer.TwoParam();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        TreeNode treeNode = new("Node");
        menuStripMain = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        newFileToolStripMenuItem = new ToolStripMenuItem();
        newParamFileToolStripMenuItem = new ToolStripMenuItem();
        openFileToolStripMenuItem = new ToolStripMenuItem();
        openParamFileToolStripMenuItem = new ToolStripMenuItem();
        openBinFileToolStripMenuItem = new ToolStripMenuItem();
        openBinXmlFileToolStripMenuItem = new ToolStripMenuItem();
        saveFileToolStripMenuItem = new ToolStripMenuItem();
        saveParamFileToolStripMenuItem = new ToolStripMenuItem();
        saveBinFileToolStripMenuItem = new ToolStripMenuItem();
        saveBinXmlFileToolStripMenuItem = new ToolStripMenuItem();
        settingToolStripMenuItem = new ToolStripMenuItem();
        setMagicKeyToolStripMenuItem = new ToolStripMenuItem();
        autoSaveBinFileToolStripMenuItem = new ToolStripMenuItem();
        helpToolStripMenuItem = new ToolStripMenuItem();
        aboutToolStripMenuItem = new ToolStripMenuItem();
        viewToolStripMenuItem = new ToolStripMenuItem();
        liveViewToolStripMenuItem = new ToolStripMenuItem();
        convertToolStripMenuItem = new ToolStripMenuItem();
        uARTLogToolStripMenuItem = new ToolStripMenuItem();
        xUCommandToolStripMenuItem = new ToolStripMenuItem();
        burnFWToolStripMenuItem = new ToolStripMenuItem();
        sendIQDataToolStripMenuItem = new ToolStripMenuItem();
        getIQDataToolStripMenuItem = new ToolStripMenuItem();
        txbMsgLog = new TextBox();
        statusStripMain = new StatusStrip();
        statusLabelInfo = new ToolStripStatusLabel();
        statusLabelConnection = new ToolStripStatusLabel();
        StatuslabelTimer = new ToolStripStatusLabel();
        splitConItemPage = new SplitContainer();
        btnPageItemCollapse = new Button();
        treePageItem = new TreeView();
        splitConItemMain = new SplitContainer();
        panelMainTitle = new Panel();
        buttonIQIndex = new Button();
        textBoxIQIndex = new TextBox();
        labelIQIndex = new Label();
        chkAutoWrite = new CheckBox();
        btnWritePage = new Button();
        btnReadPage = new Button();
        labelMainTitle = new Label();
        panelMainContent = new Panel();
        btnPageItemExpand = new Button();
        panelLogAction = new Panel();
        toolStripMain = new ToolStrip();
        toolBtnConnect = new ToolStripButton();
        toolLabelClientHostName = new ToolStripLabel();
        toolTxbClientHostName = new ToolStripTextBox();
        toolStripLabel1 = new ToolStripLabel();
        DeviceIDComboBox = new ToolStripComboBox();
        toolLabelChannelID = new ToolStripLabel();
        ChannelIDComboBox = new ToolStripComboBox();
        toolLabelSensorPadID = new ToolStripLabel();
        SensorPadIDComboBox = new ToolStripComboBox();
        toolLabelPort = new ToolStripLabel();
        toolTxbClientPort = new ToolStripTextBox();
        toolLabelID = new ToolStripLabel();
        toolTxbClientID = new ToolStripTextBox();
        toolBtnStepBack = new ToolStripButton();
        toolBtnStepNext = new ToolStripButton();
        toolBtnReadAll = new ToolStripButton();
        toolBtnWriteAll = new ToolStripButton();
        toolCbxPlugIn = new ToolStripComboBox();
        toolBtnGetRawImage = new ToolStripButton();
        toolBtnGetRawStream = new ToolStripButton();
        toolBtnGetIspYuv = new ToolStripButton();
        toolBtnGetJpeg = new ToolStripButton();
        toolGetScalarOutImage = new ToolStripButton();
        toolCbxApiVersion = new ToolStripComboBox();
        labelUart = new Label();
        textBoxUartPort = new TextBox();
        btnUartConn = new Button();
        button_One = new Button();
        button_Two = new Button();
        menuStripMain.SuspendLayout();
        statusStripMain.SuspendLayout();
        ((ISupportInitialize)splitConItemPage).BeginInit();
        splitConItemPage.Panel1.SuspendLayout();
        splitConItemPage.Panel2.SuspendLayout();
        splitConItemPage.SuspendLayout();
        ((ISupportInitialize)splitConItemMain).BeginInit();
        splitConItemMain.Panel1.SuspendLayout();
        splitConItemMain.Panel2.SuspendLayout();
        splitConItemMain.SuspendLayout();
        panelMainTitle.SuspendLayout();
        toolStripMain.SuspendLayout();
        SuspendLayout();
        menuStripMain.Items.AddRange(new ToolStripItem[7] { fileToolStripMenuItem, settingToolStripMenuItem, helpToolStripMenuItem, viewToolStripMenuItem, convertToolStripMenuItem, uARTLogToolStripMenuItem, xUCommandToolStripMenuItem });
        menuStripMain.Location = new Point(0, 0);
        menuStripMain.Name = "menuStripMain";
        menuStripMain.Padding = new Padding(7, 3, 0, 3);
        menuStripMain.Size = new Size(1267, 27);
        menuStripMain.TabIndex = 2;
        menuStripMain.Text = "MainMenu";
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[3] { newFileToolStripMenuItem, openFileToolStripMenuItem, saveFileToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(39, 21);
        fileToolStripMenuItem.Text = "File";
        newFileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1] { newParamFileToolStripMenuItem });
        newFileToolStripMenuItem.Name = "newFileToolStripMenuItem";
        newFileToolStripMenuItem.Size = new Size(108, 22);
        newFileToolStripMenuItem.Text = "New";
        newParamFileToolStripMenuItem.Name = "newParamFileToolStripMenuItem";
        newParamFileToolStripMenuItem.Size = new Size(134, 22);
        newParamFileToolStripMenuItem.Text = "Param file";
        newParamFileToolStripMenuItem.Click += new EventHandler(newParamFileToolStripMenuItem_Click);
        openFileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[3] { openParamFileToolStripMenuItem, openBinFileToolStripMenuItem, openBinXmlFileToolStripMenuItem });
        openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
        openFileToolStripMenuItem.Size = new Size(108, 22);
        openFileToolStripMenuItem.Text = "Open";
        openParamFileToolStripMenuItem.Name = "openParamFileToolStripMenuItem";
        openParamFileToolStripMenuItem.Size = new Size(141, 22);
        openParamFileToolStripMenuItem.Text = "Param file";
        openParamFileToolStripMenuItem.Click += new EventHandler(openParamFileToolStripMenuItem_Click);
        openBinFileToolStripMenuItem.Name = "openBinFileToolStripMenuItem";
        openBinFileToolStripMenuItem.Size = new Size(141, 22);
        openBinFileToolStripMenuItem.Text = "Bin file";
        openBinFileToolStripMenuItem.Click += new EventHandler(openBinFileToolStripMenuItem_Click);
        openBinXmlFileToolStripMenuItem.Name = "openBinXmlFileToolStripMenuItem";
        openBinXmlFileToolStripMenuItem.Size = new Size(141, 22);
        openBinXmlFileToolStripMenuItem.Text = "Bin Xml file";
        openBinXmlFileToolStripMenuItem.Click += new EventHandler(openBinXmlFileToolStripMenuItem_Click);
        saveFileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[3] { saveParamFileToolStripMenuItem, saveBinFileToolStripMenuItem, saveBinXmlFileToolStripMenuItem });
        saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
        saveFileToolStripMenuItem.Size = new Size(108, 22);
        saveFileToolStripMenuItem.Text = "Save";
        saveParamFileToolStripMenuItem.Name = "saveParamFileToolStripMenuItem";
        saveParamFileToolStripMenuItem.Size = new Size(141, 22);
        saveParamFileToolStripMenuItem.Text = "Param file";
        saveParamFileToolStripMenuItem.Click += new EventHandler(saveParamFileToolStripMenuItem_Click);
        saveBinFileToolStripMenuItem.Name = "saveBinFileToolStripMenuItem";
        saveBinFileToolStripMenuItem.Size = new Size(141, 22);
        saveBinFileToolStripMenuItem.Text = "Bin file";
        saveBinFileToolStripMenuItem.Click += new EventHandler(saveBinFileToolStripMenuItem_Click);
        saveBinXmlFileToolStripMenuItem.Name = "saveBinXmlFileToolStripMenuItem";
        saveBinXmlFileToolStripMenuItem.Size = new Size(141, 22);
        saveBinXmlFileToolStripMenuItem.Text = "Bin Xml file";
        saveBinXmlFileToolStripMenuItem.Click += new EventHandler(saveBinXmlFileToolStripMenuItem_Click);
        settingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2] { setMagicKeyToolStripMenuItem, autoSaveBinFileToolStripMenuItem });
        settingToolStripMenuItem.Name = "settingToolStripMenuItem";
        settingToolStripMenuItem.Size = new Size(60, 21);
        settingToolStripMenuItem.Text = "Setting";
        setMagicKeyToolStripMenuItem.Name = "setMagicKeyToolStripMenuItem";
        setMagicKeyToolStripMenuItem.Size = new Size(179, 22);
        setMagicKeyToolStripMenuItem.Text = "Magic Key";
        setMagicKeyToolStripMenuItem.Click += new EventHandler(setMagicKeyToolStripMenuItem_Click);
        autoSaveBinFileToolStripMenuItem.Name = "autoSaveBinFileToolStripMenuItem";
        autoSaveBinFileToolStripMenuItem.Size = new Size(179, 22);
        autoSaveBinFileToolStripMenuItem.Text = "Auto Save Bin File";
        autoSaveBinFileToolStripMenuItem.Click += new EventHandler(autoSaveBinFileToolStripMenuItem_Click);
        helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1] { aboutToolStripMenuItem });
        helpToolStripMenuItem.Name = "helpToolStripMenuItem";
        helpToolStripMenuItem.Size = new Size(47, 21);
        helpToolStripMenuItem.Text = "Help";
        aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
        aboutToolStripMenuItem.Size = new Size(111, 22);
        aboutToolStripMenuItem.Text = "About";
        aboutToolStripMenuItem.Click += new EventHandler(aboutToolStripMenuItem_Click);
        viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1] { liveViewToolStripMenuItem });
        viewToolStripMenuItem.Name = "viewToolStripMenuItem";
        viewToolStripMenuItem.Size = new Size(47, 21);
        viewToolStripMenuItem.Text = "View";
        viewToolStripMenuItem.Visible = false;
        liveViewToolStripMenuItem.Name = "liveViewToolStripMenuItem";
        liveViewToolStripMenuItem.Size = new Size(129, 22);
        liveViewToolStripMenuItem.Text = "Live View";
        liveViewToolStripMenuItem.Click += new EventHandler(liveViewToolStripMenuItem_Click);
        convertToolStripMenuItem.Name = "convertToolStripMenuItem";
        convertToolStripMenuItem.Size = new Size(65, 21);
        convertToolStripMenuItem.Text = "Convert";
        convertToolStripMenuItem.Click += new EventHandler(convertToolStripMenuItem_Click);
        convertToolStripMenuItem.Visible = false;
        uARTLogToolStripMenuItem.Name = "uARTLogToolStripMenuItem";
        uARTLogToolStripMenuItem.Size = new Size(75, 21);
        uARTLogToolStripMenuItem.Text = "UART log";
        uARTLogToolStripMenuItem.Click += new EventHandler(uARTLogToolStripMenuItem_Click);
        xUCommandToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[3] { burnFWToolStripMenuItem, sendIQDataToolStripMenuItem, getIQDataToolStripMenuItem });
        xUCommandToolStripMenuItem.Name = "xUCommandToolStripMenuItem";
        xUCommandToolStripMenuItem.Size = new Size(101, 21);
        xUCommandToolStripMenuItem.Text = "XU Command";
        xUCommandToolStripMenuItem.Visible = false;
        burnFWToolStripMenuItem.Name = "burnFWToolStripMenuItem";
        burnFWToolStripMenuItem.Size = new Size(154, 22);
        burnFWToolStripMenuItem.Text = "BurnFW";
        burnFWToolStripMenuItem.Click += new EventHandler(burnFWToolStripMenuItem_Click);
        sendIQDataToolStripMenuItem.Name = "sendIQDataToolStripMenuItem";
        sendIQDataToolStripMenuItem.Size = new Size(154, 22);
        sendIQDataToolStripMenuItem.Text = "Send IQ Data";
        sendIQDataToolStripMenuItem.Click += new EventHandler(sendIQDataToolStripMenuItem_Click);
        getIQDataToolStripMenuItem.Name = "getIQDataToolStripMenuItem";
        getIQDataToolStripMenuItem.Size = new Size(154, 22);
        getIQDataToolStripMenuItem.Text = "Get IQ Data";
        getIQDataToolStripMenuItem.Click += new EventHandler(getIQDataToolStripMenuItem_Click);
        txbMsgLog.Location = new Point(3, 29);
        txbMsgLog.Margin = new Padding(3, 4, 3, 4);
        txbMsgLog.Multiline = true;
        txbMsgLog.Name = "txbMsgLog";
        txbMsgLog.ScrollBars = ScrollBars.Both;
        txbMsgLog.Size = new Size(977, 165);
        txbMsgLog.TabIndex = 6;
        statusStripMain.Items.AddRange(new ToolStripItem[3] { statusLabelInfo, statusLabelConnection, StatuslabelTimer });
        statusStripMain.Location = new Point(0, 715);
        statusStripMain.Name = "statusStripMain";
        statusStripMain.Padding = new Padding(1, 0, 16, 0);
        statusStripMain.Size = new Size(1267, 26);
        statusStripMain.TabIndex = 8;
        statusStripMain.Text = "statusStrip1";
        statusLabelInfo.AutoSize = false;
        statusLabelInfo.BorderStyle = Border3DStyle.Etched;
        statusLabelInfo.Name = "statusLabelInfo";
        statusLabelInfo.Size = new Size(480, 21);
        statusLabelInfo.Text = "Welcome to SigmaStar IPCam IQ Tool";
        statusLabelInfo.TextAlign = ContentAlignment.MiddleLeft;
        statusLabelConnection.BorderSides = ToolStripStatusLabelBorderSides.Left;
        statusLabelConnection.BorderStyle = Border3DStyle.Bump;
        statusLabelConnection.Name = "statusLabelConnection";
        statusLabelConnection.Size = new Size(187, 21);
        statusLabelConnection.Text = "Client Port Status - Disconnect";
        StatuslabelTimer.BorderSides = ToolStripStatusLabelBorderSides.Left;
        StatuslabelTimer.BorderStyle = Border3DStyle.Bump;
        StatuslabelTimer.Name = "StatuslabelTimer";
        StatuslabelTimer.Size = new Size(189, 21);
        StatuslabelTimer.Text = "Auto Save Bin File Remain(sec)";
        splitConItemPage.BorderStyle = BorderStyle.FixedSingle;
        splitConItemPage.FixedPanel = FixedPanel.Panel1;
        splitConItemPage.IsSplitterFixed = true;
        splitConItemPage.Location = new Point(0, 68);
        splitConItemPage.Margin = new Padding(3, 4, 3, 4);
        splitConItemPage.Name = "splitConItemPage";
        splitConItemPage.Panel1.Controls.Add(btnPageItemCollapse);
        splitConItemPage.Panel1.Controls.Add(treePageItem);
        splitConItemPage.Panel2.Controls.Add(splitConItemMain);
        splitConItemPage.Size = new Size(1176, 686);
        splitConItemPage.SplitterDistance = 160;
        splitConItemPage.SplitterWidth = 5;
        splitConItemPage.TabIndex = 10;
        btnPageItemCollapse.BackColor = SystemColors.Window;
        btnPageItemCollapse.Cursor = Cursors.Hand;
        btnPageItemCollapse.FlatAppearance.BorderSize = 0;
        btnPageItemCollapse.FlatStyle = FlatStyle.Flat;
        btnPageItemCollapse.Image = ExtensionMethods.GetImageByName("arrowLeft.png");
        btnPageItemCollapse.Location = new Point(136, 224);
        btnPageItemCollapse.Margin = new Padding(3, 4, 3, 4);
        btnPageItemCollapse.Name = "btnPageItemCollapse";
        btnPageItemCollapse.Size = new Size(19, 63);
        btnPageItemCollapse.TabIndex = 0;
        btnPageItemCollapse.UseVisualStyleBackColor = false;
        btnPageItemCollapse.Click += new EventHandler(btnPageItemCollapse_Click);
        treePageItem.Font = new Font("Times New Roman", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
        treePageItem.Location = new Point(1, 1);
        treePageItem.Margin = new Padding(3, 4, 3, 4);
        treePageItem.Name = "treePageItem";
        treePageItem.Nodes.AddRange(new TreeNode[] { treeNode });
        treePageItem.Size = new Size(156, 680);
        treePageItem.TabIndex = 0;
        splitConItemMain.BorderStyle = BorderStyle.FixedSingle;
        splitConItemMain.Dock = DockStyle.Fill;
        splitConItemMain.Location = new Point(0, 0);
        splitConItemMain.Margin = new Padding(3, 4, 3, 4);
        splitConItemMain.Name = "splitConItemMain";
        splitConItemMain.Orientation = Orientation.Horizontal;
        splitConItemMain.Panel1.Controls.Add(panelMainTitle);
        splitConItemMain.Panel1.Controls.Add(panelMainContent);
        splitConItemMain.Panel1.Controls.Add(btnPageItemExpand);
        splitConItemMain.Panel2.Controls.Add(panelLogAction);
        splitConItemMain.Panel2.Controls.Add(txbMsgLog);
        splitConItemMain.Size = new Size(1011, 686);
        splitConItemMain.SplitterDistance = 484;
        splitConItemMain.SplitterWidth = 5;
        splitConItemMain.TabIndex = 0;
        splitConItemMain.SplitterMoved += new SplitterEventHandler(splitConItemMain_Moved);
        panelMainTitle.AutoSize = true;
        panelMainTitle.BackColor = Color.LightYellow;
        panelMainTitle.BorderStyle = BorderStyle.FixedSingle;
        panelMainTitle.Controls.Add(button_Two);
        panelMainTitle.Controls.Add(button_One);
        panelMainTitle.Controls.Add(buttonIQIndex);
        panelMainTitle.Controls.Add(textBoxIQIndex);
        panelMainTitle.Controls.Add(labelIQIndex);
        panelMainTitle.Controls.Add(chkAutoWrite);
        panelMainTitle.Controls.Add(btnWritePage);
        panelMainTitle.Controls.Add(btnReadPage);
        panelMainTitle.Controls.Add(labelMainTitle);
        panelMainTitle.ForeColor = SystemColors.Control;
        panelMainTitle.Location = new Point(0, 0);
        panelMainTitle.Margin = new Padding(3, 4, 3, 4);
        panelMainTitle.Name = "panelMainTitle";
        panelMainTitle.Size = new Size(1007, 32);
        panelMainTitle.TabIndex = 2;
        buttonIQIndex.BackColor = SystemColors.Control;
        buttonIQIndex.ForeColor = SystemColors.ControlText;
        buttonIQIndex.Location = new Point(91, 3);
        buttonIQIndex.Name = "buttonIQIndex";
        buttonIQIndex.Size = new Size(61, 23);
        buttonIQIndex.TabIndex = 6;
        buttonIQIndex.Text = "Refresh";
        buttonIQIndex.UseVisualStyleBackColor = true;
        buttonIQIndex.Click += new EventHandler(buttonIQIndex_Click);
        textBoxIQIndex.BackColor = SystemColors.Control;
        textBoxIQIndex.ForeColor = SystemColors.ControlText;
        textBoxIQIndex.Location = new Point(46, 4);
        textBoxIQIndex.Name = "textBoxIQIndex";
        textBoxIQIndex.ReadOnly = true;
        textBoxIQIndex.Size = new Size(34, 21);
        textBoxIQIndex.TabIndex = 5;
        labelIQIndex.AutoSize = true;
        labelIQIndex.BackColor = Color.LightYellow;
        labelIQIndex.ForeColor = SystemColors.ControlText;
        labelIQIndex.Location = new Point(4, 8);
        labelIQIndex.Name = "labelIQIndex";
        labelIQIndex.Size = new Size(39, 15);
        labelIQIndex.TabIndex = 4;
        labelIQIndex.Text = "Index :";
        chkAutoWrite.AutoSize = true;
        chkAutoWrite.BackColor = Color.LightYellow;
        chkAutoWrite.ForeColor = SystemColors.ControlText;
        chkAutoWrite.Location = new Point(906, 8);
        chkAutoWrite.Name = "chkAutoWrite";
        chkAutoWrite.Size = new Size(81, 19);
        chkAutoWrite.TabIndex = 3;
        chkAutoWrite.Text = "Auto Write";
        chkAutoWrite.UseVisualStyleBackColor = false;
        chkAutoWrite.CheckedChanged += new EventHandler(checkAutoWrite_CheckedChanged);
        btnWritePage.BackColor = SystemColors.Control;
        btnWritePage.ForeColor = SystemColors.ControlText;
        btnWritePage.Location = new Point(810, 4);
        btnWritePage.Name = "btnWritePage";
        btnWritePage.Size = new Size(80, 23);
        btnWritePage.TabIndex = 2;
        btnWritePage.Text = "Write Page";
        btnWritePage.UseVisualStyleBackColor = false;
        btnWritePage.Click += new EventHandler(btnWritePage_Click);
        btnReadPage.BackColor = SystemColors.Control;
        btnReadPage.ForeColor = SystemColors.ControlText;
        btnReadPage.Location = new Point(720, 4);
        btnReadPage.Name = "btnReadPage";
        btnReadPage.Size = new Size(80, 23);
        btnReadPage.TabIndex = 1;
        btnReadPage.Text = "Read Page";
        btnReadPage.UseVisualStyleBackColor = false;
        btnReadPage.Click += new EventHandler(btnReadPage_Click);
        labelMainTitle.AutoSize = true;
        labelMainTitle.Font = new Font("Times New Roman", 14.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelMainTitle.ForeColor = Color.Blue;
        labelMainTitle.Location = new Point(174, 3);
        labelMainTitle.Name = "labelMainTitle";
        labelMainTitle.Size = new Size(130, 22);
        labelMainTitle.TabIndex = 0;
        labelMainTitle.Text = "labelMainTitle";
        panelMainContent.AutoScroll = true;
        panelMainContent.BackColor = SystemColors.Window;
        panelMainContent.Location = new Point(22, 32);
        panelMainContent.Margin = new Padding(3, 4, 3, 4);
        panelMainContent.Name = "panelMainContent";
        panelMainContent.Padding = new Padding(20);
        panelMainContent.Size = new Size(1300, 445);
        panelMainContent.TabIndex = 1;
        panelMainContent.MouseClick += new MouseEventHandler(panelMainContent_MouseClick);
        btnPageItemExpand.FlatAppearance.BorderSize = 0;
        btnPageItemExpand.FlatStyle = FlatStyle.Flat;
        btnPageItemExpand.Image = ExtensionMethods.GetImageByName("arrowRight.png");
        btnPageItemExpand.Location = new Point(0, 224);
        btnPageItemExpand.Margin = new Padding(3, 4, 3, 4);
        btnPageItemExpand.Name = "btnPageItemExpand";
        btnPageItemExpand.Size = new Size(21, 63);
        btnPageItemExpand.TabIndex = 0;
        btnPageItemExpand.UseVisualStyleBackColor = true;
        btnPageItemExpand.Click += new EventHandler(btnPageItemExpand_Click);
        panelLogAction.Location = new Point(0, 0);
        panelLogAction.Name = "panelLogAction";
        panelLogAction.Size = new Size(980, 30);
        panelLogAction.TabIndex = 7;
        toolStripMain.Items.AddRange(new ToolStripItem[24]
        {
            toolBtnConnect, toolLabelClientHostName, toolTxbClientHostName, toolStripLabel1, DeviceIDComboBox, toolLabelChannelID, ChannelIDComboBox, toolLabelSensorPadID, SensorPadIDComboBox, toolLabelPort,
            toolTxbClientPort, toolLabelID, toolTxbClientID, toolBtnStepBack, toolBtnStepNext, toolBtnReadAll, toolBtnWriteAll, toolCbxPlugIn, toolBtnGetRawImage, toolBtnGetRawStream,
            toolBtnGetIspYuv, toolBtnGetJpeg, toolGetScalarOutImage, toolCbxApiVersion
        });
        toolStripMain.Location = new Point(0, 27);
        toolStripMain.Name = "toolStripMain";
        toolStripMain.Size = new Size(1267, 39);
        toolStripMain.TabIndex = 11;
        toolStripMain.Text = "toolStripMain";
        toolBtnConnect.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnConnect.Image = ExtensionMethods.GetImageByName("connect.png");
        toolBtnConnect.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnConnect.ImageTransparentColor = Color.Magenta;
        toolBtnConnect.Name = "toolBtnConnect";
        toolBtnConnect.Size = new Size(36, 36);
        toolBtnConnect.Text = "Connect";
        toolBtnConnect.Click += new EventHandler(toolBtnConnect_Click);
        toolLabelClientHostName.Font = new Font("Times New Roman", 10f, FontStyle.Bold);
        toolLabelClientHostName.Name = "toolLabelClientHostName";
        toolLabelClientHostName.Size = new Size(80, 36);
        toolLabelClientHostName.Text = "Host Name:";
        toolTxbClientHostName.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        toolTxbClientHostName.Name = "toolTxbClientHostName";
        toolTxbClientHostName.Size = new Size(116, 39);
        toolTxbClientHostName.Text = "192.168.1.10";
        toolTxbClientHostName.KeyPress += new KeyPressEventHandler(toolTxbClientHostName_KeyPress);
        toolStripLabel1.Font = new Font("Times New Roman", 9.75f, FontStyle.Bold);
        toolStripLabel1.Name = "toolStripLabel1";
        toolStripLabel1.Size = new Size(69, 36);
        toolStripLabel1.Text = "Device ID : ";
        DeviceIDComboBox.AutoSize = false;
        DeviceIDComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        DeviceIDComboBox.Font = new Font("Times New Roman", 9f);
        DeviceIDComboBox.Items.AddRange(new object[2] { "0", "1" });
        DeviceIDComboBox.Name = "DeviceIDComboBox";
        DeviceIDComboBox.Size = new Size(50, 39);
        DeviceIDComboBox.SelectedIndexChanged += new EventHandler(DeviceIDComboBox_SelectedIndexChanged);
        toolLabelChannelID.Font = new Font("Times New Roman", 9.75f, FontStyle.Bold);
        toolLabelChannelID.Name = "toolLabelChannelID";
        toolLabelChannelID.Size = new Size(79, 36);
        toolLabelChannelID.Text = "Channel ID : ";
        ChannelIDComboBox.AutoSize = false;
        ChannelIDComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        ChannelIDComboBox.Font = new Font("Times New Roman", 9f);
        ChannelIDComboBox.Items.AddRange(new object[16]
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "10", "11", "12", "13", "14", "15"
        });
        ChannelIDComboBox.Name = "ChannelIDComboBox";
        ChannelIDComboBox.Size = new Size(50, 39);
        ChannelIDComboBox.SelectedIndexChanged += new EventHandler(ChannelIDComboBox_SelectedIndexChanged);
        ChannelIDComboBox.Click += new EventHandler(ChannelIDComboBox_Click);
        toolLabelSensorPadID.ActiveLinkColor = Color.Red;
        toolLabelSensorPadID.Font = new Font("Times New Roman", 9.75f, FontStyle.Bold);
        toolLabelSensorPadID.Name = "toolLabelSensorPadID";
        toolLabelSensorPadID.Size = new Size(92, 36);
        toolLabelSensorPadID.Text = "SensorPad ID : ";
        SensorPadIDComboBox.AutoSize = false;
        SensorPadIDComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        SensorPadIDComboBox.Items.AddRange(new object[11]
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "10"
        });
        SensorPadIDComboBox.Name = "SensorPadIDComboBox";
        SensorPadIDComboBox.Size = new Size(50, 25);
        SensorPadIDComboBox.SelectedIndexChanged += new EventHandler(SensorPadIDComboBox_SelectedIndexChanged);
        toolLabelPort.Font = new Font("Times New Roman", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
        toolLabelPort.Name = "toolLabelPort";
        toolLabelPort.Size = new Size(35, 36);
        toolLabelPort.Text = "Port:";
        toolTxbClientPort.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        toolTxbClientPort.Name = "toolTxbClientPort";
        toolTxbClientPort.Size = new Size(50, 39);
        toolTxbClientPort.Text = "9876";
        toolTxbClientPort.KeyPress += new KeyPressEventHandler(toolTxbClientPort_KeyPress);
        toolLabelID.Font = new Font("Times New Roman", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
        toolLabelID.Name = "toolLabelID";
        toolLabelID.Size = new Size(24, 36);
        toolLabelID.Text = "ID:";
        toolTxbClientID.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        toolTxbClientID.Name = "toolTxbClientID";
        toolTxbClientID.Size = new Size(50, 39);
        toolTxbClientID.KeyPress += new KeyPressEventHandler(toolTxbClientID_KeyPress);
        toolBtnStepBack.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnStepBack.Enabled = false;
        toolBtnStepBack.Image = ExtensionMethods.GetImageByName("back.png");
        toolBtnStepBack.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnStepBack.ImageTransparentColor = Color.Magenta;
        toolBtnStepBack.Name = "toolBtnStepBack";
        toolBtnStepBack.Size = new Size(36, 36);
        toolBtnStepBack.Text = "Undo";
        toolBtnStepBack.Click += new EventHandler(toolBtnStepBack_Click);
        toolBtnStepNext.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnStepNext.Enabled = false;
        toolBtnStepNext.Image = ExtensionMethods.GetImageByName("next.png");
        toolBtnStepNext.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnStepNext.ImageTransparentColor = Color.Magenta;
        toolBtnStepNext.Name = "toolBtnStepNext";
        toolBtnStepNext.Size = new Size(36, 36);
        toolBtnStepNext.Text = "Redo";
        toolBtnStepNext.Click += new EventHandler(toolBtnStepNext_Click);
        toolBtnReadAll.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnReadAll.Image = ExtensionMethods.GetImageByName("toolBtnReadAll.png");
        toolBtnReadAll.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnReadAll.ImageTransparentColor = Color.Magenta;
        toolBtnReadAll.Name = "toolBtnReadAll";
        toolBtnReadAll.Size = new Size(36, 36);
        toolBtnReadAll.Text = "Read ALL";
        toolBtnReadAll.Click += new EventHandler(toolBtnReadAll_Click);
        toolBtnWriteAll.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnWriteAll.Image = ExtensionMethods.GetImageByName("toolBtnWriteAll.png");
        toolBtnWriteAll.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnWriteAll.ImageTransparentColor = Color.Magenta;
        toolBtnWriteAll.Name = "toolBtnWriteAll";
        toolBtnWriteAll.Size = new Size(36, 36);
        toolBtnWriteAll.Text = "Write ALL";
        toolBtnWriteAll.Click += new EventHandler(toolBtnWriteAll_Click);
        toolCbxPlugIn.DropDownStyle = ComboBoxStyle.DropDownList;
        toolCbxPlugIn.Items.AddRange(new object[1] { "[Select Plugin]" });
        toolCbxPlugIn.Name = "toolCbxPlugIn";
        toolCbxPlugIn.Size = new Size(121, 39);
        toolCbxPlugIn.SelectedIndexChanged += new EventHandler(toolCbxPlugIn_SelectedIndexChanged);
        toolBtnGetRawImage.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnGetRawImage.Enabled = false;
        toolBtnGetRawImage.Image = ExtensionMethods.GetImageByName("rawimage.png");
        toolBtnGetRawImage.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnGetRawImage.ImageTransparentColor = Color.Magenta;
        toolBtnGetRawImage.Name = "toolBtnGetRawImage";
        toolBtnGetRawImage.Size = new Size(36, 36);
        toolBtnGetRawImage.Text = "Get Raw Image";
        toolBtnGetRawImage.Click += new EventHandler(toolBtnGetRawImage_Click);
        toolBtnGetRawStream.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnGetRawStream.Enabled = false;
        toolBtnGetRawStream.Image = ExtensionMethods.GetImageByName("rawstream.png");
        toolBtnGetRawStream.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnGetRawStream.ImageTransparentColor = Color.Magenta;
        toolBtnGetRawStream.Name = "toolBtnGetRawStream";
        toolBtnGetRawStream.Size = new Size(36, 36);
        toolBtnGetRawStream.Text = "Get Raw Stream Image";
        toolBtnGetRawStream.ToolTipText = "Get Raw Stream Image";
        toolBtnGetRawStream.Click += new EventHandler(toolBtnGetRawStream_Click);
        toolBtnGetIspYuv.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnGetIspYuv.Enabled = false;
        toolBtnGetIspYuv.Image = ExtensionMethods.GetImageByName("yuvimage.png");
        toolBtnGetIspYuv.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnGetIspYuv.ImageTransparentColor = Color.Magenta;
        toolBtnGetIspYuv.Name = "toolBtnGetIspYuv";
        toolBtnGetIspYuv.Size = new Size(36, 36);
        toolBtnGetIspYuv.Text = "Get ISP Output Yuv";
        toolBtnGetIspYuv.Click += new EventHandler(toolBtnGetIspYuv_Click);
        toolBtnGetJpeg.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolBtnGetJpeg.Enabled = false;
        toolBtnGetJpeg.Image = ExtensionMethods.GetImageByName("jpgimage.png");
        toolBtnGetJpeg.ImageScaling = ToolStripItemImageScaling.None;
        toolBtnGetJpeg.ImageTransparentColor = Color.Magenta;
        toolBtnGetJpeg.Name = "toolBtnGetJpeg";
        toolBtnGetJpeg.Size = new Size(36, 36);
        toolBtnGetJpeg.Text = "Get JPEG Image";
        toolBtnGetJpeg.Click += new EventHandler(toolBtnGetJpeg_Click);
        toolGetScalarOutImage.DisplayStyle = ToolStripItemDisplayStyle.Image;
        toolGetScalarOutImage.Enabled = false;
        toolGetScalarOutImage.Image = ExtensionMethods.GetImageByName("scaleimage.png");
        toolGetScalarOutImage.ImageScaling = ToolStripItemImageScaling.None;
        toolGetScalarOutImage.ImageTransparentColor = Color.Magenta;
        toolGetScalarOutImage.Name = "toolGetScalarOutImage";
        toolGetScalarOutImage.Size = new Size(36, 36);
        toolGetScalarOutImage.Text = "Get Scalar Output Image";
        toolGetScalarOutImage.Click += new EventHandler(toolGetScalarOutImage_Click);
        toolCbxApiVersion.DropDownStyle = ComboBoxStyle.DropDownList;
        toolCbxApiVersion.Items.AddRange(new object[2] { "API v1.0", "API v2.0" });
        toolCbxApiVersion.Name = "toolCbxApiVersion";
        toolCbxApiVersion.Size = new Size(100, 25);
        toolCbxApiVersion.SelectedIndexChanged += new EventHandler(toolCbxApiVersion_SelectedIndexChanged);
        labelUart.AutoSize = true;
        labelUart.Font = new Font("Times New Roman", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelUart.Location = new Point(1014, 34);
        labelUart.Name = "labelUart";
        labelUart.Size = new Size(33, 15);
        labelUart.TabIndex = 15;
        labelUart.Text = "Port:";
        textBoxUartPort.Location = new Point(1054, 31);
        textBoxUartPort.Multiline = true;
        textBoxUartPort.Name = "textBoxUartPort";
        textBoxUartPort.Size = new Size(100, 22);
        textBoxUartPort.TabIndex = 16;
        textBoxUartPort.Text = "COM1";
        btnUartConn.Font = new Font("Times New Roman", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
        btnUartConn.ForeColor = Color.FromArgb(192, 0, 192);
        btnUartConn.Image = ExtensionMethods.GetImageByName("refresh.png");
        btnUartConn.Location = new Point(955, 22);
        btnUartConn.Name = "btnUartConn";
        btnUartConn.Size = new Size(40, 40);
        btnUartConn.TabIndex = 13;
        btnUartConn.UseVisualStyleBackColor = true;
        btnUartConn.Click += new EventHandler(btnUartConn_Click);
        button_One.ForeColor = Color.Black;
        button_One.Location = new Point(543, 4);
        button_One.Name = "button_One";
        button_One.Size = new Size(75, 23);
        button_One.TabIndex = 7;
        button_One.Text = "1";
        button_One.UseVisualStyleBackColor = true;
        button_One.Click += new EventHandler(button_One_Click);
        button_Two.ForeColor = Color.Black;
        button_Two.Location = new Point(624, 4);
        button_Two.Name = "button_Two";
        button_Two.Size = new Size(90, 23);
        button_Two.TabIndex = 8;
        button_Two.Text = "2";
        button_Two.UseVisualStyleBackColor = true;
        button_Two.Click += new EventHandler(button_Two_Click);
        AutoScaleDimensions = new SizeF(7f, 15f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1267, 741);
        Controls.Add(textBoxUartPort);
        Controls.Add(labelUart);
        Controls.Add(btnUartConn);
        Controls.Add(toolStripMain);
        Controls.Add(splitConItemPage);
        Controls.Add(statusStripMain);
        Controls.Add(menuStripMain);
        Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        Icon = ExtensionMethods.GetIconByName("sigmastar.ico");
        MainMenuStrip = menuStripMain;
        Margin = new Padding(3, 4, 3, 4);
        MinimumSize = new Size(640, 480);
        Name = "MainForm";
        Text = "IPCam IQ Tool";
        FormClosed += new FormClosedEventHandler(MainForm_FormClosed);
        Load += new EventHandler(MainForm_Load);
        SizeChanged += new EventHandler(MainForm_SizeChanged);
        menuStripMain.ResumeLayout(false);
        menuStripMain.PerformLayout();
        statusStripMain.ResumeLayout(false);
        statusStripMain.PerformLayout();
        splitConItemPage.Panel1.ResumeLayout(false);
        splitConItemPage.Panel2.ResumeLayout(false);
        ((ISupportInitialize)splitConItemPage).EndInit();
        splitConItemPage.ResumeLayout(false);
        splitConItemMain.Panel1.ResumeLayout(false);
        splitConItemMain.Panel1.PerformLayout();
        splitConItemMain.Panel2.ResumeLayout(false);
        splitConItemMain.Panel2.PerformLayout();
        ((ISupportInitialize)splitConItemMain).EndInit();
        splitConItemMain.ResumeLayout(false);
        panelMainTitle.ResumeLayout(false);
        panelMainTitle.PerformLayout();
        toolStripMain.ResumeLayout(false);
        toolStripMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
