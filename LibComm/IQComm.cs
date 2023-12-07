using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SSCamIQTool.LibComm;

public class IQComm
{
    public struct NetTaskContent
    {
        public NetworkMethod run;

        public object obj;
    }

    public delegate void NetworkMethod(object obj);

    private delegate void ShowProgressBarHandler(string str);

    private delegate void UpdateTransProgressByValueHandler(int value);

    private delegate void UpdateTransProgressByNetSpeedHandler(double netSpeed);

    private delegate void UpdateTransFinishHandler(string strMsg);

    public class CALLBACK_DATA_STRUCT
    {
        public double nReceiveByte;

        public IQComm lpIQcomm;

        public Control control;

        public Stopwatch sw;
    }

    public delegate uint callBackHandler(ushort precent, CALLBACK_DATA_STRUCT lpCallbackData);

    private const int IqServerHeaderLen = 8;

    private const int IqServerPayloadLen = 102400;

    private const int RESERVED_VALUE = 0;

    private const int UartBufferLen = 512000;

    private TcpClient m_tcpClient;

    private NetworkStream m_streamClient;

    private DialogProgress m_progressBar;

    private bool m_bConnection;

    private bool m_bTransData;

    private string m_ApplicationPath = "D:\\";

    private byte[] end_of_line = new byte[1] { 13 };

    private ChipID _chipID;

    private CONNECT_MODE _connectMode;

    public Queue<NetTaskContent> netTaskQueue = new Queue<NetTaskContent>();

    public bool _continue = true;

    public string COM_num = "COM4";

    public SerialPort comport = new SerialPort("COM4", 115200, Parity.None, 8, StopBits.One);

    public string message;

    private RingBuff rb = new RingBuff(512000);

    private Thread readThread;

    public unsafe IntPtr m_ait_handel = (IntPtr)(void*)null;

    public bool m_bUSBConnection;

    public bool m_bUARTConnection;

    public ChipID ChipIDName
    {
        get
        {
            return _chipID;
        }
        set
        {
            _chipID = value;
        }
    }

    public CONNECT_MODE ConnectMode
    {
        get
        {
            return _connectMode;
        }
        set
        {
            _connectMode = value;
        }
    }

    public IQConfig m_config { get; set; }

    public Dictionary<ulong, string> m_chiplist { get; set; }

    private string FormatString(string transformString)
    {
        if (string.IsNullOrEmpty(transformString))
        {
            return transformString;
        }
        return transformString.Substring(0, 1).ToUpper() + (transformString.Length > 1 ? transformString.Substring(1).ToLower() : "");
    }

    public void SetApplicationPath(string strPath)
    {
        m_ApplicationPath = strPath;
    }

    private void ShowProgressBarFunction(string str)
    {
        ShowProgressBar(str);
    }

    public void StartShowProgressBar(Control control)
    {
        control.BeginInvoke(new ShowProgressBarHandler(ShowProgressBarFunction), "Recv Data...");
    }

    private void TransProgressByValue(int value)
    {
        if (m_progressBar != null)
        {
            m_progressBar.ProgressValue = value;
        }
    }

    private void TransProgressByNetSpeed(double netSpeed)
    {
        if (m_progressBar != null)
        {
            m_progressBar.NetSpeed = netSpeed;
        }
    }

    public void TransFinish(string strMsg)
    {
        if (strMsg.Equals(""))
        {
            m_progressBar.SetProgressFinsh(bSuccess: true);
        }
        else
        {
            m_progressBar.SetProgressFinsh(bSuccess: false);
        }
    }

    public void ShowProgressBar(string str)
    {
        m_progressBar = new DialogProgress("Connecting", str);
        m_progressBar.StartPosition = FormStartPosition.CenterParent;
        m_progressBar.ProgressVisible = true;
        m_progressBar.btnOKVisible = false;
        m_progressBar.ShowDialog();
    }

    public void updateTaskQueue(NetworkMethod method, object obj)
    {
        NetTaskContent item = default;
        item.run = method;
        item.obj = obj;
        netTaskQueue.Enqueue(item);
        if (!m_bTransData)
        {
            m_bTransData = true;
            new Thread(NetworkTask).Start();
        }
    }

    private void NetworkTask()
    {
        while (netTaskQueue.Count > 0 && IsConnected())
        {
            NetTaskContent netTaskContent = netTaskQueue.Dequeue();
            netTaskContent.run(netTaskContent.obj);
        }
        m_bTransData = false;
    }

