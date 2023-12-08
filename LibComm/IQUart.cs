using System.IO.Ports;

namespace SSCamIQTool.LibComm;

public class IQUart
{
    private bool m_bConnection = false;

    public SerialPort Comport;

    public void SetPort(SerialPort comport)
    {
        Comport = comport;
    }

    public void UartConnected()
    {
        m_bConnection = true;
    }

    public void DisConnected()
    {
        m_bConnection = false;
    }

    public bool IsConnected()
    {
        return m_bConnection;
    }
}
