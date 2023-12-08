using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class FormConvertList : Form
{
    private readonly IContainer components = null;

    private Button btnOK;

    private GroupBox groupBox1;

    public ComboBox comboBoxCvtXml;

    public RadioButton radioBtnSelXmlOnBoard;

    public RadioButton radioBtnSelCvtXml;

    private GroupBox groupBox2;

    public RadioButton radioBtnCvtToBin;

    public RadioButton radioBtnCvtToXml;

    private Button btnCancel;

    public FormConvertList()
    {
        InitializeComponent();
    }

    private void FormConvertList_Load(object sender, EventArgs e)
    {
        string path = Application.StartupPath + "/CvtXml";
        try
        {
            comboBoxCvtXml.Items.Clear();
            string[] files = Directory.GetFiles(path, "*.xml");
            for (int i = 0; i < files.Length; i++)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
                _ = comboBoxCvtXml.Items.Add(fileNameWithoutExtension);
            }
            comboBoxCvtXml.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
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
        btnOK = new Button();
        groupBox1 = new GroupBox();
        comboBoxCvtXml = new ComboBox();
        radioBtnSelXmlOnBoard = new RadioButton();
        radioBtnSelCvtXml = new RadioButton();
        groupBox2 = new GroupBox();
        radioBtnCvtToBin = new RadioButton();
        radioBtnCvtToXml = new RadioButton();
        btnCancel = new Button();
        groupBox1.SuspendLayout();
        groupBox2.SuspendLayout();
        SuspendLayout();
        btnOK.Location = new System.Drawing.Point(116, 170);
        btnOK.Name = "btnOK";
        btnOK.Size = new System.Drawing.Size(75, 23);
        btnOK.TabIndex = 3;
        btnOK.Text = "OK";
        btnOK.UseVisualStyleBackColor = true;
        btnOK.Click += new EventHandler(btnOK_Click);
        groupBox1.Controls.Add(comboBoxCvtXml);
        groupBox1.Controls.Add(radioBtnSelXmlOnBoard);
        groupBox1.Controls.Add(radioBtnSelCvtXml);
        groupBox1.Location = new System.Drawing.Point(12, 12);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new System.Drawing.Size(260, 100);
        groupBox1.TabIndex = 4;
        groupBox1.TabStop = false;
        groupBox1.Text = "Convert to Destination";
        comboBoxCvtXml.FormattingEnabled = true;
        comboBoxCvtXml.Location = new System.Drawing.Point(13, 43);
        comboBoxCvtXml.Name = "comboBoxCvtXml";
        comboBoxCvtXml.Size = new System.Drawing.Size(121, 20);
        comboBoxCvtXml.TabIndex = 5;
        radioBtnSelXmlOnBoard.AutoSize = true;
        radioBtnSelXmlOnBoard.Location = new System.Drawing.Point(13, 69);
        radioBtnSelXmlOnBoard.Name = "radioBtnSelXmlOnBoard";
        radioBtnSelXmlOnBoard.Size = new System.Drawing.Size(92, 16);
        radioBtnSelXmlOnBoard.TabIndex = 4;
        radioBtnSelXmlOnBoard.Text = "Xml On Board";
        radioBtnSelXmlOnBoard.UseVisualStyleBackColor = true;
        radioBtnSelCvtXml.AutoSize = true;
        radioBtnSelCvtXml.Checked = true;
        radioBtnSelCvtXml.Location = new System.Drawing.Point(13, 21);
        radioBtnSelCvtXml.Name = "radioBtnSelCvtXml";
        radioBtnSelCvtXml.Size = new System.Drawing.Size(84, 16);
        radioBtnSelCvtXml.TabIndex = 3;
        radioBtnSelCvtXml.TabStop = true;
        radioBtnSelCvtXml.Text = "Convert Xml";
        radioBtnSelCvtXml.UseVisualStyleBackColor = true;
        groupBox2.Controls.Add(radioBtnCvtToBin);
        groupBox2.Controls.Add(radioBtnCvtToXml);
        groupBox2.Location = new System.Drawing.Point(12, 121);
        groupBox2.Name = "groupBox2";
        groupBox2.Size = new System.Drawing.Size(260, 43);
        groupBox2.TabIndex = 5;
        groupBox2.TabStop = false;
        groupBox2.Text = "Convert to File Type ( Xml / Bin)";
        radioBtnCvtToBin.AutoSize = true;
        radioBtnCvtToBin.Location = new System.Drawing.Point(134, 21);
        radioBtnCvtToBin.Name = "radioBtnCvtToBin";
        radioBtnCvtToBin.Size = new System.Drawing.Size(56, 16);
        radioBtnCvtToBin.TabIndex = 1;
        radioBtnCvtToBin.Text = "To Bin";
        radioBtnCvtToBin.UseVisualStyleBackColor = true;
        radioBtnCvtToXml.AutoSize = true;
        radioBtnCvtToXml.Checked = true;
        radioBtnCvtToXml.Location = new System.Drawing.Point(13, 21);
        radioBtnCvtToXml.Name = "radioBtnCvtToXml";
        radioBtnCvtToXml.Size = new System.Drawing.Size(59, 16);
        radioBtnCvtToXml.TabIndex = 0;
        radioBtnCvtToXml.TabStop = true;
        radioBtnCvtToXml.Text = "To Xml";
        radioBtnCvtToXml.UseVisualStyleBackColor = true;
        btnCancel.Location = new System.Drawing.Point(197, 170);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new System.Drawing.Size(75, 23);
        btnCancel.TabIndex = 6;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += new EventHandler(btnCancel_Click);
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(284, 200);
        Controls.Add(btnCancel);
        Controls.Add(groupBox2);
        Controls.Add(groupBox1);
        Controls.Add(btnOK);
        Name = "FormConvertList";
        Text = "FormConvertList";
        Load += new EventHandler(FormConvertList_Load);
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        groupBox2.ResumeLayout(false);
        groupBox2.PerformLayout();
        ResumeLayout(false);
    }
}
