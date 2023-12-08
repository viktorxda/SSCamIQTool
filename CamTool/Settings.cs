using System.Configuration;

namespace SSCamIQTool.CamTool;

internal sealed class Settings : ApplicationSettingsBase
{
    public static Settings Default { get; } = (Settings)Synchronized(new Settings());

    public string ApVersion = "2.01.115";

    public string ErrConnectionFail = "Connection Failed";

    public string MsgNotConnected = "Tool is not connected to camera";

    [UserScopedSetting, DefaultSettingValue("192.168.1.10")]
    public string ClientHostName
    {
        get => (string)this["ClientHostName"];
        set => this["ClientHostName"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("9876")]
    public int ClientPort
    {
        get => (int)this["ClientPort"];
        set => this["ClientPort"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("20")]
    public int GuiGroupStartX
    {
        get => (int)this["GuiGroupStartX"];
        set => this["GuiGroupStartX"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("20")]
    public int GuiGroupStartY
    {
        get => (int)this["GuiGroupStartY"];
        set => this["GuiGroupStartY"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("20")]
    public int GuiGroupSpaceX
    {
        get => (int)this["GuiGroupSpaceX"];
        set => this["GuiGroupSpaceX"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("20")]
    public int GuiGroupSpaceY
    {
        get => (int)this["GuiGroupSpaceY"];
        set => this["GuiGroupSpaceY"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("80")]
    public int GuiNumScrollWidth
    {
        get => (int)this["GuiNumScrollWidth"];
        set => this["GuiNumScrollWidth"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("120")]
    public int GuiButtonWidth
    {
        get => (int)this["GuiButtonWidth"];
        set => this["GuiButtonWidth"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("200")]
    public int GuiParamNameWidth
    {
        get => (int)this["GuiParamNameWidth"];
        set => this["GuiParamNameWidth"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("2")]
    public int GuiBorderWidth
    {
        get => (int)this["GuiBorderWidth"];
        set => this["GuiBorderWidth"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("20")]
    public int GuiRWButtonSpace
    {
        get => (int)this["GuiRWButtonSpace"];
        set => this["GuiRWButtonSpace"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("6000000")]
    public int ConnectionTimeout
    {
        get => (int)this["ConnectionTimeout"];
        set => this["ConnectionTimeout"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("100000")]
    public int SendApiTimeout
    {
        get => (int)this["SendApiTimeout"];
        set => this["SendApiTimeout"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("I2_isp_api.xml")]
    public string ApiModeXmlFile
    {
        get => (string)this["ApiModeXmlFile"];
        set => this["ApiModeXmlFile"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("/image")]
    public string SaveFolder
    {
        get => (string)this["SaveFolder"];
        set => this["SaveFolder"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("raw")]
    public string SaveRawName
    {
        get => (string)this["SaveRawName"];
        set => this["SaveRawName"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("ispout")]
    public string SaveIspOutputName
    {
        get => (string)this["SaveIspOutputName"];
        set => this["SaveIspOutputName"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("image")]
    public string SaveJpegName
    {
        get => (string)this["SaveJpegName"];
        set => this["SaveJpegName"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("scout")]
    public string SaveScOuputName
    {
        get => (string)this["SaveScOuputName"];
        set => this["SaveScOuputName"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("/bin/ffplay.exe")]
    public string LiveViewExe
    {
        get => (string)this["LiveViewExe"];
        set => this["LiveViewExe"] = value;
    }
}
