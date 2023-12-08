using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class FormAbout : Form
{
    private readonly IContainer components = null;

    private PictureBox picBoxLogo;

    private Label labelToolVersion;

    private Label labelAboutCompany;

    private Button btnOK;

    public FormAbout()
    {
        InitializeComponent();
        Label label = labelToolVersion;
        label.Text = label.Text + " v" + Settings.Default.ApVersion;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
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
        labelToolVersion = new Label();
        labelAboutCompany = new Label();
        btnOK = new Button();
        picBoxLogo = new PictureBox();
        ((ISupportInitialize)picBoxLogo).BeginInit();
        SuspendLayout();
        // 
        // labelToolVersion
        // 
        labelToolVersion.AutoSize = true;
        labelToolVersion.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        labelToolVersion.Location = new System.Drawing.Point(26, 91);
        labelToolVersion.Name = "labelToolVersion";
        labelToolVersion.Size = new System.Drawing.Size(151, 19);
        labelToolVersion.TabIndex = 1;
        labelToolVersion.Text = "SStar IPCam IQ Tool";
        // 
        // labelAboutCompany
        // 
        labelAboutCompany.AutoSize = true;
        labelAboutCompany.Font = new System.Drawing.Font("Times New Roman", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        labelAboutCompany.Location = new System.Drawing.Point(27, 147);
        labelAboutCompany.Name = "labelAboutCompany";
        labelAboutCompany.Size = new System.Drawing.Size(232, 17);
        labelAboutCompany.TabIndex = 2;
        labelAboutCompany.Text = "2018 (C) SigmaStar Technology Corp.";
        // 
        // btnOK
        // 
        btnOK.Location = new System.Drawing.Point(102, 191);
        btnOK.Name = "btnOK";
        btnOK.Size = new System.Drawing.Size(75, 23);
        btnOK.TabIndex = 4;
        btnOK.Text = "OK";
        btnOK.UseVisualStyleBackColor = true;
        btnOK.Click += new EventHandler(btnOK_Click);
        // 
        // picBoxLogo
        // 
        picBoxLogo.Image = ExtensionMethods.GetImageByName("sigmastar.png");
        picBoxLogo.Location = new System.Drawing.Point(30, 13);
        picBoxLogo.Margin = new Padding(3, 4, 3, 4);
        picBoxLogo.Name = "picBoxLogo";
        picBoxLogo.Size = new System.Drawing.Size(200, 60);
        picBoxLogo.SizeMode = PictureBoxSizeMode.CenterImage;
        picBoxLogo.TabIndex = 0;
        picBoxLogo.TabStop = false;
        // 
        // FormAbout
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(284, 226);
        Controls.Add(btnOK);
        Controls.Add(labelAboutCompany);
        Controls.Add(labelToolVersion);
        Controls.Add(picBoxLogo);
        Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Icon = ExtensionMethods.GetIconByName("sigmastar.ico");
        Margin = new Padding(3, 4, 3, 4);
        Name = "FormAbout";
        StartPosition = FormStartPosition.CenterParent;
        Text = "About";
        ((ISupportInitialize)picBoxLogo).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
