using SSCamIQTool.LibComm;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public class FormConvert : Form
{
    private string SrcXml;

    private string SrcBin;

    private string[] MultiSrcBin;

    private string[] MultiSrcXml;

    private IQComm m_DbgParser;

    public ChipID m_chipID = ChipID.I3;

    public ApiVersion apiVersion;

    private string m_CvtDesFile = "";

    private GuiParser m_GuiContainer = new GuiParser();

    private FormConvertList formcvtlist = new FormConvertList();

    private IContainer components = null;

    private Button btnConvert;

    private TreeView SrctreePageItem;

    private Panel panelSrcContent;

    private Panel panelDesContent;

    private Button btnSelSrc;

    private Label labelSrcTitle;

    private Label labelDesTitle;

    private TreeView DestreePageItem;

    private StatusStrip statusStrip1;

    private ToolStripStatusLabel StatusLabelInfo;

    private TextBox txbMsgLog;

    private Label label1;

    private Label label2;

    private Label labelFileTitle;

    private TreeView FiletreePageItem;

    public FormConvert()
    {
        InitializeComponent();
    }

    public void SetPageActionMode(PAGE_ACTION action)
    {
    }

    public void SetAutoWriteMode(bool Mode)
    {
    }

    public void SetBackStepMode(bool mode)
    {
    }

    public void SetNextStepMode(bool mode)
    {
    }

    private void SaveCvtDesTunningParamters(string Src)
    {
        string text = "";
        if (Src == "")
        {
            MessageBox.Show("Please Select Conver Source File (bin or xml).", "Conver Source File", MessageBoxButtons.OK);
            return;
        }
        string text2 = "";
        if (Src != "")
        {
            text2 = Path.GetDirectoryName(Src) + "\\" + Path.GetFileNameWithoutExtension(Src) + "_ConverTo_" + formcvtlist.comboBoxCvtXml.SelectedItem.ToString() + ".xml";
        }
        text = m_GuiContainer.SaveCvtDesXml(text2);
        if (!text2.Equals("") && !text.Equals(""))
        {
            text = "Save File " + text2 + " Error!!";
            MessageBox.Show(text, "Save Parameters File", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
    }

    private void SaveCvtDesToBin()
    {
        if (SrcBin == "" && SrcXml == "")
        {
            MessageBox.Show("Please Select Conver Source File (bin or xml).", "Conver Source File", MessageBoxButtons.OK);
            return;
        }
        string text = "";
        if (SrcBin != "" && SrcXml == "")
        {
            text = Path.GetDirectoryName(SrcBin) + "\\" + Path.GetFileNameWithoutExtension(SrcBin) + "_ConverTo_" + formcvtlist.comboBoxCvtXml.SelectedItem.ToString() + ".bin";
        }
        else if (SrcXml != "" && SrcBin == "")
        {
            text = Path.GetDirectoryName(SrcXml) + "\\" + Path.GetFileNameWithoutExtension(SrcXml) + "_ConverTo_" + formcvtlist.comboBoxCvtXml.SelectedItem.ToString() + ".bin";
        }
        uint magicKey = 1234u;
        byte[] cvtDesBinBytes = m_GuiContainer.GetCvtDesBinBytes(m_chipID, magicKey, m_GuiContainer.m_BinChecksumVer);
        BinaryWriter binaryWriter = new BinaryWriter(File.Open(text, FileMode.Create));
        binaryWriter.Write(cvtDesBinBytes);
        binaryWriter.Close();
        if (!text.Equals(""))
        {
            MessageBox.Show("Save File " + text + " Success.", "Save Parameters File", MessageBoxButtons.OK);
        }
    }

    public void CompressGZipfile(string src, string zip)
    {
        FileStream fileStream = File.OpenRead(src);
        FileStream fileStream2 = File.Create(zip);
        GZipStream gZipStream = new GZipStream(fileStream2, CompressionMode.Compress);
        byte[] array = new byte[2048];
        int count;
        while ((count = fileStream.Read(array, 0, array.Length)) != 0)
        {
            gZipStream.Write(array, 0, count);
        }
        fileStream.Close();
        gZipStream.Close();
        fileStream2.Close();
    }

    private void SaveCvtDesToBinXml(string Src)
    {
        string text = "";
        if (Src == "")
        {
            MessageBox.Show("Please Select Conver Source Bin File.", "Conver Source File", MessageBoxButtons.OK);
            return;
        }
        text = Path.GetDirectoryName(Src) + "\\" + Path.GetFileNameWithoutExtension(Src) + "_ConverTo_" + formcvtlist.comboBoxCvtXml.SelectedItem.ToString() + ".bin";
        try
        {
            uint magicKey = 1234u;
            uint videomagicKey = 2036624993u;
            string text2 = Application.StartupPath + "\\XmlTmp.xml";
            string text3 = Application.StartupPath + "\\XmlTmp.zip";
            if (File.Exists(text2))
            {
                File.Delete(text2);
            }
            if (File.Exists(text3))
            {
                File.Delete(text3);
            }
            m_GuiContainer.SaveCvtDesXml(text2);
            m_GuiContainer.GetChipIDByXml(text2);
            CompressGZipfile(text2, text3);
            byte[] array = !(m_GuiContainer.m_binVersion == "0.0") ? m_GuiContainer.GetCvtDesBinBytesXmlCRC(m_GuiContainer.XmlChipID, magicKey, videomagicKey, text3, m_GuiContainer.m_BinChecksumVer) : m_GuiContainer.GetCvtDesBinBytes(m_GuiContainer.XmlChipID, magicKey, m_GuiContainer.m_BinChecksumVer);
            byte[] array2 = File.ReadAllBytes(text3);
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(text, FileMode.Create));
            byte[] array3 = new byte[array.Length + array2.Length];
            array.CopyTo(array3, 0);
            array2.CopyTo(array3, array.Length);
            binaryWriter.Write(array3);
            binaryWriter.Close();
            if (File.Exists(text2))
            {
                File.Delete(text2);
            }
            if (File.Exists(text3))
            {
                File.Delete(text3);
            }
        }
        catch (Exception)
        {
        }
        if (text.Equals(""))
        {
            MessageBox.Show("Save File " + text + " Fail.", "Save Parameters File", MessageBoxButtons.OK);
        }
    }

    public string ReadXMLGroup(IQComm comm, FILE_TRANSFER_ID type, string srcpath)
    {
        string result = "";
        short apiId = (short)type;
        if (comm.ReceiveXMLApiPacket(this, apiId, out var pbyRcvData) > 0)
        {
            byte[] array = new byte[pbyRcvData.Length - 8];
            for (int i = 8; i < pbyRcvData.Length; i++)
            {
                array[i - 8] = pbyRcvData[i];
            }
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(srcpath, FileMode.Create));
            binaryWriter.Write(array);
            binaryWriter.Close();
        }
        else
        {
            result = "Warning!!  I3 ISP API XML on board not found. " + Name + " receive packet fail !! ";
        }
        return result;
    }

    private void GetXML()
    {
        string text = m_CvtDesFile = Application.StartupPath + "\\XmlOnBoard.xml";
        string text2 = ReadXMLGroup(m_DbgParser, FILE_TRANSFER_ID.ISP_XML_DATA, text);
        if (!text2.Equals(""))
        {
            MessageBox.Show(text2, "Get Xml On Board", MessageBoxButtons.OK);
        }
        else
        {
            m_GuiContainer.InitCvtDesGUI(text);
        }
    }

    private void btnConvert_Click(object sender, EventArgs e)
    {
        if (MultiSrcBin == null && MultiSrcXml == null)
        {
            MessageBox.Show("Please Select Conver Source File (bin or xml).", "Conver Source File", MessageBoxButtons.OK);
        }
        else
        {
            if (formcvtlist.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (formcvtlist.radioBtnSelCvtXml.Checked)
            {
                if (formcvtlist.comboBoxCvtXml.Items.Count == 0)
                {
                    MessageBox.Show("Xml in CvtXml Folder Cannot Be Found.", "Conver Source File", MessageBoxButtons.OK);
                    return;
                }
                string text = formcvtlist.comboBoxCvtXml.SelectedItem.ToString();
                string text2 = Application.StartupPath + "\\CvtXml\\" + text + ".xml";
                if (!File.Exists(text2))
                {
                    MessageBox.Show(text2 + " Cannot Be Found.", "Conver Source File", MessageBoxButtons.OK);
                    return;
                }
                m_CvtDesFile = text2;
                m_GuiContainer.InitCvtDesGUI(text2);
            }
            else if (formcvtlist.radioBtnSelXmlOnBoard.Checked)
            {
                GetXML();
            }
            for (int i = 0; i < MultiSrcBin.Length; i++)
            {
                if (m_GuiContainer.LoadCvtSrc(MultiSrcBin[i]))
                {
                    m_GuiContainer.InitFileGUI(i, MultiSrcBin[i]);
                    m_GuiContainer.InitCvtDesGUI(m_CvtDesFile);
                    m_GuiContainer.copyCvtSrcToCvtDes();
                    if (formcvtlist.radioBtnCvtToBin.Checked)
                    {
                        SaveCvtDesToBinXml(MultiSrcBin[i]);
                    }
                    else if (formcvtlist.radioBtnCvtToXml.Checked)
                    {
                        SaveCvtDesTunningParamters(MultiSrcBin[i]);
                    }
                }
            }
            for (int j = 0; j < MultiSrcXml.Length; j++)
            {
                if (m_GuiContainer.LoadCvtSrc(MultiSrcXml[j]))
                {
                    m_GuiContainer.InitFileGUI(j + MultiSrcBin.Length, MultiSrcXml[j]);
                    m_GuiContainer.InitCvtDesGUI(m_CvtDesFile);
                    m_GuiContainer.copyCvtSrcToCvtDes();
                    if (formcvtlist.radioBtnCvtToBin.Checked)
                    {
                        SaveCvtDesToBinXml(MultiSrcXml[j]);
                    }
                    else if (formcvtlist.radioBtnCvtToXml.Checked)
                    {
                        SaveCvtDesTunningParamters(MultiSrcXml[j]);
                    }
                }
            }
        }
    }

    private void SelSrcBin()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Bin file (*.bin)|*.bin";
        openFileDialog.InitialDirectory = Application.StartupPath;
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            m_GuiContainer.LoadCvtSrcBinFile(openFileDialog.FileName);
        }
    }

    private void SelSrcXml()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Parameters file (*.xml)| *.xml| All files (*.*)|*.*";
        openFileDialog.InitialDirectory = Application.StartupPath;
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            SrcXml = openFileDialog.FileName;
            m_GuiContainer.InitCvtSrcGUI(openFileDialog.FileName);
        }
    }

    private void SelSrc()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Parameters file  Bin file  (*.bin)| *.bin| All files (*.*)|*.*";
        openFileDialog.InitialDirectory = Application.StartupPath;
        if (openFileDialog.ShowDialog() != DialogResult.OK || !Path.GetExtension(openFileDialog.FileName).Equals(".bin"))
        {
            return;
        }
        SrcBin = openFileDialog.FileName;
        SrcXml = "";
        if (m_GuiContainer.LoadCvtSrcBinXmlFile(openFileDialog.FileName) != 0)
        {
            m_GuiContainer.GetCvtSrcBinFileVer(openFileDialog.FileName);
            string text = Application.StartupPath + "\\CvtXml\\I3_isp_api_" + m_GuiContainer.BinAPIVerMajor + "_" + m_GuiContainer.BinAPIVerMinor + ".xml";
            if (File.Exists(text))
            {
                m_GuiContainer.InitCvtSrcGUI(text);
                m_GuiContainer.LoadCvtSrcBinFile(openFileDialog.FileName);
            }
            else
            {
                MessageBox.Show(text + " does not exist.");
            }
        }
    }

    private void SelMultiSrc()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Bin file  (*.bin)| *.bin| Parameters file (*.xml)| *.xml| All files (*.*)|*.*";
        openFileDialog.InitialDirectory = Application.StartupPath;
        openFileDialog.Multiselect = true;
        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        int num = 0;
        int num2 = 0;
        for (int i = 0; i < openFileDialog.FileNames.Length; i++)
        {
            if (Path.GetExtension(openFileDialog.FileNames[i]).Equals(".bin"))
            {
                num++;
            }
            if (Path.GetExtension(openFileDialog.FileNames[i]).Equals(".xml"))
            {
                num2++;
            }
        }
        MultiSrcBin = new string[num];
        MultiSrcXml = new string[num2];
        int num3 = 0;
        int num4 = 0;
        for (int j = 0; j < openFileDialog.FileNames.Length; j++)
        {
            if (Path.GetExtension(openFileDialog.FileNames[j]).Equals(".bin"))
            {
                MultiSrcBin[num3++] = openFileDialog.FileNames[j];
                if (m_GuiContainer.LoadCvtSrc(openFileDialog.FileNames[j]))
                {
                    m_GuiContainer.InitFileGUI(j, openFileDialog.FileNames[j]);
                }
            }
            else if (Path.GetExtension(openFileDialog.FileNames[j]).Equals(".xml"))
            {
                MultiSrcXml[num4++] = openFileDialog.FileNames[j];
                if (m_GuiContainer.LoadCvtSrc(openFileDialog.FileNames[j]))
                {
                    m_GuiContainer.InitFileGUI(j, openFileDialog.FileNames[j]);
                }
            }
        }
    }

    private void btnSelSrc_Click(object sender, EventArgs e)
    {
        SelMultiSrc();
    }

    private void SelDesBin()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Bin file (*.bin)|*.bin";
        openFileDialog.InitialDirectory = Application.StartupPath;
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            m_GuiContainer.LoadCvtDesBinFile(openFileDialog.FileName);
        }
    }

    private void SelDesXml()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Parameters file (*.xml)| *.xml| All files (*.*)|*.*";
        openFileDialog.InitialDirectory = Application.StartupPath;
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            m_GuiContainer.InitCvtDesGUI(openFileDialog.FileName);
        }
    }

    private void btnSelDest_Click(object sender, EventArgs e)
    {
    }

    public void SetDebugParser(IQComm debugParser)
    {
        m_DbgParser = debugParser;
    }

    private void FormConvert_Load(object sender, EventArgs e)
    {
        m_GuiContainer.SetCvtSrcLayout(SrctreePageItem, panelSrcContent, labelSrcTitle, StatusLabelInfo);
        m_GuiContainer.SetDebugLog(txbMsgLog);
        m_GuiContainer.SetPageGuiMode(SetPageActionMode, SetAutoWriteMode);
        m_GuiContainer.SetUndoRedoMode(SetBackStepMode, SetNextStepMode);
        m_GuiContainer.SetCvtDesLayout(DestreePageItem, panelDesContent, labelDesTitle, StatusLabelInfo);
        m_GuiContainer.SetDebugLog(txbMsgLog);
        m_GuiContainer.SetPageGuiMode(SetPageActionMode, SetAutoWriteMode);
        m_GuiContainer.SetUndoRedoMode(SetBackStepMode, SetNextStepMode);
        m_GuiContainer.SetFileLayout(FiletreePageItem, null, labelFileTitle, StatusLabelInfo);
        if (m_DbgParser.IsConnected())
        {
            formcvtlist.radioBtnSelXmlOnBoard.Enabled = true;
        }
        else
        {
            formcvtlist.radioBtnSelXmlOnBoard.Enabled = false;
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
        TreeNode treeNode = new TreeNode("Node0");
        TreeNode treeNode2 = new TreeNode("Node0");
        TreeNode treeNode3 = new TreeNode("Node0");
        btnConvert = new Button();
        SrctreePageItem = new TreeView();
        panelSrcContent = new Panel();
        panelDesContent = new Panel();
        btnSelSrc = new Button();
        labelSrcTitle = new Label();
        labelDesTitle = new Label();
        DestreePageItem = new TreeView();
        statusStrip1 = new StatusStrip();
        StatusLabelInfo = new ToolStripStatusLabel();
        txbMsgLog = new TextBox();
        label1 = new Label();
        label2 = new Label();
        labelFileTitle = new Label();
        FiletreePageItem = new TreeView();
        statusStrip1.SuspendLayout();
        SuspendLayout();
        btnConvert.Location = new System.Drawing.Point(1084, 7);
        btnConvert.Name = "btnConvert";
        btnConvert.Size = new System.Drawing.Size(75, 23);
        btnConvert.TabIndex = 5;
        btnConvert.Text = "Convert";
        btnConvert.UseVisualStyleBackColor = true;
        btnConvert.Click += new EventHandler(btnConvert_Click);
        SrctreePageItem.Font = new System.Drawing.Font("Times New Roman", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        SrctreePageItem.Location = new System.Drawing.Point(183, 34);
        SrctreePageItem.Margin = new Padding(3, 4, 3, 4);
        SrctreePageItem.Name = "SrctreePageItem";
        treeNode.Name = "Node0";
        treeNode.Text = "Node0";
        SrctreePageItem.Nodes.AddRange(new TreeNode[1] { treeNode });
        SrctreePageItem.Size = new System.Drawing.Size(156, 293);
        SrctreePageItem.TabIndex = 6;
        panelSrcContent.AutoScroll = true;
        panelSrcContent.BackColor = System.Drawing.SystemColors.Window;
        panelSrcContent.Location = new System.Drawing.Point(345, 34);
        panelSrcContent.Margin = new Padding(3, 4, 3, 4);
        panelSrcContent.Name = "panelSrcContent";
        panelSrcContent.Padding = new Padding(20);
        panelSrcContent.Size = new System.Drawing.Size(814, 293);
        panelSrcContent.TabIndex = 7;
        panelDesContent.AutoScroll = true;
        panelDesContent.BackColor = System.Drawing.SystemColors.Window;
        panelDesContent.Location = new System.Drawing.Point(345, 357);
        panelDesContent.Margin = new Padding(3, 4, 3, 4);
        panelDesContent.Name = "panelDesContent";
        panelDesContent.Padding = new Padding(20);
        panelDesContent.Size = new System.Drawing.Size(814, 293);
        panelDesContent.TabIndex = 8;
        btnSelSrc.Location = new System.Drawing.Point(1003, 7);
        btnSelSrc.Name = "btnSelSrc";
        btnSelSrc.Size = new System.Drawing.Size(75, 23);
        btnSelSrc.TabIndex = 10;
        btnSelSrc.Text = "Select Src";
        btnSelSrc.UseVisualStyleBackColor = true;
        btnSelSrc.Click += new EventHandler(btnSelSrc_Click);
        labelSrcTitle.AutoSize = true;
        labelSrcTitle.Font = new System.Drawing.Font("Times New Roman", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        labelSrcTitle.ForeColor = System.Drawing.Color.Blue;
        labelSrcTitle.Location = new System.Drawing.Point(565, 4);
        labelSrcTitle.Name = "labelSrcTitle";
        labelSrcTitle.Size = new System.Drawing.Size(69, 22);
        labelSrcTitle.TabIndex = 12;
        labelSrcTitle.Text = "Group";
        labelDesTitle.AutoSize = true;
        labelDesTitle.Font = new System.Drawing.Font("Times New Roman", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        labelDesTitle.ForeColor = System.Drawing.Color.Blue;
        labelDesTitle.Location = new System.Drawing.Point(565, 331);
        labelDesTitle.Name = "labelDesTitle";
        labelDesTitle.Size = new System.Drawing.Size(69, 22);
        labelDesTitle.TabIndex = 13;
        labelDesTitle.Text = "Group";
        DestreePageItem.Font = new System.Drawing.Font("Times New Roman", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        DestreePageItem.Location = new System.Drawing.Point(183, 357);
        DestreePageItem.Margin = new Padding(3, 4, 3, 4);
        DestreePageItem.Name = "DestreePageItem";
        treeNode2.Name = "Node0";
        treeNode2.Text = "Node0";
        DestreePageItem.Nodes.AddRange(new TreeNode[1] { treeNode2 });
        DestreePageItem.Size = new System.Drawing.Size(156, 293);
        DestreePageItem.TabIndex = 14;
        statusStrip1.Items.AddRange(new ToolStripItem[1] { StatusLabelInfo });
        statusStrip1.Location = new System.Drawing.Point(0, 728);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new System.Drawing.Size(1176, 22);
        statusStrip1.TabIndex = 15;
        statusStrip1.Text = "statusStrip1";
        StatusLabelInfo.Name = "StatusLabelInfo";
        StatusLabelInfo.Size = new System.Drawing.Size(42, 17);
        StatusLabelInfo.Text = "Status";
        txbMsgLog.Location = new System.Drawing.Point(12, 658);
        txbMsgLog.Margin = new Padding(3, 4, 3, 4);
        txbMsgLog.Multiline = true;
        txbMsgLog.Name = "txbMsgLog";
        txbMsgLog.ScrollBars = ScrollBars.Both;
        txbMsgLog.Size = new System.Drawing.Size(1147, 66);
        txbMsgLog.TabIndex = 16;
        label1.AutoSize = true;
        label1.Font = new System.Drawing.Font("Times New Roman", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        label1.ForeColor = System.Drawing.Color.Blue;
        label1.Location = new System.Drawing.Point(183, 4);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(82, 22);
        label1.TabIndex = 17;
        label1.Text = "Source ";
        label2.AutoSize = true;
        label2.Font = new System.Drawing.Font("Times New Roman", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        label2.ForeColor = System.Drawing.Color.Blue;
        label2.Location = new System.Drawing.Point(183, 331);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(115, 22);
        label2.TabIndex = 18;
        label2.Text = "Destination";
        labelFileTitle.AutoSize = true;
        labelFileTitle.Font = new System.Drawing.Font("Times New Roman", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
        labelFileTitle.ForeColor = System.Drawing.Color.Blue;
        labelFileTitle.Location = new System.Drawing.Point(12, 4);
        labelFileTitle.Name = "labelFileTitle";
        labelFileTitle.Size = new System.Drawing.Size(43, 22);
        labelFileTitle.TabIndex = 19;
        labelFileTitle.Text = "File";
        FiletreePageItem.Font = new System.Drawing.Font("Times New Roman", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        FiletreePageItem.Location = new System.Drawing.Point(12, 34);
        FiletreePageItem.Margin = new Padding(3, 4, 3, 4);
        FiletreePageItem.Name = "FiletreePageItem";
        treeNode3.Name = "Node0";
        treeNode3.Text = "Node0";
        FiletreePageItem.Nodes.AddRange(new TreeNode[1] { treeNode3 });
        FiletreePageItem.Size = new System.Drawing.Size(165, 616);
        FiletreePageItem.TabIndex = 20;
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1176, 750);
        Controls.Add(FiletreePageItem);
        Controls.Add(labelFileTitle);
        Controls.Add(label2);
        Controls.Add(label1);
        Controls.Add(txbMsgLog);
        Controls.Add(statusStrip1);
        Controls.Add(DestreePageItem);
        Controls.Add(labelDesTitle);
        Controls.Add(labelSrcTitle);
        Controls.Add(btnSelSrc);
        Controls.Add(panelDesContent);
        Controls.Add(panelSrcContent);
        Controls.Add(SrctreePageItem);
        Controls.Add(btnConvert);
        Name = "FormConvert";
        Text = "FormConvert";
        Load += new EventHandler(FormConvert_Load);
        statusStrip1.ResumeLayout(false);
        statusStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
