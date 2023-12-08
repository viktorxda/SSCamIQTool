using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class FormSclSetting : Form
{
    public delegate void SetSclSettingHandler(int device, int channel, int port);

    private readonly SetSclSettingHandler SetSclSetting;

    private readonly IContainer components = null;

    private Label SclDeviceLabel;

    private Label SclChannelLabel;

    private Label SclPortLabel;

    private NumericUpDown SclDeviceNumericUpDown;

    private NumericUpDown SclChannelNumericUpDown;

    private NumericUpDown SclPortNumericUpDown;

    private Button SclSetSettingButton;

    public FormSclSetting(int device, int channel, int port, SetSclSettingHandler setSclConfig)
    {
        InitializeComponent();
        SetSclSetting = setSclConfig;
        SclDeviceNumericUpDown.Value = device;
        SclChannelNumericUpDown.Value = channel;
        SclPortNumericUpDown.Value = port;
    }

    private void SclSetSettingButton_Click(object sender, EventArgs e)
    {
        int device = int.Parse(SclDeviceNumericUpDown.Value.ToString());
        int channel = int.Parse(SclChannelNumericUpDown.Value.ToString());
        int port = int.Parse(SclPortNumericUpDown.Value.ToString());
        SetSclSetting(device, channel, port);
        Close();
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
        SclDeviceLabel = new Label();
        SclChannelLabel = new Label();
        SclPortLabel = new Label();
        SclDeviceNumericUpDown = new NumericUpDown();
        SclChannelNumericUpDown = new NumericUpDown();
        SclPortNumericUpDown = new NumericUpDown();
        SclSetSettingButton = new Button();
        ((ISupportInitialize)SclDeviceNumericUpDown).BeginInit();
        ((ISupportInitialize)SclChannelNumericUpDown).BeginInit();
        ((ISupportInitialize)SclPortNumericUpDown).BeginInit();
        SuspendLayout();
        SclDeviceLabel.AutoSize = true;
        SclDeviceLabel.Location = new System.Drawing.Point(10, 21);
        SclDeviceLabel.Name = "SclDeviceLabel";
        SclDeviceLabel.Size = new System.Drawing.Size(53, 12);
        SclDeviceLabel.TabIndex = 0;
        SclDeviceLabel.Text = "Device：";
        SclChannelLabel.AutoSize = true;
        SclChannelLabel.Location = new System.Drawing.Point(10, 56);
        SclChannelLabel.Name = "SclChannelLabel";
        SclChannelLabel.Size = new System.Drawing.Size(59, 12);
        SclChannelLabel.TabIndex = 1;
        SclChannelLabel.Text = "Channel：";
        SclPortLabel.AutoSize = true;
        SclPortLabel.Location = new System.Drawing.Point(12, 93);
        SclPortLabel.Name = "SclPortLabel";
        SclPortLabel.Size = new System.Drawing.Size(41, 12);
        SclPortLabel.TabIndex = 2;
        SclPortLabel.Text = "Port：";
        SclDeviceNumericUpDown.Location = new System.Drawing.Point(79, 19);
        SclDeviceNumericUpDown.Name = "SclDeviceNumericUpDown";
        SclDeviceNumericUpDown.Size = new System.Drawing.Size(58, 21);
        SclDeviceNumericUpDown.TabIndex = 3;
        SclChannelNumericUpDown.Location = new System.Drawing.Point(79, 54);
        SclChannelNumericUpDown.Name = "SclChannelNumericUpDown";
        SclChannelNumericUpDown.Size = new System.Drawing.Size(58, 21);
        SclChannelNumericUpDown.TabIndex = 4;
        SclPortNumericUpDown.Location = new System.Drawing.Point(79, 89);
        SclPortNumericUpDown.Name = "SclPortNumericUpDown";
        SclPortNumericUpDown.Size = new System.Drawing.Size(58, 21);
        SclPortNumericUpDown.TabIndex = 5;
        SclSetSettingButton.Location = new System.Drawing.Point(184, 54);
        SclSetSettingButton.Name = "SclSetSettingButton";
        SclSetSettingButton.Size = new System.Drawing.Size(63, 23);
        SclSetSettingButton.TabIndex = 6;
        SclSetSettingButton.Text = "Set";
        SclSetSettingButton.UseVisualStyleBackColor = true;
        SclSetSettingButton.Click += new EventHandler(SclSetSettingButton_Click);
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(271, 127);
        Controls.Add(SclSetSettingButton);
        Controls.Add(SclPortNumericUpDown);
        Controls.Add(SclChannelNumericUpDown);
        Controls.Add(SclDeviceNumericUpDown);
        Controls.Add(SclPortLabel);
        Controls.Add(SclChannelLabel);
        Controls.Add(SclDeviceLabel);
        Name = "FormSclSetting";
        Text = "FormSclSetting";
        ((ISupportInitialize)SclDeviceNumericUpDown).EndInit();
        ((ISupportInitialize)SclChannelNumericUpDown).EndInit();
        ((ISupportInitialize)SclPortNumericUpDown).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
