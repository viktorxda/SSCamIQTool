using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SSCamIQTool.LibComm;

public class ImageCapture
{
    private IQComm captureComm;

    private SensorInfo sensorInfo;

    private Control control;

    private ChipID m_chipID;

    private CONNECT_MODE connectMode;

    private uint m_RawCount;

    private uint m_RawLoopCount;

    private int m_SclDev;

    private int m_SclChn;

    private int m_SclPort;

    private int m_HdrEnable;

    private CAPTURE_PICTURE_INFO_S stCaptureInfo;

    [DllImport("libFbcDecode.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int fbc_decode(int chipType, string src, string dst, int width, int height);

    public ImageCapture(IQComm captureComm, SensorInfo sensorInfo, Control control)
    {
        this.captureComm = captureComm;
        this.sensorInfo = sensorInfo;
        this.control = control;
    }

    public void SetChipID(ChipID id)
    {
        m_chipID = id;
    }

    public void SetConnectMode(CONNECT_MODE mode)
    {
        connectMode = mode;
    }

    public void SetRawCaptureSetting(uint frame_count, uint loop_count)
    {
        m_RawCount = frame_count;
        m_RawLoopCount = loop_count;
    }

    private void GetFbcDecodeChipType(ChipID chipId, ref CHIP_INFO_ID chipInfoId)
    {
        switch (chipId)
        {
            case ChipID.I6E:
            case ChipID.P3:
                chipInfoId = CHIP_INFO_ID.eCHIP_INFO_ID_PUDDING;
                break;
            case ChipID.M6:
            case ChipID.I7:
            case ChipID.M6P:
            case ChipID.I6C:
            case ChipID.P5:
                chipInfoId = CHIP_INFO_ID.eCHIP_INFO_ID_TIRAMISU;
                break;
            default:
                chipInfoId = CHIP_INFO_ID.eCHIP_INFO_ID_MAX;
                break;
        }
    }

    public void SetSclSetting(int device, int channel, int port)
    {
        m_SclDev = device;
        m_SclChn = channel;
        m_SclPort = port;
    }

    private void GetImageResolution(CAMERA_MODE_TYPE type)
    {
        int num = 0;
        int num2 = 0;
        byte[] pbyRcvData = null;
        byte[] array = null;
        short[] src = new short[1] { 106 };
        short[] src2 = new short[1] { 1 };
        int[] src3 = new int[1];
        int num3 = 0;
        int num4 = 0;
        CONNECT_MODE cONNECT_MODE = captureComm.ConnectMode;
        array = new byte[12];
        Buffer.BlockCopy(src, 0, array, num4, 2);
        num4 += 2;
        Buffer.BlockCopy(src2, 0, array, num4, 2);
        num4 += 2;
        Buffer.BlockCopy(src3, 0, array, num4, 4);
        num4 += 4;
        Buffer.BlockCopy(new int[1] { (int)type }, 0, array, num4, 4);
        num3 = cONNECT_MODE switch
        {
            CONNECT_MODE.MODE_SOCKET => captureComm.ReceiveApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, array, out pbyRcvData),
            CONNECT_MODE.MODE_UART => captureComm.ReceiveUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_API, array, out pbyRcvData),
            _ => captureComm.ReceiveUSBApiPacketData(array, out pbyRcvData),
        };
        if (num3 >= 8)
        {
            num2 = BitConverter.ToUInt16(pbyRcvData, 2);
            pbyRcvData = pbyRcvData.Skip(4 + 4 * num2).ToArray();
            uint width = BitConverter.ToUInt32(pbyRcvData, num * 4);
            num++;
            uint height = BitConverter.ToUInt32(pbyRcvData, num * 4);
            num++;
            if (num3 > 16)
            {
                stCaptureInfo.Width = width;
                stCaptureInfo.Height = height;
                stCaptureInfo.Count = BitConverter.ToUInt32(pbyRcvData, num * 4);
                num++;
                stCaptureInfo.HdrMode = BitConverter.ToUInt32(pbyRcvData, num * 4);
                num++;
                stCaptureInfo.CompressType = BitConverter.ToUInt32(pbyRcvData, num * 4);
            }
            sensorInfo.Width = (int)width;
            sensorInfo.Height = (int)height;
        }
    }

