using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class FormSetMagicKey : Form
{
    public delegate void SetKeyHandler(uint key);

    private readonly SetKeyHandler SetMagicKey;

    private readonly IContainer components = null;

    private TextBox textBoxMagicKey;

    private Label labelMagicKey;

    private Button buttonSetMagicKey;

    private Label labelHint;

    public FormSetMagicKey(uint key, SetKeyHandler setKey)
    {
        InitializeComponent();
        SetMagicKey = setKey;
        textBoxMagicKey.Text = key.ToString();
    }

    private void buttonSetMagicKey_Click(object sender, EventArgs e)
    {
        uint num;
        try
        {
            num = uint.Parse(textBoxMagicKey.Text);
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show(ex.Message, "Invalid Number", MessageBoxButtons.OK);
            return;
        }
        SetMagicKey(num);
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
        textBoxMagicKey = new TextBox();
        labelMagicKey = new Label();
        buttonSetMagicKey = new Button();
        labelHint = new Label();
        SuspendLayout();
        textBoxMagicKey.Location = new System.Drawing.Point(119, 62);
        textBoxMagicKey.Name = "textBoxMagicKey";
        textBoxMagicKey.Size = new System.Drawing.Size(107, 21);
        textBoxMagicKey.TabIndex = 0;
        labelMagicKey.AutoSize = true;
        labelMagicKey.Location = new System.Drawing.Point(36, 65);
        labelMagicKey.Name = "labelMagicKey";
        labelMagicKey.Size = new System.Drawing.Size(59, 12);
        labelMagicKey.TabIndex = 1;
        labelMagicKey.Text = "Magic Key";
        buttonSetMagicKey.Location = new System.Drawing.Point(241, 60);
        buttonSetMagicKey.Name = "buttonSetMagicKey";
        buttonSetMagicKey.Size = new System.Drawing.Size(75, 23);
        buttonSetMagicKey.TabIndex = 2;
        buttonSetMagicKey.Text = "Save";
        buttonSetMagicKey.UseVisualStyleBackColor = true;
        buttonSetMagicKey.Click += new EventHandler(buttonSetMagicKey_Click);
        labelHint.AutoSize = true;
        labelHint.Location = new System.Drawing.Point(12, 86);
        labelHint.Name = "labelHint";
        labelHint.Size = new System.Drawing.Size(125, 12);
        labelHint.TabIndex = 3;
        labelHint.Text = "(1 ~ 9)-digit number";
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(355, 170);
        Controls.Add(labelHint);
        Controls.Add(buttonSetMagicKey);
        Controls.Add(labelMagicKey);
        Controls.Add(textBoxMagicKey);
        Name = "FormSetMagicKey";
        Text = "SetMagicKey";
        ResumeLayout(false);
        PerformLayout();
    }
}
