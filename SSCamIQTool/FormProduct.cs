using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public class FormProduct : Form
{
    private IContainer components = null;

    private GroupBox grouppRODUCT;

    private Button buttonOK;

    private RadioButton radioButtonCarCam;

    private RadioButton radioButtonIPCam;

    private Label lbIPCamera;

    private Label labelColorR;

    private RadioButton radioButtonUart;

    public FormProduct()
    {
        InitializeComponent();
    }

    public void productstyle(out string name)
    {
        name = "Ethernet Protocol";
        if (radioButtonIPCam.Checked)
        {
            name = "Ethernet Protocol";
        }
        else if (radioButtonCarCam.Checked)
        {
            name = "USB Protocol";
        }
        else if (radioButtonUart.Checked)
        {
            name = "UART Protocol";
        }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
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
        ComponentResourceManager resources = new ComponentResourceManager(typeof(FormProduct));
        grouppRODUCT = new GroupBox();
        buttonOK = new Button();
        radioButtonCarCam = new RadioButton();
        radioButtonIPCam = new RadioButton();
        lbIPCamera = new Label();
        labelColorR = new Label();
        radioButtonUart = new RadioButton();
        grouppRODUCT.SuspendLayout();
        SuspendLayout();
        grouppRODUCT.BackColor = System.Drawing.Color.Lavender;
        grouppRODUCT.Controls.Add(radioButtonUart);
        grouppRODUCT.Controls.Add(buttonOK);
        grouppRODUCT.Controls.Add(radioButtonCarCam);
        grouppRODUCT.Controls.Add(radioButtonIPCam);
        grouppRODUCT.Controls.Add(lbIPCamera);
        grouppRODUCT.Controls.Add(labelColorR);
        grouppRODUCT.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        grouppRODUCT.ForeColor = System.Drawing.Color.FromArgb(64, 0, 64);
        grouppRODUCT.Location = new System.Drawing.Point(19, 24);
        grouppRODUCT.Name = "grouppRODUCT";
        grouppRODUCT.Size = new System.Drawing.Size(246, 215);
        grouppRODUCT.TabIndex = 153;
        grouppRODUCT.TabStop = false;
        grouppRODUCT.Text = "Choose Protocol";
        buttonOK.BackColor = System.Drawing.Color.MintCream;
        buttonOK.Location = new System.Drawing.Point(80, 173);
        buttonOK.Name = "buttonOK";
        buttonOK.Size = new System.Drawing.Size(75, 27);
        buttonOK.TabIndex = 154;
        buttonOK.Text = "OK";
        buttonOK.UseVisualStyleBackColor = false;
        buttonOK.Click += new EventHandler(buttonOK_Click);
        radioButtonCarCam.AutoSize = true;
        radioButtonCarCam.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        radioButtonCarCam.Location = new System.Drawing.Point(66, 84);
        radioButtonCarCam.Name = "radioButtonCarCam";
        radioButtonCarCam.Size = new System.Drawing.Size(57, 23);
        radioButtonCarCam.TabIndex = 153;
        radioButtonCarCam.Text = "USB";
        radioButtonCarCam.UseVisualStyleBackColor = true;
        radioButtonIPCam.AutoSize = true;
        radioButtonIPCam.Checked = true;
        radioButtonIPCam.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        radioButtonIPCam.Location = new System.Drawing.Point(66, 40);
        radioButtonIPCam.Name = "radioButtonIPCam";
        radioButtonIPCam.Size = new System.Drawing.Size(77, 23);
        radioButtonIPCam.TabIndex = 152;
        radioButtonIPCam.TabStop = true;
        radioButtonIPCam.Text = "Ethernet";
        radioButtonIPCam.UseVisualStyleBackColor = true;
        lbIPCamera.AutoSize = true;
        lbIPCamera.Location = new System.Drawing.Point(43, 88);
        lbIPCamera.Name = "lbIPCamera";
        lbIPCamera.Size = new System.Drawing.Size(0, 19);
        lbIPCamera.TabIndex = 151;
        labelColorR.AutoSize = true;
        labelColorR.BackColor = System.Drawing.Color.Red;
        labelColorR.Location = new System.Drawing.Point(171, 24);
        labelColorR.Name = "labelColorR";
        labelColorR.Size = new System.Drawing.Size(0, 19);
        labelColorR.TabIndex = 118;
        radioButtonUart.AutoSize = true;
        radioButtonUart.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        radioButtonUart.Location = new System.Drawing.Point(66, 125);
        radioButtonUart.Name = "radioButtonUart";
        radioButtonUart.Size = new System.Drawing.Size(67, 23);
        radioButtonUart.TabIndex = 155;
        radioButtonUart.Text = "UART";
        radioButtonUart.UseVisualStyleBackColor = true;
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(284, 262);
        Controls.Add(grouppRODUCT);
        Icon = GuiParser.GetIconByName("sigmastar.ico");
        Name = "FormProduct";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Choose Protocol";
        grouppRODUCT.ResumeLayout(false);
        grouppRODUCT.PerformLayout();
        ResumeLayout(false);
    }
}
