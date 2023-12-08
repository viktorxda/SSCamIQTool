using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class FormGlobalToneSetParam : Form
{
    private byte SftSize = 1;

    private long[] u32GlobalToneSft = new long[1];

    private byte SftType = 1;

    private string[] m_sfdTitle = new string[1];

    private long[] m_sfdValMin = new long[1];

    private long[] m_sfdValMax = new long[1];

    private readonly IContainer components = null;

    private GroupBox groupBox1;

    private DataGridView GlobalToneSft;

    private Label label2;

    private Button btnCancel;

    private Button btnOK;

    public byte SftSize_Changed
    {
        get => SftSize;
        set
        {
            SftSize = value;
            InitGlobalToneSft();
        }
    }

    public long[] GlobalToneSft_Changed
    {
        get => u32GlobalToneSft;
        set
        {
            u32GlobalToneSft = value;
            GlobalToneSft_ValueChanged();
        }
    }

    public FormGlobalToneSetParam()
    {
        InitializeComponent();
        btnOK.DialogResult = DialogResult.OK;
        btnCancel.DialogResult = DialogResult.Cancel;
        InitGlobalToneSft();
    }

    private void InitGlobalToneSft()
    {
        GlobalToneSft.ColumnCount = SftSize;
        GlobalToneSft.RowCount = 1;
        GlobalToneSft.ColumnHeadersVisible = true;
        if (SftType == 1)
        {
            for (int i = 0; i < SftSize; i++)
            {
                GlobalToneSft.Columns[i].HeaderText = (i + 1).ToString();
            }
        }
        else
        {
            for (int j = 0; j < m_sfdTitle.Length; j++)
            {
                GlobalToneSft.Columns[j].HeaderText = m_sfdTitle[j];
            }
        }
        Array.Resize(ref u32GlobalToneSft, SftSize);
    }

    public void SetSelfDlgParam(byte type, string tipText, ref string[] title, ref long[] min, ref long[] max)
    {
        SftType = type;
        label2.Text = tipText;
        m_sfdTitle = title;
        m_sfdValMin = min;
        m_sfdValMax = max;
    }

    private void GlobalToneSft_ValueChanged()
    {
        for (byte b = 0; b < SftSize; b++)
        {
            GlobalToneSft.Rows[0].Cells[b].Value = u32GlobalToneSft[b];
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
    }

    private void GlobalToneSft_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }
        DataGridViewCell dataGridViewCell = GlobalToneSft.Rows[e.RowIndex].Cells[e.ColumnIndex];
        if (SftType == 1)
        {
            try
            {
                int num = Convert.ToByte(dataGridViewCell.Value);
                u32GlobalToneSft[(e.RowIndex * GlobalToneSft.ColumnCount) + e.ColumnIndex] = num;
                return;
            }
            catch
            {
                _ = MessageBox.Show("The value must be a non-negative integer.");
                dataGridViewCell.Value = u32GlobalToneSft[(e.RowIndex * GlobalToneSft.ColumnCount) + e.ColumnIndex];
                return;
            }
        }
        if (SftType != 2)
        {
            return;
        }
        try
        {
            long num2 = Convert.ToInt64(dataGridViewCell.Value);
            if (e.ColumnIndex >= 0 && e.ColumnIndex < m_sfdValMin.Length && num2 < m_sfdValMin[e.ColumnIndex])
            {
                throw new Exception("this val < MIN Val");
            }
            if (e.ColumnIndex >= 0 && e.ColumnIndex < m_sfdValMax.Length && num2 > m_sfdValMax[e.ColumnIndex])
            {
                throw new Exception("this val > MAX val");
            }
            u32GlobalToneSft[(e.RowIndex * GlobalToneSft.ColumnCount) + e.ColumnIndex] = num2;
        }
        catch (Exception)
        {
            dataGridViewCell.Value = u32GlobalToneSft[(e.RowIndex * GlobalToneSft.ColumnCount) + e.ColumnIndex];
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
        groupBox1 = new GroupBox();
        GlobalToneSft = new DataGridView();
        label2 = new Label();
        btnCancel = new Button();
        btnOK = new Button();
        groupBox1.SuspendLayout();
        ((ISupportInitialize)GlobalToneSft).BeginInit();
        SuspendLayout();
        groupBox1.Controls.Add(GlobalToneSft);
        groupBox1.Controls.Add(label2);
        groupBox1.Location = new System.Drawing.Point(13, 13);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new System.Drawing.Size(861, 126);
        groupBox1.TabIndex = 0;
        groupBox1.TabStop = false;
        groupBox1.Text = "Setting";
        GlobalToneSft.AllowUserToAddRows = false;
        GlobalToneSft.AllowUserToDeleteRows = false;
        GlobalToneSft.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        GlobalToneSft.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
        GlobalToneSft.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        GlobalToneSft.Location = new System.Drawing.Point(101, 33);
        GlobalToneSft.Name = "GlobalToneSft";
        GlobalToneSft.RowTemplate.Height = 23;
        GlobalToneSft.Size = new System.Drawing.Size(754, 59);
        GlobalToneSft.TabIndex = 1;
        GlobalToneSft.CellValueChanged += new DataGridViewCellEventHandler(GlobalToneSft_CellValueChanged);
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(15, 59);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(53, 12);
        label2.TabIndex = 0;
        label2.Text = "SetParam";
        btnCancel.Location = new System.Drawing.Point(770, 145);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new System.Drawing.Size(75, 23);
        btnCancel.TabIndex = 55;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += new EventHandler(btnCancel_Click);
        btnOK.Location = new System.Drawing.Point(669, 146);
        btnOK.Name = "btnOK";
        btnOK.Size = new System.Drawing.Size(75, 23);
        btnOK.TabIndex = 54;
        btnOK.Text = "OK";
        btnOK.UseVisualStyleBackColor = true;
        btnOK.Click += new EventHandler(btnOK_Click);
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(886, 168);
        Controls.Add(btnCancel);
        Controls.Add(btnOK);
        Controls.Add(groupBox1);
        Name = "FormGlobalToneSetParam";
        Text = "FormGlobalToneSetParam";
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        ((ISupportInitialize)GlobalToneSft).EndInit();
        ResumeLayout(false);
    }
}
