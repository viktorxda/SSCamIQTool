using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public class FormAutoSaveBinFile : Form
{
    public delegate void AutoSaveBinHandler(uint Times);

    private AutoSaveBinHandler autoSaveBin;

    private IContainer components = null;

    private Label SaveBinFile_label;

    private TextBox SaveBinFile_textbox;

    private Button SaveBinFile_button;

    public FormAutoSaveBinFile(uint Times, AutoSaveBinHandler setTimes)
    {
        InitializeComponent();
        autoSaveBin = setTimes;
        SaveBinFile_textbox.Text = Times.ToString();
    }

    private void AutoSaveBinFile_Click(object sender, EventArgs e)
    {
        uint num = 0u;
        try
        {
            num = uint.Parse(SaveBinFile_textbox.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Invalid Number", MessageBoxButtons.OK);
            return;
        }
        autoSaveBin(num);
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
        SaveBinFile_label = new Label();
        SaveBinFile_textbox = new TextBox();
        SaveBinFile_button = new Button();
        SuspendLayout();
        SaveBinFile_label.AutoSize = true;
        SaveBinFile_label.Location = new System.Drawing.Point(36, 65);
        SaveBinFile_label.Name = "SaveBinFile_label";
        SaveBinFile_label.Size = new System.Drawing.Size(59, 12);
        SaveBinFile_label.TabIndex = 0;
        SaveBinFile_label.Text = "Times(min)";
        SaveBinFile_textbox.Location = new System.Drawing.Point(119, 62);
        SaveBinFile_textbox.Name = "SaveBinFile_textbox";
        SaveBinFile_textbox.Size = new System.Drawing.Size(107, 22);
        SaveBinFile_textbox.TabIndex = 1;
        SaveBinFile_textbox.Text = "0";
        SaveBinFile_button.Location = new System.Drawing.Point(241, 60);
        SaveBinFile_button.Name = "SaveBinFile_button";
        SaveBinFile_button.Size = new System.Drawing.Size(75, 23);
        SaveBinFile_button.TabIndex = 2;
        SaveBinFile_button.Text = "Set";
        SaveBinFile_button.UseVisualStyleBackColor = true;
        SaveBinFile_button.Click += new EventHandler(AutoSaveBinFile_Click);
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(355, 170);
        Controls.Add(SaveBinFile_button);
        Controls.Add(SaveBinFile_textbox);
        Controls.Add(SaveBinFile_label);
        Name = "FormAutoSaveBinFile";
        Text = "FormAutoSaveBinFile";
        ResumeLayout(false);
        PerformLayout();
    }
}
