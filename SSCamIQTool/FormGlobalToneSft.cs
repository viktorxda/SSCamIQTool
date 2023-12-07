using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public class FormGlobalToneSft : Form
{
    private byte SftSize = 1;

    private long[] u32GlobalToneSft = new long[1];

    private byte SftType = 1;

    private string[] m_sfdTitle = new string[1];

    private long[] m_sfdValMin = new long[1];

    private long[] m_sfdValMax = new long[1];

    private IContainer components = null;

    private GroupBox gpbRawSize;

    private DataGridView GlobalToneSft;

    private Label label2;

    private Button btnOK;

    private Button btnCancel;

    public byte SftSize_Changed
    {
        get
        {
            return SftSize;
        }
        set
        {
            SftSize = value;
            InitGlobalToneSft();
        }
    }

    public long[] GlobalToneSft_Changed
    {
        get
        {
            return u32GlobalToneSft;
        }
        set
        {
            u32GlobalToneSft = value;
            GlobalToneSft_ValueChanged();
        }
    }

    public FormGlobalToneSft()
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
        for (int i = 0; i < SftSize; i++)
        {
            GlobalToneSft.Columns[i].HeaderText = (i + 1).ToString();
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
                u32GlobalToneSft[e.RowIndex * GlobalToneSft.ColumnCount + e.ColumnIndex] = num;
                return;
            }
            catch
            {
                MessageBox.Show("The value must be a non-negative integer.");
                dataGridViewCell.Value = u32GlobalToneSft[e.RowIndex * GlobalToneSft.ColumnCount + e.ColumnIndex];
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
            u32GlobalToneSft[e.RowIndex * GlobalToneSft.ColumnCount + e.ColumnIndex] = num2;
        }
        catch (Exception)
        {
            dataGridViewCell.Value = u32GlobalToneSft[e.RowIndex * GlobalToneSft.ColumnCount + e.ColumnIndex];
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
        DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
        gpbRawSize = new GroupBox();
        GlobalToneSft = new DataGridView();
        label2 = new Label();
        btnOK = new Button();
        btnCancel = new Button();
        gpbRawSize.SuspendLayout();
        ((ISupportInitialize)GlobalToneSft).BeginInit();
        SuspendLayout();
        gpbRawSize.Controls.Add(GlobalToneSft);
        gpbRawSize.Controls.Add(label2);
        gpbRawSize.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        gpbRawSize.Location = new System.Drawing.Point(21, 12);
        gpbRawSize.Name = "gpbRawSize";
        gpbRawSize.Size = new System.Drawing.Size(944, 115);
        gpbRawSize.TabIndex = 51;
        gpbRawSize.TabStop = false;
        gpbRawSize.Text = "Setting";
        GlobalToneSft.AllowUserToAddRows = false;
        GlobalToneSft.AllowUserToDeleteRows = false;
        GlobalToneSft.AllowUserToResizeRows = false;
        GlobalToneSft.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        GlobalToneSft.BackgroundColor = System.Drawing.SystemColors.ControlLight;
        GlobalToneSft.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Window;
        dataGridViewCellStyle.Font = new System.Drawing.Font("Segoe UI Symbol", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
        dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
        dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        dataGridViewCellStyle.WrapMode = DataGridViewTriState.False;
        GlobalToneSft.DefaultCellStyle = dataGridViewCellStyle;
        GlobalToneSft.Location = new System.Drawing.Point(97, 25);
        GlobalToneSft.Name = "GlobalToneSft";
        GlobalToneSft.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
        GlobalToneSft.RowTemplate.Height = 24;
        GlobalToneSft.Size = new System.Drawing.Size(832, 55);
        GlobalToneSft.TabIndex = 47;
        GlobalToneSft.CellValueChanged += new DataGridViewCellEventHandler(GlobalToneSft_CellValueChanged);
        label2.AutoSize = true;
        label2.Font = new System.Drawing.Font("Segoe UI Symbol", 9f);
        label2.Location = new System.Drawing.Point(6, 46);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(85, 15);
        label2.TabIndex = 46;
        label2.Text = "GlobalToneSft:";
        btnOK.Location = new System.Drawing.Point(774, 127);
        btnOK.Name = "btnOK";
        btnOK.Size = new System.Drawing.Size(75, 23);
        btnOK.TabIndex = 52;
        btnOK.Text = "OK";
        btnOK.UseVisualStyleBackColor = true;
        btnCancel.Location = new System.Drawing.Point(875, 126);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new System.Drawing.Size(75, 23);
        btnCancel.TabIndex = 53;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(977, 168);
        Controls.Add(btnCancel);
        Controls.Add(btnOK);
        Controls.Add(gpbRawSize);
        Name = "FormGlobalToneSft";
        Text = "Form1";
        gpbRawSize.ResumeLayout(false);
        gpbRawSize.PerformLayout();
        ((ISupportInitialize)GlobalToneSft).EndInit();
        ResumeLayout(false);
    }
}
