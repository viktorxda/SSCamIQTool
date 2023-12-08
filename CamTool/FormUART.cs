using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class FormUART : Form
{
    private readonly CommunicationManager comm = new();

    private readonly string transType = string.Empty;

    private readonly IContainer components = null;

    private GroupBox groupBox2;

    private Label label5;

    private ComboBox cboData;

    private Label label4;

    private ComboBox cboStop;

    private Label label3;

    private Label label2;

    private ComboBox cboParity;

    private Label Label1;

    private ComboBox cboBaud;

    private ComboBox cboPort;

    private Button cmdClose;

    private Button cmdOpen;

    private Button cmdSend;

    private TextBox txtSend;

    private RichTextBox rtbDisplay;

    public FormUART()
    {
        InitializeComponent();
    }

    public void FormUART_Load(object sender, EventArgs e)
    {
        LoadValues();
        SetDefaults();
        SetControlState();
    }

    private void cmdOpen_Click(object sender, EventArgs e)
    {
        comm.PortName = cboPort.Text;
        comm.Parity = cboParity.Text;
        comm.StopBits = cboStop.Text;
        comm.DataBits = cboData.Text;
        comm.BaudRate = cboBaud.Text;
        comm.DisplayWindow = rtbDisplay;
        _ = comm.OpenPort();
        if (comm.isPortOpen)
        {
            cmdOpen.Enabled = false;
            cmdClose.Enabled = true;
            cmdSend.Enabled = true;
            txtSend.Enabled = true;
        }
    }

    private void SetDefaults()
    {
        cboPort.SelectedIndex = -1;
        cboBaud.SelectedText = "9600";
        cboParity.SelectedIndex = 0;
        cboStop.SelectedIndex = 1;
        cboData.SelectedIndex = 1;
    }

    private void LoadValues()
    {
        comm.SetPortNameValues(cboPort);
        comm.SetParityValues(cboParity);
        comm.SetStopBitValues(cboStop);
    }

    private void SetControlState()
    {
        cmdSend.Enabled = false;
        cmdClose.Enabled = false;
    }

    private void sendData()
    {
        comm.WriteData(txtSend.Text);
        txtSend.SelectAll();
    }

    private void cmdSend_Click(object sender, EventArgs e)
    {
        sendData();
    }

    private void cmdClose_Click(object sender, EventArgs e)
    {
        comm.ClosePort();
        if (!comm.isPortOpen)
        {
            cmdOpen.Enabled = true;
            cmdClose.Enabled = false;
            cmdSend.Enabled = false;
            txtSend.Enabled = false;
        }
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
        groupBox2 = new GroupBox();
        label5 = new Label();
        cboData = new ComboBox();
        label4 = new Label();
        cboStop = new ComboBox();
        label3 = new Label();
        label2 = new Label();
        cboParity = new ComboBox();
        Label1 = new Label();
        cboBaud = new ComboBox();
        cboPort = new ComboBox();
        cmdClose = new Button();
        cmdOpen = new Button();
        cmdSend = new Button();
        txtSend = new TextBox();
        rtbDisplay = new RichTextBox();
        groupBox2.SuspendLayout();
        SuspendLayout();
        groupBox2.Controls.Add(label5);
        groupBox2.Controls.Add(cboData);
        groupBox2.Controls.Add(label4);
        groupBox2.Controls.Add(cboStop);
        groupBox2.Controls.Add(label3);
        groupBox2.Controls.Add(label2);
        groupBox2.Controls.Add(cboParity);
        groupBox2.Controls.Add(Label1);
        groupBox2.Controls.Add(cboBaud);
        groupBox2.Controls.Add(cboPort);
        groupBox2.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        groupBox2.Location = new System.Drawing.Point(635, 33);
        groupBox2.Name = "groupBox2";
        groupBox2.Size = new System.Drawing.Size(100, 290);
        groupBox2.TabIndex = 17;
        groupBox2.TabStop = false;
        groupBox2.Text = "Options";
        label5.AutoSize = true;
        label5.Location = new System.Drawing.Point(6, 225);
        label5.Name = "label5";
        label5.Size = new System.Drawing.Size(73, 19);
        label5.TabIndex = 19;
        label5.Text = "Data Bits";
        cboData.FormattingEnabled = true;
        cboData.Items.AddRange(new object[3] { "7", "8", "9" });
        cboData.Location = new System.Drawing.Point(9, 247);
        cboData.Name = "cboData";
        cboData.Size = new System.Drawing.Size(76, 27);
        cboData.TabIndex = 14;
        label4.AutoSize = true;
        label4.Location = new System.Drawing.Point(5, 173);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(70, 19);
        label4.TabIndex = 18;
        label4.Text = "Stop Bits";
        cboStop.FormattingEnabled = true;
        cboStop.Location = new System.Drawing.Point(9, 195);
        cboStop.Name = "cboStop";
        cboStop.Size = new System.Drawing.Size(76, 27);
        cboStop.TabIndex = 13;
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(6, 121);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(49, 19);
        label3.TabIndex = 17;
        label3.Text = "Parity";
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(5, 69);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(81, 19);
        label2.TabIndex = 16;
        label2.Text = "Baud Rate";
        cboParity.FormattingEnabled = true;
        cboParity.Location = new System.Drawing.Point(9, 143);
        cboParity.Name = "cboParity";
        cboParity.Size = new System.Drawing.Size(76, 27);
        cboParity.TabIndex = 12;
        Label1.AutoSize = true;
        Label1.Location = new System.Drawing.Point(6, 17);
        Label1.Name = "Label1";
        Label1.Size = new System.Drawing.Size(37, 19);
        Label1.TabIndex = 15;
        Label1.Text = "Port";
        cboBaud.FormattingEnabled = true;
        cboBaud.Items.AddRange(new object[10] { "300", "600", "1200", "2400", "4800", "9600", "14400", "28800", "36000", "115000" });
        cboBaud.Location = new System.Drawing.Point(9, 91);
        cboBaud.Name = "cboBaud";
        cboBaud.Size = new System.Drawing.Size(76, 27);
        cboBaud.TabIndex = 11;
        cboPort.FormattingEnabled = true;
        cboPort.Location = new System.Drawing.Point(9, 39);
        cboPort.Name = "cboPort";
        cboPort.Size = new System.Drawing.Size(76, 27);
        cboPort.TabIndex = 10;
        cmdClose.BackColor = System.Drawing.Color.Lavender;
        cmdClose.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        cmdClose.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
        cmdClose.Location = new System.Drawing.Point(628, 397);
        cmdClose.Name = "cmdClose";
        cmdClose.Size = new System.Drawing.Size(107, 30);
        cmdClose.TabIndex = 16;
        cmdClose.Text = "Close Port";
        cmdClose.UseVisualStyleBackColor = false;
        cmdClose.Click += new EventHandler(cmdClose_Click);
        cmdOpen.BackColor = System.Drawing.Color.LightYellow;
        cmdOpen.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        cmdOpen.Location = new System.Drawing.Point(628, 345);
        cmdOpen.Name = "cmdOpen";
        cmdOpen.Size = new System.Drawing.Size(107, 30);
        cmdOpen.TabIndex = 15;
        cmdOpen.Text = "Open Port";
        cmdOpen.UseVisualStyleBackColor = false;
        cmdOpen.Click += new EventHandler(cmdOpen_Click);
        cmdSend.BackColor = System.Drawing.Color.LightCyan;
        cmdSend.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        cmdSend.Location = new System.Drawing.Point(547, 442);
        cmdSend.Name = "cmdSend";
        cmdSend.Size = new System.Drawing.Size(75, 30);
        cmdSend.TabIndex = 14;
        cmdSend.Text = "Send";
        cmdSend.UseVisualStyleBackColor = false;
        cmdSend.Click += new EventHandler(cmdSend_Click);
        txtSend.Enabled = false;
        txtSend.Location = new System.Drawing.Point(17, 442);
        txtSend.Multiline = true;
        txtSend.Name = "txtSend";
        txtSend.Size = new System.Drawing.Size(524, 30);
        txtSend.TabIndex = 13;
        rtbDisplay.BackColor = System.Drawing.Color.GhostWhite;
        rtbDisplay.Location = new System.Drawing.Point(17, 33);
        rtbDisplay.Name = "rtbDisplay";
        rtbDisplay.Size = new System.Drawing.Size(605, 402);
        rtbDisplay.TabIndex = 12;
        rtbDisplay.Text = "";
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = System.Drawing.Color.MintCream;
        ClientSize = new System.Drawing.Size(753, 504);
        Controls.Add(groupBox2);
        Controls.Add(cmdClose);
        Controls.Add(cmdOpen);
        Controls.Add(cmdSend);
        Controls.Add(txtSend);
        Controls.Add(rtbDisplay);
        Name = "FormUART";
        StartPosition = FormStartPosition.CenterParent;
        Text = "UART log";
        Load += new EventHandler(FormUART_Load);
        groupBox2.ResumeLayout(false);
        groupBox2.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
