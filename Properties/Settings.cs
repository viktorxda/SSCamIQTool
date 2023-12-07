using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SSCamIQTool.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
    private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

    public static Settings Default => defaultInstance;

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("192.168.1.10")]
    public string ClientHostName
    {
        get
        {
            return (string)this["ClientHostName"];
        }
        set
        {
            this["ClientHostName"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("9876")]
    public int ClientPort
    {
        get
        {
            return (int)this["ClientPort"];
        }
        set
        {
            this["ClientPort"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("Connection Fail!!")]
    public string ErrConnectionFail
    {
        get
        {
            return (string)this["ErrConnectionFail"];
        }
        set
        {
            this["ErrConnectionFail"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("20")]
    public int GuiGroupStartX
    {
        get
        {
            return (int)this["GuiGroupStartX"];
        }
        set
        {
            this["GuiGroupStartX"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("20")]
    public int GuiGroupStartY
    {
        get
        {
            return (int)this["GuiGroupStartY"];
        }
        set
        {
            this["GuiGroupStartY"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("20")]
    public int GuiGroupSpaceX
    {
        get
        {
            return (int)this["GuiGroupSpaceX"];
        }
        set
        {
            this["GuiGroupSpaceX"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("20")]
    public int GuiGroupSpaceY
    {
        get
        {
            return (int)this["GuiGroupSpaceY"];
        }
        set
        {
            this["GuiGroupSpaceY"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("80")]
    public int GuiNumScrollWidth
    {
        get
        {
            return (int)this["GuiNumScrollWidth"];
        }
        set
        {
            this["GuiNumScrollWidth"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("120")]
    public int GuiButtonWidth
    {
        get
        {
            return (int)this["GuiButtonWidth"];
        }
        set
        {
            this["GuiButtonWidth"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("200")]
    public int GuiParamNameWidth
    {
        get
        {
            return (int)this["GuiParamNameWidth"];
        }
        set
        {
            this["GuiParamNameWidth"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("2")]
    public int GuiBorderWidth
    {
        get
        {
            return (int)this["GuiBorderWidth"];
        }
        set
        {
            this["GuiBorderWidth"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("20")]
    public int GuiRWButtonSpace
    {
        get
        {
            return (int)this["GuiRWButtonSpace"];
        }
        set
        {
            this["GuiRWButtonSpace"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("Tool is not connected to camera")]
    public string MsgNotConnected
    {
        get
        {
            return (string)this["MsgNotConnected"];
        }
        set
        {
            this["MsgNotConnected"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("6000000")]
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
    [DefaultSettingValue("I2_isp_api.xml")]
    public string ApiModeXmlFile
    {
        get
        {
            return (string)this["ApiModeXmlFile"];
        }
        set
        {
            this["ApiModeXmlFile"] = value;
        }
    }

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
    [DefaultSettingValue("\\Bin\\ffplay.exe")]
    public string LiveViewExe
    {
        get
        {
            return (string)this["LiveViewExe"];
        }
        set
        {
            this["LiveViewExe"] = value;
        }
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("2.01.114")]
    public string ApVersion
    {
        get
        {
            return (string)this["ApVersion"];
        }
        set
        {
            this["ApVersion"] = value;
        }
    }
}
