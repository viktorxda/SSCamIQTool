using System;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

internal class CommunicationManager
{
    public enum TransmissionType
    {
        Text,
        Hex
    }

    public enum MessageType
    {
        Incoming,
        Outgoing,
        Normal,
        Warning,
        Error
    }

    private readonly Color[] MessageColor = new Color[5]
    {
        Color.Blue,
        Color.Green,
        Color.Black,
        Color.Orange,
        Color.Red
    };

    private readonly SerialPort comPort = new();

    public bool AutoEOL { get; set; } = true;

    public bool isPortOpen => comPort.IsOpen;

    public string BaudRate { get; set; } = string.Empty;

    public string Parity { get; set; } = string.Empty;

    public string StopBits { get; set; } = string.Empty;

    public string DataBits { get; set; } = string.Empty;

    public string PortName { get; set; } = string.Empty;

    public TransmissionType CurrentTransmissionType { get; set; }

    public RichTextBox DisplayWindow { get; set; }

    public CommunicationManager(string baud, string par, string sBits, string dBits, string name, RichTextBox rtb)
    {
        BaudRate = baud;
        Parity = par;
        StopBits = sBits;
        DataBits = dBits;
        PortName = name;
        DisplayWindow = rtb;
        comPort.DataReceived += comPort_DataReceived;
    }

    public CommunicationManager()
    {
        BaudRate = string.Empty;
        Parity = string.Empty;
        StopBits = string.Empty;
        DataBits = string.Empty;
        PortName = "COM1";
        DisplayWindow = null;
        comPort.DataReceived += comPort_DataReceived;
    }

    public void WriteData(string msg)
    {
        if (!comPort.IsOpen)
        {
            DisplayData(MessageType.Error, "Open Port before sending data!\n");
        }
        switch (CurrentTransmissionType)
        {
            case TransmissionType.Text:
                comPort.Write(msg);
                SendEndOfLine();
                DisplayData(MessageType.Outgoing, msg + "\n");
                break;
            case TransmissionType.Hex:
                try
                {
                    byte[] array = HexToByte(msg);
                    comPort.Write(array, 0, array.Length);
                    SendEndOfLine();
                    DisplayData(MessageType.Outgoing, ByteToHex(array) + "\n");
                    break;
                }
                catch (FormatException ex)
                {
                    DisplayData(MessageType.Error, ex.Message + "\n");
                    break;
                }
                finally
                {
                    DisplayWindow.SelectAll();
                }
            default:
                comPort.Write(msg);
                SendEndOfLine();
                DisplayData(MessageType.Outgoing, msg + "\n");
                break;
        }
    }

    public void SendEndOfLine()
    {
        byte[] buffer = new byte[1] { 13 };
        if (comPort.IsOpen && AutoEOL)
        {
            comPort.Write(buffer, 0, 1);
        }
    }

    private byte[] HexToByte(string msg)
    {
        msg = msg.Replace(" ", "");
        byte[] array = new byte[msg.Length / 2];
        for (int i = 0; i < msg.Length; i += 2)
        {
            array[i / 2] = Convert.ToByte(msg.Substring(i, 2), 16);
        }
        return array;
    }

    private string ByteToHex(byte[] comByte)
    {
        StringBuilder stringBuilder = new(comByte.Length * 3);
        foreach (byte b in comByte)
        {
            _ = stringBuilder.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
        }
        return stringBuilder.ToString().ToUpper();
    }

    [STAThread]
    private void DisplayData(MessageType type, string msg)
    {
        _ = DisplayWindow.Invoke((EventHandler)delegate
        {
            DisplayWindow.SelectedText = string.Empty;
            DisplayWindow.SelectionFont = new Font(DisplayWindow.SelectionFont, FontStyle.Bold);
            DisplayWindow.SelectionColor = MessageColor[(int)type];
            DisplayWindow.AppendText(msg);
            DisplayWindow.ScrollToCaret();
        });
    }

    public bool OpenPort()
    {
        try
        {
            ClosePort();
            comPort.BaudRate = int.Parse(BaudRate);
            comPort.DataBits = int.Parse(DataBits);
            comPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits);
            comPort.Parity = (Parity)Enum.Parse(typeof(Parity), Parity);
            comPort.PortName = PortName;
            comPort.Open();
            DisplayData(MessageType.Normal, string.Concat("Port opened at ", DateTime.Now, "\n"));
            return true;
        }
        catch (Exception ex)
        {
            DisplayData(MessageType.Error, ex.Message + "\n");
            return false;
        }
    }

    public void ClosePort()
    {
        if (comPort.IsOpen)
        {
            comPort.Close();
        }
    }

    public void SetParityValues(object obj)
    {
        string[] names = Enum.GetNames(typeof(Parity));
        foreach (string item in names)
        {
            _ = ((ComboBox)obj).Items.Add(item);
        }
    }

    public void SetStopBitValues(object obj)
    {
        string[] names = Enum.GetNames(typeof(StopBits));
        foreach (string item in names)
        {
            _ = ((ComboBox)obj).Items.Add(item);
        }
    }

    public void SetPortNameValues(object obj)
    {
        string[] portNames = SerialPort.GetPortNames();
        Array.Sort(portNames);
        string[] array = portNames;
        foreach (string item in array)
        {
            _ = ((ComboBox)obj).Items.Add(item);
        }
    }

    private void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        switch (CurrentTransmissionType)
        {
            case TransmissionType.Text:
                {
                    string text = comPort.ReadExisting().Trim();
                    if (text.Length > 0)
                    {
                        DisplayData(MessageType.Incoming, text + "\n");
                    }
                    break;
                }
            case TransmissionType.Hex:
                {
                    int bytesToRead = comPort.BytesToRead;
                    byte[] array = new byte[bytesToRead];
                    _ = comPort.Read(array, 0, bytesToRead);
                    if (bytesToRead > 0)
                    {
                        DisplayData(MessageType.Incoming, ByteToHex(array) + "\n");
                    }
                    break;
                }
            default:
                {
                    string text = comPort.ReadExisting().Trim();
                    if (text.Length > 0)
                    {
                        DisplayData(MessageType.Incoming, text + "\n");
                    }
                    break;
                }
        }
    }
}
