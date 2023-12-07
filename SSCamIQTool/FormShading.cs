using SSCamIQTool.LibComm;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public class FormShading : Form
{
    private delegate void UpdateTableHandler();

    private delegate void ShowMessageHandler(string strMsg);

    private long[] arData;

    private GuiGroup m_GridGuiGroup;

    private GuiItem m_GridGuiItem;

    private IQComm m_DbgComm;

    private int nColumn;

    private int nRow;

    private int nWidth = 61;

    private int nHeight = 69;

    private DataColumn column;

    private DataRow row;

    private DataTable table = new DataTable();

    private DataSet set = new DataSet();

    private IContainer components = null;

    private Button btnReadTable;

    private Button btnWriteTable;

    private Label labelMultiCtrlTitle;

    private DataGridView dataGridViewShading;

    private ComboBox comboBoxSelect;

    private Label labelMin;

    private Label labelMid;

    private Label labelMax;

    private Panel panelForColor;

    public FormShading()
    {
        InitializeComponent();
    }

    public FormShading(GuiGroup group, IQComm comm, string strTag)
    {
        string text = "";
        InitializeComponent();
        m_GridGuiGroup = group;
        m_DbgComm = comm;
        m_GridGuiItem = m_GridGuiGroup.FindItemByName(strTag);
        text = m_GridGuiGroup.Name + " --> " + m_GridGuiItem.Text;
        nColumn = m_GridGuiItem.XSize;
        nRow = m_GridGuiItem.YSize;
        _ = m_GridGuiItem.MaxValue;
        _ = m_GridGuiItem.MinValue;
        string[] array = m_GridGuiItem.Paramters.Split('x');
        string s = array[0];
        string s2 = array[1];
        nWidth = int.Parse(s);
        nHeight = int.Parse(s2);
        _ = m_GridGuiItem.Paramters;
        ParseGridHeader(m_GridGuiItem.Paramters, nColumn, nRow, out var _, out var _);
        arData = new long[nColumn * nRow];
        labelMultiCtrlTitle.Text = text;
        Text = text;
        string[] array2 = text.Split('>');
        _ = array2[array2.Length - 1];
        for (int i = 0; i < nRow; i++)
        {
            comboBoxSelect.Items.Add("table" + i);
            table = new DataTable("table" + i);
            Addvalue(ref table, i, ref arData);
            set.Tables.Add(table);
        }
        dataGridViewShading.DataSource = set.Tables["table0"];
        dataGridViewShading.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 7f);
        dataGridViewShading.DefaultCellStyle.Font = new Font("Times New Roman", 7f);
        dataGridViewShading.RowHeadersDefaultCellStyle.Font = new Font("Times New Roman", 7f);
        Load += FormShading_Load;
        UpdateTableValue();
    }

    private void FormShading_Load(object sender, EventArgs e)
    {
        AddHeadervalue();
        for (int i = 0; i < dataGridViewShading.Columns.Count; i++)
        {
            dataGridViewShading.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        ShowColor();
        dataGridViewShading.DoubleBuffered(setting: true);
    }

    protected void AddHeadervalue()
    {
        for (int i = 1; i < dataGridViewShading.Rows.Count; i++)
        {
            dataGridViewShading.Rows[i - 1].HeaderCell.Value = string.Concat(i);
        }
    }

    private void panelForColor_Paint(object sender, PaintEventArgs e)
    {
        Graphics graphics = CreateGraphics();
        for (int i = 0; i < 360; i++)
        {
            Rectangle rect = new Rectangle(670 + i, 10, 1, 20);
            HsvToRgb(i, 1.0, 1.0, out var r, out var g, out var b);
            SolidBrush brush = new SolidBrush(Color.FromArgb(r, g, b));
            graphics.FillRectangle(brush, rect);
        }
    }

    private void Addvalue(ref DataTable table, int arrayNum, ref long[] arData)
    {
        table.Clear();
        table.Columns.Clear();
        table.Rows.Clear();
        long[] array = new long[nColumn];
        int num = nColumn * arrayNum;
        int num2 = nColumn * (arrayNum + 1);
        int num3 = 0;
        for (int i = num; i < num2; i++)
        {
            array[num3] = arData[i];
            num3++;
        }
        for (int j = 1; j < nWidth + 1; j++)
        {
            column = new DataColumn();
            column.DataType = Type.GetType("System.Int32");
            column.ColumnName = string.Concat(j);
            table.Columns.Add(column);
        }
        int num4 = 0;
        for (int k = 1; k < nHeight + 1; k++)
        {
            row = table.NewRow();
            for (int l = 1; l < nWidth + 1; l++)
            {
                string columnName = string.Concat(l);
                row[columnName] = array[num4];
                num4++;
            }
            table.Rows.Add(row);
        }
    }

    private void AddReadArrayvalue(int arrayNum, ref long[] arData)
    {
        set.Tables["table" + arrayNum].Clear();
        set.Tables["table" + arrayNum].Columns.Clear();
        set.Tables["table" + arrayNum].Rows.Clear();
        long[] array = new long[nColumn];
        int num = nColumn * arrayNum;
        int num2 = nColumn * (arrayNum + 1);
        int num3 = 0;
        for (int i = num; i < num2; i++)
        {
            array[num3] = arData[i];
            num3++;
        }
        for (int j = 1; j < nWidth + 1; j++)
        {
            column = new DataColumn();
            column.DataType = Type.GetType("System.Int32");
            column.ColumnName = string.Concat(j);
            set.Tables["table" + arrayNum].Columns.Add(column);
        }
        int num4 = 0;
        for (int k = 1; k < nHeight + 1; k++)
        {
            row = set.Tables["table" + arrayNum].NewRow();
            for (int l = 1; l < nWidth + 1; l++)
            {
                string columnName = string.Concat(l);
                row[columnName] = array[num4];
                num4++;
            }
            set.Tables["table" + arrayNum].Rows.Add(row);
        }
    }

    protected void UpdateTableValue()
    {
        long[] dataValue = m_GridGuiItem.DataValue;
        _ = nColumn;
        labelMultiCtrlTitle.Text.Split('>');
        for (int i = 0; i < nRow; i++)
        {
            AddReadArrayvalue(i, ref dataValue);
        }
        dataGridViewShading.DataSource = set.Tables[comboBoxSelect.Text];
        AddHeadervalue();
    }

    private void ShowMessageBox(string strMsg)
    {
        MessageBox.Show(strMsg, "Status", MessageBoxButtons.OK);
    }

    public void ReadTableTask(object obj)
    {
        _ = nColumn;
        labelMultiCtrlTitle.Text.Split('>');
        if (m_GridGuiGroup.ReadGroup(m_DbgComm).Equals(""))
        {
            Invoke(new UpdateTableHandler(UpdateTableValue));
            Invoke(new UpdateTableHandler(ShowColor));
            Invoke(new ShowMessageHandler(ShowMessageBox), "Read shading success");
        }
        else
        {
            Invoke(new ShowMessageHandler(ShowMessageBox), "Read shading failed");
        }
    }

    private void btnReadTable_Click(object sender, EventArgs e)
    {
        if (m_DbgComm.IsConnected())
        {
            m_DbgComm.updateTaskQueue(ReadTableTask, null);
        }
    }

    private void SendTableTask(object obj)
    {
        if (m_GridGuiGroup.WriteGroup(m_DbgComm) != "")
        {
            Invoke(new ShowMessageHandler(ShowMessageBox), "Write shading failed");
        }
    }

    protected void WriteTableToBoard()
    {
        long[] dataValue = m_GridGuiItem.DataValue;
        labelMultiCtrlTitle.Text.Split('>');
        if (m_GridGuiGroup.Action != API_ACTION.RW)
        {
            return;
        }
        for (int i = 0; i < nRow; i++)
        {
            int num = i * nColumn;
            for (int j = 0; j < nHeight; j++)
            {
                for (int k = 0; k < nWidth; k++)
                {
                    dataValue[num] = Convert.ToInt64(set.Tables["table" + i].Rows[j][k]);
                    num++;
                }
            }
        }
        if (m_DbgComm.IsConnected())
        {
            m_DbgComm.updateTaskQueue(SendTableTask, null);
        }
    }

    private void comboBoxSelect_SelectedIndexChanged(object sender, EventArgs e)
    {
        dataGridViewShading.DataSource = set.Tables[comboBoxSelect.Text];
        AddHeadervalue();
        ShowColor();
    }

    private void btnWriteTable_Click(object sender, EventArgs e)
    {
        WriteTableToBoard();
    }

    protected void ShowColor()
    {
        for (int i = 0; i < nHeight; i++)
        {
            for (int j = 1; j < nWidth + 1; j++)
            {
                double num = int.Parse(dataGridViewShading.Rows[i].Cells[string.Concat(j)].Value.ToString());
                num = 360.0 * num / 2048.0;
                HsvToRgb(num, 1.0, 1.0, out var r, out var g, out var b);
                dataGridViewShading.Rows[i].Cells[string.Concat(j)].Style.BackColor = Color.FromArgb(r, g, b);
            }
        }
        for (int k = 0; k < dataGridViewShading.Columns.Count; k++)
        {
            dataGridViewShading.Columns[k].SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }

    public void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
    {
        double num;
        for (num = h; num < 0.0; num += 360.0)
        {
        }
        while (num >= 360.0)
        {
            num -= 360.0;
        }
        double num4;
        double num3;
        double num2;
        if (V <= 0.0)
        {
            num4 = num3 = num2 = 0.0;
        }
        else if (S <= 0.0)
        {
            num4 = num3 = num2 = V;
        }
        else
        {
            double num5 = num / 60.0;
            int num6 = (int)Math.Floor(num5);
            double num7 = num5 - num6;
            double num8 = V * (1.0 - S);
            double num9 = V * (1.0 - S * num7);
            double num10 = V * (1.0 - S * (1.0 - num7));
            switch (num6)
            {
                case 0:
                    num4 = V;
                    num3 = num10;
                    num2 = num8;
                    break;
                case 1:
                    num4 = num9;
                    num3 = V;
                    num2 = num8;
                    break;
                case 2:
                    num4 = num8;
                    num3 = V;
                    num2 = num10;
                    break;
                case 3:
                    num4 = num8;
                    num3 = num9;
                    num2 = V;
                    break;
                case 4:
                    num4 = num10;
                    num3 = num8;
                    num2 = V;
                    break;
                case 5:
                    num4 = V;
                    num3 = num8;
                    num2 = num9;
                    break;
                case 6:
                    num4 = V;
                    num3 = num10;
                    num2 = num8;
                    break;
                case -1:
                    num4 = V;
                    num3 = num8;
                    num2 = num9;
                    break;
                default:
                    num4 = num3 = num2 = V;
                    break;
            }
        }
        r = Clamp((int)(num4 * 255.0));
        g = Clamp((int)(num3 * 255.0));
        b = Clamp((int)(num2 * 255.0));
    }

    private int Clamp(int i)
    {
        if (i < 0)
        {
            return 0;
        }
        if (i > 255)
        {
            return 255;
        }
        return i;
    }

    public void ParseGridHeader(string strParams, int nColumn, int nRow, out string[] pHeaderX, out string[] pHeaderY)
    {
        pHeaderX = new string[nColumn];
        pHeaderY = new string[nRow];
        try
        {
            if (strParams.Contains(';'))
            {
                string[] array = strParams.Split(';');
                if (array.Length != 0)
                {
                    if (array[0].Contains(':'))
                    {
                        string[] array2 = array[0].Split(':');
                        for (int i = 0; i < nColumn; i++)
                        {
                            pHeaderX[i] = array2[0] + (Convert.ToInt32(array2[1]) + i);
                        }
                    }
                    else
                    {
                        pHeaderX = array[0].Split(',');
                    }
                }
                else
                {
                    pHeaderX = null;
                    pHeaderY = null;
                }
                if (array.Length > 1)
                {
                    if (array[1].Contains(':'))
                    {
                        string[] array2 = array[1].Split(':');
                        for (int j = 0; j < nRow; j++)
                        {
                            pHeaderY[j] = array2[0] + (Convert.ToInt32(array2[1]) + j);
                        }
                    }
                    else
                    {
                        pHeaderY = array[0].Split(',');
                    }
                }
                else
                {
                    pHeaderY = null;
                }
            }
            else
            {
                pHeaderX = null;
                pHeaderY = null;
            }
        }
        catch
        {
            pHeaderX = null;
            pHeaderY = null;
        }
        if (pHeaderX == null)
        {
            pHeaderX = new string[nColumn];
            for (int k = 0; k < nColumn; k++)
            {
                pHeaderX[k] = (k + 1).ToString();
            }
        }
        if (pHeaderY == null)
        {
            pHeaderY = new string[nRow];
            for (int l = 0; l < nRow; l++)
            {
                pHeaderY[l] = (l + 1).ToString();
            }
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
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
        btnReadTable = new Button();
        btnWriteTable = new Button();
        labelMultiCtrlTitle = new Label();
        dataGridViewShading = new DataGridView();
        comboBoxSelect = new ComboBox();
        labelMin = new Label();
        labelMid = new Label();
        labelMax = new Label();
        panelForColor = new Panel();
        ((ISupportInitialize)dataGridViewShading).BeginInit();
        panelForColor.SuspendLayout();
        SuspendLayout();
        btnReadTable.Location = new Point(349, 10);
        btnReadTable.Name = "btnReadTable";
        btnReadTable.Size = new Size(75, 23);
        btnReadTable.TabIndex = 0;
        btnReadTable.Text = "Read";
        btnReadTable.UseVisualStyleBackColor = true;
        btnReadTable.Click += new EventHandler(btnReadTable_Click);
        btnWriteTable.Location = new Point(430, 10);
        btnWriteTable.Name = "btnWriteTable";
        btnWriteTable.Size = new Size(75, 23);
        btnWriteTable.TabIndex = 1;
        btnWriteTable.Text = "Write";
        btnWriteTable.UseVisualStyleBackColor = true;
        btnWriteTable.Click += new EventHandler(btnWriteTable_Click);
        labelMultiCtrlTitle.AutoSize = true;
        labelMultiCtrlTitle.Font = new Font("Times New Roman", 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelMultiCtrlTitle.Location = new Point(8, 9);
        labelMultiCtrlTitle.Name = "labelMultiCtrlTitle";
        labelMultiCtrlTitle.Size = new Size(135, 19);
        labelMultiCtrlTitle.TabIndex = 4;
        labelMultiCtrlTitle.Text = "labelMultiCtrlTitle";
        dataGridViewShading.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dataGridViewShading.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewShading.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders;
        dataGridViewCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle.BackColor = SystemColors.Control;
        dataGridViewCellStyle.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        dataGridViewCellStyle.ForeColor = SystemColors.WindowText;
        dataGridViewCellStyle.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle.WrapMode = DataGridViewTriState.True;
        dataGridViewShading.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
        dataGridViewShading.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = SystemColors.Window;
        dataGridViewCellStyle2.Font = new Font("Times New Roman", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 136);
        dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
        dataGridViewCellStyle2.SelectionBackColor = SystemColors.Info;
        dataGridViewCellStyle2.SelectionForeColor = Color.Blue;
        dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
        dataGridViewShading.DefaultCellStyle = dataGridViewCellStyle2;
        dataGridViewShading.Location = new Point(12, 99);
        dataGridViewShading.Name = "dataGridViewShading";
        dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle3.BackColor = SystemColors.Control;
        dataGridViewCellStyle3.Font = new Font("Times New Roman", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
        dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
        dataGridViewCellStyle3.SelectionBackColor = Color.LightGoldenrodYellow;
        dataGridViewCellStyle3.SelectionForeColor = Color.Blue;
        dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
        dataGridViewShading.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
        dataGridViewShading.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
        dataGridViewShading.RowTemplate.Height = 13;
        dataGridViewShading.Size = new Size(1204, 396);
        dataGridViewShading.TabIndex = 6;
        comboBoxSelect.FormattingEnabled = true;
        comboBoxSelect.Location = new Point(513, 10);
        comboBoxSelect.Name = "comboBoxSelect";
        comboBoxSelect.Size = new Size(121, 20);
        comboBoxSelect.TabIndex = 7;
        comboBoxSelect.Text = "table0";
        comboBoxSelect.SelectedIndexChanged += new EventHandler(comboBoxSelect_SelectedIndexChanged);
        labelMin.AutoSize = true;
        labelMin.Font = new Font("Times New Roman", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelMin.Location = new Point(670, 40);
        labelMin.Name = "labelMin";
        labelMin.Size = new Size(13, 15);
        labelMin.TabIndex = 9;
        labelMin.Text = "0";
        labelMid.AutoSize = true;
        labelMid.Font = new Font("Times New Roman", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelMid.Location = new Point(153, 0);
        labelMid.Name = "labelMid";
        labelMid.Size = new Size(31, 15);
        labelMid.TabIndex = 10;
        labelMid.Text = "1024";
        labelMax.AutoSize = true;
        labelMax.Font = new Font("Times New Roman", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelMax.Location = new Point(1000, 40);
        labelMax.Name = "labelMax";
        labelMax.Size = new Size(31, 15);
        labelMax.TabIndex = 11;
        labelMax.Text = "2048";
        panelForColor.Controls.Add(labelMid);
        panelForColor.Location = new Point(667, 42);
        panelForColor.Name = "panelForColor";
        panelForColor.Size = new Size(352, 15);
        panelForColor.TabIndex = 28;
        panelForColor.Paint += new PaintEventHandler(panelForColor_Paint);
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1228, 526);
        Controls.Add(labelMin);
        Controls.Add(labelMax);
        Controls.Add(comboBoxSelect);
        Controls.Add(dataGridViewShading);
        Controls.Add(labelMultiCtrlTitle);
        Controls.Add(btnWriteTable);
        Controls.Add(btnReadTable);
        Controls.Add(panelForColor);
        Name = "FormShading";
        Text = "FormShading";
        ((ISupportInitialize)dataGridViewShading).EndInit();
        panelForColor.ResumeLayout(false);
        panelForColor.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
