namespace SSCamIQTool.LibComm;

public class ConnectSetting
{
    public string HostName { get; private set; }

    public int Port { get; private set; }

    public int RemoteID { get; set; }

    public ConnectSetting()
    {
        HostName = "";
        Port = 0;
        RemoteID = 0;
    }

    public ConnectSetting(string hostName, int port, int remoteID = 0)
    {
        HostName = hostName;
        Port = port;
        RemoteID = remoteID;
    }

    public bool IsEmpty()
    {
        return HostName.Equals("");
    }

    public void Clear()
    {
        HostName = "";
        Port = 0;
        RemoteID = 0;
    }

    public void CopyFrom(ConnectSetting setting)
    {
        HostName = setting.HostName;
        Port = setting.Port;
        RemoteID = setting.RemoteID;
    }
}
