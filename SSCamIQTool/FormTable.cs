using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public class FormTable : Form
{
    public delegate void ShowMessageLog(string strMsg);

    private delegate void UpdateTableHandler();

    private delegate void ShowMessageHandler(string strMsg);

    private int m_nGridCellW = 60;

    private int m_nDefaultFormWdith = 1200;

    private int m_nDefaultFormHeight = 680;

    private static GuiGroup guiGroup;

    private static GuiItem guiItem;

    private IQComm m_CameraComm;

    private bool m_bInitOk;

    private bool m_bAutoWrite = true;

    private bool m_bWrite;

    private static MainForm undoredoParam = new MainForm();

    private string tagName;

    private long[] CellValue;

    private IContainer components = null;

    private DataGridView dataGridShowTable;

    private Label labelShowTableTitle;

    private Button btnReadTable;

    private Button btnWriteTable;

    private Button btnImportTable;

    private Button btnExportTable;

    public FormTable()
    {
        InitializeComponent();
    }

    public FormTable(GuiGroup group, IQComm comm, string itemTag, bool bAW)
    {
        InitializeComponent();
        guiGroup = group;
        guiItem = group.FindItemByName(itemTag);
        m_CameraComm = comm;
        m_bAutoWrite = bAW;
        labelShowTableTitle.Text = guiGroup.Name + " --> " + guiItem.Text;
        Text = guiGroup.Name;
        dataGridShowTable.ColumnCount = guiItem.XSize;
        dataGridShowTable.RowCount = guiItem.YSize;
        dataGridShowTable.AllowUserToAddRows = false;
        if (guiItem.Text == "Manual.Luma.Lut" || guiItem.Text == "Manual.Hue.Lut" || guiItem.Text == "Manual.Sat.Lut" || guiItem.Text == "Auto.Luma.Lut" || guiItem.Text == "Auto.Hue.Lut" || guiItem.Text == "Auto.Sat.Lut")
        {
            Color[] array = new Color[16]
            {
                Color.FromArgb(128, 128, 128),
                Color.FromArgb(255, 0, 0),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(0, 0, 255),
                Color.FromArgb(0, 255, 255),
                Color.FromArgb(255, 0, 255),
                Color.FromArgb(255, 255, 0),
                Color.FromArgb(230, 160, 50),
                Color.FromArgb(160, 200, 30),
                Color.FromArgb(150, 210, 200),
                Color.FromArgb(200, 120, 40),
                Color.FromArgb(172, 65, 50),
                Color.FromArgb(165, 120, 120),
                Color.FromArgb(191, 170, 155),
                Color.FromArgb(183, 123, 123),
                Color.FromArgb(200, 150, 100)
            };
            for (int i = 0; i < guiItem.XSize; i++)
            {
                if (i < 16)
                {
                    dataGridShowTable.EnableHeadersVisualStyles = false;
                    dataGridShowTable.Columns[i].HeaderCell.Style.BackColor = array[i];
                }
            }
        }
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
            for (int j = 0; j < dataGridShowTable.Columns.Count; j++)
            {
                dataGridShowTable.Columns[j].Width = m_nGridCellW;
            }
        }
        for (int k = 0; k < dataGridShowTable.Columns.Count; k++)
        {
            dataGridShowTable.Columns[k].HeaderText = (k + 1).ToString();
            dataGridShowTable.Columns[k].SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        for (int l = 0; l < dataGridShowTable.Rows.Count; l++)
        {
            dataGridShowTable.Rows[l].HeaderCell.Value = l.ToString();
        }
        if (guiItem.Paramters != "")
        {
            string[] array2 = guiItem.Paramters.Split(':');
            if (array2[0] != "")
            {
                string[] array3 = array2[0].Split(',');
                int num = 0;
                for (int m = 0; m < array3.Length; m++)
                {
                    if (array3[m].Contains("(") && array3[m].Contains(")"))
                    {
                        string text = array3[m].Substring(0, array3[m].IndexOf('('));
                        string[] array4 = array3[m].Substring(array3[m].IndexOf('(') + 1, array3[m].IndexOf(')') - array3[m].IndexOf('(') - 1).Split('/');
                        string[] array5 = array4[0].Split('~');
                        for (int n = int.Parse(array5[0]); n <= int.Parse(array5[1]); n += int.Parse(array4[1]))
                        {
                            if (num < dataGridShowTable.Columns.Count)
                            {
                                dataGridShowTable.Columns[num++].HeaderText = text + n;
                            }
                        }
                    }
                    else if (num < dataGridShowTable.Columns.Count)
                    {
                        dataGridShowTable.Columns[num++].HeaderText = array3[m];
                    }
                }
            }
            if (array2[1] != "")
            {
                string[] array6 = array2[1].Split(',');
                int num2 = 0;
                for (int num3 = 0; num3 < array6.Length; num3++)
                {
                    if (array6[num3].Contains("(") && array6[num3].Contains(")"))
                    {
                        string text2 = array6[num3].Substring(0, array6[num3].IndexOf('('));
                        string[] array7 = array6[num3].Substring(array6[num3].IndexOf('(') + 1, array6[num3].IndexOf(')') - array6[num3].IndexOf('(') - 1).Split('/');
                        string[] array8 = array7[0].Split('~');
                        for (int num4 = int.Parse(array8[0]); num4 <= int.Parse(array8[1]); num4 += int.Parse(array7[1]))
                        {
                            if (num2 < dataGridShowTable.Rows.Count)
                            {
                                dataGridShowTable.Rows[num2++].HeaderCell.Value = text2 + num4;
                            }
                        }
                    }
                    else if (num2 < dataGridShowTable.Rows.Count)
                    {
                        dataGridShowTable.Columns[num2++].HeaderText = array6[num3];
                    }
                }
            }
        }
        dataGridShowTable.Width = dataGridShowTable.RowHeadersWidth + m_nGridCellW * dataGridShowTable.Columns.Count + 150;
        dataGridShowTable.Height = dataGridShowTable.ColumnHeadersHeight + dataGridShowTable.Rows[0].Height * dataGridShowTable.Rows.Count + 2;
        int num5 = dataGridShowTable.Location.X;
        int num6 = dataGridShowTable.Width + dataGridShowTable.Location.X * 2 + (Size.Width - ClientSize.Width);
        int num7 = dataGridShowTable.Location.Y + dataGridShowTable.Height + num5 + btnReadTable.Height + (Size.Height - ClientSize.Height);
        if (num6 > m_nDefaultFormWdith)
        {
            Width = m_nDefaultFormWdith;
        }
        else if (num6 < labelShowTableTitle.Width + labelShowTableTitle.Location.X * 2)
        {
            Width = labelShowTableTitle.Width + labelShowTableTitle.Location.X * 2 + (Size.Width - ClientSize.Width);
        }
        else
        {
            Width = num6;
        }
        dataGridShowTable.Width = ClientSize.Width - dataGridShowTable.Location.X * 2;
        if (num7 > m_nDefaultFormHeight)
        {
            Height = m_nDefaultFormHeight;
        }
        else
        {
            Height = num7;
        }
        dataGridShowTable.Height = Height - (dataGridShowTable.Location.Y + (Size.Height - ClientSize.Height) + num5 + btnReadTable.Height);
        btnReadTable.Location = new Point(btnReadTable.Location.X, dataGridShowTable.Location.Y + dataGridShowTable.Height + num5 / 2);
        btnWriteTable.Location = new Point(btnWriteTable.Location.X, dataGridShowTable.Location.Y + dataGridShowTable.Height + num5 / 2);
        for (int num8 = 0; num8 < dataGridShowTable.Rows.Count; num8++)
        {
            for (int num9 = 0; num9 < dataGridShowTable.Rows[num8].Cells.Count; num9++)
            {
                dataGridShowTable.Rows[num8].Cells[num9].Value = 0;
            }
        }
        UpdateTableValue();
        m_bWrite = false;
        m_bInitOk = true;
    }

    protected void UpdateTableValue()
    {
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
        MessageBox.Show(strMsg, "Status", MessageBoxButtons.OK);
    }

    public void ReadTableTask(object obj)
    {
        if (guiGroup.ReadGroup(m_CameraComm).Equals(""))
        {
            Invoke(new UpdateTableHandler(UpdateTableValue));
            return;
        }
        Invoke(new ShowMessageHandler(ShowMessageBox), "Read table failed");
    }

    public void SendTableTask(object obj)
    {
        if (guiGroup.WriteGroup(m_CameraComm) != "")
        {
            Invoke(new ShowMessageHandler(ShowMessageBox), "Write table failed");
        }
    }

    protected void WriteTable()
    {
        long[] dataValue = guiItem.DataValue;
        int num = 0;
        if (guiGroup.Action == API_ACTION.RW || guiGroup.Action == API_ACTION.W)
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
            m_CameraComm.updateTaskQueue(ReadTableTask, null);
        }
    }

    private void btnWriteTable_Click(object sender, EventArgs e)
    {
        WriteTable();
        tagName = guiItem.Tag;
        CellValue = new long[guiItem.XSize * guiItem.YSize];
        for (int i = 0; i < guiItem.YSize; i++)
        {
            for (int j = 0; j < guiItem.XSize; j++)
            {
                CellValue[j + i * guiItem.XSize] = Convert.ToInt32(dataGridShowTable.Rows[i].Cells[j].Value);
            }
        }
        undoredoParam.btnRecordParameter(tagName, 0, CellValue);
    }

    private void btnImportTable_Click(object sender, EventArgs e)
    {
        string text = "";
        string text2 = "";
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "data file (*.txt)|*.txt|data file (*.lut)|*.lut";
        openFileDialog.Title = "Import Table Params";
        if (openFileDialog.ShowDialog() == DialogResult.OK && openFileDialog.FileName != "")
        {
            text = openFileDialog.FileName;
            text2 = ImportTableParams(openFileDialog.FileName);
            UpdateTableValue();
        }
        if (!text.Equals(""))
        {
            if (!text2.Equals(""))
            {
                MessageBox.Show(text2, "Import Table", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                MessageBox.Show("Import Success", "Status", MessageBoxButtons.OK);
            }
        }
    }

    private string ImportTableParams(string strFileName)
    {
        try
        {
            using (FileStream stream = File.Open(strFileName, FileMode.Open, FileAccess.Read))
            {
                using StreamReader streamReader = new StreamReader(stream);
                int num = 0;
                while (!streamReader.EndOfStream)
                {
                    string[] array = streamReader.ReadLine().Split(',');
                    if (array.Length == guiItem.XSize)
                    {
                        long[] array2 = Array.ConvertAll(array, (s) => long.Parse(s));
                        array2.CopyTo(guiItem.DataValue, num);
                        num += array2.Length;
                        continue;
                    }
                    return "invalid format: column number is not equal";
                }
            }
            return "";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private string ExportTableParams(string strFileName)
    {
        StreamWriter streamWriter = new StreamWriter(strFileName);
        for (int i = 0; i < dataGridShowTable.Rows.Count; i++)
        {
            for (int j = 0; j < dataGridShowTable.Rows[i].Cells.Count; j++)
            {
                streamWriter.Write(dataGridShowTable.Rows[i].Cells[j].Value);
                if (j != dataGridShowTable.Rows[i].Cells.Count - 1)
                {
                    streamWriter.Write(",");
                }
            }
            if (i != dataGridShowTable.Rows.Count - 1)
            {
                streamWriter.Write("\n");
            }
        }
        streamWriter.Close();
        return "";
    }

    private void FormTable_FormClosing(object sender, FormClosingEventArgs e)
    {
        dataGridShowTable.EndEdit();
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

    private void btnExportTable_Click(object sender, EventArgs e)
    {
        string text = "";
        string text2 = "";
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "data file (*.txt)|*.txt|data file (*.lut)|*.lut";
        saveFileDialog.Title = "Export Table Params";
        if (saveFileDialog.ShowDialog() == DialogResult.OK && saveFileDialog.FileName != "")
        {
            text = saveFileDialog.FileName;
            text2 = ExportTableParams(saveFileDialog.FileName);
        }
        if (!text.Equals(""))
        {
            if (!text2.Equals(""))
            {
                MessageBox.Show(text2, "Export Table", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                MessageBox.Show("Export Success", "Status", MessageBoxButtons.OK);
            }
        }
    }

    public static int CompareMeth(KeyValuePair<int, int> a, KeyValuePair<int, int> b)
    {
        if (a.Key < b.Key)
        {
            return -1;
        }
        if (a.Key == b.Key && a.Value < b.Value)
        {
            return -1;
        }
        return 1;
    }

    private void FormTable_KeyDown(object sender, KeyEventArgs e)
    {
        if (!e.Control || e.KeyCode != Keys.V)
        {
            return;
        }
        try
        {
            IDataObject dataObject = Clipboard.GetDataObject();
            string text = "";
            if (dataObject.GetDataPresent(DataFormats.Text) | dataObject.GetDataPresent(DataFormats.OemText))
            {
                text = (string)dataObject.GetData(DataFormats.Text);
            }
            if (text == "")
            {
                return;
            }
            string[] array = text.Split('\n');
            List<List<int>> list = new List<List<int>>();
            for (int i = 0; i < array.Count(); i++)
            {
                string[] array2 = Regex.Split(array[i], "[ \\s]+");
                List<int> list2 = new List<int>();
                for (int j = 0; j < array2.Count(); j++)
                {
                    if (int.TryParse(array2[j], out var result))
                    {
                        list2.Add(result);
                    }
                }
                list.Add(list2);
            }
            if (list.Count() == 0)
            {
                return;
            }
            _ = list.Count;
            int cellCount = dataGridShowTable.GetCellCount(DataGridViewElementStates.Selected);
            List<KeyValuePair<int, int>> list3 = new List<KeyValuePair<int, int>>();
            for (int k = 0; k < cellCount; k++)
            {
                int rowIndex = dataGridShowTable.SelectedCells[k].RowIndex;
                int columnIndex = dataGridShowTable.SelectedCells[k].ColumnIndex;
                list3.Add(new KeyValuePair<int, int>(rowIndex, columnIndex));
            }
            if (list3.Count() == 0)
            {
                return;
            }
            list3.Sort(CompareMeth);
            int num = 0;
            List<KeyValuePair<int, int>> list4 = new List<KeyValuePair<int, int>>();
            List<List<KeyValuePair<int, int>>> list5 = new List<List<KeyValuePair<int, int>>>();
            for (int l = 0; l < list3.Count(); l++)
            {
                if (l == 0)
                {
                    num = list3[l].Key;
                }
                else if (num != list3[l].Key)
                {
                    list5.Add(list4);
                    num = list3[l].Key;
                    list4 = new List<KeyValuePair<int, int>>();
                }
                list4.Add(list3[l]);
            }
            if (list4.Count() > 0)
            {
                list5.Add(list4);
            }
            int num2 = 0;
            for (int m = 0; m < list5.Count(); m++)
            {
                for (int n = 0; n < list5[m].Count(); n++)
                {
                    int rowIndex = list5[m][n].Key;
                    int columnIndex = list5[m][n].Value;
                    if (m < list.Count() && n < list[m].Count())
                    {
                        num2 = 1;
                        dataGridShowTable.Rows[rowIndex].Cells[columnIndex].Value = list[m][n];
                    }
                    else
                    {
                        dataGridShowTable.Rows[rowIndex].Cells[columnIndex].Value = 0;
                    }
                }
            }
            if (num2 == 1)
            {
                WriteTable();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
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
        dataGridShowTable = new DataGridView();
        labelShowTableTitle = new Label();
        btnReadTable = new Button();
        btnWriteTable = new Button();
        btnImportTable = new Button();
        btnExportTable = new Button();
        ((ISupportInitialize)dataGridShowTable).BeginInit();
        SuspendLayout();
        dataGridShowTable.AllowUserToAddRows = false;
        dataGridShowTable.AllowUserToDeleteRows = false;
        dataGridShowTable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dataGridShowTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridShowTable.BackgroundColor = Color.WhiteSmoke;
        dataGridShowTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridShowTable.Location = new Point(12, 68);
        dataGridShowTable.Name = "dataGridShowTable";
        dataGridShowTable.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
        dataGridShowTable.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dataGridShowTable.Size = new Size(240, 150);
        dataGridShowTable.TabIndex = 0;
        dataGridShowTable.CellValueChanged += new DataGridViewCellEventHandler(dataGridShowTable_CellValueChanged);
        dataGridShowTable.ColumnAdded += new DataGridViewColumnEventHandler(dataGridShowTable_ColumnAdded);
        labelShowTableTitle.AutoSize = true;
        labelShowTableTitle.Font = new Font("Times New Roman", 10f, FontStyle.Regular, GraphicsUnit.Point, 136);
        labelShowTableTitle.Location = new Point(14, 31);
        labelShowTableTitle.Name = "labelShowTableTitle";
        labelShowTableTitle.Size = new Size(145, 20);
        labelShowTableTitle.TabIndex = 1;
        labelShowTableTitle.Text = "labelShowTableTitle";
        btnReadTable.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnReadTable.Location = new Point(14, 360);
        btnReadTable.Margin = new Padding(3, 4, 3, 4);
        btnReadTable.Name = "btnReadTable";
        btnReadTable.Size = new Size(51, 23);
        btnReadTable.TabIndex = 2;
        btnReadTable.Text = "Read";
        btnReadTable.UseVisualStyleBackColor = true;
        btnReadTable.Click += new EventHandler(btnReadTable_Click);
        btnWriteTable.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnWriteTable.Location = new Point(71, 360);
        btnWriteTable.Name = "btnWriteTable";
        btnWriteTable.Size = new Size(52, 23);
        btnWriteTable.TabIndex = 3;
        btnWriteTable.Text = "Write";
        btnWriteTable.UseVisualStyleBackColor = true;
        btnWriteTable.Click += new EventHandler(btnWriteTable_Click);
        btnImportTable.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnImportTable.Location = new Point(356, 369);
        btnImportTable.Name = "btnImportTable";
        btnImportTable.Size = new Size(57, 23);
        btnImportTable.TabIndex = 4;
        btnImportTable.Text = "Import";
        btnImportTable.UseVisualStyleBackColor = true;
        btnImportTable.Click += new EventHandler(btnImportTable_Click);
        btnExportTable.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnExportTable.Cursor = Cursors.Default;
        btnExportTable.Location = new Point(293, 369);
        btnExportTable.Name = "btnExportTable";
        btnExportTable.Size = new Size(57, 23);
        btnExportTable.TabIndex = 5;
        btnExportTable.Text = "Export";
        btnExportTable.UseVisualStyleBackColor = true;
        btnExportTable.Click += new EventHandler(btnExportTable_Click);
        AutoScaleDimensions = new SizeF(7f, 15f);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        ClientSize = new Size(425, 398);
        Controls.Add(btnExportTable);
        Controls.Add(btnImportTable);
        Controls.Add(btnWriteTable);
        Controls.Add(btnReadTable);
        Controls.Add(labelShowTableTitle);
        Controls.Add(dataGridShowTable);
        Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        KeyPreview = true;
        Margin = new Padding(3, 4, 3, 4);
        MinimumSize = new Size(200, 200);
        Name = "FormTable";
        Text = "FormTable";
        FormClosing += new FormClosingEventHandler(FormTable_FormClosing);
        KeyDown += new KeyEventHandler(FormTable_KeyDown);
        ((ISupportInitialize)dataGridShowTable).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
