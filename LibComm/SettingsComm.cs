using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SSCamIQTool.LibComm;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
internal sealed class SettingsComm : ApplicationSettingsBase
{
    private static SettingsComm defaultInstance = (SettingsComm)Synchronized(new SettingsComm());

    public static SettingsComm Default => defaultInstance;

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("\\Image")]
    public string SaveFolder
    {
        get
        {
            return (string)this["SaveFolder"];
        }
        set
        {
            this["SaveFolder"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("raw")]
    public string SaveRawName
    {
        get
        {
            return (string)this["SaveRawName"];
        }
        set
        {
            this["SaveRawName"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("ispout")]
    public string SaveIspOutputName
    {
        get
        {
            return (string)this["SaveIspOutputName"];
        }
        set
        {
            this["SaveIspOutputName"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("image")]
    public string SaveJpegName
    {
        get
        {
            return (string)this["SaveJpegName"];
        }
        set
        {
            this["SaveJpegName"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("scout")]
    public string SaveScOuputName
    {
        get
        {
            return (string)this["SaveScOuputName"];
        }
        set
        {
            this["SaveScOuputName"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("100000")]
    public int SendApiTimeout
    {
        get
        {
            return (int)this["SendApiTimeout"];
        }
        set
        {
            this["SendApiTimeout"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("30000")]
    public int ConnectionTimeout
    {
        get
        {
            return (int)this["ConnectionTimeout"];
        }
        set
        {
            this["ConnectionTimeout"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("raw_stream")]
    public string SaveRawStreamName
    {
        get
        {
            return (string)this["SaveRawStreamName"];
        }
        set
        {
            this["SaveRawStreamName"] = value;
        }
    }
}