    private string GetImageInfo(ImageInfo imageStatus)
    {
        byte[] pbyRcvData = null;
        byte[] pbyRcvData2 = null;
        short apiIDValue = captureComm.GetApiIDValue(m_chipID, "QueryWBInfo");
        short apiIDValue2 = captureComm.GetApiIDValue(m_chipID, "QueryExposureInfo");
        switch (m_chipID)
        {
            case ChipID.I1:
                captureComm.ReceiveApiPacket(apiIDValue2, out pbyRcvData);
                captureComm.ReceiveApiPacket(apiIDValue, out pbyRcvData2);
                break;
            case ChipID.I3:
                captureComm.ReceiveApiPacket(apiIDValue2, out pbyRcvData);
                captureComm.ReceiveApiPacket(apiIDValue, out pbyRcvData2);
                break;
            case ChipID.M5:
            case ChipID.M5U:
                if (connectMode == CONNECT_MODE.MODE_USB)
                {
                    captureComm.ReceiveUsbApiPacket(apiIDValue2, out pbyRcvData);
                    captureComm.ReceiveUsbApiPacket(apiIDValue, out pbyRcvData2);
                }
                break;
            case ChipID.I2:
                if (connectMode == CONNECT_MODE.MODE_SOCKET)
                {
                    captureComm.ReceiveApiPacket(apiIDValue2, out pbyRcvData);
                    captureComm.ReceiveApiPacket(apiIDValue, out pbyRcvData2);
                }
                else if (connectMode == CONNECT_MODE.MODE_USB)
                {
                    captureComm.ReceiveUsbApiPacket(apiIDValue2, out pbyRcvData);
                    captureComm.ReceiveUsbApiPacket(apiIDValue, out pbyRcvData2);
                }
                break;
            case ChipID.I5:
                if (connectMode == CONNECT_MODE.MODE_SOCKET)
                {
                    captureComm.ReceiveApiPacket(apiIDValue2, out pbyRcvData);
                    captureComm.ReceiveApiPacket(apiIDValue, out pbyRcvData2);
                }
                else if (connectMode == CONNECT_MODE.MODE_USB)
                {
                    captureComm.ReceiveUsbApiPacket(apiIDValue2, out pbyRcvData);
                    captureComm.ReceiveUsbApiPacket(apiIDValue, out pbyRcvData2);
                }
                break;
            default:
                if (connectMode == CONNECT_MODE.MODE_SOCKET)
                {
                    captureComm.ReceiveApiPacket(apiIDValue2, out pbyRcvData);
                    captureComm.ReceiveApiPacket(apiIDValue, out pbyRcvData2);
                }
                else if (connectMode == CONNECT_MODE.MODE_USB)
                {
                    captureComm.ReceiveUsbApiPacket(apiIDValue2, out pbyRcvData);
                    captureComm.ReceiveUsbApiPacket(apiIDValue, out pbyRcvData2);
                }
                else if (connectMode == CONNECT_MODE.MODE_UART)
                {
                    captureComm.ReceiveUartApiPacket(apiIDValue2, out pbyRcvData);
                    captureComm.ReceiveUartApiPacket(apiIDValue, out pbyRcvData2);
                }
                break;
        }
        if (pbyRcvData != null && pbyRcvData2 != null)
        {
            PacketParser.ConvertBufferToType(imageStatus, m_chipID, pbyRcvData, pbyRcvData2);
        }
        return imageStatus.ToString();
    }

