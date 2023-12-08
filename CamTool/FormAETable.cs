using SSCamIQTool.LibComm;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class FormAETable : Form
{
    public delegate void ShowMessageLog(string strMsg);

    private delegate void UpdateTableHandler();

    private delegate void ShowMessageHandler(string strMsg);

    private readonly int m_nGridCellW = 60;

    private readonly int m_nDefaultFormWdith = 1200;

    private readonly int m_nDefaultFormHeight = 680;

    private readonly GuiGroup guiGroup;

    private readonly GuiItem guiItem;

    private readonly GuiItem guiItem_ID;

    private readonly long[] ID_value;

    private readonly IQComm m_CameraComm;

    private readonly bool m_bInitOk;

    private readonly bool m_bAutoWrite = true;

    private bool m_bWrite;

    private readonly IContainer components = null;

    private Label labelShowTableTitle;

    private DataGridView dataGridShowTable;

    private Button btnReadTable;

    private Button btnWriteTable;

    private Button btnImportTable;

    private Label labelID;

    public FormAETable()
    {
        InitializeComponent();
    }

    public FormAETable(GuiGroup group, IQComm comm, string itemTag, bool bAW)
    {
        InitializeComponent();
        guiGroup = group;
        guiItem = group.FindItemByName(itemTag);
        guiItem_ID = group.FindItemByName("API_AEAttr_WEIGHTING_ID");
        ID_value = guiItem_ID.DataValue;
        if (ID_value[0] == 0L)
        {
            labelID.Text = "Weighting PAGEle ID:Average";
        }
        else if (ID_value[0] == 1)
        {
            labelID.Text = "Weighting PAGEle ID:Center";
        }
        else if (ID_value[0] == 2)
        {
            labelID.Text = "Weighting PAGEle ID:Spot";
        }
        m_CameraComm = comm;
        m_bAutoWrite = bAW;
        labelShowTableTitle.Text = guiGroup.Name + " --> " + guiItem.Text;
        dataGridShowTable.ColumnCount = guiItem.XSize;
        dataGridShowTable.RowCount = guiItem.YSize;
        dataGridShowTable.AllowUserToAddRows = false;
        if (group.Action == API_ACTION.R)
        {
            dataGridShowTable.ReadOnly = true;
            btnWriteTable.Enabled = false;
            btnWriteTable.Visible = false;
        }
        if (group.Action == API_ACTION.W)
        {
            btnReadTable.Enabled = false;
            btnReadTable.Visible = false;
        }
        if (dataGridShowTable.ColumnCount > 32)
        {
            dataGridShowTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            for (int i = 0; i < dataGridShowTable.Columns.Count; i++)
            {
                dataGridShowTable.Columns[i].Width = m_nGridCellW;
            }
        }
        for (int j = 0; j < dataGridShowTable.Columns.Count; j++)
        {
            dataGridShowTable.Columns[j].HeaderText = (j + 1).ToString();
            dataGridShowTable.Columns[j].SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        for (int k = 0; k < dataGridShowTable.Rows.Count; k++)
        {
            dataGridShowTable.Rows[k].HeaderCell.Value = k.ToString();
        }
        if (guiItem.Paramters != "")
        {
            string[] array = guiItem.Paramters.Split(':');
            if (array[0] != "")
            {
                string[] array2 = array[0].Split(',');
                int num = 0;
                for (int l = 0; l < array2.Length; l++)
                {
                    if (array2[l].Contains("(") && array2[l].Contains(")"))
                    {
                        string text = array2[l].Substring(0, array2[l].IndexOf('('));
                        string[] array3 = array2[l].Substring(array2[l].IndexOf('(') + 1, array2[l].IndexOf(')') - array2[l].IndexOf('(') - 1).Split('/');
                        string[] array4 = array3[0].Split('~');
                        for (int m = int.Parse(array4[0]); m <= int.Parse(array4[1]); m += int.Parse(array3[1]))
                        {
                            if (num < dataGridShowTable.Columns.Count)
                            {
                                dataGridShowTable.Columns[num++].HeaderText = text + m;
                            }
                        }
                    }
                    else if (num < dataGridShowTable.Columns.Count)
                    {
                        dataGridShowTable.Columns[num++].HeaderText = array2[l];
                    }
                }
            }
            if (array[1] != "")
            {
                string[] array5 = array[1].Split(',');
                int num2 = 0;
                for (int n = 0; n < array5.Length; n++)
                {
                    if (array5[n].Contains("(") && array5[n].Contains(")"))
                    {
                        string text2 = array5[n].Substring(0, array5[n].IndexOf('('));
                        string[] array6 = array5[n].Substring(array5[n].IndexOf('(') + 1, array5[n].IndexOf(')') - array5[n].IndexOf('(') - 1).Split('/');
                        string[] array7 = array6[0].Split('~');
                        for (int num3 = int.Parse(array7[0]); num3 <= int.Parse(array7[1]); num3 += int.Parse(array6[1]))
                        {
                            if (num2 < dataGridShowTable.Rows.Count)
                            {
                                dataGridShowTable.Rows[num2++].HeaderCell.Value = text2 + num3;
                            }
                        }
                    }
                    else if (num2 < dataGridShowTable.Rows.Count)
                    {
                        dataGridShowTable.Columns[num2++].HeaderText = array5[n];
                    }
                }
            }
        }
        dataGridShowTable.Width = dataGridShowTable.RowHeadersWidth + (m_nGridCellW * dataGridShowTable.Columns.Count);
        dataGridShowTable.Height = dataGridShowTable.ColumnHeadersHeight + (dataGridShowTable.Rows[0].Height * dataGridShowTable.Rows.Count) + 2;
        int num4 = dataGridShowTable.Location.X;
        int num5 = dataGridShowTable.Width + (dataGridShowTable.Location.X * 2) + (Size.Width - ClientSize.Width);
        int num6 = dataGridShowTable.Location.Y + dataGridShowTable.Height + num4 + btnReadTable.Height + (Size.Height - ClientSize.Height);
        Width = num5 > m_nDefaultFormWdith
            ? m_nDefaultFormWdith
            : num5 < labelShowTableTitle.Width + (labelShowTableTitle.Location.X * 2)
                ? labelShowTableTitle.Width + (labelShowTableTitle.Location.X * 2) + (Size.Width - ClientSize.Width)
                : num5;
        dataGridShowTable.Width = ClientSize.Width - (dataGridShowTable.Location.X * 2);
        Height = num6 > m_nDefaultFormHeight ? m_nDefaultFormHeight : num6;
        dataGridShowTable.Height = Height - (dataGridShowTable.Location.Y + (Size.Height - ClientSize.Height) + num4 + btnReadTable.Height);
        btnReadTable.Location = new Point(btnReadTable.Location.X, dataGridShowTable.Location.Y + dataGridShowTable.Height + (num4 / 2));
        btnWriteTable.Location = new Point(btnWriteTable.Location.X, dataGridShowTable.Location.Y + dataGridShowTable.Height + (num4 / 2));
        btnImportTable.Location = new Point(btnImportTable.Location.X, dataGridShowTable.Location.Y + dataGridShowTable.Height + (num4 / 2));
        for (int num7 = 0; num7 < dataGridShowTable.Rows.Count; num7++)
        {
            for (int num8 = 0; num8 < dataGridShowTable.Rows[num7].Cells.Count; num8++)
            {
                dataGridShowTable.Rows[num7].Cells[num8].Value = 0;
            }
        }
        if (m_CameraComm.IsConnected())
        {
            m_CameraComm.updateTaskQueue(ReadTableTask, null);
        }
        UpdateTableValue();
        m_bWrite = false;
        m_bInitOk = true;
    }

    protected void UpdateTableValue()
    {
        if (guiItem.openfile_flag)
        {
            return;
        }
        long[] dataValue = guiItem.DataValue;
        int num = 0;
        for (int i = 0; i < dataGridShowTable.Rows.Count; i++)
        {
            for (int j = 0; j < dataGridShowTable.Rows[i].Cells.Count; j++)
            {
                if (num < dataValue.Length)
                {
                    dataGridShowTable.Rows[i].Cells[j].Value = dataValue[num];
                }
                num++;
            }
        }
    }

    private void ShowMessageBox(string strMsg)
    {
        _ = MessageBox.Show(strMsg, "Status", MessageBoxButtons.OK);
    }

    public void ReadTableTask(object obj)
    {
        if (guiGroup.ReadGroup(m_CameraComm).Equals(""))
        {
            try
            {
                _ = Invoke(new UpdateTableHandler(UpdateTableValue));
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
        _ = Invoke(new ShowMessageHandler(ShowMessageBox), "Read table failed");
    }

    public void SendTableTask(object obj)
    {
        if (guiGroup.WriteGroup(m_CameraComm) != "")
        {
            _ = Invoke(new ShowMessageHandler(ShowMessageBox), "Write table failed");
        }
    }

    protected void WriteTable()
    {
        long[] dataValue = guiItem.DataValue;
        int num = 0;
        if (guiGroup.Action is API_ACTION.RW or API_ACTION.W)
        {
            for (int i = 0; i < dataGridShowTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridShowTable.Rows[i].Cells.Count; j++)
                {
                    if (num < dataValue.Length)
                    {
                        dataValue[num] = Convert.ToInt64(dataGridShowTable.Rows[i].Cells[j].Value);
                    }
                    num++;
                }
            }
            if (guiGroup.Action == API_ACTION.RW && m_bAutoWrite && m_CameraComm.IsConnected())
            {
                m_CameraComm.updateTaskQueue(SendTableTask, null);
            }
        }
        m_bWrite = false;
    }

    private void btnReadTable_Click(object sender, EventArgs e)
    {
        if (m_CameraComm.IsConnected())
        {
            guiItem.openfile_flag = false;
            m_CameraComm.updateTaskQueue(ReadTableTask, null);
        }
    }

    private void btnWriteTable_Click(object sender, EventArgs e)
    {
        WriteTable();
        SaveInnerText();
    }

    public void SaveInnerText()
    {
    }

    private void btnImportTable_Click(object sender, EventArgs e)
    {
        string text = "";
        string text2 = "";
        OpenFileDialog openFileDialog = new()
        {
            Filter = "data file (*.lut)|*.lut",
            Title = "Import Table Params"
        };
        if (openFileDialog.ShowDialog() == DialogResult.OK && openFileDialog.FileName != "")
        {
            text = openFileDialog.FileName;
            text2 = ImportTableParams(openFileDialog.FileName);
            UpdateTableValue();
        }
        if (!text.Equals(""))
        {
            _ = !text2.Equals("")
                ? MessageBox.Show(text2, "Import Table", MessageBoxButtons.OK, MessageBoxIcon.Hand)
                : MessageBox.Show("Import Success", "Status", MessageBoxButtons.OK);
        }
    }

    private string ImportTableParams(string strFileName)
    {
        try
        {
            using FileStream stream = File.Open(strFileName, FileMode.Open, FileAccess.Read);
            using StreamReader streamReader = new(stream);
            int num = 0;
            while (!streamReader.EndOfStream)
            {
                string[] array = streamReader.ReadLine().Split(',');
                if (array.Length == guiItem.XSize)
                {
                    long[] array2 = Array.ConvertAll(array, long.Parse);
                    array2.CopyTo(guiItem.DataValue, num);
                    num += array2.Length;
                    continue;
                }
                return "invalid format: column number is not equal";
            }
            return "";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private void FormAETable_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveInnerText();
        _ = dataGridShowTable.EndEdit();
        if (m_bWrite)
        {
            WriteTable();
        }
    }

    private void dataGridShowTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && m_bInitOk)
        {
            DataGridViewCell dataGridViewCell = dataGridShowTable.Rows[e.RowIndex].Cells[e.ColumnIndex];
            try
            {
                long num = Convert.ToInt64(dataGridViewCell.Value);
                if (num > guiItem.MaxValue)
                {
                    dataGridViewCell.Value = guiItem.MaxValue;
                }
                if (num < guiItem.MinValue)
                {
                    dataGridViewCell.Value = guiItem.MinValue;
                }
            }
            catch
            {
                dataGridViewCell.Value = guiItem.MinValue;
            }
        }
        m_bWrite = true;
    }

    private void dataGridShowTable_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
    {
        e.Column.FillWeight = 1f;
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
        labelShowTableTitle = new Label();
        dataGridShowTable = new DataGridView();
        btnReadTable = new Button();
        btnWriteTable = new Button();
        btnImportTable = new Button();
        labelID = new Label();
        ((ISupportInitialize)dataGridShowTable).BeginInit();
        SuspendLayout();
        labelShowTableTitle.AutoSize = true;
        labelShowTableTitle.Font = new Font("Times New Roman", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelShowTableTitle.Location = new Point(13, 9);
        labelShowTableTitle.Name = "labelShowTableTitle";
        labelShowTableTitle.Size = new Size(139, 16);
        labelShowTableTitle.TabIndex = 2;
        labelShowTableTitle.Text = "labelShowTableTitle";
        dataGridShowTable.AllowUserToAddRows = false;
        dataGridShowTable.AllowUserToDeleteRows = false;
        dataGridShowTable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dataGridShowTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridShowTable.BackgroundColor = Color.WhiteSmoke;
        dataGridShowTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridShowTable.Location = new Point(16, 72);
        dataGridShowTable.Name = "dataGridShowTable";
        dataGridShowTable.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
        dataGridShowTable.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dataGridShowTable.Size = new Size(240, 150);
        dataGridShowTable.TabIndex = 3;
        dataGridShowTable.CellValueChanged += new DataGridViewCellEventHandler(dataGridShowTable_CellValueChanged);
        dataGridShowTable.ColumnAdded += new DataGridViewColumnEventHandler(dataGridShowTable_ColumnAdded);
        btnReadTable.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnReadTable.Location = new Point(16, 344);
        btnReadTable.Margin = new Padding(3, 4, 3, 4);
        btnReadTable.Name = "btnReadTable";
        btnReadTable.Size = new Size(51, 23);
        btnReadTable.TabIndex = 4;
        btnReadTable.Text = "Read";
        btnReadTable.UseVisualStyleBackColor = true;
        btnReadTable.Click += new EventHandler(btnReadTable_Click);
        btnWriteTable.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnWriteTable.Location = new Point(83, 344);
        btnWriteTable.Name = "btnWriteTable";
        btnWriteTable.Size = new Size(52, 23);
        btnWriteTable.TabIndex = 5;
        btnWriteTable.Text = "Write";
        btnWriteTable.UseVisualStyleBackColor = true;
        btnWriteTable.Click += new EventHandler(btnWriteTable_Click);
        btnImportTable.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnImportTable.Location = new Point(340, 344);
        btnImportTable.Name = "btnImportTable";
        btnImportTable.Size = new Size(57, 23);
        btnImportTable.TabIndex = 6;
        btnImportTable.Text = "Import";
        btnImportTable.UseVisualStyleBackColor = true;
        btnImportTable.Click += new EventHandler(btnImportTable_Click);
        labelID.AutoSize = true;
        labelID.Font = new Font("Times New Roman", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelID.Location = new Point(13, 43);
        labelID.Name = "labelID";
        labelID.Size = new Size(21, 16);
        labelID.TabIndex = 7;
        labelID.Text = "ID";
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(425, 398);
        Controls.Add(labelID);
        Controls.Add(btnImportTable);
        Controls.Add(btnWriteTable);
        Controls.Add(btnReadTable);
        Controls.Add(dataGridShowTable);
        Controls.Add(labelShowTableTitle);
        Name = "FormAETable";
        Text = "FormAETable";
        FormClosing += new FormClosingEventHandler(FormAETable_FormClosing);
        ((ISupportInitialize)dataGridShowTable).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
