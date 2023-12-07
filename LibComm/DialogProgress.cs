using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.LibComm;

public class DialogProgress : Form
{
    private string m_strProcessText = "Transfer Data...";

    private double m_netSpeedKBps;

    private IContainer components = null;

    private Button btnProcessOk;

    private Label lbProgressTitle;

    private ProgressBar pgbarProcessing;

    public int ProgressValue
    {
        get
        {
            return pgbarProcessing.Value;
        }
        set
        {
            if (value <= pgbarProcessing.Maximum && value >= pgbarProcessing.Minimum)
            {
                pgbarProcessing.Value = value;
            }
            else
            {
                pgbarProcessing.Value = pgbarProcessing.Minimum;
            }
            lbProgressTitle.Text = m_strProcessText + pgbarProcessing.Value + "%        " + m_netSpeedKBps.ToString("f2") + "KB/s";
        }
    }

    public double NetSpeed
    {
        get
        {
            return m_netSpeedKBps;
        }
        set
        {
            m_netSpeedKBps = value;
            lbProgressTitle.Text = m_strProcessText + pgbarProcessing.Value + "%        " + m_netSpeedKBps.ToString("f2") + "KB/s";
        }
    }

    public bool btnOKVisible
    {
        get
        {
            return btnProcessOk.Visible;
        }
        set
        {
            btnProcessOk.Visible = value;
        }
    }

    public bool ProgressVisible
    {
        get
        {
            return pgbarProcessing.Visible;
        }
        set
        {
            pgbarProcessing.Visible = value;
        }
    }

    public DialogProgress()
    {
        InitializeComponent();
        btnProcessOk.Enabled = false;
    }

    public DialogProgress(string strTitle, string strProcess)
    {
        InitializeComponent();
        Text = strTitle;
        m_strProcessText = strProcess;
        lbProgressTitle.Text = strProcess;
        btnProcessOk.Enabled = false;
    }

    public void SetProgressFinsh(bool bSuccess)
    {
        pgbarProcessing.Value = pgbarProcessing.Maximum;
        lbProgressTitle.Text = "Finish " + pgbarProcessing.Value + "%";
        if (bSuccess)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            DialogResult = DialogResult.Abort;
            Close();
        }
    }

    private void btnProcessOk_Click(object sender, EventArgs e)
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
        btnProcessOk = new Button();
        lbProgressTitle = new Label();
        pgbarProcessing = new ProgressBar();
        SuspendLayout();
        btnProcessOk.Location = new System.Drawing.Point(136, 116);
        btnProcessOk.Name = "btnProcessOk";
        btnProcessOk.Size = new System.Drawing.Size(75, 23);
        btnProcessOk.TabIndex = 14;
        btnProcessOk.Text = "OK";
        btnProcessOk.UseVisualStyleBackColor = true;
        btnProcessOk.Click += new EventHandler(btnProcessOk_Click);
        lbProgressTitle.AutoSize = true;
        lbProgressTitle.Font = new System.Drawing.Font("Times New Roman", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        lbProgressTitle.Location = new System.Drawing.Point(60, 42);
        lbProgressTitle.Name = "lbProgressTitle";
        lbProgressTitle.Size = new System.Drawing.Size(63, 12);
        lbProgressTitle.TabIndex = 13;
        lbProgressTitle.Text = "Processing...";
        pgbarProcessing.Location = new System.Drawing.Point(43, 67);
        pgbarProcessing.Name = "pgbarProcessing";
        pgbarProcessing.Size = new System.Drawing.Size(273, 23);
        pgbarProcessing.TabIndex = 12;
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(358, 180);
        Controls.Add(btnProcessOk);
        Controls.Add(lbProgressTitle);
        Controls.Add(pgbarProcessing);
        Name = "DialogProgress";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Connecting";
        ResumeLayout(false);
        PerformLayout();
    }
}
