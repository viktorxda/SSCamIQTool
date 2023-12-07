using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public class WDRCurve_v2 : UserControl
{
    private IQComm comm;

    private static GuiGroup gammaGroup;

    private static GuiGroup gammaGroup_temp;

    private static FormGlobalToneVer1 saveGlobalToneVer1 = new FormGlobalToneVer1();

    private static FormGlobalToneGammaVer1 saveGlobalToneGammaVer1 = new FormGlobalToneGammaVer1();

    private static FormGlobalToneDeGammaVer1 saveGlobalToneDeGammaVer1 = new FormGlobalToneDeGammaVer1();

    private static FormGlobalTone saveGlobalTone = new FormGlobalTone();

    private static FormGlobalToneGamma saveGlobalToneGamma = new FormGlobalToneGamma();

    private static FormGlobalToneDeGamma saveGlobalToneDeGamma = new FormGlobalToneDeGamma();

    private static int enableGenerateCurve = 0;

    private IContainer components = null;

    private TabControl WDRCurve_v2_tabControl;

    private TabPage GlobalTone_tabPage;

    private TabPage GlobalToneGamma_tabPage;

    private TabPage GlobalToneDeGamma_tabPage;

    public int OpMode { get; set; }

    public event EventHandler TabChangedEvent0;

    public event EventHandler TabChangedEvent1;

    public event EventHandler TabChangedEvent2;

    public WDRCurve_v2(GuiGroup group, IQComm comm)
    {
        InitializeComponent();
        gammaGroup = group;
        gammaGroup_temp = group;
        if (comm.ChipIDName >= ChipID.I6)
        {
            enableGenerateCurve = 1;
        }
        List<string> nameList = new List<string> { "API_WDRCurveFull_OP_TYPE_GlobalTone", "API_WDRCurveFull_AUTO_GlobalToneLut", "API_WDRCurveFull_MANUAL_GlobalToneLut" };
        List<string> nameList2 = new List<string> { "API_WDRCurveFull_OP_TYPE_Gamma", "API_WDRCurveFull_AUTO_GammaLut", "API_WDRCurveFull_MANUAL_GammaLut" };
        List<string> nameList3 = new List<string> { "API_WDRCurveFull_OP_TYPE_DeGamma", "API_WDRCurveFull_AUTO_DeGammaLut", "API_WDRCurveFull_MANUAL_DeGammaLut" };
        if (enableGenerateCurve == 1)
        {
            if (CheckItemTag(group, nameList))
            {
                FormGlobalToneVer1 value = new FormGlobalToneVer1(gammaGroup_temp)
                {
                    FormBorderStyle = FormBorderStyle.None,
                    TopLevel = false,
                    Visible = true,
                    Top = 0,
                    Left = 0
                };
                GlobalTone_tabPage.Controls.Add(value);
                GlobalTone_tabPage.Show();
                saveGlobalToneVer1 = value;
                TabChangedEvent0 += saveGlobalToneVer1.SetWinFormFocus;
            }
            else
            {
                GlobalTone_tabPage.Parent = null;
            }
            if (CheckItemTag(group, nameList2))
            {
                FormGlobalToneGammaVer1 value2 = new FormGlobalToneGammaVer1(gammaGroup_temp)
                {
                    FormBorderStyle = FormBorderStyle.None,
                    TopLevel = false,
                    Visible = true,
                    Top = 0,
                    Left = 0
                };
                GlobalToneGamma_tabPage.Controls.Add(value2);
                GlobalToneGamma_tabPage.Show();
                saveGlobalToneGammaVer1 = value2;
                TabChangedEvent1 += saveGlobalToneGammaVer1.SetWinFormFocus;
            }
            else
            {
                GlobalToneGamma_tabPage.Parent = null;
            }
            if (CheckItemTag(group, nameList3))
            {
                FormGlobalToneDeGammaVer1 value3 = new FormGlobalToneDeGammaVer1(gammaGroup_temp)
                {
                    FormBorderStyle = FormBorderStyle.None,
                    TopLevel = false,
                    Visible = true,
                    Top = 0,
                    Left = 0
                };
                GlobalToneDeGamma_tabPage.Controls.Add(value3);
                GlobalToneDeGamma_tabPage.Show();
                saveGlobalToneDeGammaVer1 = value3;
                TabChangedEvent2 += saveGlobalToneDeGammaVer1.SetWinFormFocus;
            }
            else
            {
                GlobalToneDeGamma_tabPage.Parent = null;
            }
            this.comm = comm;
        }
        else
        {
            if (CheckItemTag(group, nameList))
            {
                FormGlobalTone value4 = new FormGlobalTone(gammaGroup_temp)
                {
                    FormBorderStyle = FormBorderStyle.None,
                    TopLevel = false,
                    Visible = true,
                    Top = 0,
                    Left = 0
                };
                GlobalTone_tabPage.Controls.Add(value4);
                GlobalTone_tabPage.Show();
                saveGlobalTone = value4;
                TabChangedEvent0 += saveGlobalTone.SetWinFormFocus;
            }
            else
            {
                GlobalTone_tabPage.Parent = null;
            }
            if (CheckItemTag(group, nameList2))
            {
                FormGlobalToneGamma value5 = new FormGlobalToneGamma(gammaGroup_temp)
                {
                    FormBorderStyle = FormBorderStyle.None,
                    TopLevel = false,
                    Visible = true,
                    Top = 0,
                    Left = 0
                };
                GlobalToneGamma_tabPage.Controls.Add(value5);
                GlobalToneGamma_tabPage.Show();
                saveGlobalToneGamma = value5;
                TabChangedEvent1 += saveGlobalToneGamma.SetWinFormFocus;
            }
            else
            {
                GlobalToneGamma_tabPage.Parent = null;
            }
            if (CheckItemTag(group, nameList3))
            {
                FormGlobalToneDeGamma value6 = new FormGlobalToneDeGamma(gammaGroup_temp)
                {
                    FormBorderStyle = FormBorderStyle.None,
                    TopLevel = false,
                    Visible = true,
                    Top = 0,
                    Left = 0
                };
                GlobalToneDeGamma_tabPage.Controls.Add(value6);
                GlobalToneDeGamma_tabPage.Show();
                saveGlobalToneDeGamma = value6;
                TabChangedEvent2 += saveGlobalToneDeGamma.SetWinFormFocus;
            }
            else
            {
                GlobalToneDeGamma_tabPage.Parent = null;
            }
            this.comm = comm;
        }
        WDRCurve_v2_tabControl_SelectedIndexChanged(this, new EventArgs());
    }

    public void SaveGammaValue()
    {
        if (enableGenerateCurve == 1)
        {
            if (TabChangedEvent0 != null)
            {
                saveGlobalToneVer1.SaveGlobalToneValue();
            }
            if (TabChangedEvent1 != null)
            {
                saveGlobalToneGammaVer1.SaveGlobalToneGamma();
            }
            if (TabChangedEvent2 != null)
            {
                saveGlobalToneDeGammaVer1.SaveGlobalToneDeGamma();
            }
        }
        else
        {
            if (TabChangedEvent0 != null)
            {
                saveGlobalTone.SaveGlobalToneValue();
            }
            if (TabChangedEvent1 != null)
            {
                saveGlobalToneGamma.SaveGlobalToneGamma();
            }
            if (TabChangedEvent2 != null)
            {
                saveGlobalToneDeGamma.SaveGlobalToneDeGamma();
            }
        }
    }

    public void UpdatePage()
    {
        if (enableGenerateCurve == 1)
        {
            if (TabChangedEvent0 != null)
            {
                saveGlobalToneVer1.ReadPage();
            }
            if (TabChangedEvent1 != null)
            {
                saveGlobalToneGammaVer1.ReadPage();
            }
            if (TabChangedEvent2 != null)
            {
                saveGlobalToneDeGammaVer1.ReadPage();
            }
        }
        else
        {
            if (TabChangedEvent0 != null)
            {
                saveGlobalTone.ReadPage();
            }
            if (TabChangedEvent1 != null)
            {
                saveGlobalToneGamma.ReadPage();
            }
            if (TabChangedEvent2 != null)
            {
                saveGlobalToneDeGamma.ReadPage();
            }
        }
    }

    private void WDRCurve_v2_tabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        int selectedIndex = WDRCurve_v2_tabControl.SelectedIndex;
        int num = 0;
        if (TabChangedEvent0 != null)
        {
            if (num == selectedIndex)
            {
                TabChangedEvent0(this, new EventArgs());
                return;
            }
            num++;
        }
        if (TabChangedEvent1 != null)
        {
            if (num == selectedIndex)
            {
                TabChangedEvent1(this, new EventArgs());
                return;
            }
            num++;
        }
        if (TabChangedEvent2 != null)
        {
            if (num == selectedIndex)
            {
                TabChangedEvent2(this, new EventArgs());
            }
            else
            {
                num++;
            }
        }
    }

    private bool CheckItemTag(GuiGroup group, List<string> nameList)
    {
        for (int i = 0; i < nameList.Count; i++)
        {
            if (GetItemFieldIdx(group, nameList[i]) == -1)
            {
                return false;
            }
        }
        return true;
    }

    private int GetItemFieldIdx(GuiGroup group, string name)
    {
        int result = -1;
        for (int i = 0; i < group.ItemList.Count; i++)
        {
            if (group.ItemList[i].Tag.Equals(name))
            {
                return i;
            }
        }
        return result;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        WDRCurve_v2_tabControl = new TabControl();
        GlobalTone_tabPage = new TabPage();
        GlobalToneGamma_tabPage = new TabPage();
        GlobalToneDeGamma_tabPage = new TabPage();
        WDRCurve_v2_tabControl.SuspendLayout();
        SuspendLayout();
        WDRCurve_v2_tabControl.Controls.Add(GlobalTone_tabPage);
        WDRCurve_v2_tabControl.Controls.Add(GlobalToneGamma_tabPage);
        WDRCurve_v2_tabControl.Controls.Add(GlobalToneDeGamma_tabPage);
        WDRCurve_v2_tabControl.Location = new System.Drawing.Point(0, 0);
        WDRCurve_v2_tabControl.Name = "WDRCurve_v2_tabControl";
        WDRCurve_v2_tabControl.SelectedIndex = 0;
        WDRCurve_v2_tabControl.Size = new System.Drawing.Size(957, 644);
        WDRCurve_v2_tabControl.TabIndex = 0;
        WDRCurve_v2_tabControl.SelectedIndexChanged += new EventHandler(WDRCurve_v2_tabControl_SelectedIndexChanged);
        GlobalTone_tabPage.Location = new System.Drawing.Point(4, 22);
        GlobalTone_tabPage.Name = "GlobalTone_tabPage";
        GlobalTone_tabPage.Padding = new Padding(3);
        GlobalTone_tabPage.Size = new System.Drawing.Size(949, 618);
        GlobalTone_tabPage.TabIndex = 0;
        GlobalTone_tabPage.Text = "GlobalTone";
        GlobalTone_tabPage.UseVisualStyleBackColor = true;
        GlobalToneGamma_tabPage.Location = new System.Drawing.Point(4, 22);
        GlobalToneGamma_tabPage.Name = "GlobalToneGamma_tabPage";
        GlobalToneGamma_tabPage.Padding = new Padding(3);
        GlobalToneGamma_tabPage.Size = new System.Drawing.Size(949, 618);
        GlobalToneGamma_tabPage.TabIndex = 1;
        GlobalToneGamma_tabPage.Text = "Curve1";
        GlobalToneGamma_tabPage.UseVisualStyleBackColor = true;
        GlobalToneDeGamma_tabPage.Location = new System.Drawing.Point(4, 22);
        GlobalToneDeGamma_tabPage.Name = "GlobalToneDeGamma_tabPage";
        GlobalToneDeGamma_tabPage.Size = new System.Drawing.Size(949, 618);
        GlobalToneDeGamma_tabPage.TabIndex = 2;
        GlobalToneDeGamma_tabPage.Text = "Curve2";
        GlobalToneDeGamma_tabPage.UseVisualStyleBackColor = true;
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(WDRCurve_v2_tabControl);
        Name = "WDRCurve_v2";
        Size = new System.Drawing.Size(957, 641);
        WDRCurve_v2_tabControl.ResumeLayout(false);
        ResumeLayout(false);
    }
}
