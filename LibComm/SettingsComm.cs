using System.Configuration;

namespace SSCamIQTool.LibComm;

internal sealed class SettingsComm : ApplicationSettingsBase
{
    public static SettingsComm Default { get; } = (SettingsComm)Synchronized(new SettingsComm());

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

    [UserScopedSetting, DefaultSettingValue("raw_stream")]
    public string SaveRawStreamName
    {
        get => (string)this["SaveRawStreamName"];
        set => this["SaveRawStreamName"] = value;
    }

    [UserScopedSetting, DefaultSettingValue("30000")]
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
}