    private string GetImageFilePath(EXPOSURE_TYPE_E exposure_type, CAMERA_MODE_TYPE type, string strName)
    {
        string text = "";
        string text2 = "";
        string text3 = "";
        string text4 = "";
        string text5 = "";
        DateTime now = DateTime.Now;
        ImageInfo imageInfo = new ImageInfo();
        imageInfo.CaptureType = type;
        imageInfo.HdrEnable = m_HdrEnable;
        imageInfo.ExposureType = exposure_type;
        text3 = Application.StartupPath + SettingsComm.Default.SaveFolder;
        switch (type)
        {
            case CAMERA_MODE_TYPE.CAMERA_MODE_ISP_OUT:
                text2 = SettingsComm.Default.SaveIspOutputName;
                text = ".yuv";
                break;
            case CAMERA_MODE_TYPE.CAMERA_MODE_ENC_JPEG:
                text2 = SettingsComm.Default.SaveJpegName;
                text = ".jpg";
                break;
            case CAMERA_MODE_TYPE.CAMERA_MODE_SCL_OUT:
                text2 = SettingsComm.Default.SaveScOuputName;
                text = ".yuv";
                break;
            case CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM:
                text2 = SettingsComm.Default.SaveRawStreamName;
                text = ".raw";
                break;
            default:
                text2 = SettingsComm.Default.SaveRawName;
                text = ".raw";
                break;
        }
        text5 = GetImageInfo(imageInfo);
        text4 = type != 0 ? sensorInfo.ToString() : sensorInfo.ScOutToString();
        if (type == CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW || type == CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM)
        {
            return text3 + "\\" + text2 + "_" + strName + "_" + text4 + now.Month.ToString("D2") + now.Day.ToString("D2") + now.Hour.ToString("D2") + now.Minute.ToString("D2") + now.Second.ToString("D2") + "_[" + text5 + "]" + text;
        }
        return text3 + "\\" + text2 + "_" + strName + "_" + text4 + now.Month.ToString("D2") + now.Day.ToString("D2") + now.Hour.ToString("D2") + now.Minute.ToString("D2") + now.Second.ToString("D2") + "_[" + text5 + "]" + text;
    }