    public string Connect(string hostName, int port, int nTimeout)
    {
        string result = "";
        try
        {
            m_tcpClient = new TcpClient();
            m_tcpClient.BeginConnect(hostName, port, null, null).AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5.0));
            if (!m_tcpClient.Connected)
            {
                throw new Exception("Failed to connect.");
            }
            m_streamClient = m_tcpClient.GetStream();
            m_bConnection = true;
            m_tcpClient.SendTimeout = nTimeout;
            m_tcpClient.ReceiveTimeout = nTimeout;
        }
        catch (Exception ex)
        {
            result = ex.ToString();
            m_bConnection = false;
        }
        return result;
    }

    public string MatchServer(int ID)
    {
        string text = "";
        string text2 = "LinkFromIQTool:";
        string text3 = "";
        _ = text2.Length;
        byte[] array = new byte[text2.Length + 4 + 1];
        try
        {
            array = Encoding.Default.GetBytes(text2 + ID);
            m_streamClient.Write(array, 0, array.Length);
            m_streamClient.Flush();
        }
        catch (Exception ex)
        {
            text += ex.ToString();
        }
        if (text.Equals(""))
        {
            try
            {
                Array.Clear(array, 0, array.Length);
                m_streamClient.Read(array, 0, array.Length);
                text3 = Encoding.Default.GetString(array);
                if (text3.StartsWith("LinkFailed"))
                {
                    text = "Match Failed\n\nID is Wrong";
                }
                else if (!text3.StartsWith("LinkSucceed"))
                {
                    text = "Receive:" + text3;
                }
            }
            catch (Exception ex2)
            {
                text += ex2.ToString();
            }
        }
        return text;
    }

    public void Disconnect()
    {
        if (m_streamClient != null)
        {
            m_streamClient.Close();
        }
        if (m_tcpClient != null)
        {
            m_tcpClient.Close();
            m_tcpClient = null;
        }
        if (comport != null)
        {
            comport.Close();
        }
        m_bConnection = false;
    }

    public bool IsConnected()
    {
        return ConnectMode switch
        {
            CONNECT_MODE.MODE_SOCKET => m_bConnection,
            CONNECT_MODE.MODE_USB => m_bUSBConnection,
            CONNECT_MODE.MODE_UART => m_bUARTConnection,
            _ => false,
        };
    }

    public static T Deserialize<T>(byte[] array) where T : struct
    {
        int num = Marshal.SizeOf(typeof(T));
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        Marshal.Copy(array, 0, intPtr, num);
        T result = (T)Marshal.PtrToStructure(intPtr, typeof(T));
        Marshal.FreeHGlobal(intPtr);
        return result;
    }

    public IQ_CMD_RESPONSE_S SendPacket(CAMERA_CMD_TYPE cmdType, uint nDbgItemId, uint nTransFlag, byte[] pSendData, int nSendLength)
    {
        string text = "";
        int[] array = new int[2];
        int[] array2 = new int[3];
        int num = array.Length * 4;
        int num2 = array2.Length * 4;
        byte[] array3 = new byte[num + num2 + nSendLength];
        int num3 = 0;
        int num4 = Marshal.SizeOf(typeof(IQ_CMD_RESPONSE_S));
        string text2 = "";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();
        IQ_CMD_RESPONSE_S result = default;
        result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        result.DataLen = 0;
        Array.Clear(array3, 0, array3.Length);
        array[0] = (int)cmdType;
        array[1] = array2.Length * 4 + nSendLength;
        array2[0] = (int)nDbgItemId;
        array2[1] = (int)cmdType;
        array2[2] = (int)nTransFlag;
        Buffer.BlockCopy(array, 0, array3, num3, array.Length * 4);
        num3 += array.Length * 4;
        Buffer.BlockCopy(array2, 0, array3, num3, array2.Length * 4);
        num3 += array2.Length * 4;
        try
        {
            if (pSendData != null)
            {
                Buffer.BlockCopy(pSendData, 0, array3, num3, nSendLength);
            }
            num3 += nSendLength;
            m_streamClient.Write(array3, 0, num3);
            result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_OK;
            m_streamClient.Flush();
        }
        catch (Exception ex)
        {
            Disconnect();
            text = text + ex.Message.ToString() + Environment.NewLine;
        }
        text2 = text2 + "Dbg" + nDbgItemId + ": Write " + stopwatch.ElapsedMilliseconds + " ms, ";
        if (result.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            try
            {
                byte[] array4 = new byte[num4];
                int num5 = m_streamClient.Read(array4, 0, num4);
                result = Deserialize<IQ_CMD_RESPONSE_S>(array4);
                if (num5 != num4 || result.ResCode != 0)
                {
                    result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                    text += "iq server response error";
                }
            }
            catch (Exception ex2)
            {
                text = text + ex2.Message.ToString() + Environment.NewLine;
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                Disconnect();
            }
        }
        text2 = text2 + "Read " + stopwatch.ElapsedMilliseconds + " ms, ";
        stopwatch.Stop();
        return result;
    }

    public IQ_CMD_RESPONSE_S UploadPacket(CAMERA_CMD_TYPE cmdType, int nItemId, byte[] pSendData, int nSendLength)
    {
        string text = "";
        int[] array = new int[2];
        int[] array2 = new int[4];
        int num = 0;
        int num2 = array.Length * 4;
        int num3 = array2.Length * 4;
        byte[] array3 = new byte[num2 + num3 + nSendLength];
        int num4 = Marshal.SizeOf(typeof(IQ_CMD_RESPONSE_S));
        string text2 = "";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();
        IQ_CMD_RESPONSE_S result = new IQ_CMD_RESPONSE_S
        {
            ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR,
            DataLen = 0
        };
        Array.Clear(array3, 0, array3.Length);
        array[0] = (int)cmdType;
        array[1] = num3 + nSendLength;
        array2[0] = nItemId;
        array2[1] = nSendLength;
        array2[2] = 0;
        array2[3] = 0;
        Buffer.BlockCopy(array, 0, array3, num, num2);
        num += num2;
        Buffer.BlockCopy(array2, 0, array3, num, num3);
        num += num3;
        try
        {
            if (pSendData != null)
            {
                Buffer.BlockCopy(pSendData, 0, array3, num, nSendLength);
            }
            num += nSendLength;
            m_streamClient.Write(array3, 0, num);
            result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_OK;
            m_streamClient.Flush();
        }
        catch (Exception ex)
        {
            Disconnect();
            text = text + ex.Message.ToString() + Environment.NewLine;
        }
        text2 = text2 + "ItemID = " + nItemId + " : Write " + stopwatch.ElapsedMilliseconds + " ms, ";
        if (result.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            try
            {
                byte[] array4 = new byte[num4];
                int num5 = m_streamClient.Read(array4, 0, num4);
                result = Deserialize<IQ_CMD_RESPONSE_S>(array4);
                if (num5 != num4 || result.ResCode != 0)
                {
                    result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                    text += "iq server response error";
                }
            }
            catch (Exception ex2)
            {
                text = text + ex2.Message.ToString() + Environment.NewLine;
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                Disconnect();
            }
        }
        text2 = text2 + "Read " + stopwatch.ElapsedMilliseconds + " ms.";
        stopwatch.Stop();
        return result;
    }

    public int ReceiveImagePacket(Control control, CAMERA_CMD_TYPE cmdType, uint itemId, uint nTransFlag, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        double num5 = 0.0;
        int num6 = 0;
        Stopwatch stopwatch = new Stopwatch();
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendPacket(cmdType, itemId, nTransFlag, null, 0);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            num3 = iQ_CMD_RESPONSE_S.DataLen;
            byte[] array = new byte[num3];
            pbyRcvData = new byte[num3];
            while (num3 > 0)
            {
                try
                {
                    stopwatch.Start();
                    num = m_streamClient.Read(array, 0, num3);
                    Buffer.BlockCopy(array, 0, pbyRcvData, num2, num);
                    num2 += num;
                    num3 -= num;
                    num4 = num2 * 100 / iQ_CMD_RESPONSE_S.DataLen;
                    num6 += num;
                    if (stopwatch.ElapsedMilliseconds >= 1000)
                    {
                        num5 = num6 / 1024 / (stopwatch.ElapsedMilliseconds / 1000.0);
                        num6 = 0;
                        stopwatch.Reset();
                        control.Invoke(new UpdateTransProgressByNetSpeedHandler(TransProgressByNetSpeed), num5);
                    }
                    control.Invoke(new UpdateTransProgressByValueHandler(TransProgressByValue), num4);
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    control.Invoke(new UpdateTransFinishHandler(TransFinish), text);
                    return -1;
                }
            }
            control.Invoke(new UpdateTransFinishHandler(TransFinish), text);
        }
        else
        {
            pbyRcvData = null;
        }
        return num2;
    }

    public int ReceivePacket(CAMERA_CMD_TYPE cmdType, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int i = 0;
        int num2 = 0;
        int dstOffset = 0;
        byte[] array = new byte[4];
        int[] array2 = new int[1];
        Array.Clear(array, 0, array.Length);
        array2[0] = 0;
        Buffer.BlockCopy(array2, 0, array, dstOffset, array2.Length * 4);
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendApiPacket(cmdType, array, array.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num2];
                byte[] array3 = new byte[num2];
                try
                {
                    for (; i < num2; i += num)
                    {
                        num = m_streamClient.Read(array3, 0, num2 - i);
                        Buffer.BlockCopy(array3, 0, pbyRcvData, i, num);
                        array3.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public IQ_CMD_RESPONSE_S SendApiPacket(CAMERA_CMD_TYPE cmdType, byte[] pSendData, int nSendLength)
    {
        string text = "";
        byte[] array = new byte[8 + nSendLength];
        int[] array2 = new int[2];
        int num = 0;
        int num2 = Marshal.SizeOf(typeof(IQ_CMD_RESPONSE_S));
        int num3 = 0;
        string text2 = "";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();
        IQ_CMD_RESPONSE_S result = default;
        result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        result.DataLen = 0;
        Array.Clear(array, 0, array.Length);
        array2[0] = (int)cmdType;
        array2[1] = nSendLength;
        Buffer.BlockCopy(array2, 0, array, num, array2.Length * 4);
        num += array2.Length * 4;
        try
        {
            if (pSendData != null)
            {
                Buffer.BlockCopy(pSendData, 0, array, num, nSendLength);
            }
            num += nSendLength;
            m_streamClient.Write(array, 0, num);
            result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_OK;
            m_streamClient.Flush();
        }
        catch (Exception ex)
        {
            Disconnect();
            text = text + ex.Message.ToString() + Environment.NewLine;
        }
        if (result.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            try
            {
                byte[] array3 = new byte[num2];
                num3 = m_streamClient.Read(array3, 0, num2);
                result = Deserialize<IQ_CMD_RESPONSE_S>(array3);
                Console.WriteLine(string.Concat("ResCode = ", result.ResCode, ", DataLen = ", result.DataLen));
                if (num3 != num2 || result.ResCode != 0)
                {
                    result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                    text += "iq server response error";
                }
            }
            catch (Exception ex2)
            {
                text = text + ex2.Message.ToString() + Environment.NewLine;
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                Disconnect();
            }
        }
        text2 = text2 + "Read " + stopwatch.ElapsedMilliseconds + " ms, ";
        stopwatch.Stop();
        return result;
    }

    public void Readnew()
    {
        while (_continue)
        {
            try
            {
                if (comport.BytesToRead > 0)
                {
                    int bytesToRead = comport.BytesToRead;
                    byte[] array = new byte[bytesToRead];
                    int num = comport.Read(array, 0, bytesToRead);
                    if (num != bytesToRead)
                    {
                        Console.WriteLine("Uart Read Len fail.");
                    }
                    else
                    {
                        rb.WriteBuff(array, num);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Readnew execept");
            }
            Thread.Sleep(30);
        }
    }

    public int IndexOf(byte[] source, byte[] pattern, int startpos = 0)
    {
        int num = source.Length - pattern.Length;
        for (int i = startpos; i < num; i++)
        {
            if (source[i] != pattern[0])
            {
                continue;
            }
            bool flag = true;
            for (int j = 1; j < pattern.Length; j++)
            {
                if (source[i + j] != pattern[j])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                return i;
            }
        }
        return -1;
    }

    public int findByteCount(byte[] source, byte pattern, int startpos = 0)
    {
        int num = 0;
        for (int i = startpos; i < source.Length; i++)
        {
            if (source[i] == pattern)
            {
                num++;
            }
        }
        return num;
    }

    public void insertPrefix(byte[] src, byte[] dst, byte match, byte prefix)
    {
        int num = 0;
        for (int i = 0; i < src.Length; i++)
        {
            if (src[i] != match)
            {
                dst[num++] = src[i];
                continue;
            }
            dst[num++] = prefix;
            dst[num++] = src[i];
        }
    }

    public string SendUartStart()
    {
        string text = "";
        try
        {
            comport.Write("iq uart start");
        }
        catch (Exception ex)
        {
            text = text + ex.Message.ToString() + Environment.NewLine;
        }
        return text;
    }

    public IQ_CMD_RESPONSE_S SendUartApiPacket(CAMERA_CMD_TYPE cmdType, byte[] pSendData, int nSendLength)
    {
        string text = "";
        byte[] array = new byte[8 + nSendLength];
        int[] array2 = new int[2];
        int num = 0;
        int num2 = Marshal.SizeOf(typeof(IQ_CMD_RESPONSE_S));
        IQ_CMD_RESPONSE_S result = default;
        result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        result.DataLen = 0;
        Array.Clear(array, 0, array.Length);
        byte[] pattern = new byte[5] { 50, 50, 50, 50, 50 };
        byte[] pattern2 = new byte[5] { 49, 49, 49, 49, 49 };
        string text2 = BitConverter.ToString(pSendData);
        if (text2.Contains("32") && IndexOf(pSendData, pattern) != -1)
        {
            int num3 = findByteCount(pSendData, 50);
            byte[] array3 = new byte[nSendLength + num3];
            if (array3.Length >= pSendData.Length)
            {
                insertPrefix(pSendData, array3, 50, 126);
                nSendLength += num3;
                Array.Clear(pSendData, 0, pSendData.Length);
                Array.Resize(ref pSendData, nSendLength);
                Buffer.BlockCopy(array3, 0, pSendData, 0, nSendLength);
                Array.Resize(ref array, 8 + nSendLength);
                Array.Clear(array, 0, array.Length);
            }
        }
        if (text2.Contains("31") && IndexOf(pSendData, pattern2) != -1)
        {
            int num4 = findByteCount(pSendData, 49);
            byte[] array4 = new byte[nSendLength + num4];
            if (array4.Length >= pSendData.Length)
            {
                insertPrefix(pSendData, array4, 49, 126);
                nSendLength += num4;
                Array.Clear(pSendData, 0, pSendData.Length);
                Array.Resize(ref pSendData, nSendLength);
                Buffer.BlockCopy(array4, 0, pSendData, 0, nSendLength);
                Array.Resize(ref array, 8 + nSendLength);
                Array.Clear(array, 0, array.Length);
            }
        }
        array2[0] = (int)cmdType;
        array2[1] = nSendLength;
        Buffer.BlockCopy(array2, 0, array, num, array2.Length * 4);
        num += array2.Length * 4;
        try
        {
            if (pSendData != null)
            {
                Buffer.BlockCopy(pSendData, 0, array, num, nSendLength);
            }
            num += nSendLength;
            comport.Write(array, 0, num);
            Thread.Sleep(500);
            byte[] array5 = new byte[num2];
            if (rb.ReadBuff(array5, num2, MovePosition: true) != num2)
            {
                text += "receive data len is not enugnous";
            }
            result = Deserialize<IQ_CMD_RESPONSE_S>(array5);
            if (result.ResCode != 0)
            {
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                text += "iq server response error";
            }
        }
        catch (Exception ex)
        {
            text = text + ex.Message.ToString() + Environment.NewLine;
        }
        return result;
    }

    public IQ_CMD_RESPONSE_S SendUartPacket(CAMERA_CMD_TYPE cmdType, uint nDbgItemId, uint nTransFlag, byte[] pSendData, int nSendLength)
    {
        int num = 20;
        int num2 = 0;
        string text = "";
        int[] array = new int[2];
        int[] array2 = new int[3];
        int num3 = array.Length * 4;
        int num4 = array2.Length * 4;
        byte[] array3 = new byte[num3 + num4 + nSendLength];
        int num5 = 0;
        int num6 = Marshal.SizeOf(typeof(IQ_CMD_RESPONSE_S));
        IQ_CMD_RESPONSE_S result = default;
        result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        result.DataLen = 0;
        Array.Clear(array3, 0, array3.Length);
        array[0] = (int)cmdType;
        array[1] = array2.Length * 4 + nSendLength;
        array2[0] = (int)nDbgItemId;
        array2[1] = (int)cmdType;
        array2[2] = (int)nTransFlag;
        Buffer.BlockCopy(array, 0, array3, num5, array.Length * 4);
        num5 += array.Length * 4;
        Buffer.BlockCopy(array2, 0, array3, num5, array2.Length * 4);
        num5 += array2.Length * 4;
        try
        {
            if (pSendData != null)
            {
                Buffer.BlockCopy(pSendData, 0, array3, num5, nSendLength);
            }
            num5 += nSendLength;
            comport.Write(array3, 0, num5);
            if (cmdType == CAMERA_CMD_TYPE.CAMERA_CMD_GET_PIC)
            {
                Thread.Sleep(1000);
            }
            else
            {
                Thread.Sleep(500);
            }
            result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_OK;
        }
        catch (Exception ex)
        {
            Disconnect();
            text = text + ex.Message.ToString() + Environment.NewLine;
        }
        if (result.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            try
            {
                byte[] array4 = new byte[num6];
                while (num > 0)
                {
                    if (rb.ReadBuff(array4, num6, MovePosition: true) != num6)
                    {
                        Thread.Sleep(100);
                        num--;
                        continue;
                    }
                    num2 = 1;
                    break;
                }
                if (num2 != 1)
                {
                    text += "receive data len is not enugnous";
                    MessageBox.Show("receive data len is not enugnous");
                    return result;
                }
                result = Deserialize<IQ_CMD_RESPONSE_S>(array4);
                if (result.ResCode != 0)
                {
                    result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                    text += "iq server response error";
                    MessageBox.Show("receive response error");
                }
            }
            catch (Exception ex2)
            {
                text = text + ex2.Message.ToString() + Environment.NewLine;
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                Disconnect();
            }
        }
        return result;
    }

    public int ReceiveUartPacket(CAMERA_CMD_TYPE cmdType, out byte[] pbyRcvData)
    {
        string text = "";
        int result = 0;
        int num = 0;
        int dstOffset = 0;
        byte[] array = new byte[4];
        int[] array2 = new int[1];
        Array.Clear(array, 0, array.Length);
        array2[0] = 0;
        Buffer.BlockCopy(array2, 0, array, dstOffset, array2.Length * 4);
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendUartApiPacket(cmdType, array, array.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num];
                _ = new byte[num];
                Thread.Sleep(getWaitDataTime(iQ_CMD_RESPONSE_S.DataLen));
                try
                {
                    if (rb.ReadBuff(pbyRcvData, num, MovePosition: true) != num)
                    {
                        MessageBox.Show("data not enough.");
                        return -1;
                    }
                    result = num;
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return result;
    }

    public int ReceiveUartImagePacket(Control control, CAMERA_CMD_TYPE cmdType, uint itemId, uint nTransFlag, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        double num5 = 0.0;
        int num6 = 0;
        Stopwatch stopwatch = new Stopwatch();
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendUartPacket(cmdType, itemId, nTransFlag, null, 0);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            num3 = iQ_CMD_RESPONSE_S.DataLen;
            byte[] array = new byte[num3];
            pbyRcvData = new byte[num3];
            while (num2 < num3)
            {
                stopwatch.Start();
                num = rb.ReadBuff(array, num3 - num2, MovePosition: true);
                if (num == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }
                Buffer.BlockCopy(array, 0, pbyRcvData, num2, num);
                array.Initialize();
                num2 += num;
                num4 = num2 * 100 / num3;
                num6 += num;
                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    num5 = num6 / 1024 / (stopwatch.ElapsedMilliseconds / 1000.0);
                    num6 = 0;
                    stopwatch.Reset();
                    control.Invoke(new UpdateTransProgressByNetSpeedHandler(TransProgressByNetSpeed), num5);
                }
                control.Invoke(new UpdateTransProgressByValueHandler(TransProgressByValue), num4);
            }
            control.Invoke(new UpdateTransFinishHandler(TransFinish), text);
        }
        else
        {
            pbyRcvData = null;
        }
        return num2;
    }

    public string connectUartPage(string UART_COM_num)
    {
        string result = "";
        COM_num = UART_COM_num;
        if (COM_num != "COM4")
        {
            comport = new SerialPort(COM_num, 115200, Parity.None, 8, StopBits.One);
        }
        if (!comport.IsOpen)
        {
            try
            {
                comport.ReadBufferSize = 51200;
                comport.Open();
                m_bUARTConnection = true;
            }
            catch (Exception ex)
            {
                m_bUARTConnection = false;
                return ex.Message.ToString();
            }
        }
        _continue = true;
        readThread = new Thread(Readnew);
        readThread.Start();
        return result;
    }

    public void disconnectUartPage()
    {
        _continue = false;
        m_bUARTConnection = false;
        string text = "";
        try
        {
            comport.Write("close");
            comport.Close();
        }
        catch (Exception ex)
        {
            text = text + ex.Message.ToString() + Environment.NewLine;
        }
    }

    public static void getStrInt(string msg, out string intStr)
    {
        intStr = Regex.Replace(msg, "[^0-9]", "");
    }

    public static int getWaitDataTime(int data_len)
    {
        if (data_len <= 0)
        {
            return 5;
        }
        int num2 = data_len / 11;
        _ = data_len % 11;
        if (num2 <= 0)
        {
            return 5;
        }
        return num2 + 5;
    }

    public int ReceiveUartApiPacket(short ApiId, out byte[] pbyRcvData)
    {
        byte[] bytes = BitConverter.GetBytes(ApiId);
        int num = 0;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, bytes, bytes.Length);
        Thread.Sleep(getWaitDataTime(iQ_CMD_RESPONSE_S.DataLen));
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num];
                if (rb.ReadBuff(pbyRcvData, num, MovePosition: true) != num)
                {
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveUartInitialApiPacket(short ApiId, byte[] bufferInitial, out byte[] pbyRcvData)
    {
        byte[] bytes = BitConverter.GetBytes(ApiId);
        int num = 0;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = AFWindowsAPIID(ChipIDName, ApiId) != 1 ? SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, bufferInitial, bytes.Length) : SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, bufferInitial, 20);
        Thread.Sleep(getWaitDataTime(iQ_CMD_RESPONSE_S.DataLen));
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num];
                if (rb.ReadBuff(pbyRcvData, num, MovePosition: true) != num)
                {
                    num = 0;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveUartInitialApiPacketByType(CAMERA_CMD_TYPE type, short ApiId, byte[] bufferInitial, out byte[] pbyRcvData)
    {
        int num = 0;
        byte[] array = null;
        array = BitConverter.GetBytes(ApiId);
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = AFWindowsAPIID(ChipIDName, ApiId) != 1 ? SendUartApiPacket(type, bufferInitial, array.Length) : SendUartApiPacket(type, bufferInitial, 20);
        Thread.Sleep(getWaitDataTime(iQ_CMD_RESPONSE_S.DataLen));
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num];
                if (rb.ReadBuff(pbyRcvData, num, MovePosition: true) != num)
                {
                    num = 0;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveUartApiPacket(CAMERA_CMD_TYPE cmdType, byte[] pSendData, out byte[] pbyRcvData)
    {
        int num = 0;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendUartApiPacket(cmdType, pSendData, pSendData.Length);
        Thread.Sleep(getWaitDataTime(iQ_CMD_RESPONSE_S.DataLen));
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num];
                if (rb.ReadBuff(pbyRcvData, num, MovePosition: true) != num)
                {
                    num = 0;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveApiPacket(CAMERA_CMD_TYPE cmdType, byte[] pSendData, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int i = 0;
        int num2 = 0;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendApiPacket(cmdType, pSendData, pSendData.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num2];
                byte[] array = new byte[num2];
                try
                {
                    for (; i < num2; i += num)
                    {
                        num = m_streamClient.Read(array, 0, num2 - i);
                        Buffer.BlockCopy(array, 0, pbyRcvData, i, num);
                        array.Initialize();
                    }
                    Console.WriteLine("nLength = " + i + " nDataLength = " + num2);
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveInitialApiPacketByType(CAMERA_CMD_TYPE type, short ApiId, byte[] bufferInitial, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int num2 = 0;
        int i = 0;
        byte[] array = null;
        byte[] array2 = null;
        array = BitConverter.GetBytes(ApiId);
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = AFWindowsAPIID(ChipIDName, ApiId) != 1 ? SendApiPacket(type, bufferInitial, array.Length) : SendApiPacket(type, bufferInitial, 20);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num2];
                array2 = new byte[num2];
                try
                {
                    for (; i < num2; i += num)
                    {
                        num = m_streamClient.Read(array2, 0, num2 - i);
                        Buffer.BlockCopy(array2, 0, pbyRcvData, i, num);
                        array2.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveInitialApiPacket(short ApiId, byte[] bufferInitial, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int i = 0;
        int num2 = 0;
        byte[] bytes = BitConverter.GetBytes(ApiId);
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = AFWindowsAPIID(ChipIDName, ApiId) != 1 ? SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, bufferInitial, bytes.Length) : SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, bufferInitial, 20);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num2];
                byte[] array = new byte[num2];
                try
                {
                    for (; i < num2; i += num)
                    {
                        num = m_streamClient.Read(array, 0, num2 - i);
                        Buffer.BlockCopy(array, 0, pbyRcvData, i, num);
                        array.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveApiPacket(short ApiId, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int i = 0;
        int num2 = 0;
        byte[] bytes = BitConverter.GetBytes(ApiId);
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, bytes, bytes.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num2];
                byte[] array = new byte[num2];
                try
                {
                    for (; i < num2; i += num)
                    {
                        num = m_streamClient.Read(array, 0, num2 - i);
                        Buffer.BlockCopy(array, 0, pbyRcvData, i, num);
                        array.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveCaliAWBApiPacket(short ApiId, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int i = 0;
        int num2 = 0;
        byte[] array = new byte[60];
        array[0] = 1;
        array[2] = 1;
        array[8] = 0;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_DOWNLOAD_FILE, array, array.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num2];
                byte[] array2 = new byte[num2];
                try
                {
                    for (; i < num2; i += num)
                    {
                        num = m_streamClient.Read(array2, 0, num2 - i);
                        Buffer.BlockCopy(array2, 0, pbyRcvData, i, num);
                        array2.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveCaliDPCApiPacket(short ApiId, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int i = 0;
        int num2 = 0;
        byte[] array = new byte[60];
        array[0] = 1;
        array[2] = 1;
        array[8] = 6;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_DOWNLOAD_FILE, array, array.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num2];
                byte[] array2 = new byte[num2];
                try
                {
                    for (; i < num2; i += num)
                    {
                        num = m_streamClient.Read(array2, 0, num2 - i);
                        Buffer.BlockCopy(array2, 0, pbyRcvData, i, num);
                        array2.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveXMLApiPacket(Control control, short ApiId, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        byte[] array = new byte[12]
        {
            Convert.ToByte(ApiId),
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            6,
            0,
            0,
            0
        };
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        iQ_CMD_RESPONSE_S.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        iQ_CMD_RESPONSE_S.DataLen = 0;
        int num4 = 0;
        double num5 = 0.0;
        int num6 = 0;
        Stopwatch stopwatch = new Stopwatch();
        if (ConnectMode == CONNECT_MODE.MODE_SOCKET)
        {
            iQ_CMD_RESPONSE_S = SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_DOWNLOAD_FILE, array, array.Length);
        }
        else if (ConnectMode == CONNECT_MODE.MODE_UART)
        {
            iQ_CMD_RESPONSE_S = SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_DOWNLOAD_FILE, array, array.Length);
        }
        else
        {
            _ = ConnectMode;
            _ = 2;
        }
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num3 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num3];
                byte[] array2 = new byte[num3];
                try
                {
                    while (num2 < num3)
                    {
                        stopwatch.Start();
                        if (ConnectMode == CONNECT_MODE.MODE_SOCKET)
                        {
                            num = m_streamClient.Read(array2, 0, num3 - num2);
                        }
                        else if (ConnectMode == CONNECT_MODE.MODE_UART)
                        {
                            num = rb.ReadBuff(array2, num3 - num2, MovePosition: true);
                        }
                        if (num == 0)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        Buffer.BlockCopy(array2, 0, pbyRcvData, num2, num);
                        array2.Initialize();
                        num2 += num;
                        if (ConnectMode == CONNECT_MODE.MODE_UART)
                        {
                            num6 = num2 * 100 / num3;
                            num4 += num;
                            if (stopwatch.ElapsedMilliseconds >= 1000)
                            {
                                num5 = num4 / 1024 / (stopwatch.ElapsedMilliseconds / 1000.0);
                                num4 = 0;
                                stopwatch.Reset();
                                control.Invoke(new UpdateTransProgressByNetSpeedHandler(TransProgressByNetSpeed), num5);
                            }
                            control.Invoke(new UpdateTransProgressByValueHandler(TransProgressByValue), num6);
                        }
                    }
                    if (ConnectMode == CONNECT_MODE.MODE_UART)
                    {
                        control.Invoke(new UpdateTransFinishHandler(TransFinish), text);
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    if (ConnectMode == CONNECT_MODE.MODE_UART)
                    {
                        control.Invoke(new UpdateTransFinishHandler(TransFinish), text);
                    }
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    public int ReceiveXMLApiPacket(short ApiId, out byte[] pbyRcvData, ushort APIVerMajor, ushort APIVerMinor)
    {
        string text = "";
        int num = 0;
        int i = 0;
        int num2 = 0;
        byte[] array = new byte[102400];
        array[0] = Convert.ToByte(ApiId);
        array[2] = 1;
        array[8] = Convert.ToByte(APIVerMinor & 0xFF);
        array[9] = Convert.ToByte(APIVerMinor >> 8 & 0xFF);
        array[10] = Convert.ToByte(APIVerMajor & 0xFF);
        array[11] = Convert.ToByte(APIVerMajor >> 8 & 0xFF);
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_DOWNLOAD_FILE, array, array.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num2];
                byte[] array2 = new byte[num2];
                try
                {
                    for (; i < num2; i += num)
                    {
                        if (ConnectMode == CONNECT_MODE.MODE_SOCKET)
                        {
                            num = m_streamClient.Read(array2, 0, num2 - i);
                        }
                        else if (ConnectMode == CONNECT_MODE.MODE_UART)
                        {
                            num = rb.ReadBuff(array2, num2 - i, MovePosition: true);
                        }
                        Buffer.BlockCopy(array2, 0, pbyRcvData, i, num);
                        array2.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    Disconnect();
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return num;
    }

    private uint localFun(ushort precent, CALLBACK_DATA_STRUCT lpCallbackData)
    {
        lpCallbackData.control.Invoke(new UpdateTransProgressByValueHandler(TransProgressByValue), precent);
        return 0u;
    }

    [DllImport("AitGUI.dll")]
    private static extern int ShowVideoDevDlg(byte[] msg, int buf_len);

    [DllImport("AitUVCExtApi.dll")]
    private unsafe static extern int AITAPI_OpenDeviceByPath(byte[] msg, IntPtr* pDevHandle);

    [DllImport("AitUVCExtApi.dll")]
    private static extern int AITAPI_GetFWVersion(IntPtr pDevHandle, byte[] msg);

    [DllImport("AitUVCExtApi.dll", EntryPoint = "AITAPI_UpdateFW_842x")]
    private unsafe static extern int AITAPI_UpdateFW(IntPtr pDevHandle, byte[] data, int len, void* ProgressCB, void* callbackData, byte mode);

    [DllImport("AitUVCExtApi.dll")]
    private unsafe static extern int AITAPI_SendData(IntPtr pDevHandle, byte[] data, int len, void* ProgressCB, void* callbackData, byte StorageType, byte PackageType);

    [DllImport("AitUVCExtApi.dll")]
    private static extern int AITAPI_ReadPartialFlashData(IntPtr pDevHandle, uint FlashAddr, uint Len, byte[] buf, uint[] byteRet, callBackHandler callBackFunc, CALLBACK_DATA_STRUCT lpCallBackData);

    [DllImport("AitUVCExtApi.dll", EntryPoint = "AITAPI_IspCommand")]
    private static extern int AITXU_IspCommand(IntPtr pDevHandle, byte[] indata, byte[] outdata);

    public void EnableIQServerMode()
    {
        byte[] array = new byte[8];
        byte[] outdata = new byte[8];
        array[0] = 25;
        array[1] = 1;
        AITXU_IspCommand(m_ait_handel, array, outdata);
    }

    public unsafe string USBConnect()
    {
        string result = "";
        try
        {
            byte[] msg = new byte[1024];
            ShowVideoDevDlg(msg, 1024);
            fixed (IntPtr* pDevHandle = &m_ait_handel)
            {
                if (AITAPI_OpenDeviceByPath(msg, pDevHandle) != 0)
                {
                    result = "Oepn USB device failed!";
                    m_bUSBConnection = false;
                    Console.WriteLine("Oepn USB device failed!");
                }
                else
                {
                    EnableIQServerMode();
                    m_bUSBConnection = true;
                    Console.WriteLine("Oepn USB device succeed!");
                }
            }
        }
        catch (Exception ex)
        {
            result = ex.ToString();
            m_bUSBConnection = false;
        }
        return result;
    }

    public IQ_CMD_RESPONSE_S SendUSBApiPacket(CAMERA_CMD_TYPE cmdType, byte[] pSendData, int nSendLength)
    {
        string text = "";
        byte[] array = new byte[102408];
        int[] array2 = new int[2];
        int num = 0;
        int num2 = Marshal.SizeOf(typeof(IQ_CMD_RESPONSE_S));
        string text2 = "";
        byte b = 0;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();
        IQ_CMD_RESPONSE_S result = default;
        result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        result.DataLen = 0;
        Array.Clear(array, 0, array.Length);
        array2[0] = (int)cmdType;
        array2[1] = nSendLength;
        Buffer.BlockCopy(array2, 0, array, num, array2.Length * 4);
        num += array2.Length * 4;
        b = (byte)(cmdType != CAMERA_CMD_TYPE.CAMERA_CMD_GET_MODE && cmdType != CAMERA_CMD_TYPE.CAMERA_CMD_GET_API && cmdType != CAMERA_CMD_TYPE.CAMERA_CMD_GET_PIC ? 6 : 7);
        try
        {
            if (pSendData != null)
            {
                Buffer.BlockCopy(pSendData, 0, array, num, nSendLength);
            }
            num += nSendLength;
            string text3 = "";
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Reset();
            stopwatch2.Start();
            if (Send_IQData(m_ait_handel, array, num, 0, b) != 0)
            {
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                Console.WriteLine("Send IQ Data error!");
            }
            else
            {
                byte[] array3 = new byte[num2];
                Get_IQResponseData(m_ait_handel, array3, num2);
                result = Deserialize<IQ_CMD_RESPONSE_S>(array3);
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_OK;
                Console.WriteLine("Send IQ Data OK!");
            }
            text3 = text3 + "[USB] Get_IQData : buffer size = " + num.ToString().PadLeft(8) + ", cost time = " + stopwatch2.ElapsedMilliseconds.ToString().PadLeft(6) + " ms";
            stopwatch2.Stop();
            Console.WriteLine(text3);
        }
        catch (Exception ex)
        {
            result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
            text = text + ex.Message.ToString() + Environment.NewLine;
            Console.WriteLine("SendApiPacket : {0}\n", text);
        }
        text2 = text2 + "Read " + stopwatch.ElapsedMilliseconds + " ms, ";
        stopwatch.Stop();
        Console.WriteLine(text2);
        return result;
    }

    public IQ_CMD_RESPONSE_S SendUSBPacket(CAMERA_CMD_TYPE cmdType, uint nDbgItemId, uint nTransFlag, byte[] pSendData, int nSendLength)
    {
        string text = "";
        byte[] array = new byte[102408];
        int[] array2 = new int[2];
        int[] array3 = new int[3];
        _ = array2.Length;
        _ = array3.Length;
        int num = 0;
        byte b = 0;
        int num2 = Marshal.SizeOf(typeof(IQ_CMD_RESPONSE_S));
        string text2 = "";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();
        IQ_CMD_RESPONSE_S result = default;
        result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        result.DataLen = 0;
        Array.Clear(array, 0, array.Length);
        array2[0] = (int)cmdType;
        array2[1] = array3.Length * 4 + nSendLength;
        array3[0] = (int)nDbgItemId;
        array3[1] = (int)cmdType;
        array3[2] = (int)nTransFlag;
        Buffer.BlockCopy(array2, 0, array, num, array2.Length * 4);
        num += array2.Length * 4;
        Buffer.BlockCopy(array3, 0, array, num, array3.Length * 4);
        num += array3.Length * 4;
        b = (byte)(cmdType != CAMERA_CMD_TYPE.CAMERA_CMD_GET_MODE && cmdType != CAMERA_CMD_TYPE.CAMERA_CMD_GET_API && cmdType != CAMERA_CMD_TYPE.CAMERA_CMD_GET_PIC ? 6 : 7);
        try
        {
            if (pSendData != null)
            {
                Buffer.BlockCopy(pSendData, 0, array, num, nSendLength);
            }
            num += nSendLength;
            string text3 = "";
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Reset();
            stopwatch2.Start();
            if (Send_IQData(m_ait_handel, array, num, 0, b) != 0)
            {
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                Console.WriteLine("Send IQ Data error!");
            }
            else
            {
                byte[] array4 = new byte[num2];
                Get_IQResponseData(m_ait_handel, array4, num2);
                result = Deserialize<IQ_CMD_RESPONSE_S>(array4);
                result.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_OK;
                Console.WriteLine("Send IQ Data OK!");
            }
            text3 = text3 + "[USB] Get_IQData : buffer size = " + num.ToString().PadLeft(8) + ", cost time = " + stopwatch2.ElapsedMilliseconds.ToString().PadLeft(6) + " ms";
            stopwatch2.Stop();
            Console.WriteLine(text3);
        }
        catch (Exception ex)
        {
            Disconnect();
            text = text + ex.Message.ToString() + Environment.NewLine;
        }
        text2 = text2 + "Dbg" + nDbgItemId + ": Write " + stopwatch.ElapsedMilliseconds + " ms, ";
        text2 = text2 + "Read " + stopwatch.ElapsedMilliseconds + " ms, ";
        stopwatch.Stop();
        return result;
    }

    public unsafe int Send_IQData(IntPtr in_ait_handel, byte[] Data, int DataSize, byte storageType, byte packType)
    {
        if (m_bUSBConnection)
        {
            Console.WriteLine("[Send_IQData] - IQ Data Size = {0}\n", DataSize);
            return AITAPI_SendData(in_ait_handel, Data, DataSize, null, null, storageType, packType);
        }
        return 0;
    }

    public int ReceiveUSBInitialApiPacket(short ApiId, byte[] bufferInitial, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        byte[] array = new byte[8 + bufferInitial.Length];
        int[] array2 = new int[2];
        int num2 = 0;
        Array.Clear(array, 0, array.Length);
        array2[0] = 6;
        array2[1] = bufferInitial.Length;
        Buffer.BlockCopy(array2, 0, array, num2, array2.Length * 4);
        num2 += array2.Length * 4;
        num = bufferInitial.Length;
        pbyRcvData = new byte[num];
        try
        {
            string text2 = "";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            if (bufferInitial != null)
            {
                byte[] bytes = BitConverter.GetBytes(ApiId);
                if (AFWindowsAPIID(ChipIDName, ApiId) == 1)
                {
                    Buffer.BlockCopy(bufferInitial, 0, array, num2, 20);
                    num2 += 20;
                }
                else
                {
                    Buffer.BlockCopy(bytes, 0, bufferInitial, 0, bytes.Length);
                    Buffer.BlockCopy(bufferInitial, 0, array, num2, bufferInitial.Length);
                    num2 += bufferInitial.Length;
                }
            }
            if (Get_IQData(m_ait_handel, (ushort)num2, array, (ushort)num, pbyRcvData) != 0)
            {
                Console.WriteLine("Get IQ Data error!");
                return -1;
            }
            Console.WriteLine("Get IQ Data OK!");
            text2 = text2 + "[USB][All]   API ID = " + ApiId.ToString().PadLeft(5) + ", Get_IQData : buffer size = " + num.ToString().PadLeft(8) + "(bytes), cost time = " + stopwatch.ElapsedMilliseconds.ToString().PadLeft(6) + " ms";
            stopwatch.Stop();
            Console.WriteLine(text2);
        }
        catch (Exception ex)
        {
            text = text + ex.Message.ToString() + Environment.NewLine;
            Disconnect();
            pbyRcvData = null;
            return -1;
        }
        return pbyRcvData.Length;
    }

    public int ReceiveUSBInitialApiPacketByType(CAMERA_CMD_TYPE type, short ApiId, byte[] bufferInitial, out byte[] pbyRcvData)
    {
        int num = 0;
        int num2 = 0;
        string text = "";
        byte[] array = null;
        int[] array2 = new int[2];
        byte[] array3 = new byte[8 + bufferInitial.Length];
        Array.Clear(array3, 0, array3.Length);
        array2[0] = (int)type;
        array2[1] = bufferInitial.Length;
        Buffer.BlockCopy(array2, 0, array3, num, array2.Length * 4);
        num += array2.Length * 4;
        num2 = bufferInitial.Length;
        pbyRcvData = new byte[num2];
        try
        {
            string text2 = "";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            if (bufferInitial != null)
            {
                array = BitConverter.GetBytes(ApiId);
                if (AFWindowsAPIID(ChipIDName, ApiId) == 1)
                {
                    Buffer.BlockCopy(bufferInitial, 0, array3, num, 20);
                    num += 20;
                }
                else
                {
                    Buffer.BlockCopy(array, 0, bufferInitial, 0, array.Length);
                    Buffer.BlockCopy(bufferInitial, 0, array3, num, bufferInitial.Length);
                    num += bufferInitial.Length;
                }
            }
            if (Get_IQData(m_ait_handel, (ushort)num, array3, (ushort)num2, pbyRcvData) != 0)
            {
                Console.WriteLine("Get IQ Data error!");
                return -1;
            }
            Console.WriteLine("Get IQ Data OK!");
            text2 = text2 + "[USB][All]   API ID = " + ApiId.ToString().PadLeft(5) + ", Get_IQData : buffer size = " + num2.ToString().PadLeft(8) + "(bytes), cost time = " + stopwatch.ElapsedMilliseconds.ToString().PadLeft(6) + " ms";
            stopwatch.Stop();
        }
        catch (Exception ex)
        {
            text = text + ex.Message.ToString() + Environment.NewLine;
            Disconnect();
            pbyRcvData = null;
            return -1;
        }
        return pbyRcvData.Length;
    }

    public int ReceiveUSBApiPacket(byte[] pSendData, out byte[] pReceiveData)
    {
        byte[] array = new byte[pSendData.Length];
        ushort out_IQ_Size = (ushort)array.Length;
        string text = "";
        byte[] array2 = new byte[102408];
        int[] array3 = new int[2];
        int num = 0;
        pReceiveData = null;
        string value = "";
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        iQ_CMD_RESPONSE_S.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
        iQ_CMD_RESPONSE_S.DataLen = 0;
        Array.Clear(array2, 0, array2.Length);
        array3[0] = 6;
        array3[1] = pSendData.Length;
        Buffer.BlockCopy(array3, 0, array2, num, array3.Length * 4);
        num += array3.Length * 4;
        try
        {
            string text2 = "";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            if (pSendData != null)
            {
                Buffer.BlockCopy(pSendData, 0, array2, num, pSendData.Length);
            }
            num += pSendData.Length;
            if (Get_IQData(m_ait_handel, (ushort)num, array2, out_IQ_Size, array) != 0)
            {
                iQ_CMD_RESPONSE_S.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
                Console.WriteLine("Get IQ Data error!");
            }
            else
            {
                iQ_CMD_RESPONSE_S.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_OK;
                Console.WriteLine("Get IQ Data OK!");
            }
            text2 = text2 + "[USB][All]  Get_IQData                  : buffer size = " + out_IQ_Size.ToString().PadLeft(8) + "(bytes), cost time = " + stopwatch.ElapsedMilliseconds.ToString().PadLeft(6) + " ms";
            stopwatch.Stop();
            Console.WriteLine(text2);
            if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
            {
                pReceiveData = new byte[array.Length];
                if (array != null)
                {
                    Buffer.BlockCopy(array, 0, pReceiveData, 0, array.Length);
                }
            }
        }
        catch (Exception ex)
        {
            iQ_CMD_RESPONSE_S.ResCode = IQ_RESPONSE_CODE_E.IQ_RES_ERROR;
            text = text + ex.Message.ToString() + Environment.NewLine;
            Console.WriteLine("SendApiPacket : {0}\n", text);
        }
        Console.WriteLine(value);
        return array.Length;
    }

    public int ReceiveUSBApiPacketData(byte[] pSendData, out byte[] pbyRcvData)
    {
        string text = "";
        int result = 0;
        int num = 0;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, pSendData, pSendData.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num];
                result = num;
                try
                {
                    if (Get_IQData(m_ait_handel, 0, null, (uint)num, pbyRcvData) != 0)
                    {
                        pbyRcvData = null;
                        text += "Get IQ Data error!";
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return result;
    }

    public int ReceiveUsbApiPacket(short ApiId, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int result = 0;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendUSBPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, (uint)ApiId, 0u, null, 0);
        text = "";
        pbyRcvData = null;
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK && iQ_CMD_RESPONSE_S.DataLen > 0)
        {
            try
            {
                pbyRcvData = new byte[iQ_CMD_RESPONSE_S.DataLen];
                num = iQ_CMD_RESPONSE_S.DataLen;
                _ = new byte[num];
                result = iQ_CMD_RESPONSE_S.DataLen;
                if (Get_IQData(m_ait_handel, 0, null, (uint)num, pbyRcvData) != 0)
                {
                    pbyRcvData = null;
                    Console.WriteLine("Get IQ Data error!");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                result = 0;
                text = text + ex.Message.ToString() + Environment.NewLine;
                Disconnect();
                pbyRcvData = null;
                return -1;
            }
        }
        return result;
    }

    public int ReceiveUSBPacket(CAMERA_CMD_TYPE cmdType, out byte[] pbyRcvData)
    {
        string text = "";
        int result = 0;
        int num = 0;
        byte[] array = new byte[4];
        int[] array2 = new int[1];
        Array.Clear(array, 0, array.Length);
        array2[0] = 0;
        Buffer.BlockCopy(array2, 0, array, 0, array2.Length * 4);
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendUSBApiPacket(cmdType, array, array.Length);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            if (iQ_CMD_RESPONSE_S.DataLen > 0)
            {
                num = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[num];
                result = num;
                try
                {
                    if (Get_IQData(m_ait_handel, 0, null, (uint)num, pbyRcvData) != 0)
                    {
                        pbyRcvData = null;
                        text += "Get IQ Data error!";
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    text = text + ex.Message.ToString() + Environment.NewLine;
                    pbyRcvData = null;
                    return -1;
                }
            }
            else
            {
                pbyRcvData = null;
            }
        }
        else
        {
            pbyRcvData = null;
        }
        return result;
    }

    public int ReceiveUSBImagePacket(Control control, CAMERA_CMD_TYPE cmdType, uint itemId, uint nTransFlag, out byte[] pbyRcvData)
    {
        string text = "";
        int num = 0;
        int num2 = 0;
        double num3 = 0.0;
        int num4 = 0;
        pbyRcvData = null;
        Stopwatch stopwatch = new Stopwatch();
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = SendUSBPacket(cmdType, itemId, nTransFlag, null, 0);
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK && iQ_CMD_RESPONSE_S.DataLen > 0)
        {
            pbyRcvData = new byte[iQ_CMD_RESPONSE_S.DataLen];
            num2 = iQ_CMD_RESPONSE_S.DataLen;
            _ = new byte[num2];
            num = iQ_CMD_RESPONSE_S.DataLen;
            try
            {
                num2 = iQ_CMD_RESPONSE_S.DataLen;
                pbyRcvData = new byte[iQ_CMD_RESPONSE_S.DataLen];
                num = iQ_CMD_RESPONSE_S.DataLen;
                CALLBACK_DATA_STRUCT cALLBACK_DATA_STRUCT = new CALLBACK_DATA_STRUCT();
                cALLBACK_DATA_STRUCT.lpIQcomm = this;
                cALLBACK_DATA_STRUCT.control = control;
                callBackHandler lpFunc = localFun;
                if (Get_IQData(m_ait_handel, 0, null, (uint)num2, pbyRcvData, lpFunc, cALLBACK_DATA_STRUCT) != 0)
                {
                    Disconnect();
                    pbyRcvData = null;
                    Console.WriteLine("Get IQ Data error!");
                    return -1;
                }
                num4 = num;
                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    num3 = num4 / 1024 / (stopwatch.ElapsedMilliseconds / 1000.0);
                    num4 = 0;
                    stopwatch.Reset();
                    control.Invoke(new UpdateTransProgressByNetSpeedHandler(TransProgressByNetSpeed), num3);
                }
            }
            catch (Exception ex)
            {
                text = text + ex.Message.ToString() + Environment.NewLine;
                Disconnect();
                pbyRcvData = null;
                control.Invoke(new UpdateTransFinishHandler(TransFinish), text);
                return -1;
            }
            control.Invoke(new UpdateTransFinishHandler(TransFinish), text);
        }
        return num;
    }

    public unsafe int Get_IQData(IntPtr in_ait_handel, ushort CmdData_Size, byte[] CmdData_Data, uint Out_IQ_Size, byte[] Out_IQData, callBackHandler lpFunc = null, CALLBACK_DATA_STRUCT lpCallData = null)
    {
        _ = new ushort[1];
        uint[] byteRet = new uint[1];
        int num = 0;
        uint num2 = uint.MaxValue;
        if (CmdData_Size > num2 || Out_IQ_Size > num2)
        {
            return -1;
        }
        string text = "";
        Stopwatch stopwatch = new Stopwatch();
        if (CmdData_Data != null && CmdData_Size != 0)
        {
            stopwatch.Reset();
            stopwatch.Start();
            num = AITAPI_SendData(in_ait_handel, CmdData_Data, CmdData_Size, null, null, 3, 7);
            text = text + "[USB][Send] API_UpdateFlash             : buffer size = " + CmdData_Size.ToString().PadLeft(8) + "(bytes), data = " + CmdData_Data.ToString() + ", cost time = " + stopwatch.ElapsedMilliseconds.ToString().PadLeft(6) + " ms";
            stopwatch.Stop();
            Console.WriteLine(text);
            if (num != 0)
            {
                return num;
            }
        }
        text = null;
        stopwatch.Reset();
        stopwatch.Start();
        num = AITAPI_ReadPartialFlashData(in_ait_handel, 0u, Out_IQ_Size, Out_IQData, byteRet, lpFunc, lpCallData);
        text = text + "[USB][Read] AITAPI_ReadPartialFlashData : buffer size = " + Out_IQ_Size.ToString().PadLeft(8) + "(bytes), data = " + Out_IQData.ToString() + ", cost time = " + stopwatch.ElapsedMilliseconds.ToString().PadLeft(6) + " ms";
        stopwatch.Stop();
        Console.WriteLine(text);
        return num;
    }

    public int Get_IQResponseData(IntPtr in_ait_handel, byte[] Data, int DataSize)
    {
        if (m_bUSBConnection)
        {
            Console.WriteLine("[Get_IQResponseData] - IQ Data Size = {0}\n", DataSize);
            uint[] byteRet = new uint[1];
            return AITAPI_ReadPartialFlashData(in_ait_handel, 9u, (ushort)DataSize, Data, byteRet, null, null);
        }
        return 0;
    }

    public string GetChipNameByID(ChipID chipId, STRING_OPERATE_TYPE_E type)
    {
        string text = "";
        text = m_chiplist[(ulong)chipId];
        switch (type)
        {
            case STRING_OPERATE_TYPE_E.TYPE_UPPER_E:
                text = text.ToUpper();
                break;
            case STRING_OPERATE_TYPE_E.TYPE_LOWER_E:
                text = text.ToLower();
                break;
            case STRING_OPERATE_TYPE_E.TYPE_UPPER_FIRST_E:
                text = text.ToLower();
                text = text.Substring(0, 1).ToUpper() + text.Substring(1);
                break;
        }
        return text;
    }

    public ulong GetChipIDByName(string name)
    {
        ulong result = 0uL;
        Chiplist[] chipList = m_config.ChipList;
        foreach (Chiplist chiplist in chipList)
        {
            if (string.Compare(name, chiplist.name, ignoreCase: true) == 0 || string.Compare(name, chiplist.alias, ignoreCase: true) == 0)
            {
                result = chiplist.id;
            }
        }
        return result;
    }

    public int ChipIsSupport(ChipID chipId)
    {
        int result = 0;
        if (m_chiplist.ContainsKey((ulong)chipId))
        {
            result = 1;
        }
        return result;
    }

    private int AFWindowsAPIID(ChipID chipId, short api_id)
    {
        int result = 0;
        if ((chipId >= ChipID.I3 || chipId == ChipID.M5 || chipId == ChipID.M5U) && GetApiIDValue(chipId, "AFWindow") == api_id)
        {
            result = 1;
        }
        return result;
    }

    public int GetChipSupportScl(ChipID chipId)
    {
        int result = 0;
        Chiplist[] chipList = m_config.ChipList;
        foreach (Chiplist chiplist in chipList)
        {
            if ((ulong)chipId == chiplist.id)
            {
                result = chiplist.SupportSCL;
                break;
            }
        }
        return result;
    }

    public void GetCalibrateLibChipInfoID(ChipID chipId, ref CHIP_INFO_ID chipInfoId)
    {
        uint num = 0u;
        Chiplist[] chipList = m_config.ChipList;
        foreach (Chiplist chiplist in chipList)
        {
            if ((ulong)chipId == chiplist.id)
            {
                num = chiplist.CalibrateChipID;
                break;
            }
        }
        chipInfoId = (CHIP_INFO_ID)num;
    }

    public short GetApiIDValue(ChipID chipId, string name)
    {
        short result = 0;
        try
        {
            string text = m_chiplist[(ulong)chipId];
            if (string.IsNullOrEmpty(text))
            {
                return result;
            }
            Parameter[] parameters = m_config.Parameters;
            foreach (Parameter parameter in parameters)
            {
                if (parameter.chip == text)
                {
                    result = Convert.ToInt16(parameter.GetType().GetProperty(name).GetValue(parameter, null));
                    break;
                }
            }
            return result;
        }
        catch
        {
            return result;
        }
    }
}
