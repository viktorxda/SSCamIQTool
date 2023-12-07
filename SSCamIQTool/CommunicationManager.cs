using System;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

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

    private bool _addEOL = true;

    private string _baudRate = string.Empty;

    private string _parity = string.Empty;

    private string _stopBits = string.Empty;

    private string _dataBits = string.Empty;

    private string _portName = string.Empty;

    private TransmissionType _transType;

    private RichTextBox _displayWindow;

    private Color[] MessageColor = new Color[5]
    {
        Color.Blue,
        Color.Green,
        Color.Black,
        Color.Orange,
        Color.Red
    };

    private SerialPort comPort = new SerialPort();

    public bool AutoEOL
    {
        get
        {
            return _addEOL;
        }
        set
        {
            _addEOL = value;
        }
    }

    public bool isPortOpen => comPort.IsOpen;

    public string BaudRate
    {
        get
        {
            return _baudRate;
        }
        set
        {
            _baudRate = value;
        }
    }

    public string Parity
    {
        get
        {
            return _parity;
        }
        set
        {
            _parity = value;
        }
    }

    public string StopBits
    {
        get
        {
            return _stopBits;
        }
        set
        {
            _stopBits = value;
        }
    }

    public string DataBits
    {
        get
        {
            return _dataBits;
        }
        set
        {
            _dataBits = value;
        }
    }

    public string PortName
    {
        get
        {
            return _portName;
        }
        set
        {
            _portName = value;
        }
    }

    public TransmissionType CurrentTransmissionType
    {
        get
        {
            return _transType;
        }
        set
        {
            _transType = value;
        }
    }

    public RichTextBox DisplayWindow
    {
        get
        {
            return _displayWindow;
        }
        set
        {
            _displayWindow = value;
        }
    }

    public CommunicationManager(string baud, string par, string sBits, string dBits, string name, RichTextBox rtb)
    {
        _baudRate = baud;
        _parity = par;
        _stopBits = sBits;
        _dataBits = dBits;
        _portName = name;
        _displayWindow = rtb;
        comPort.DataReceived += comPort_DataReceived;
    }

    public CommunicationManager()
    {
        _baudRate = string.Empty;
        _parity = string.Empty;
        _stopBits = string.Empty;
        _dataBits = string.Empty;
        _portName = "COM1";
        _displayWindow = null;
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
                    _displayWindow.SelectAll();
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
        if (comPort.IsOpen && _addEOL)
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
        StringBuilder stringBuilder = new StringBuilder(comByte.Length * 3);
        foreach (byte b in comByte)
        {
            stringBuilder.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
        }
        return stringBuilder.ToString().ToUpper();
    }

    [STAThread]
    private void DisplayData(MessageType type, string msg)
    {
        _displayWindow.Invoke((EventHandler)delegate
        {
            _displayWindow.SelectedText = string.Empty;
            _displayWindow.SelectionFont = new Font(_displayWindow.SelectionFont, FontStyle.Bold);
            _displayWindow.SelectionColor = MessageColor[(int)type];
            _displayWindow.AppendText(msg);
            _displayWindow.ScrollToCaret();
        });
    }

    public bool OpenPort()
    {
        try
        {
            ClosePort();
            comPort.BaudRate = int.Parse(_baudRate);
            comPort.DataBits = int.Parse(_dataBits);
            comPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), _stopBits);
            comPort.Parity = (Parity)Enum.Parse(typeof(Parity), _parity);
            comPort.PortName = _portName;
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
            ((ComboBox)obj).Items.Add(item);
        }
    }

    public void SetStopBitValues(object obj)
    {
        string[] names = Enum.GetNames(typeof(StopBits));
        foreach (string item in names)
        {
            ((ComboBox)obj).Items.Add(item);
        }
    }

    public void SetPortNameValues(object obj)
    {
        string[] portNames = SerialPort.GetPortNames();
        Array.Sort(portNames);
        string[] array = portNames;
        foreach (string item in array)
        {
            ((ComboBox)obj).Items.Add(item);
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
                    comPort.Read(array, 0, bytesToRead);
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