    private int runCmd(string dir, string exe_path, string cmd_str)
    {
        using (Process process = new Process())
        {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = exe_path;
            process.StartInfo.Arguments = cmd_str;
            if (!string.IsNullOrEmpty(dir))
            {
                process.StartInfo.WorkingDirectory = dir;
            }
            if (!File.Exists(exe_path))
            {
                return -1;
            }
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                return process.ExitCode;
            }
        }
        return 0;
    }

    private string GetModulePath(MODULE_TYPE_E type)
    {
        string text = "";
        text = Directory.GetCurrentDirectory();
        switch (type)
        {
            case MODULE_TYPE_E.MODULE_DSC_DIR_E:
                text += "\\Module\\DSC\\";
                break;
            case MODULE_TYPE_E.MODULE_DSC_EXE_E:
                text += "\\Module\\DSC\\DSC.exe";
                break;
        }
        return text;
    }

    private string GetDscDecodePara()
    {
        string text = "";
        text = GetModulePath(MODULE_TYPE_E.MODULE_DSC_DIR_E) + "config.cfg";
        return "-F " + text;
    }

    private string RunDscDecode(string origin_file, string file)
    {
        string text = "";
        string result = "";
        string text2 = "";
        string text3 = "";
        string path = GetModulePath(MODULE_TYPE_E.MODULE_DSC_DIR_E) + "file_list.txt";
        text3 = GetModulePath(MODULE_TYPE_E.MODULE_DSC_DIR_E) + "\\dsc_origin.out.yuv";
        using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
        {
            StreamWriter streamWriter = new StreamWriter(stream);
            streamWriter.WriteLine("dsc_origin.dsc");
            streamWriter.Close();
        }
        text = GetDscDecodePara();
        text2 = GetModulePath(MODULE_TYPE_E.MODULE_DSC_EXE_E);
        if (runCmd(GetModulePath(MODULE_TYPE_E.MODULE_DSC_DIR_E), text2, text) == 0)
        {
            if (File.Exists(text3))
            {
                new FileInfo(text3).MoveTo(file);
            }
        }
        else
        {
            result = "DSC Decode Fail.";
        }
        return result;
    }

    private string DscDecodeRaw(string origin_file, string strFileName)
    {
        return RunDscDecode(origin_file, strFileName);
    }

    private string FbcDecodeRaw(string origin_file, string strFileName)
    {
        int num = 0;
        string result = "";
        CHIP_INFO_ID chipInfoId = CHIP_INFO_ID.eCHIP_INFO_ID_PUDDING;
        if (!File.Exists(origin_file))
        {
            return "File Not Exist:" + origin_file;
        }
        FileInfo fileInfo = new FileInfo(origin_file);
        num = sensorInfo.Height;
        if (fileInfo.Length == sensorInfo.Width * sensorInfo.Height * 2)
        {
            num = sensorInfo.Height * 2;
        }
        GetFbcDecodeChipType(m_chipID, ref chipInfoId);
        if (fbc_decode((int)chipInfoId, origin_file, strFileName, sensorInfo.Width, num) != 0)
        {
            result = "decode raw fail";
        }
        else
        {
            try
            {
                fileInfo.Delete();
            }
            catch (IOException ex)
            {
                result = ex.Message;
            }
        }
        return result;
    }

    public string SaveImage(EXPOSURE_TYPE_E exposure_type, byte[] pImageFile, CAMERA_MODE_TYPE type, string strPrefix)
    {
        string result = "";
        string text = "";
        string text2 = "";
        string text3 = "";
        GetImageResolution(type);
        text = GetImageFilePath(exposure_type, type, strPrefix);
        text2 = Application.StartupPath + SettingsComm.Default.SaveFolder;
        if (pImageFile != null)
        {
            try
            {
                if (!Directory.Exists(text2))
                {
                    Directory.CreateDirectory(text2);
                }
                RAW_COMPRESS_TYPE_E rAW_COMPRESS_TYPE_E = RAW_COMPRESS_TYPE_E.RAW_COMPRESS_NONE_E;
                rAW_COMPRESS_TYPE_E = (RAW_COMPRESS_TYPE_E)GeRawCompress(12804);
                if (type == CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW)
                {
                    switch (rAW_COMPRESS_TYPE_E)
                    {
                        case RAW_COMPRESS_TYPE_E.RAW_COMPRESS_DSC_E:
                            {
                                text3 = GetModulePath(MODULE_TYPE_E.MODULE_DSC_DIR_E) + "\\dsc_origin.dsc";
                                using (FileStream output3 = File.Open(text3, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                {
                                    using BinaryWriter binaryWriter3 = new BinaryWriter(output3);
                                    binaryWriter3.Write(pImageFile);
                                }
                                result = DscDecodeRaw(text3, text);
                                break;
                            }
                        case RAW_COMPRESS_TYPE_E.RAW_COMPRESS_FBC_E:
                            {
                                text3 = text2 + "\\origin_no_decode.raw";
                                using (FileStream output2 = File.Open(text3, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                {
                                    using BinaryWriter binaryWriter2 = new BinaryWriter(output2);
                                    binaryWriter2.Write(pImageFile);
                                }
                                if (m_HdrEnable == 1)
                                {
                                    text = text2 + "\\fbc_decode.raw";
                                    result = FbcDecodeRaw(text3, text);
                                }
                                else
                                {
                                    result = FbcDecodeRaw(text3, text);
                                }
                                if (m_HdrEnable == 1)
                                {
                                    string imageFilePath = GetImageFilePath(EXPOSURE_TYPE_E.EXPOSURE_SHORT_E, type, "short");
                                    SplitFileBySize(text, imageFilePath, 0, sensorInfo.Width * sensorInfo.Height * 2);
                                    string imageFilePath2 = GetImageFilePath(EXPOSURE_TYPE_E.EXPOSURE_LONG_E, type, "long");
                                    SplitFileBySize(text, imageFilePath2, sensorInfo.Width * sensorInfo.Height * 2, sensorInfo.Width * sensorInfo.Height * 2);
                                    new FileInfo(text).Delete();
                                }
                                break;
                            }
                        default:
                            {
                                using (FileStream output = File.Open(text, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                {
                                    using BinaryWriter binaryWriter = new BinaryWriter(output);
                                    binaryWriter.Write(pImageFile);
                                }
                                break;
                            }
                    }
                }
                else
                {
                    using FileStream output4 = File.Open(text, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    using BinaryWriter binaryWriter4 = new BinaryWriter(output4);
                    binaryWriter4.Write(pImageFile);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
        }
        else
        {
            result = "Image File is null";
        }
        return result;
    }

    private string DecodeRawStream(int count, string dir, string strOriginFile, string strImageFile)
    {
        int num = 0;
        string text = "";
        string text2 = "";
        string text3 = "";
        string text4 = "";
        List<string> list = new List<string>();
        if (stCaptureInfo.CompressType == 0)
        {
            return text;
        }
        text2 = GetDecodeInputRawSuffix();
        SplitFile(count, dir, strOriginFile);
        for (num = 0; num < count; num++)
        {
            text3 = dir + "\\split_" + num + text2;
            text4 = dir + "\\decode_" + num + ".raw";
            if (!File.Exists(text3))
            {
                text = "File not Exist";
                break;
            }
            switch ((RAW_COMPRESS_TYPE_E)stCaptureInfo.CompressType)
            {
                case RAW_COMPRESS_TYPE_E.RAW_COMPRESS_DSC_E:
                    text = DscDecodeRaw(text3, text4);
                    break;
                case RAW_COMPRESS_TYPE_E.RAW_COMPRESS_FBC_E:
                    text = FbcDecodeRaw(text3, text4);
                    break;
            }
            try
            {
                File.Delete(text3);
            }
            catch (IOException ex)
            {
                text = ex.Message;
            }
            if (text != "")
            {
                break;
            }
            list.Add(text4);
        }
        CombineFile(list, strImageFile);
        RemoveFileList(list);
        return text;
    }

    private void SetRawFrameCount()
    {
        int[] data = new int[1] { (int)m_RawCount };
        CONNECT_MODE cONNECT_MODE = captureComm.ConnectMode;
        byte[] apiBufferByDataArray = PacketParser.GetApiBufferByDataArray(12803, data);
        if (captureComm.IsConnected())
        {
            switch (cONNECT_MODE)
            {
                case CONNECT_MODE.MODE_USB:
                    captureComm.SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    captureComm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_UART:
                    captureComm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, apiBufferByDataArray, apiBufferByDataArray.Length);
                    break;
            }
        }
    }

    private void SetSclModInfo()
    {
        int[] data = new int[3] { m_SclDev, m_SclChn, m_SclPort };
        CONNECT_MODE cONNECT_MODE = captureComm.ConnectMode;
        byte[] sendBufferByDataArray = PacketParser.GetSendBufferByDataArray(12805, data);
        if (captureComm.IsConnected())
        {
            switch (cONNECT_MODE)
            {
                case CONNECT_MODE.MODE_USB:
                    captureComm.SendUSBApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, sendBufferByDataArray, sendBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_SOCKET:
                    captureComm.SendApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, sendBufferByDataArray, sendBufferByDataArray.Length);
                    break;
                case CONNECT_MODE.MODE_UART:
                    captureComm.SendUartApiPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_API, sendBufferByDataArray, sendBufferByDataArray.Length);
                    break;
            }
        }
    }

    private ushort GeRawCompress(short RawCompressApiID)
    {
        ushort result = 0;
        byte[] pbyRcvData = null;
        byte[] bufferInitial = new byte[100];
        int num = 0;
        switch (captureComm.ConnectMode)
        {
            case CONNECT_MODE.MODE_USB:
                num = captureComm.ReceiveUSBInitialApiPacket(RawCompressApiID, bufferInitial, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = captureComm.ReceiveApiPacket(RawCompressApiID, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                num = captureComm.ReceiveUartApiPacket(RawCompressApiID, out pbyRcvData);
                break;
        }
        if (num > 0)
        {
            result = BitConverter.ToUInt16(pbyRcvData, 8);
        }
        return result;
    }

    private int GetImageByProtocol(CAMERA_MODE_TYPE type, uint itemId, uint nTransCnt, int ShowProgress, out byte[] pImageBuffer)
    {
        int result = 0;
        if (ShowProgress == 1)
        {
            captureComm.StartShowProgressBar(control);
        }
        pImageBuffer = null;
        if (captureComm.ConnectMode == CONNECT_MODE.MODE_USB)
        {
            result = captureComm.ReceiveUSBImagePacket(control, CAMERA_CMD_TYPE.CAMERA_CMD_GET_PIC, itemId, nTransCnt, out pImageBuffer);
        }
        else if (captureComm.ConnectMode == CONNECT_MODE.MODE_UART)
        {
            result = captureComm.ReceiveUartImagePacket(control, CAMERA_CMD_TYPE.CAMERA_CMD_GET_PIC, itemId, nTransCnt, out pImageBuffer);
        }
        else if (captureComm.ConnectMode == CONNECT_MODE.MODE_SOCKET)
        {
            result = captureComm.ReceiveImagePacket(control, CAMERA_CMD_TYPE.CAMERA_CMD_GET_PIC, itemId, nTransCnt, out pImageBuffer);
        }
        return result;
    }

    private int GetHdrMode()
    {
        byte[] pbyRcvData = null;
        if (captureComm.ConnectMode == CONNECT_MODE.MODE_USB)
        {
            captureComm.ReceiveUSBPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_MODE, out pbyRcvData);
        }
        else if (captureComm.ConnectMode == CONNECT_MODE.MODE_UART)
        {
            captureComm.ReceiveUartPacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_MODE, out pbyRcvData);
        }
        else if (captureComm.ConnectMode == CONNECT_MODE.MODE_SOCKET)
        {
            captureComm.ReceivePacket(CAMERA_CMD_TYPE.CAMERA_CMD_GET_MODE, out pbyRcvData);
        }
        if (pbyRcvData[0] == 0)
        {
            return 0;
        }
        return 1;
    }

    private string GetDecodeInputRawSuffix()
    {
        string result = ".raw";
        RAW_COMPRESS_TYPE_E compressType = (RAW_COMPRESS_TYPE_E)stCaptureInfo.CompressType;
        if (compressType == RAW_COMPRESS_TYPE_E.RAW_COMPRESS_DSC_E)
        {
            result = ".dsc";
        }
        return result;
    }

    private void SplitFileBySize(string strOrigin, string strDst, int nStarIndex, int nSplitSize)
    {
        FileInfo fileInfo = new FileInfo(strOrigin);
        if (nStarIndex + nSplitSize > fileInfo.Length)
        {
            return;
        }
        using FileStream fileStream = new FileStream(strOrigin, FileMode.Open, FileAccess.Read);
        fileStream.Seek(nStarIndex, SeekOrigin.Begin);
        using (BinaryReader binaryReader = new BinaryReader(fileStream))
        {
            byte[] buffer = binaryReader.ReadBytes(nSplitSize);
            using FileStream output = new FileStream(strDst, FileMode.Create);
            using BinaryWriter binaryWriter = new BinaryWriter(output);
            binaryWriter.Write(buffer);
        }
        fileStream.Dispose();
    }

    private void SplitFile(int count, string dir, string origin_file)
    {
        string text = "";
        int count2 = (int)(new FileInfo(origin_file).Length / count);
        text = GetDecodeInputRawSuffix();
        using FileStream input = new FileStream(origin_file, FileMode.Open, FileAccess.Read);
        using BinaryReader binaryReader = new BinaryReader(input);
        int num = 0;
        while (count > 0)
        {
            string path = dir + "\\split_" + num + text;
            byte[] buffer = binaryReader.ReadBytes(count2);
            using (FileStream output = new FileStream(path, FileMode.Create))
            {
                using BinaryWriter binaryWriter = new BinaryWriter(output);
                binaryWriter.Write(buffer);
            }
            count--;
            num++;
        }
    }

    public void CombineFile(List<string> infileName, string outfileName)
    {
        int num = 0;
        int count = infileName.Count;
        FileStream[] array = new FileStream[count];
        using FileStream fileStream = new FileStream(outfileName, FileMode.Create);
        for (int i = 0; i < count; i++)
        {
            try
            {
                array[i] = new FileStream(infileName[i], FileMode.Open);
                while ((num = array[i].ReadByte()) != -1)
                {
                    fileStream.WriteByte((byte)num);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                array[i].Close();
            }
        }
    }

    private string RemoveFileList(List<string> file_list)
    {
        string result = "";
        foreach (string item in file_list)
        {
            if (File.Exists(item))
            {
                try
                {
                    File.Delete(item);
                }
                catch (IOException ex)
                {
                    result = ex.Message;
                }
            }
        }
        return result;
    }

    private int GetRawData(uint itemId, uint nTransCnt, ref string strResult)
    {
        int num = 0;
        int num2 = 1;
        int showProgress = 0;
        int num3 = 0;
        string strPrefix = "long";
        byte[] pImageBuffer = null;
        m_HdrEnable = GetHdrMode();
        if (m_HdrEnable == 1)
        {
            num2 = 2;
        }
        strResult = "";
        for (num = 0; num < num2; num++)
        {
            if (num >= 1)
            {
                showProgress = 1;
            }
            if (GetImageByProtocol(CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW, itemId, nTransCnt, showProgress, out pImageBuffer) <= 0)
            {
                strResult = "Get Raw Data Fail.";
                break;
            }
            if (num == 1)
            {
                strPrefix = "short";
                strResult = SaveImage(EXPOSURE_TYPE_E.EXPOSURE_SHORT_E, pImageBuffer, CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW, strPrefix);
            }
            else
            {
                strResult = SaveImage(EXPOSURE_TYPE_E.EXPOSURE_LONG_E, pImageBuffer, CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW, strPrefix);
            }
            if (strResult != "")
            {
                break;
            }
            num3++;
        }
        return num3;
    }

    private int GetRawStream(uint itemId, uint nTransCnt, ref string strResult)
    {
        int num = 0;
        int num2 = 0;
        int showProgress = 0;
        string text = "";
        string text2 = "";
        string text3 = "";
        string text4 = "";
        byte[] pImageBuffer = null;
        List<string> list = new List<string>();
        text3 = Application.StartupPath + SettingsComm.Default.SaveFolder;
        if (!Directory.Exists(text3))
        {
            Directory.CreateDirectory(text3);
        }
        for (num = 0; num < m_RawLoopCount; num++)
        {
            if (num >= 1)
            {
                showProgress = 1;
            }
            if (GetImageByProtocol(CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM, itemId, nTransCnt, showProgress, out pImageBuffer) < 0)
            {
                strResult = "Get camera image error!";
                break;
            }
            text = text3 + "\\raw_stream_" + num + ".raw";
            using (FileStream output = File.Open(text, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using BinaryWriter binaryWriter = new BinaryWriter(output);
                binaryWriter.Write(pImageBuffer);
            }
            list.Add(text);
            GetImageResolution(CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM);
            if (stCaptureInfo.Count != 0)
            {
                num2 += (int)stCaptureInfo.Count;
            }
        }
        text4 = m_HdrEnable != 0 ? "hdr" : "normal";
        if (stCaptureInfo.CompressType == 0)
        {
            if (list.Count > 0)
            {
                text2 = GetImageFilePath(EXPOSURE_TYPE_E.EXPOSURE_LONG_E, CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM, text4);
                CombineFile(list, text2);
            }
        }
        else if (list.Count > 0)
        {
            text = text3 + "\\raw_stream_combine.raw";
            text2 = GetImageFilePath(EXPOSURE_TYPE_E.EXPOSURE_LONG_E, CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM, text4);
            CombineFile(list, text);
            DecodeRawStream(num2, text3, text, text2);
            if (File.Exists(text))
            {
                try
                {
                    File.Delete(text);
                }
                catch (IOException ex)
                {
                    strResult = ex.Message;
                }
            }
        }
        RemoveFileList(list);
        return list.Count;
    }

    public string GetImageByMode(CAMERA_MODE_TYPE type)
    {
        int num = 0;
        string strResult = "";
        string text = "";
        uint num2 = 0u;
        IQ_CMD_RESPONSE_S iQ_CMD_RESPONSE_S = default;
        byte[] pImageBuffer = null;
        if (type == CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM)
        {
            SetRawFrameCount();
            if (captureComm.ConnectMode == CONNECT_MODE.MODE_USB)
            {
                iQ_CMD_RESPONSE_S = captureComm.SendUSBPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_MODE, (uint)type, num2, null, 0);
            }
            else if (captureComm.ConnectMode == CONNECT_MODE.MODE_UART)
            {
                iQ_CMD_RESPONSE_S = captureComm.SendUartPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_MODE, (uint)type, num2, null, 0);
            }
            else if (captureComm.ConnectMode == CONNECT_MODE.MODE_SOCKET)
            {
                iQ_CMD_RESPONSE_S = captureComm.SendPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_MODE, (uint)type, num2, null, 0);
            }
        }
        if (type == CAMERA_MODE_TYPE.CAMERA_MODE_SCL_OUT && captureComm.GetChipSupportScl(m_chipID) == 1)
        {
            SetSclModInfo();
        }
        if (captureComm.ConnectMode == CONNECT_MODE.MODE_USB)
        {
            iQ_CMD_RESPONSE_S = captureComm.SendUSBPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_MODE, (uint)type, num2, null, 0);
        }
        else if (captureComm.ConnectMode == CONNECT_MODE.MODE_UART)
        {
            iQ_CMD_RESPONSE_S = captureComm.SendUartPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_MODE, (uint)type, num2, null, 0);
        }
        else if (captureComm.ConnectMode == CONNECT_MODE.MODE_SOCKET)
        {
            iQ_CMD_RESPONSE_S = captureComm.SendPacket(CAMERA_CMD_TYPE.CAMERA_CMD_SET_MODE, (uint)type, num2, null, 0);
        }
        if (iQ_CMD_RESPONSE_S.ResCode == IQ_RESPONSE_CODE_E.IQ_RES_OK)
        {
            switch (type)
            {
                case CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW:
                    num = GetRawData((uint)type, num2, ref strResult);
                    break;
                case CAMERA_MODE_TYPE.CAMERA_MODE_ISP_RAW_STRM:
                    num = GetRawStream((uint)type, num2, ref strResult);
                    break;
                case CAMERA_MODE_TYPE.CAMERA_MODE_SCL_OUT:
                case CAMERA_MODE_TYPE.CAMERA_MODE_ISP_OUT:
                case CAMERA_MODE_TYPE.CAMERA_MODE_ENC_JPEG:
                    text = m_HdrEnable != 0 ? "hdr" : "normal";
                    num = GetImageByProtocol(type, (uint)type, num2, 0, out pImageBuffer);
                    strResult = num > 0 ? SaveImage(EXPOSURE_TYPE_E.EXPOSURE_LONG_E, pImageBuffer, type, text) : "Get camera image error!";
                    break;
                default:
                    strResult = "Type Not Support";
                    break;
            }
            if (num <= 0)
            {
                if (!captureComm.IsConnected())
                {
                    strResult = "Please check connection";
                }
                if (strResult == "")
                {
                    strResult = "Get camera image error!";
                }
            }
        }
        return strResult;
    }
}
