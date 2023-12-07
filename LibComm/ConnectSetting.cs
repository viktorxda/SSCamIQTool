namespace SSCamIQTool.LibComm;

public class ConnectSetting
{
    private string hostName;

    private int port;

    private int remoteID;

    public string HostName => hostName;

    public int Port => port;

    public int RemoteID
    {
        get
        {
            return remoteID;
        }
        set
        {
            remoteID = value;
        }
    }

    public ConnectSetting()
    {
        hostName = "";
        port = 0;
        remoteID = 0;
    }

    public ConnectSetting(string hostName, int port, int remoteID = 0)
    {
        this.hostName = hostName;
        this.port = port;
        this.remoteID = remoteID;
    }

    public bool IsEmpty()
    {
        if (!hostName.Equals(""))
        {
            return false;
        }
        return true;
    }

    public void Clear()
    {
        hostName = "";
        port = 0;
        remoteID = 0;
    }

    public void CopyFrom(ConnectSetting setting)
    {
        hostName = setting.hostName;
        port = setting.port;
        remoteID = setting.remoteID;
    }
}
