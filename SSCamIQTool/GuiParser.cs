using SSCamIQTool.LibComm;
using SSCamIQTool.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Application = System.Windows.Forms.Application;

namespace SSCamIQTool.SSCamIQTool;

public class GuiParser
{
    public delegate void DelegateMethod();

    public delegate void SetMode(bool mode);

    public delegate void SetAction(PAGE_ACTION action);

    private delegate void UpdateTransFinishHandler(string strMsg);

    private delegate void UpdateTransProgressByValueHandler(int value, double speed);

    private delegate void ShowMessageHandler(string msgBox, string msgText);

    private delegate void UpdateGUIHandler();

    private XmlDocument m_xmlDoc = new XmlDocument();

    private XmlDocument m_xmlCacheDoc = new XmlDocument();

    private List<GuiPage> m_GuiPageCacheSavedOne = new List<GuiPage>();

    private List<GuiPage> m_GuiPageCacheSavedTwo = new List<GuiPage>();

    private List<int> m_CuiPageCurrentSaved = new List<int>();

    private List<GuiPage> m_GuiPageArray = new List<GuiPage>();

    private List<GuiPage> m_GuiPageSaved = new List<GuiPage>();

    private List<GuiPage> m_GuiPageRollback = new List<GuiPage>();

    private List<List<Control>>[] m_PageControlArray;

    private ActionList actionList = new ActionList();

    private GroupBox[][] m_pGroupBoxArray;

    private string m_GuiRootName = "";

    private Panel m_ContentPanel;

    private Label m_ContentTitle;

    private TreeView m_treePageItem;

    private TextBox m_textMsgLog;

    private ToolStripStatusLabel m_labelItemInfo;

    public List<GuiPage> m_CvtSrcGuiPageArray = new List<GuiPage>();

    private List<List<Control>> m_CvtSrcPageControlArray = new List<List<Control>>();

    private GroupBox[] m_pCvtSrcGroupBoxArray;

    private Panel m_CvtSrcContentPanel;

    private Label m_CvtSrcContentTitle;

    private TreeView m_CvtSrctreePageItem;

    private ToolStripStatusLabel m_CvtSrclabelItemInfo;

    public List<GuiPage> m_CvtDesGuiPageArray = new List<GuiPage>();

    private List<List<Control>> m_CvtDesPageControlArray = new List<List<Control>>();

    private GroupBox[] m_pCvtDesGroupBoxArray;

    private Panel m_CvtDesContentPanel;

    private Label m_CvtDesContentTitle;

    private TreeView m_CvtDestreePageItem;

    private ToolStripStatusLabel m_CvtDeslabelItemInfo;

    private Panel m_FileContentPanel;

    private Label m_FileContentTitle;

    private TreeView m_FiletreePageItem;

    private ToolStripStatusLabel m_FilelabelItemInfo;

    private DialogProgress m_dialogProgress;

    private IQComm m_DbgParser;

    private bool m_bAutoWrite = true;

    private bool m_noAutoWrite;

    private bool m_bIsCreatingGui = true;

    private int m_CurrPageIndex;

    public DelegateMethod CheckConnection;

    private SetAction SetPageActionMode;

    private SetMode SetAutoWriteMode;

    private SetMode SetBackStepMode;

    private SetMode SetNextStepMode;

    private int recvLengthForCaliSpeed;

    public ushort BinAPIVerMajor;

    public ushort BinAPIVerMinor;

    public ushort m_BinChecksumVer;

    public bool APIVerIsMatch;

    public string m_ispVersion = "";

    private ushort EvbAPIVerMajor;

    private ushort EvbAPIVerMinor;

    public string m_videoVersion = "";

    public ChipID BinChipID;

    public ushort XmlAPIVerMajor;

    public ushort XmlAPIVerMinor;

    public bool isFWAPIParmChange;

    public ChipID XmlChipID;

    public string m_binVersion = "";

    private const int FONT_SIZE = 9;

    private string chipname;

    private string Binchipname;

    private string Xmlchipname;

    public TreeNode m_previousSelectedNode;

    public static Bitmap GetImageByName(string image)
    {
        string name = "SSCamIQTool.Resources." + image;
        return new(Assembly.GetExecutingAssembly().GetManifestResourceStream(name));
    }

    public static Icon GetIconByName(string image)
    {
        string name = "SSCamIQTool.Resources." + image;
        return new(Assembly.GetExecutingAssembly().GetManifestResourceStream(name));
    }

    public bool AutoWrite
    {
        get
        {
            return m_bAutoWrite;
        }
        set
        {
            m_bAutoWrite = value;
        }
    }

    private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        TreeView treeView = (TreeView)sender;
        for (int i = 0; i < treeView.Nodes[0].Nodes.Count; i++)
        {
            if (treeView.Nodes[0].Nodes[i].IsSelected)
            {
                m_CurrPageIndex = i;
                CreatePageGUI(m_GuiPageArray[i]);
                if (m_GuiPageArray[i].bRead)
                {
                    UpdateGUI();
                }
                else if (m_DbgParser.IsConnected() && (m_GuiPageArray[i].Action == PAGE_ACTION.R || m_GuiPageArray[i].Action == PAGE_ACTION.RW))
                {
                    ReadPage();
                }
                break;
            }
        }
        if (m_previousSelectedNode != null)
        {
            m_previousSelectedNode.BackColor = m_treePageItem.BackColor;
            m_previousSelectedNode.ForeColor = m_treePageItem.ForeColor;
        }
        e.Node.BackColor = SystemColors.Highlight;
        e.Node.ForeColor = Color.White;
        m_previousSelectedNode = m_treePageItem.SelectedNode;
    }

    private void treeView_KeyDown(object sender, KeyEventArgs e)
    {
        TreeView treeView = (TreeView)sender;
        if (treeView.SelectedNode != null && e.Control && e.KeyCode == Keys.C)
        {
            Clipboard.SetText(treeView.SelectedNode.Text);
        }
    }

    private void CvtSrctreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        TreeView treeView = (TreeView)sender;
        for (int i = 0; i < treeView.Nodes[0].Nodes.Count; i++)
        {
            if (treeView.Nodes[0].Nodes[i].IsSelected)
            {
                m_CurrPageIndex = i;
                CreateCvtSrcPageGUI(m_CvtSrcGuiPageArray[i]);
                if (m_CvtSrcGuiPageArray[i].bRead)
                {
                    UpdateCvtSrcGUI();
                }
                break;
            }
        }
        if (m_previousSelectedNode != null)
        {
            m_previousSelectedNode.BackColor = m_treePageItem.BackColor;
            m_previousSelectedNode.ForeColor = m_treePageItem.ForeColor;
        }
    }

    public bool LoadCvtSrc(string file)
    {
        if (Path.GetExtension(file).Equals(".bin"))
        {
            if (LoadCvtSrcBinXmlFile(file) == 0)
            {
                return true;
            }
            GetCvtSrcBinFileVer(file);
            string text = Application.StartupPath + "\\CvtXml\\I3_isp_api_" + BinAPIVerMajor + "_" + BinAPIVerMinor + ".xml";
            if (File.Exists(text))
            {
                InitCvtSrcGUI(text);
                LoadCvtSrcBinFile(file);
                return true;
            }
            MessageBox.Show(text + " does not exist.");
            return false;
        }
        if (Path.GetExtension(file).Equals(".xml"))
        {
            if (LoadCvtSrcXmlFile(file) == 0)
            {
                return true;
            }
            MessageBox.Show(file + " cannot be load.");
            return false;
        }
        return false;
    }

    public void chooseProduct(out string product_style)
    {
        product_style = "";
        FormProduct formProduct = new FormProduct();
        formProduct.ShowDialog();
        formProduct.Focus();
        formProduct.productstyle(out product_style);
    }

    public void copyCvtSrcToCvtDes()
    {
        if (m_CvtSrcGuiPageArray.Count == 0)
        {
            MessageBox.Show("Warning! API of Souce Bin File Not Exist.", "Connection", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return;
        }
        if (m_CvtDesGuiPageArray.Count == 0)
        {
            MessageBox.Show("Warning! API of Destination Bin File Not Exist.", "Connection", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return;
        }
        for (int i = 0; i < m_CvtDesGuiPageArray.Count; i++)
        {
            for (int j = 0; j < m_CvtDesGuiPageArray[i].GroupList.Count; j++)
            {
                for (int k = 0; k < m_CvtSrcGuiPageArray.Count; k++)
                {
                    for (int l = 0; l < m_CvtSrcGuiPageArray[k].GroupList.Count; l++)
                    {
                        if (m_CvtDesGuiPageArray[i].GroupList[j].ID != m_CvtSrcGuiPageArray[k].GroupList[l].ID)
                        {
                            continue;
                        }
                        for (int m = 0; m < m_CvtDesGuiPageArray[i].GroupList[j].ItemList.Count; m++)
                        {
                            for (int n = 0; n < m_CvtSrcGuiPageArray[k].GroupList[l].ItemList.Count; n++)
                            {
                                if (m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].Tag.Equals(m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].Tag) && m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].ValueType == m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].ValueType && m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].XSize == m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].XSize && m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].YSize == m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].YSize)
                                {
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].DataValue = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].DataValue;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].GuiType = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].GuiType;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].Info = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].Info;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].MaxValue = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].MaxValue;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].MinValue = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].MinValue;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].Paramters = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].Paramters;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].Text = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].Text;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].ValueType = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].ValueType;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].XSize = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].XSize;
                                    m_CvtDesGuiPageArray[i].GroupList[j].ItemList[m].YSize = m_CvtSrcGuiPageArray[k].GroupList[l].ItemList[n].YSize;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void FiletreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        TreeView treeView = (TreeView)sender;
        for (int i = 0; i < treeView.Nodes[0].Nodes.Count; i++)
        {
            if (treeView.Nodes[0].Nodes[i].IsSelected && LoadCvtSrc(treeView.Nodes[0].Nodes[i].Text))
            {
                copyCvtSrcToCvtDes();
            }
        }
    }

    private void CvtDestreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        TreeView treeView = (TreeView)sender;
        for (int i = 0; i < treeView.Nodes[0].Nodes.Count; i++)
        {
            if (treeView.Nodes[0].Nodes[i].IsSelected)
            {
                m_CurrPageIndex = i;
                CreateCvtDesPageGUI(m_CvtDesGuiPageArray[i]);
                if (m_CvtDesGuiPageArray[i].bRead)
                {
                    UpdateCvtDesGUI();
                }
                break;
            }
        }
        if (m_previousSelectedNode != null)
        {
            m_previousSelectedNode.BackColor = m_treePageItem.BackColor;
            m_previousSelectedNode.ForeColor = m_treePageItem.ForeColor;
        }
    }

    public void SetLayout(TreeView pageItem, Panel panelContent, Label labelTitle, ToolStripStatusLabel labelInfo)
    {
        m_treePageItem = pageItem;
        m_ContentPanel = panelContent;
        m_ContentTitle = labelTitle;
        m_labelItemInfo = labelInfo;
        m_treePageItem.AfterSelect += treeView_AfterSelect;
        m_treePageItem.KeyDown += treeView_KeyDown;
        m_ContentTitle.Click += guiItem_Click;
        m_ContentTitle.PreviewKeyDown += guiItem_PreviewKeyDown;
    }

    public void SetCvtSrcLayout(TreeView pageItem, Panel panelContent, Label labelTitle, ToolStripStatusLabel labelInfo)
    {
        m_CvtSrctreePageItem = pageItem;
        m_CvtSrcContentPanel = panelContent;
        m_CvtSrcContentTitle = labelTitle;
        m_CvtSrclabelItemInfo = labelInfo;
        m_CvtSrctreePageItem.AfterSelect += CvtSrctreeView_AfterSelect;
    }

    public void SetCvtDesLayout(TreeView pageItem, Panel panelContent, Label labelTitle, ToolStripStatusLabel labelInfo)
    {
        m_CvtDestreePageItem = pageItem;
        m_CvtDesContentPanel = panelContent;
        m_CvtDesContentTitle = labelTitle;
        m_CvtDeslabelItemInfo = labelInfo;
        m_CvtDestreePageItem.AfterSelect += CvtDestreeView_AfterSelect;
    }

    public void SetFileLayout(TreeView pageItem, Panel panelContent, Label labelTitle, ToolStripStatusLabel labelInfo)
    {
        m_FiletreePageItem = pageItem;
        m_FileContentPanel = panelContent;
        m_FileContentTitle = labelTitle;
        m_FilelabelItemInfo = labelInfo;
        m_FiletreePageItem.AfterSelect += FiletreeView_AfterSelect;
    }

    public void SetDebugLog(TextBox textDebug)
    {
        m_textMsgLog = textDebug;
    }

    public void SetDebugParser(IQComm debugParser)
    {
        m_DbgParser = debugParser;
    }

    public void SetCheckConnectionFunc(DelegateMethod pfuncCheckConnection)
    {
        CheckConnection = pfuncCheckConnection;
    }

    public void SetPageGuiMode(SetAction pAMode, SetMode aWMode)
    {
        SetPageActionMode = pAMode;
        SetAutoWriteMode = aWMode;
    }

    public void SetUndoRedoMode(SetMode backMode, SetMode nextMode)
    {
        SetBackStepMode = backMode;
        SetNextStepMode = nextMode;
    }

    private void TransFinish(string strMsg)
    {
        if (strMsg.Equals(""))
        {
            m_dialogProgress.SetProgressFinsh(bSuccess: true);
        }
        else
        {
            m_dialogProgress.SetProgressFinsh(bSuccess: false);
        }
    }

    private void TransProgressByValue(int value, double speed)
    {
        if (m_dialogProgress != null)
        {
            m_dialogProgress.ProgressValue = value;
            m_dialogProgress.NetSpeed = speed;
        }
    }

    public void ShowProgressBar(bool showProgress, string strText)
    {
        m_dialogProgress = new DialogProgress("Connecting", strText);
        if (showProgress)
        {
            m_dialogProgress.ProgressVisible = true;
        }
        else
        {
            m_dialogProgress.ProgressVisible = false;
        }
        m_dialogProgress.btnOKVisible = false;
        m_dialogProgress.ShowDialog();
    }

    public bool InitGUI(string strFileName)
    {
        if (m_ContentPanel == null || m_ContentTitle == null || m_treePageItem == null || m_textMsgLog == null)
        {
            return false;
        }
        if (GetGuiPageListFromXml(strFileName, m_GuiPageArray).Equals(""))
        {
            m_treePageItem.Nodes[0].Text = m_GuiRootName;
            m_treePageItem.Nodes[0].Nodes.Clear();
            for (int i = 0; i < m_GuiPageArray.Count; i++)
            {
                m_treePageItem.Nodes[0].Nodes.Add(m_GuiPageArray[i].Name);
            }
            m_PageControlArray = new List<List<Control>>[m_GuiPageArray.Count];
            m_pGroupBoxArray = new GroupBox[m_GuiPageArray.Count][];
            m_treePageItem.Nodes[0].ExpandAll();
            m_CurrPageIndex = 0;
            CreatePageGUI(m_GuiPageArray[0]);
            m_treePageItem.Focus();
        }
        actionList.Clear();
        SetBackStepMode(mode: false);
        SetNextStepMode(mode: false);
        return true;
    }

    public bool InitFileGUI(int FileIdx, string strFileName)
    {
        if (FileIdx == 0)
        {
            m_FiletreePageItem.Nodes[0].Text = m_GuiRootName;
            m_FiletreePageItem.Nodes[0].Nodes.Clear();
        }
        m_FiletreePageItem.Nodes[0].Nodes.Add(strFileName);
        m_FiletreePageItem.Nodes[0].ExpandAll();
        return true;
    }

    public bool InitCvtSrcGUI(string strFileName)
    {
        if (m_CvtSrcContentPanel == null || m_CvtSrcContentTitle == null || m_CvtSrctreePageItem == null || m_textMsgLog == null)
        {
            return false;
        }
        if (GetGuiPageListFromXml(strFileName, m_CvtSrcGuiPageArray).Equals(""))
        {
            m_CvtSrctreePageItem.Nodes[0].Text = m_GuiRootName;
            m_CvtSrctreePageItem.Nodes[0].Nodes.Clear();
            for (int i = 0; i < m_CvtSrcGuiPageArray.Count; i++)
            {
                m_CvtSrctreePageItem.Nodes[0].Nodes.Add(m_CvtSrcGuiPageArray[i].Name);
            }
            m_CvtSrctreePageItem.Nodes[0].ExpandAll();
            m_CurrPageIndex = 0;
            CreateCvtSrcPageGUI(m_CvtSrcGuiPageArray[0]);
        }
        actionList.Clear();
        SetBackStepMode(mode: false);
        SetNextStepMode(mode: false);
        return true;
    }

    public bool InitCvtDesGUI(string strFileName)
    {
        if (m_CvtDesContentPanel == null || m_CvtDesContentTitle == null || m_CvtDestreePageItem == null || m_textMsgLog == null)
        {
            return false;
        }
        if (GetGuiPageListFromXml(strFileName, m_CvtDesGuiPageArray).Equals(""))
        {
            m_CvtDestreePageItem.Nodes[0].Text = m_GuiRootName;
            m_CvtDestreePageItem.Nodes[0].Nodes.Clear();
            for (int i = 0; i < m_CvtDesGuiPageArray.Count; i++)
            {
                m_CvtDestreePageItem.Nodes[0].Nodes.Add(m_CvtDesGuiPageArray[i].Name);
            }
            m_CvtDestreePageItem.Nodes[0].ExpandAll();
            m_CurrPageIndex = 0;
            CreateCvtDesPageGUI(m_CvtDesGuiPageArray[0]);
        }
        actionList.Clear();
        SetBackStepMode(mode: false);
        SetNextStepMode(mode: false);
        return true;
    }

    public void RearrangeOrigLayout()
    {
        try
        {
            m_ContentPanel.VerticalScroll.Value = 0;
        }
        catch
        {
        }
        int num = 0;
        int guiGroupStartX = Settings.Default.GuiGroupStartX;
        int guiGroupStartY = Settings.Default.GuiGroupStartY;
        int guiGroupSpaceX = Settings.Default.GuiGroupSpaceX;
        int guiGroupSpaceY = Settings.Default.GuiGroupSpaceY;
        int num2 = 0;
        int num3 = 0;
        int y = guiGroupStartY;
        if (m_pGroupBoxArray == null || m_pGroupBoxArray[m_CurrPageIndex] == null)
        {
            return;
        }
        for (int i = 0; i < m_pGroupBoxArray[m_CurrPageIndex].Length; i++)
        {
            if (m_pGroupBoxArray[m_CurrPageIndex][i].Width > num)
            {
                num = m_pGroupBoxArray[m_CurrPageIndex][i].Width;
            }
        }
        if (m_ContentPanel.Width > num * 2 + guiGroupSpaceY * 2)
        {
            for (int j = 0; j < m_pGroupBoxArray[m_CurrPageIndex].Length; j++)
            {
                num2 = j % 2;
                int x = num2 == 0 ? guiGroupStartX : guiGroupStartX + m_pGroupBoxArray[m_CurrPageIndex][j - num2].Width + num2 * guiGroupSpaceX;
                if (j >= 2)
                {
                    num3 = j - 2;
                    y = m_pGroupBoxArray[m_CurrPageIndex][num3].Location.Y + m_pGroupBoxArray[m_CurrPageIndex][num3].Height + guiGroupSpaceY;
                }
                m_pGroupBoxArray[m_CurrPageIndex][j].Location = new Point(x, y);
            }
        }
        else
        {
            for (int k = 0; k < m_pGroupBoxArray[m_CurrPageIndex].Length; k++)
            {
                int x = guiGroupStartX;
                if (k > 0)
                {
                    num3 = k - 1;
                    y = m_pGroupBoxArray[m_CurrPageIndex][num3].Location.Y + m_pGroupBoxArray[m_CurrPageIndex][num3].Height + guiGroupSpaceY;
                }
                m_pGroupBoxArray[m_CurrPageIndex][k].Location = new Point(x, y);
            }
        }
        m_ContentPanel.AutoScrollMargin = new Size(0, guiGroupSpaceY);
    }

    public void RearrangeLayout()
    {
        try
        {
            m_ContentPanel.VerticalScroll.Value = 0;
        }
        catch
        {
        }
        int num = 0;
        int guiGroupStartX = Settings.Default.GuiGroupStartX;
        int guiGroupStartY = Settings.Default.GuiGroupStartY;
        int guiGroupSpaceX = Settings.Default.GuiGroupSpaceX;
        int guiGroupSpaceY = Settings.Default.GuiGroupSpaceY;
        int num2 = 0;
        int num3 = 0;
        int y = guiGroupStartY;
        if (m_pGroupBoxArray == null || m_pGroupBoxArray[m_CurrPageIndex] == null)
        {
            return;
        }
        for (int i = 0; i < m_pGroupBoxArray[m_CurrPageIndex].Length; i++)
        {
            if (m_pGroupBoxArray[m_CurrPageIndex][i].Width > num)
            {
                num = m_pGroupBoxArray[m_CurrPageIndex][i].Width;
            }
        }
        if (m_ContentPanel.AutoScrollPosition.Y < 0)
        {
            y = guiGroupStartY + m_ContentPanel.AutoScrollPosition.Y;
        }
        if (m_ContentPanel.Width > num * 2 + guiGroupSpaceY * 2)
        {
            for (int j = 0; j < m_pGroupBoxArray[m_CurrPageIndex].Length; j++)
            {
                num2 = j % 2;
                int x = num2 == 0 ? guiGroupStartX : guiGroupStartX + m_pGroupBoxArray[m_CurrPageIndex][j - num2].Width + num2 * guiGroupSpaceX;
                if (j >= 2)
                {
                    num3 = j - 2;
                    y = m_pGroupBoxArray[m_CurrPageIndex][num3].Location.Y + m_pGroupBoxArray[m_CurrPageIndex][num3].Height + guiGroupSpaceY;
                }
                m_pGroupBoxArray[m_CurrPageIndex][j].Location = new Point(x, y);
            }
        }
        else
        {
            for (int k = 0; k < m_pGroupBoxArray[m_CurrPageIndex].Length; k++)
            {
                int x = guiGroupStartX;
                if (k > 0)
                {
                    num3 = k - 1;
                    y = m_pGroupBoxArray[m_CurrPageIndex][num3].Location.Y + m_pGroupBoxArray[m_CurrPageIndex][num3].Height + guiGroupSpaceY;
                }
                m_pGroupBoxArray[m_CurrPageIndex][k].Location = new Point(x, y);
            }
        }
        m_ContentPanel.AutoScrollMargin = new Size(0, guiGroupSpaceY);
    }

    public void RearrangeCvtSrcLayout()
    {
        int num = 0;
        int guiGroupStartX = Settings.Default.GuiGroupStartX;
        int guiGroupStartY = Settings.Default.GuiGroupStartY;
        int guiGroupSpaceX = Settings.Default.GuiGroupSpaceX;
        int guiGroupSpaceY = Settings.Default.GuiGroupSpaceY;
        int num2 = 0;
        int num3 = 0;
        int y = guiGroupStartY;
        if (m_pCvtSrcGroupBoxArray == null)
        {
            return;
        }
        for (int i = 0; i < m_pCvtSrcGroupBoxArray.Length; i++)
        {
            if (m_pCvtSrcGroupBoxArray[i].Width > num)
            {
                num = m_pCvtSrcGroupBoxArray[i].Width;
            }
        }
        if (m_CvtSrcContentPanel.AutoScrollPosition.Y < 0)
        {
            y = guiGroupStartY + m_CvtSrcContentPanel.AutoScrollPosition.Y;
        }
        if (m_CvtSrcContentPanel.Width > num * 2 + guiGroupSpaceY * 2)
        {
            for (int j = 0; j < m_pCvtSrcGroupBoxArray.Length; j++)
            {
                num2 = j % 2;
                int x = num2 == 0 ? guiGroupStartX : guiGroupStartX + m_pCvtSrcGroupBoxArray[j - num2].Width + num2 * guiGroupSpaceX;
                if (j >= 2)
                {
                    num3 = j - 2;
                    y = m_pCvtSrcGroupBoxArray[num3].Location.Y + m_pCvtSrcGroupBoxArray[num3].Height + guiGroupSpaceY;
                }
                m_pCvtSrcGroupBoxArray[j].Location = new Point(x, y);
            }
        }
        else
        {
            for (int k = 0; k < m_pCvtSrcGroupBoxArray.Length; k++)
            {
                int x = guiGroupStartX;
                if (k > 0)
                {
                    num3 = k - 1;
                    y = m_pCvtSrcGroupBoxArray[num3].Location.Y + m_pCvtSrcGroupBoxArray[num3].Height + guiGroupSpaceY;
                }
                m_pCvtSrcGroupBoxArray[k].Location = new Point(x, y);
            }
        }
        m_CvtSrcContentPanel.AutoScrollMargin = new Size(0, guiGroupSpaceY);
    }

    public void RearrangeCvtDesLayout()
    {
        int num = 0;
        int guiGroupStartX = Settings.Default.GuiGroupStartX;
        int guiGroupStartY = Settings.Default.GuiGroupStartY;
        int guiGroupSpaceX = Settings.Default.GuiGroupSpaceX;
        int guiGroupSpaceY = Settings.Default.GuiGroupSpaceY;
        int num2 = 0;
        int num3 = 0;
        int y = guiGroupStartY;
        if (m_pCvtDesGroupBoxArray == null)
        {
            return;
        }
        for (int i = 0; i < m_pCvtDesGroupBoxArray.Length; i++)
        {
            if (m_pCvtDesGroupBoxArray[i].Width > num)
            {
                num = m_pCvtDesGroupBoxArray[i].Width;
            }
        }
        if (m_CvtDesContentPanel.AutoScrollPosition.Y < 0)
        {
            y = guiGroupStartY + m_CvtDesContentPanel.AutoScrollPosition.Y;
        }
        if (m_CvtDesContentPanel.Width > num * 2 + guiGroupSpaceY * 2)
        {
            for (int j = 0; j < m_pCvtDesGroupBoxArray.Length; j++)
            {
                num2 = j % 2;
                int x = num2 == 0 ? guiGroupStartX : guiGroupStartX + m_pCvtDesGroupBoxArray[j - num2].Width + num2 * guiGroupSpaceX;
                if (j >= 2)
                {
                    num3 = j - 2;
                    y = m_pCvtDesGroupBoxArray[num3].Location.Y + m_pCvtDesGroupBoxArray[num3].Height + guiGroupSpaceY;
                }
                m_pCvtDesGroupBoxArray[j].Location = new Point(x, y);
            }
        }
        else
        {
            for (int k = 0; k < m_pCvtDesGroupBoxArray.Length; k++)
            {
                int x = guiGroupStartX;
                if (k > 0)
                {
                    num3 = k - 1;
                    y = m_pCvtDesGroupBoxArray[num3].Location.Y + m_pCvtDesGroupBoxArray[num3].Height + guiGroupSpaceY;
                }
                m_pCvtDesGroupBoxArray[k].Location = new Point(x, y);
            }
        }
        m_CvtDesContentPanel.AutoScrollMargin = new Size(0, guiGroupSpaceY);
    }

    private void CreateNormalPage(GuiPage sUIPage)
    {
        RearrangeOrigLayout();
        m_ContentPanel.Update();
        int guiNumScrollWidth = Settings.Default.GuiNumScrollWidth;
        int width = 460;
        int guiParamNameWidth = Settings.Default.GuiParamNameWidth;
        int guiButtonWidth = Settings.Default.GuiButtonWidth;
        int guiButtonWidth2 = Settings.Default.GuiButtonWidth;
        int guiButtonWidth3 = Settings.Default.GuiButtonWidth;
        int num = guiNumScrollWidth * 2;
        int num2 = 0;
        m_ContentTitle.Text = sUIPage.Name;
        m_ContentPanel.Controls.Clear();
        if (m_pGroupBoxArray[m_CurrPageIndex] != null)
        {
            GroupBox[] array = m_pGroupBoxArray[m_CurrPageIndex];
            foreach (GroupBox value in array)
            {
                m_ContentPanel.Controls.Add(value);
            }
            return;
        }
        m_pGroupBoxArray[m_CurrPageIndex] = new GroupBox[sUIPage.GroupList.Count];
        m_PageControlArray[m_CurrPageIndex] = new List<List<Control>>();
        for (int j = 0; j < sUIPage.GroupList.Count; j++)
        {
            m_pGroupBoxArray[m_CurrPageIndex][j] = new GroupBox();
            m_pGroupBoxArray[m_CurrPageIndex][j].Width = width;
            m_pGroupBoxArray[m_CurrPageIndex][j].Text = sUIPage.GroupList[j].Name;
            m_pGroupBoxArray[m_CurrPageIndex][j].Font = new Font("Times New Roman", 10f, FontStyle.Regular);
            m_pGroupBoxArray[m_CurrPageIndex][j].BackColor = Color.LightGray;
            m_PageControlArray[m_CurrPageIndex].Add(new List<Control>());
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.RowCount = sUIPage.GroupList[j].ItemList.Count;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            Panel panel = new Panel();
            panel.AutoSize = true;
            panel.Height = 30;
            CheckBox checkBox = new CheckBox();
            checkBox.Checked = sUIPage.GroupList[j].InFile;
            checkBox.Enabled = true;
            checkBox.AutoSize = true;
            checkBox.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
            checkBox.Text = "InFile";
            checkBox.RightToLeft = RightToLeft.Yes;
            checkBox.Top = 5;
            checkBox.Tag = sUIPage.GroupList[j].Name;
            if (sUIPage.GroupList[j].FileMode == File_Mode.NULL)
            {
                checkBox.Visible = false;
            }
            else if (sUIPage.GroupList[j].FileMode == File_Mode.R)
            {
                checkBox.Enabled = false;
            }
            checkBox.CheckedChanged += InBinChkBox_CheckedChanged;
            panel.Controls.Add(checkBox);
            Button button = new Button();
            button.AutoSize = true;
            button.Width = 20;
            button.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
            button.Text = "Reset";
            button.Left = checkBox.Width + 20;
            button.BackColor = Color.LightCyan;
            button.Tag = sUIPage.GroupList[j].Name;
            button.Click += resetButton_Click;
            panel.Controls.Add(button);
            tableLayoutPanel.Controls.Add(panel, 0, 0);
            int num3 = 0;
            int num4 = 1;
            while (num3 < sUIPage.GroupList[j].ItemList.Count)
            {
                int xSize = sUIPage.GroupList[j].ItemList[num3].XSize;
                int ySize = sUIPage.GroupList[j].ItemList[num3].YSize;
                if (xSize * ySize > 1 && (sUIPage.GroupList[j].ItemList[num3].GuiType == "DataGrid" || sUIPage.GroupList[j].ItemList[num3].GuiType == "DataShading" || sUIPage.GroupList[j].ItemList[num3].GuiType == "DataHSV" || sUIPage.GroupList[j].ItemList[num3].GuiType == "DataAE" || sUIPage.GroupList[j].ItemList[num3].GuiType == "DataLine" || sUIPage.GroupList[j].ItemList[num3].GuiType == "DataLut"))
                {
                    Label label = new Label();
                    label.Text = sUIPage.GroupList[j].ItemList[num3].Text;
                    label.ForeColor = Color.Blue;
                    label.BackColor = Color.Silver;
                    label.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label.Width = guiParamNameWidth;
                    label.Dock = DockStyle.Fill;
                    label.Margin = new Padding(0, 0, 0, 0);
                    label.TextAlign = ContentAlignment.MiddleRight;
                    label.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    label.MouseHover += guiItem_MouseHover;
                    label.Click += guiItem_Click;
                    label.PreviewKeyDown += guiItem_PreviewKeyDown;
                    tableLayoutPanel.Controls.Add(label, 0, num4);
                    Panel panel2 = new Panel();
                    panel2.AutoSize = true;
                    Button button2 = new Button();
                    if (sUIPage.GroupList[j].Action == API_ACTION.R)
                    {
                        button2.Text = "Show Table";
                    }
                    else
                    {
                        button2.Text = "Edit Table";
                    }
                    button2.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    button2.Width = guiButtonWidth;
                    if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("DataShading"))
                    {
                        button2.Click += ShadingButton_Click;
                    }
                    else if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("DataHSV"))
                    {
                        button2.Click += HSVButton_Click;
                    }
                    else if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("DataLut"))
                    {
                        button2.Click += LutChartButton_Click;
                    }
                    else if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("DataLine"))
                    {
                        button2.Click += DataLineChart_Click;
                    }
                    else
                    {
                        button2.Click += gridButton_Click;
                    }
                    button2.MouseHover += guiItem_MouseHover;
                    panel2.Controls.Add(button2);
                    tableLayoutPanel.Controls.Add(panel2, 1, num4);
                }
                else if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("CheckBox"))
                {
                    Label label2 = new Label();
                    label2.Text = sUIPage.GroupList[j].ItemList[num3].Text;
                    label2.ForeColor = Color.Blue;
                    label2.BackColor = Color.Silver;
                    label2.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label2.Width = guiParamNameWidth;
                    label2.Dock = DockStyle.Fill;
                    label2.Margin = new Padding(0, 0, 0, 0);
                    label2.TextAlign = ContentAlignment.MiddleRight;
                    label2.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    label2.Click += guiItem_Click;
                    label2.PreviewKeyDown += guiItem_PreviewKeyDown;
                    tableLayoutPanel.Controls.Add(label2, 0, num4);
                    CheckBox checkBox2 = new CheckBox();
                    checkBox2.Dock = DockStyle.Fill;
                    checkBox2.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    checkBox2.CheckedChanged += checkBox_CheckedChanged;
                    checkBox2.Click += checkBox_Click;
                    checkBox2.MouseHover += guiItem_MouseHover;
                    checkBox2.MouseWheel += guiItem_MouseWheel;
                    tableLayoutPanel.Controls.Add(checkBox2, 1, num4);
                    m_PageControlArray[m_CurrPageIndex][j].Add(checkBox2);
                    if (sUIPage.GroupList[j].Action == API_ACTION.R)
                    {
                        checkBox2.Enabled = false;
                    }
                    checkBox2.Checked = sUIPage.GroupList[j].ItemList[num3].DataValue[0] > 0 ? true : false;
                }
                else if (sUIPage.GroupList[j].ItemList[num3].GuiType.StartsWith("RadioButton"))
                {
                    Label label3 = new Label();
                    label3.Text = sUIPage.GroupList[j].ItemList[num3].Text;
                    label3.ForeColor = Color.Blue;
                    label3.BackColor = Color.Silver;
                    label3.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label3.Width = guiParamNameWidth;
                    label3.Dock = DockStyle.Fill;
                    label3.Margin = new Padding(0, 0, 0, 0);
                    label3.TextAlign = ContentAlignment.MiddleRight;
                    label3.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    label3.Click += guiItem_Click;
                    label3.PreviewKeyDown += guiItem_PreviewKeyDown;
                    tableLayoutPanel.Controls.Add(label3, 0, num4);
                    Panel panel3 = new Panel();
                    panel3.Dock = DockStyle.Fill;
                    panel3.AutoSize = true;
                    panel3.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    RadioButton radioButton = new RadioButton();
                    radioButton.Name = "radioButton_" + sUIPage.GroupList[j].Name.Replace(" ", "") + "_" + sUIPage.GroupList[j].ItemList[num3].Tag;
                    radioButton.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    radioButton.Text = "Disable";
                    radioButton.AutoSize = true;
                    radioButton.Left = 10;
                    RadioButton radioButton2 = new RadioButton();
                    radioButton2.Name = "radioButton_" + sUIPage.GroupList[j].Name.Replace(" ", "") + "_" + sUIPage.GroupList[j].ItemList[num3].Tag;
                    radioButton2.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    radioButton2.Text = "Enable";
                    radioButton2.AutoSize = true;
                    radioButton2.Left = radioButton.Size.Width + 20;
                    panel3.Controls.Add(radioButton);
                    panel3.Controls.Add(radioButton2);
                    tableLayoutPanel.Controls.Add(panel3, 1, num4);
                    m_PageControlArray[m_CurrPageIndex][j].Add(panel3);
                    if (!sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("RadioButton"))
                    {
                        radioButton.CheckedChanged += radioButton0_CheckedChanged;
                    }
                    radioButton.MouseHover += guiItem_MouseHover;
                    radioButton2.MouseHover += guiItem_MouseHover;
                    if (sUIPage.GroupList[j].Action == API_ACTION.R)
                    {
                        panel3.Enabled = false;
                    }
                    if (sUIPage.GroupList[j].ItemList[num3].DataValue[0] == 0L)
                    {
                        radioButton2.Checked = true;
                    }
                    else
                    {
                        radioButton.Checked = true;
                    }
                }
                else if (sUIPage.GroupList[j].ItemList[num3].GuiType.StartsWith("ComboBox"))
                {
                    Label label4 = new Label();
                    label4.Text = sUIPage.GroupList[j].ItemList[num3].Text;
                    label4.ForeColor = Color.Blue;
                    label4.BackColor = Color.Silver;
                    label4.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label4.Width = guiParamNameWidth;
                    label4.Dock = DockStyle.Fill;
                    label4.Margin = new Padding(0, 0, 0, 0);
                    label4.TextAlign = ContentAlignment.MiddleRight;
                    label4.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    label4.MouseHover += guiItem_MouseHover;
                    label4.Click += guiItem_Click;
                    label4.PreviewKeyDown += guiItem_PreviewKeyDown;
                    tableLayoutPanel.Controls.Add(label4, 0, num4);
                    Panel panel4 = new Panel();
                    panel4.AutoSize = true;
                    ComboBox comboBox = new ComboBox();
                    comboBox.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    comboBox.Width = guiButtonWidth2;
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    comboBox.MouseHover += guiItem_MouseHover;
                    comboBox.MouseWheel += guiItem_MouseWheel;
                    if (!sUIPage.GroupList[j].ItemList[num3].Paramters.Equals(""))
                    {
                        string[] array2 = sUIPage.GroupList[j].ItemList[num3].Paramters.Split(',');
                        for (long num5 = 0L; num5 < array2.Length; num5++)
                        {
                            comboBox.Items.Add(array2[num5]);
                        }
                    }
                    else
                    {
                        for (long num6 = sUIPage.GroupList[j].ItemList[num3].MinValue; num6 <= sUIPage.GroupList[j].ItemList[num3].MaxValue; num6++)
                        {
                            comboBox.Items.Add(num6);
                        }
                    }
                    comboBox.SelectedIndex = 0;
                    if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("ComboBox"))
                    {
                        comboBox.SelectedIndexChanged += comboBox_SelectedIndexChanged;
                    }
                    else if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("ComboBox_show"))
                    {
                        comboBox.SelectedIndexChanged += comboBox_show_SelectedIndexChanged;
                    }
                    else
                    {
                        comboBox.SelectedIndexChanged += vComboBox_SelectedIndexChanged;
                    }
                    comboBox.DropDownClosed += comboBox_DropDownClosed;
                    panel4.Controls.Add(comboBox);
                    tableLayoutPanel.Controls.Add(panel4, 1, num4);
                    m_PageControlArray[m_CurrPageIndex][j].Add(comboBox);
                    num2 = (int)(sUIPage.GroupList[j].ItemList[num3].DataValue[0] - sUIPage.GroupList[j].ItemList[num3].MinValue);
                    if (num2 >= comboBox.Items.Count)
                    {
                        num2 = comboBox.Items.Count - 1;
                    }
                    if (num2 < 0)
                    {
                        num2 = 0;
                    }
                    comboBox.SelectedIndex = num2;
                }
                else if (sUIPage.GroupList[j].ItemList[num3].GuiType.StartsWith("TextBox"))
                {
                    Label label5 = new Label();
                    label5.Text = sUIPage.GroupList[j].ItemList[num3].Text;
                    label5.ForeColor = Color.Blue;
                    label5.BackColor = Color.Silver;
                    label5.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label5.Width = guiParamNameWidth;
                    label5.Dock = DockStyle.Fill;
                    label5.Margin = new Padding(0, 0, 0, 0);
                    label5.TextAlign = ContentAlignment.MiddleRight;
                    label5.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    label5.MouseHover += guiItem_MouseHover;
                    label5.Click += guiItem_Click;
                    label5.PreviewKeyDown += guiItem_PreviewKeyDown;
                    tableLayoutPanel.Controls.Add(label5, 0, num4);
                    TextBox textBox = new TextBox();
                    if (sUIPage.GroupList[j].Action == API_ACTION.R)
                    {
                        textBox.ReadOnly = true;
                    }
                    textBox.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    textBox.Width = guiButtonWidth3;
                    textBox.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("TextBox"))
                    {
                        textBox.TextChanged += textValueItem_TextChanged;
                        textBox.LostFocus += textValueItem_LostFocus;
                    }
                    else
                    {
                        textBox.TextChanged += textValueItem_NumTextChanged;
                        textBox.LostFocus += textValueItem_NumLostFocus;
                    }
                    tableLayoutPanel.Controls.Add(textBox, 1, num4);
                    m_PageControlArray[m_CurrPageIndex][j].Add(textBox);
                }
                else if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("NumScroll"))
                {
                    Label label6 = new Label();
                    label6.Text = sUIPage.GroupList[j].ItemList[num3].Text;
                    label6.ForeColor = Color.Blue;
                    label6.BackColor = Color.Silver;
                    label6.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label6.Width = guiParamNameWidth;
                    label6.Dock = DockStyle.Fill;
                    label6.Margin = new Padding(0, 0, 0, 0);
                    label6.TextAlign = ContentAlignment.MiddleRight;
                    label6.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    label6.MouseHover += guiItem_MouseHover;
                    label6.Click += guiItem_Click;
                    label6.PreviewKeyDown += guiItem_PreviewKeyDown;
                    tableLayoutPanel.Controls.Add(label6, 0, num4);
                    Panel panel5 = new Panel();
                    panel5.Width = guiNumScrollWidth + num;
                    panel5.Height = 25;
                    NumericUpDown numericUpDown = new NumericUpDown();
                    numericUpDown.Width = guiNumScrollWidth;
                    numericUpDown.Maximum = sUIPage.GroupList[j].ItemList[num3].MaxValue;
                    numericUpDown.Minimum = sUIPage.GroupList[j].ItemList[num3].MinValue;
                    numericUpDown.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    numericUpDown.ValueChanged += numericUpDown_ValueChanged;
                    numericUpDown.Controls[0].Click += numericUpDown_Click;
                    numericUpDown.KeyPress += numericUpDown_KeyPress;
                    numericUpDown.MouseHover += guiItem_MouseHover;
                    numericUpDown.MouseWheel += guiItem_MouseWheel;
                    if (sUIPage.GroupList[j].Action == API_ACTION.R)
                    {
                        numericUpDown.Enabled = false;
                    }
                    panel5.Controls.Add(numericUpDown);
                    m_PageControlArray[m_CurrPageIndex][j].Add(numericUpDown);
                    SstarHScrollBar sstarHScrollBar = new SstarHScrollBar();
                    sstarHScrollBar.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    sstarHScrollBar.Maximum = (int)sUIPage.GroupList[j].ItemList[num3].MaxValue;
                    sstarHScrollBar.Minimum = (int)sUIPage.GroupList[j].ItemList[num3].MinValue;
                    sstarHScrollBar.LargeChange = 1;
                    if (sUIPage.GroupList[j].Action == API_ACTION.R)
                    {
                        sstarHScrollBar.Enabled = false;
                    }
                    sstarHScrollBar.Anchor = AnchorStyles.None;
                    sstarHScrollBar.Left = numericUpDown.Size.Width;
                    sstarHScrollBar.Top = 3;
                    sstarHScrollBar.Width = num;
                    sstarHScrollBar.ValueChanged += hScrollBar_ValueChanged;
                    sstarHScrollBar.Scroll += hScrollBar_Scrolled;
                    sstarHScrollBar.MouseHover += guiItem_MouseHover;
                    panel5.Controls.Add(sstarHScrollBar);
                    tableLayoutPanel.Controls.Add(panel5, 1, num4);
                    num2 = (int)sUIPage.GroupList[j].ItemList[num3].DataValue[0];
                    if (num2 > sstarHScrollBar.Maximum)
                    {
                        num2 = sstarHScrollBar.Maximum;
                    }
                    if (num2 < sstarHScrollBar.Minimum)
                    {
                        num2 = sstarHScrollBar.Minimum;
                    }
                    sstarHScrollBar.Value = num2;
                    numericUpDown.Value = num2;
                }
                else if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("CalcButton"))
                {
                    Button button3 = new Button();
                    button3.Text = sUIPage.GroupList[j].ItemList[num3].Text;
                    button3.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(button3, 0, num4);
                    button3.Click += calcButton_Click;
                    button3.MouseHover += guiItem_MouseHover;
                }
                else if (sUIPage.GroupList[j].ItemList[num3].GuiType.Equals("CaliButton"))
                {
                    Button button4 = new Button();
                    button4.Text = sUIPage.GroupList[j].ItemList[num3].Text;
                    button4.Tag = sUIPage.GroupList[j].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(button4, 0, num4);
                    button4.Click += caliButton_Click;
                    button4.MouseHover += guiItem_MouseHover;
                }
                else
                {
                    num4--;
                    tableLayoutPanel.RowCount--;
                }
                num3++;
                num4++;
            }
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.AutoSize = true;
            m_pGroupBoxArray[m_CurrPageIndex][j].AutoSize = true;
            m_pGroupBoxArray[m_CurrPageIndex][j].Controls.Add(tableLayoutPanel);
            m_ContentPanel.Controls.Add(m_pGroupBoxArray[m_CurrPageIndex][j]);
        }
        RearrangeLayout();
        m_ContentPanel.Update();
    }

    private void CreateCvtSrcNormalPage(GuiPage sUIPage)
    {
        int guiNumScrollWidth = Settings.Default.GuiNumScrollWidth;
        int width = 460;
        int height = 60;
        int guiParamNameWidth = Settings.Default.GuiParamNameWidth;
        int guiButtonWidth = Settings.Default.GuiButtonWidth;
        int guiButtonWidth2 = Settings.Default.GuiButtonWidth;
        int guiButtonWidth3 = Settings.Default.GuiButtonWidth;
        int num = guiNumScrollWidth * 2;
        int num2 = 0;
        m_CvtSrcContentTitle.Text = sUIPage.Name;
        m_CvtSrcContentPanel.Controls.Clear();
        m_CvtSrcPageControlArray.Clear();
        m_pCvtSrcGroupBoxArray = new GroupBox[sUIPage.GroupList.Count];
        for (int i = 0; i < m_pCvtSrcGroupBoxArray.Length; i++)
        {
            m_pCvtSrcGroupBoxArray[i] = new GroupBox();
            m_pCvtSrcGroupBoxArray[i].Width = width;
            m_pCvtSrcGroupBoxArray[i].Height = height;
            m_pCvtSrcGroupBoxArray[i].Text = sUIPage.GroupList[i].Name;
            m_pCvtSrcGroupBoxArray[i].Name = "groupBox_" + sUIPage.GroupList[i].Name.Replace(" ", "_");
            m_pCvtSrcGroupBoxArray[i].Font = new Font("Times New Roman", 10f, FontStyle.Regular);
            m_pCvtSrcGroupBoxArray[i].BackColor = Color.LightGray;
            m_CvtSrcPageControlArray.Add(new List<Control>());
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.RowCount = sUIPage.GroupList[i].ItemList.Count;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            int num3 = 0;
            int num4 = 0;
            while (num3 < sUIPage.GroupList[i].ItemList.Count)
            {
                int xSize = sUIPage.GroupList[i].ItemList[num3].XSize;
                int ySize = sUIPage.GroupList[i].ItemList[num3].YSize;
                if (xSize * ySize > 1 && (sUIPage.GroupList[i].ItemList[num3].GuiType == "DataGrid" || sUIPage.GroupList[i].ItemList[num3].GuiType == "DataShading" || sUIPage.GroupList[i].ItemList[num3].GuiType == "DataHSV"))
                {
                    Label label = new Label();
                    label.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label.ForeColor = Color.Blue;
                    label.BackColor = Color.Silver;
                    label.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label.Width = guiParamNameWidth;
                    label.Dock = DockStyle.Fill;
                    label.Margin = new Padding(0, 0, 0, 0);
                    label.TextAlign = ContentAlignment.MiddleRight;
                    label.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    label.MouseHover += CvtSrcguiItem_MouseHover;
                    tableLayoutPanel.Controls.Add(label, 0, num4);
                    Panel panel = new Panel();
                    panel.AutoSize = true;
                    Button button = new Button();
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        button.Text = "Show Table";
                    }
                    else
                    {
                        button.Text = "Edit Table";
                    }
                    button.Name = "button_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    button.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    button.Width = guiButtonWidth;
                    if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("DataShading"))
                    {
                        button.Click += CvtSrcShadingButton_Click;
                    }
                    else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("DataHSV"))
                    {
                        button.Click += CvtSrcHSVButton_Click;
                    }
                    else
                    {
                        button.Click += CvtSrcgridButton_Click;
                    }
                    button.MouseHover += CvtSrcguiItem_MouseHover;
                    panel.Controls.Add(button);
                    tableLayoutPanel.Controls.Add(panel, 1, num4);
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("CheckBox"))
                {
                    Label label2 = new Label();
                    label2.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label2.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label2.ForeColor = Color.Blue;
                    label2.BackColor = Color.Silver;
                    label2.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label2.Width = guiParamNameWidth;
                    label2.Dock = DockStyle.Fill;
                    label2.Margin = new Padding(0, 0, 0, 0);
                    label2.TextAlign = ContentAlignment.MiddleRight;
                    label2.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label2, 0, num4);
                    CheckBox checkBox = new CheckBox();
                    checkBox.Name = "checkBox_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    checkBox.Dock = DockStyle.Fill;
                    checkBox.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(checkBox, 1, num4);
                    m_CvtSrcPageControlArray[i].Add(checkBox);
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        checkBox.Enabled = false;
                    }
                    checkBox.Checked = sUIPage.GroupList[i].ItemList[num3].DataValue[0] > 0 ? true : false;
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.StartsWith("RadioButton"))
                {
                    Label label3 = new Label();
                    label3.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label3.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label3.ForeColor = Color.Blue;
                    label3.BackColor = Color.Silver;
                    label3.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label3.Width = guiParamNameWidth;
                    label3.Dock = DockStyle.Fill;
                    label3.Margin = new Padding(0, 0, 0, 0);
                    label3.TextAlign = ContentAlignment.MiddleRight;
                    label3.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label3, 0, num4);
                    Panel panel2 = new Panel();
                    panel2.Dock = DockStyle.Fill;
                    panel2.AutoSize = true;
                    panel2.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    RadioButton radioButton = new RadioButton();
                    radioButton.Name = "radioButton_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    radioButton.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    radioButton.Text = "Disable";
                    radioButton.AutoSize = true;
                    radioButton.Left = 10;
                    RadioButton radioButton2 = new RadioButton();
                    radioButton2.Name = "radioButton_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    radioButton2.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    radioButton2.Text = "Enable";
                    radioButton2.AutoSize = true;
                    radioButton2.Left = radioButton.Size.Width + 20;
                    panel2.Controls.Add(radioButton);
                    panel2.Controls.Add(radioButton2);
                    tableLayoutPanel.Controls.Add(panel2, 1, num4);
                    m_CvtSrcPageControlArray[i].Add(panel2);
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        panel2.Enabled = false;
                    }
                    if (sUIPage.GroupList[i].ItemList[num3].DataValue[0] == 0L)
                    {
                        radioButton.Checked = true;
                    }
                    else
                    {
                        radioButton2.Checked = true;
                    }
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.StartsWith("ComboBox"))
                {
                    Label label4 = new Label();
                    label4.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label4.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label4.ForeColor = Color.Blue;
                    label4.BackColor = Color.Silver;
                    label4.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label4.Width = guiParamNameWidth;
                    label4.Dock = DockStyle.Fill;
                    label4.Margin = new Padding(0, 0, 0, 0);
                    label4.TextAlign = ContentAlignment.MiddleRight;
                    label4.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label4, 0, num4);
                    Panel panel3 = new Panel();
                    panel3.AutoSize = true;
                    ComboBox comboBox = new ComboBox();
                    comboBox.Name = "comboBox_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    comboBox.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    comboBox.Width = guiButtonWidth2;
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    if (!sUIPage.GroupList[i].ItemList[num3].Paramters.Equals(""))
                    {
                        string[] array = sUIPage.GroupList[i].ItemList[num3].Paramters.Split(',');
                        for (long num5 = 0L; num5 < array.Length; num5++)
                        {
                            comboBox.Items.Add(array[num5]);
                        }
                    }
                    else
                    {
                        for (long num6 = sUIPage.GroupList[i].ItemList[num3].MinValue; num6 <= sUIPage.GroupList[i].ItemList[num3].MaxValue; num6++)
                        {
                            comboBox.Items.Add(num6);
                        }
                    }
                    comboBox.SelectedIndex = 0;
                    panel3.Controls.Add(comboBox);
                    tableLayoutPanel.Controls.Add(panel3, 1, num4);
                    m_CvtSrcPageControlArray[i].Add(comboBox);
                    num2 = (int)(sUIPage.GroupList[i].ItemList[num3].DataValue[0] - sUIPage.GroupList[i].ItemList[num3].MinValue);
                    if (num2 >= comboBox.Items.Count)
                    {
                        num2 = comboBox.Items.Count - 1;
                    }
                    if (num2 < 0)
                    {
                        num2 = 0;
                    }
                    comboBox.SelectedIndex = num2;
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.StartsWith("TextBox"))
                {
                    Label label5 = new Label();
                    label5.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label5.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label5.ForeColor = Color.Blue;
                    label5.BackColor = Color.Silver;
                    label5.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label5.Width = guiParamNameWidth;
                    label5.Dock = DockStyle.Fill;
                    label5.Margin = new Padding(0, 0, 0, 0);
                    label5.TextAlign = ContentAlignment.MiddleRight;
                    label5.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label5, 0, num4);
                    TextBox textBox = new TextBox();
                    textBox.Name = "textBox_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        textBox.ReadOnly = true;
                    }
                    textBox.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    textBox.Width = guiButtonWidth3;
                    textBox.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    textBox.Text = sUIPage.GroupList[i].ItemList[num3].DataValue[0].ToString();
                    tableLayoutPanel.Controls.Add(textBox, 1, num4);
                    m_CvtSrcPageControlArray[i].Add(textBox);
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("NumScroll"))
                {
                    Label label6 = new Label();
                    label6.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label6.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label6.ForeColor = Color.Blue;
                    label6.BackColor = Color.Silver;
                    label6.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label6.Width = guiParamNameWidth;
                    label6.Dock = DockStyle.Fill;
                    label6.Margin = new Padding(0, 0, 0, 0);
                    label6.TextAlign = ContentAlignment.MiddleRight;
                    label6.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label6, 0, num4);
                    Panel panel4 = new Panel();
                    panel4.Width = guiNumScrollWidth + num;
                    panel4.Height = 25;
                    NumericUpDown numericUpDown = new NumericUpDown();
                    numericUpDown.Name = "numUpDn_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    numericUpDown.Width = guiNumScrollWidth;
                    numericUpDown.Maximum = sUIPage.GroupList[i].ItemList[num3].MaxValue;
                    numericUpDown.Minimum = sUIPage.GroupList[i].ItemList[num3].MinValue;
                    numericUpDown.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        numericUpDown.Enabled = false;
                    }
                    panel4.Controls.Add(numericUpDown);
                    m_CvtSrcPageControlArray[i].Add(numericUpDown);
                    HScrollBar hScrollBar = new HScrollBar();
                    hScrollBar.Name = "hScrollBar_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    hScrollBar.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    hScrollBar.Maximum = (int)sUIPage.GroupList[i].ItemList[num3].MaxValue;
                    hScrollBar.Minimum = (int)sUIPage.GroupList[i].ItemList[num3].MinValue;
                    hScrollBar.LargeChange = 1;
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        hScrollBar.Enabled = false;
                    }
                    hScrollBar.Anchor = AnchorStyles.None;
                    hScrollBar.Left = numericUpDown.Size.Width;
                    hScrollBar.Top = 3;
                    hScrollBar.Width = num;
                    hScrollBar.MouseWheel += guiItem_MouseWheel;
                    panel4.Controls.Add(hScrollBar);
                    tableLayoutPanel.Controls.Add(panel4, 1, num4);
                    num2 = (int)sUIPage.GroupList[i].ItemList[num3].DataValue[0];
                    if (num2 > hScrollBar.Maximum)
                    {
                        num2 = hScrollBar.Maximum;
                    }
                    if (num2 < hScrollBar.Minimum)
                    {
                        num2 = hScrollBar.Minimum;
                    }
                    hScrollBar.Value = num2;
                    numericUpDown.Value = num2;
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("CalcButton"))
                {
                    Button button2 = new Button();
                    button2.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    button2.Name = "calcButton_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    button2.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(button2, 0, num4);
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("CaliButton"))
                {
                    Button button3 = new Button();
                    button3.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    button3.Name = "calcButton_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    button3.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(button3, 0, num4);
                }
                else
                {
                    num4--;
                    tableLayoutPanel.RowCount--;
                }
                num3++;
                num4++;
            }
            _ = tableLayoutPanel.Controls.Count;
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.AutoSize = true;
            m_pCvtSrcGroupBoxArray[i].AutoSize = true;
            m_pCvtSrcGroupBoxArray[i].Controls.Add(tableLayoutPanel);
            m_CvtSrcContentPanel.Controls.Add(m_pCvtSrcGroupBoxArray[i]);
        }
        RearrangeCvtSrcLayout();
        m_CvtSrcContentPanel.Update();
    }

    private void CreateCvtDesNormalPage(GuiPage sUIPage)
    {
        int guiNumScrollWidth = Settings.Default.GuiNumScrollWidth;
        int width = 460;
        int height = 60;
        int guiParamNameWidth = Settings.Default.GuiParamNameWidth;
        int guiButtonWidth = Settings.Default.GuiButtonWidth;
        int guiButtonWidth2 = Settings.Default.GuiButtonWidth;
        int guiButtonWidth3 = Settings.Default.GuiButtonWidth;
        int num = guiNumScrollWidth * 2;
        int num2 = 0;
        m_CvtDesContentTitle.Text = sUIPage.Name;
        m_CvtDesContentPanel.Controls.Clear();
        m_CvtDesPageControlArray.Clear();
        m_pCvtDesGroupBoxArray = new GroupBox[sUIPage.GroupList.Count];
        for (int i = 0; i < m_pCvtDesGroupBoxArray.Length; i++)
        {
            m_pCvtDesGroupBoxArray[i] = new GroupBox();
            m_pCvtDesGroupBoxArray[i].Width = width;
            m_pCvtDesGroupBoxArray[i].Height = height;
            m_pCvtDesGroupBoxArray[i].Text = sUIPage.GroupList[i].Name;
            m_pCvtDesGroupBoxArray[i].Name = "groupBox_" + sUIPage.GroupList[i].Name.Replace(" ", "_");
            m_pCvtDesGroupBoxArray[i].Font = new Font("Times New Roman", 10f, FontStyle.Regular);
            m_pCvtDesGroupBoxArray[i].BackColor = Color.LightGray;
            m_CvtDesPageControlArray.Add(new List<Control>());
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.RowCount = sUIPage.GroupList[i].ItemList.Count;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            int num3 = 0;
            int num4 = 0;
            while (num3 < sUIPage.GroupList[i].ItemList.Count)
            {
                int xSize = sUIPage.GroupList[i].ItemList[num3].XSize;
                int ySize = sUIPage.GroupList[i].ItemList[num3].YSize;
                if (xSize * ySize > 1 && (sUIPage.GroupList[i].ItemList[num3].GuiType == "DataGrid" || sUIPage.GroupList[i].ItemList[num3].GuiType == "DataShading" || sUIPage.GroupList[i].ItemList[num3].GuiType == "DataHSV"))
                {
                    Label label = new Label();
                    label.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label.ForeColor = Color.Blue;
                    label.BackColor = Color.Silver;
                    label.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label.Width = guiParamNameWidth;
                    label.Dock = DockStyle.Fill;
                    label.Margin = new Padding(0, 0, 0, 0);
                    label.TextAlign = ContentAlignment.MiddleRight;
                    label.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    label.MouseHover += CvtDesguiItem_MouseHover;
                    tableLayoutPanel.Controls.Add(label, 0, num4);
                    Panel panel = new Panel();
                    panel.AutoSize = true;
                    Button button = new Button();
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        button.Text = "Show Table";
                    }
                    else
                    {
                        button.Text = "Edit Table";
                    }
                    button.Name = "button_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    button.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    button.Width = guiButtonWidth;
                    if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("DataShading"))
                    {
                        button.Click += CvtDesShadingButton_Click;
                    }
                    else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("DataHSV"))
                    {
                        button.Click += CvtDesHSVButton_Click;
                    }
                    else
                    {
                        button.Click += CvtDesgridButton_Click;
                    }
                    button.MouseHover += CvtDesguiItem_MouseHover;
                    panel.Controls.Add(button);
                    tableLayoutPanel.Controls.Add(panel, 1, num4);
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("CheckBox"))
                {
                    Label label2 = new Label();
                    label2.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label2.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label2.ForeColor = Color.Blue;
                    label2.BackColor = Color.Silver;
                    label2.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label2.Width = guiParamNameWidth;
                    label2.Dock = DockStyle.Fill;
                    label2.Margin = new Padding(0, 0, 0, 0);
                    label2.TextAlign = ContentAlignment.MiddleRight;
                    label2.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label2, 0, num4);
                    CheckBox checkBox = new CheckBox();
                    checkBox.Name = "checkBox_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    checkBox.Dock = DockStyle.Fill;
                    checkBox.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(checkBox, 1, num4);
                    m_CvtDesPageControlArray[i].Add(checkBox);
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        checkBox.Enabled = false;
                    }
                    checkBox.Checked = sUIPage.GroupList[i].ItemList[num3].DataValue[0] > 0 ? true : false;
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.StartsWith("RadioButton"))
                {
                    Label label3 = new Label();
                    label3.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label3.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label3.ForeColor = Color.Blue;
                    label3.BackColor = Color.Silver;
                    label3.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label3.Width = guiParamNameWidth;
                    label3.Dock = DockStyle.Fill;
                    label3.Margin = new Padding(0, 0, 0, 0);
                    label3.TextAlign = ContentAlignment.MiddleRight;
                    label3.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label3, 0, num4);
                    Panel panel2 = new Panel();
                    panel2.Dock = DockStyle.Fill;
                    panel2.AutoSize = true;
                    panel2.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    RadioButton radioButton = new RadioButton();
                    radioButton.Name = "radioButton_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    radioButton.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    radioButton.Text = "Disable";
                    radioButton.AutoSize = true;
                    radioButton.Left = 10;
                    RadioButton radioButton2 = new RadioButton();
                    radioButton2.Name = "radioButton_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    radioButton2.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    radioButton2.Text = "Enable";
                    radioButton2.AutoSize = true;
                    radioButton2.Left = radioButton.Size.Width + 20;
                    panel2.Controls.Add(radioButton);
                    panel2.Controls.Add(radioButton2);
                    tableLayoutPanel.Controls.Add(panel2, 1, num4);
                    m_CvtDesPageControlArray[i].Add(panel2);
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        panel2.Enabled = false;
                    }
                    if (sUIPage.GroupList[i].ItemList[num3].DataValue[0] == 0L)
                    {
                        radioButton.Checked = true;
                    }
                    else
                    {
                        radioButton2.Checked = true;
                    }
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.StartsWith("ComboBox"))
                {
                    Label label4 = new Label();
                    label4.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label4.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label4.ForeColor = Color.Blue;
                    label4.BackColor = Color.Silver;
                    label4.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label4.Width = guiParamNameWidth;
                    label4.Dock = DockStyle.Fill;
                    label4.Margin = new Padding(0, 0, 0, 0);
                    label4.TextAlign = ContentAlignment.MiddleRight;
                    label4.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label4, 0, num4);
                    Panel panel3 = new Panel();
                    panel3.AutoSize = true;
                    ComboBox comboBox = new ComboBox();
                    comboBox.Name = "comboBox_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    comboBox.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    comboBox.Width = guiButtonWidth2;
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    if (!sUIPage.GroupList[i].ItemList[num3].Paramters.Equals(""))
                    {
                        string[] array = sUIPage.GroupList[i].ItemList[num3].Paramters.Split(',');
                        for (long num5 = 0L; num5 < array.Length; num5++)
                        {
                            comboBox.Items.Add(array[num5]);
                        }
                    }
                    else
                    {
                        for (long num6 = sUIPage.GroupList[i].ItemList[num3].MinValue; num6 <= sUIPage.GroupList[i].ItemList[num3].MaxValue; num6++)
                        {
                            comboBox.Items.Add(num6);
                        }
                    }
                    comboBox.SelectedIndex = 0;
                    panel3.Controls.Add(comboBox);
                    tableLayoutPanel.Controls.Add(panel3, 1, num4);
                    m_CvtDesPageControlArray[i].Add(comboBox);
                    num2 = (int)(sUIPage.GroupList[i].ItemList[num3].DataValue[0] - sUIPage.GroupList[i].ItemList[num3].MinValue);
                    if (num2 >= comboBox.Items.Count)
                    {
                        num2 = comboBox.Items.Count - 1;
                    }
                    if (num2 < 0)
                    {
                        num2 = 0;
                    }
                    comboBox.SelectedIndex = num2;
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.StartsWith("TextBox"))
                {
                    Label label5 = new Label();
                    label5.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label5.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label5.ForeColor = Color.Blue;
                    label5.BackColor = Color.Silver;
                    label5.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label5.Width = guiParamNameWidth;
                    label5.Dock = DockStyle.Fill;
                    label5.Margin = new Padding(0, 0, 0, 0);
                    label5.TextAlign = ContentAlignment.MiddleRight;
                    label5.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label5, 0, num4);
                    TextBox textBox = new TextBox();
                    textBox.Name = "textBox_" + sUIPage.GroupList[i].Name.Replace(" ", "_") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        textBox.ReadOnly = true;
                    }
                    textBox.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    textBox.Width = guiButtonWidth3;
                    textBox.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    textBox.Text = sUIPage.GroupList[i].ItemList[num3].DataValue[0].ToString();
                    tableLayoutPanel.Controls.Add(textBox, 1, num4);
                    m_CvtDesPageControlArray[i].Add(textBox);
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("NumScroll"))
                {
                    Label label6 = new Label();
                    label6.Name = "Label_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    label6.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    label6.ForeColor = Color.Blue;
                    label6.BackColor = Color.Silver;
                    label6.Font = new Font("Times New Roman", 9f, FontStyle.Regular);
                    label6.Width = guiParamNameWidth;
                    label6.Dock = DockStyle.Fill;
                    label6.Margin = new Padding(0, 0, 0, 0);
                    label6.TextAlign = ContentAlignment.MiddleRight;
                    label6.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(label6, 0, num4);
                    Panel panel4 = new Panel();
                    panel4.Width = guiNumScrollWidth + num;
                    panel4.Height = 25;
                    NumericUpDown numericUpDown = new NumericUpDown();
                    numericUpDown.Name = "numUpDn_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    numericUpDown.Width = guiNumScrollWidth;
                    numericUpDown.Maximum = sUIPage.GroupList[i].ItemList[num3].MaxValue;
                    numericUpDown.Minimum = sUIPage.GroupList[i].ItemList[num3].MinValue;
                    numericUpDown.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        numericUpDown.Enabled = false;
                    }
                    panel4.Controls.Add(numericUpDown);
                    m_CvtDesPageControlArray[i].Add(numericUpDown);
                    HScrollBar hScrollBar = new HScrollBar();
                    hScrollBar.Name = "hScrollBar_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    hScrollBar.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    hScrollBar.Maximum = (int)sUIPage.GroupList[i].ItemList[num3].MaxValue;
                    hScrollBar.Minimum = (int)sUIPage.GroupList[i].ItemList[num3].MinValue;
                    hScrollBar.LargeChange = 1;
                    if (sUIPage.GroupList[i].Action == API_ACTION.R)
                    {
                        hScrollBar.Enabled = false;
                    }
                    hScrollBar.Anchor = AnchorStyles.None;
                    hScrollBar.Left = numericUpDown.Size.Width;
                    hScrollBar.Top = 3;
                    hScrollBar.Width = num;
                    panel4.Controls.Add(hScrollBar);
                    tableLayoutPanel.Controls.Add(panel4, 1, num4);
                    num2 = (int)sUIPage.GroupList[i].ItemList[num3].DataValue[0];
                    if (num2 > hScrollBar.Maximum)
                    {
                        num2 = hScrollBar.Maximum;
                    }
                    if (num2 < hScrollBar.Minimum)
                    {
                        num2 = hScrollBar.Minimum;
                    }
                    hScrollBar.Value = num2;
                    numericUpDown.Value = num2;
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("CalcButton"))
                {
                    Button button2 = new Button();
                    button2.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    button2.Name = "calcButton_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    button2.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(button2, 0, num4);
                }
                else if (sUIPage.GroupList[i].ItemList[num3].GuiType.Equals("CaliButton"))
                {
                    Button button3 = new Button();
                    button3.Text = sUIPage.GroupList[i].ItemList[num3].Text;
                    button3.Name = "calcButton_" + sUIPage.GroupList[i].Name.Replace(" ", "") + "_" + sUIPage.GroupList[i].ItemList[num3].Tag;
                    button3.Tag = sUIPage.GroupList[i].ItemList[num3].Tag;
                    tableLayoutPanel.Controls.Add(button3, 0, num4);
                }
                else
                {
                    num4--;
                    tableLayoutPanel.RowCount--;
                }
                num3++;
                num4++;
            }
            _ = tableLayoutPanel.Controls.Count;
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.AutoSize = true;
            m_pCvtDesGroupBoxArray[i].AutoSize = true;
            m_pCvtDesGroupBoxArray[i].Controls.Add(tableLayoutPanel);
            m_CvtDesContentPanel.Controls.Add(m_pCvtDesGroupBoxArray[i]);
        }
        RearrangeCvtDesLayout();
        m_CvtDesContentPanel.Update();
    }

    private void CreateRGBGammaPage(GuiPage sUIPage)
    {
        m_ContentTitle.Text = sUIPage.Name;
        m_ContentPanel.Controls.Clear();
        RGBGamma rGBGamma = new RGBGamma(sUIPage.GroupList[0]);
        m_ContentPanel.Controls.Add(rGBGamma);
        if (m_PageControlArray[m_CurrPageIndex] == null)
        {
            m_PageControlArray[m_CurrPageIndex] = new List<List<Control>>();
        }
        m_PageControlArray[m_CurrPageIndex].Clear();
        m_PageControlArray[m_CurrPageIndex].Add(new List<Control> { rGBGamma });
    }

    private void CreateCvtSrcRGBGammaPage(GuiPage sUIPage)
    {
        m_CvtSrcContentTitle.Text = sUIPage.Name;
        m_CvtSrcContentPanel.Controls.Clear();
        m_CvtSrcPageControlArray.Clear();
        RGBGamma rGBGamma = new RGBGamma(sUIPage.GroupList[0]);
        m_CvtSrcContentPanel.Controls.Add(rGBGamma);
        m_CvtSrcPageControlArray.Add(new List<Control> { rGBGamma });
    }

    private void CreateCvtDesRGBGammaPage(GuiPage sUIPage)
    {
        m_CvtDesContentTitle.Text = sUIPage.Name;
        m_CvtDesContentPanel.Controls.Clear();
        m_CvtDesPageControlArray.Clear();
        RGBGamma rGBGamma = new RGBGamma(sUIPage.GroupList[0]);
        m_CvtDesContentPanel.Controls.Add(rGBGamma);
        m_CvtDesPageControlArray.Add(new List<Control> { rGBGamma });
    }

    private void CreateYUVGammaPage(GuiPage sUIPage)
    {
        m_ContentTitle.Text = sUIPage.Name;
        m_ContentPanel.Controls.Clear();
        YUVGamma yUVGamma = new YUVGamma(sUIPage.GroupList[0]);
        m_ContentPanel.Controls.Add(yUVGamma);
        if (m_PageControlArray[m_CurrPageIndex] == null)
        {
            m_PageControlArray[m_CurrPageIndex] = new List<List<Control>>();
        }
        m_PageControlArray[m_CurrPageIndex].Clear();
        m_PageControlArray[m_CurrPageIndex].Add(new List<Control> { yUVGamma });
    }

    private void CreateCvtSrcYUVGammaPage(GuiPage sUIPage)
    {
        m_CvtSrcContentTitle.Text = sUIPage.Name;
        m_CvtSrcContentPanel.Controls.Clear();
        m_CvtSrcPageControlArray.Clear();
        YUVGamma yUVGamma = new YUVGamma(sUIPage.GroupList[0]);
        m_CvtSrcContentPanel.Controls.Add(yUVGamma);
        m_CvtSrcPageControlArray.Add(new List<Control> { yUVGamma });
    }

    private void CreateCvtDesYUVGammaPage(GuiPage sUIPage)
    {
        m_CvtDesContentTitle.Text = sUIPage.Name;
        m_CvtDesContentPanel.Controls.Clear();
        m_CvtDesPageControlArray.Clear();
        YUVGamma yUVGamma = new YUVGamma(sUIPage.GroupList[0]);
        m_CvtDesContentPanel.Controls.Add(yUVGamma);
        m_CvtDesPageControlArray.Add(new List<Control> { yUVGamma });
    }

    private void CreateWDRCurvePage(GuiPage sUIPage)
    {
        m_ContentTitle.Text = sUIPage.Name;
        m_ContentPanel.Controls.Clear();
        WDRCurve wDRCurve = new WDRCurve(sUIPage.GroupList[0]);
        m_ContentPanel.Controls.Add(wDRCurve);
        if (m_PageControlArray[m_CurrPageIndex] == null)
        {
            m_PageControlArray[m_CurrPageIndex] = new List<List<Control>>();
        }
        m_PageControlArray[m_CurrPageIndex].Clear();
        m_PageControlArray[m_CurrPageIndex].Add(new List<Control> { wDRCurve });
    }

    private void CreateWDRCurve_v2Page(GuiPage sUIPage)
    {
        m_ContentTitle.Text = sUIPage.Name;
        m_ContentPanel.Controls.Clear();
        WDRCurve_v2 wDRCurve_v = new WDRCurve_v2(sUIPage.GroupList[0], m_DbgParser);
        m_ContentPanel.Controls.Add(wDRCurve_v);
        if (m_PageControlArray[m_CurrPageIndex] == null)
        {
            m_PageControlArray[m_CurrPageIndex] = new List<List<Control>>();
        }
        m_PageControlArray[m_CurrPageIndex].Clear();
        m_PageControlArray[m_CurrPageIndex].Add(new List<Control> { wDRCurve_v });
    }

    private void CreateCvtSrcWDRCurvePage(GuiPage sUIPage)
    {
        m_CvtSrcContentTitle.Text = sUIPage.Name;
        m_CvtSrcContentPanel.Controls.Clear();
        m_CvtSrcPageControlArray.Clear();
        WDRCurve wDRCurve = new WDRCurve(sUIPage.GroupList[0]);
        m_CvtSrcContentPanel.Controls.Add(wDRCurve);
        m_CvtSrcPageControlArray.Add(new List<Control> { wDRCurve });
    }

    private void CreateCvtSrcWDRCurve_v2Page(GuiPage sUIPage)
    {
        m_CvtSrcContentTitle.Text = sUIPage.Name;
        m_CvtSrcContentPanel.Controls.Clear();
        m_CvtSrcPageControlArray.Clear();
        WDRCurve_v2 wDRCurve_v = new WDRCurve_v2(sUIPage.GroupList[0], m_DbgParser);
        m_CvtSrcContentPanel.Controls.Add(wDRCurve_v);
        m_CvtSrcPageControlArray.Add(new List<Control> { wDRCurve_v });
    }

    private void CreateCvtDesWDRCurvePage(GuiPage sUIPage)
    {
        m_CvtDesContentTitle.Text = sUIPage.Name;
        m_CvtDesContentPanel.Controls.Clear();
        m_CvtDesPageControlArray.Clear();
        WDRCurve wDRCurve = new WDRCurve(sUIPage.GroupList[0]);
        m_CvtDesContentPanel.Controls.Add(wDRCurve);
        m_CvtDesPageControlArray.Add(new List<Control> { wDRCurve });
    }

    private void CreateCvtDesWDRCurve_v2Page(GuiPage sUIPage)
    {
        m_CvtDesContentTitle.Text = sUIPage.Name;
        m_CvtDesContentPanel.Controls.Clear();
        m_CvtDesPageControlArray.Clear();
        WDRCurve_v2 wDRCurve_v = new WDRCurve_v2(sUIPage.GroupList[0], m_DbgParser);
        m_CvtDesContentPanel.Controls.Add(wDRCurve_v);
        m_CvtDesPageControlArray.Add(new List<Control> { wDRCurve_v });
    }

    private void CreatePageGUI(GuiPage sUIPage)
    {
        m_bIsCreatingGui = true;
        SetAutoWriteMode(sUIPage.AutoWrite);
        m_noAutoWrite = !sUIPage.AutoWrite;
        SetPageActionMode(sUIPage.Action);
        if (sUIPage.PageType.Equals("API"))
        {
            CreateNormalPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("RGBGAMMA"))
        {
            CreateRGBGammaPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("YUVGAMMA"))
        {
            CreateYUVGammaPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("WDRCurve"))
        {
            CreateWDRCurvePage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("WDRCurveFull"))
        {
            CreateWDRCurve_v2Page(sUIPage);
        }
        m_bIsCreatingGui = false;
    }

    private void CreateCvtSrcPageGUI(GuiPage sUIPage)
    {
        m_bIsCreatingGui = true;
        SetAutoWriteMode(sUIPage.AutoWrite);
        m_noAutoWrite = !sUIPage.AutoWrite;
        SetPageActionMode(sUIPage.Action);
        if (sUIPage.PageType.Equals("API"))
        {
            CreateCvtSrcNormalPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("RGBGAMMA"))
        {
            CreateCvtSrcRGBGammaPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("YUVGAMMA"))
        {
            CreateCvtSrcYUVGammaPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("WDRCurve"))
        {
            CreateCvtSrcWDRCurvePage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("WDRCurveFull"))
        {
            CreateCvtSrcWDRCurve_v2Page(sUIPage);
        }
        m_bIsCreatingGui = false;
    }

    private void CreateCvtDesPageGUI(GuiPage sUIPage)
    {
        m_bIsCreatingGui = true;
        SetAutoWriteMode(sUIPage.AutoWrite);
        m_noAutoWrite = !sUIPage.AutoWrite;
        SetPageActionMode(sUIPage.Action);
        if (sUIPage.PageType.Equals("API"))
        {
            CreateCvtDesNormalPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("RGBGAMMA"))
        {
            CreateCvtDesRGBGammaPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("YUVGAMMA"))
        {
            CreateCvtDesYUVGammaPage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("WDRCurve"))
        {
            CreateCvtDesWDRCurvePage(sUIPage);
        }
        else if (sUIPage.PageType.Equals("WDRCurveFull"))
        {
            CreateCvtDesWDRCurve_v2Page(sUIPage);
        }
        m_bIsCreatingGui = false;
    }

    private void checkBox_CheckedChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        CheckBox checkBox = (CheckBox)sender;
        GuiGroup guiGroup = null;
        long[] array = new long[1];
        string text = checkBox.Tag.ToString();
        guiGroup = GetGuiGroupByItemName(text);
        if (!m_bIsCreatingGui)
        {
            array[0] = checkBox.Checked ? 1 : 0;
            guiGroup.SetItemValue(text, array);
            if (m_bAutoWrite && !m_noAutoWrite && m_DbgParser.IsConnected() && guiGroup.Action == API_ACTION.RW)
            {
                m_DbgParser.updateTaskQueue(SendApiTask, guiGroup);
            }
        }
    }

    private void InBinChkBox_CheckedChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        CheckBox checkBox = (CheckBox)sender;
        string text = checkBox.Tag.ToString();
        foreach (GuiGroup group in m_GuiPageArray[m_CurrPageIndex].GroupList)
        {
            if (group.Name == text)
            {
                group.InFile = checkBox.Checked;
                break;
            }
        }
    }

    private void checkBox_Click(object sender, EventArgs e)
    {
        CheckBox checkBox = (CheckBox)sender;
        long[] array = new long[1];
        string itemTag = checkBox.Tag.ToString();
        array[0] = checkBox.Checked ? 1 : 0;
        actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, itemTag, 0, array));
        SetBackStepMode(mode: true);
        SetNextStepMode(mode: false);
    }

    private void resetButton_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        Button button = (Button)sender;
        actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, "ResetBegin", 0, null));
        for (int i = 0; i < m_GuiPageArray[m_CurrPageIndex].GroupList.Count; i++)
        {
            if (button.Tag.ToString() == m_GuiPageArray[m_CurrPageIndex].GroupList[i].Name)
            {
                m_GuiPageArray[m_CurrPageIndex].GroupList[i].StoreRollback(m_GuiPageRollback[m_CurrPageIndex].GroupList[i]);
                m_GuiPageArray[m_CurrPageIndex].GroupList[i].UpdateGroup(m_GuiPageSaved[m_CurrPageIndex].GroupList[i], m_CurrPageIndex, actionList);
                SetControlGroupValue(m_PageControlArray[m_CurrPageIndex][i], m_GuiPageArray[m_CurrPageIndex].GroupList[i]);
            }
        }
        actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, "ResetEnd", 0, null));
        SetBackStepMode(mode: true);
        SetNextStepMode(mode: false);
    }

    private void hScrollBar_ValueChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        HScrollBar hScrollBar = (HScrollBar)sender;
        if (hScrollBar.Parent.Controls[0] is NumericUpDown numericUpDown)
        {
            numericUpDown.Value = hScrollBar.Value;
        }
    }

    private void hScrollBar_Scrolled(object sender, ScrollEventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        if (ScrollEventType.EndScroll == e.Type)
        {
            HScrollBar hScrollBar = (HScrollBar)sender;
            long[] array = new long[1];
            string itemTag = hScrollBar.Tag.ToString();
            array[0] = hScrollBar.Value;
            actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, itemTag, 0, array));
            SetBackStepMode(mode: true);
            SetNextStepMode(mode: false);
        }
    }

    private void numericUpDown_ValueChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        NumericUpDown numericUpDown = (NumericUpDown)sender;
        GuiGroup guiGroup = null;
        long[] array = new long[1];
        string text = numericUpDown.Tag.ToString();
        if (numericUpDown.Parent.Controls[1] is HScrollBar hScrollBar)
        {
            hScrollBar.Value = (int)numericUpDown.Value;
        }
        guiGroup = GetGuiGroupByItemName(text);
        if (!m_bIsCreatingGui)
        {
            array[0] = (long)numericUpDown.Value;
            guiGroup.SetItemValue(text, array);
            if (m_bAutoWrite && !m_noAutoWrite && m_DbgParser.IsConnected() && guiGroup.Action == API_ACTION.RW)
            {
                m_DbgParser.updateTaskQueue(SendApiTask, guiGroup);
            }
        }
    }

    public void buttonUpDown_KeyPress(string itemTag, int dataIndex, long[] dataValue)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        long[] array = new long[dataValue.Length];
        for (int i = 0; i < dataValue.Length; i++)
        {
            array[i] = dataValue[i];
        }
        actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, itemTag, dataIndex, array));
        SetBackStepMode(mode: true);
        SetNextStepMode(mode: false);
    }

    private void numericUpDown_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        if (e.KeyChar == '\r')
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;
            long[] array = new long[1];
            string itemTag = numericUpDown.Tag.ToString();
            array[0] = (long)numericUpDown.Value;
            actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, itemTag, 0, array));
            SetBackStepMode(mode: true);
            SetNextStepMode(mode: false);
        }
    }

    private void textValueItem_LostFocus(object sender, EventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        string text = textBox.Tag.ToString();
        if (!textBox.ReadOnly)
        {
            char[] array = textBox.Text.ToCharArray();
            long[] array2 = new long[FindGuiItemByItemName(m_GuiPageArray[m_CurrPageIndex], text).DataValue.Length];
            Array.ConvertAll(array, (Converter<char, long>)((s) => s)).CopyTo(array2, 0);
            actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, text, -1, array2));
            SetBackStepMode(mode: true);
            SetNextStepMode(mode: false);
        }
    }

    private void textValueItem_NumLostFocus(object sender, EventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        long[] array = new long[1];
        string text = textBox.Tag.ToString();
        if (!textBox.ReadOnly)
        {
            FindGuiItemByItemName(m_GuiPageArray[m_CurrPageIndex], text);
            try
            {
                array[0] = long.Parse(textBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Status", MessageBoxButtons.OK);
                return;
            }
            actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, text, -1, array));
            SetBackStepMode(mode: true);
            SetNextStepMode(mode: false);
        }
    }

    private void numericUpDown_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        NumericUpDown numericUpDown = (NumericUpDown)(sender as Control).Parent;
        long[] array = new long[1];
        string itemTag = numericUpDown.Tag.ToString();
        array[0] = (long)numericUpDown.Value;
        actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, itemTag, 0, array));
        SetBackStepMode(mode: true);
        SetNextStepMode(mode: false);
    }

    private void radioButton0_CheckedChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        RadioButton radioButton = (RadioButton)sender;
        GuiGroup guiGroup = null;
        GuiItem guiItem = null;
        long[] array = new long[1];
        string text = radioButton.Tag.ToString();
        guiGroup = GetGuiGroupByItemName(text);
        if (!m_bIsCreatingGui)
        {
            guiItem = guiGroup.FindItemByName(text);
            array[0] = radioButton.Checked ? 1 : 0;
            guiGroup.SetItemValue(text, array);
            actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, text, 0, array));
            SetBackStepMode(mode: true);
            SetNextStepMode(mode: false);
            if (m_bAutoWrite && !m_noAutoWrite && m_DbgParser.IsConnected() && (guiGroup.Action == API_ACTION.RW || guiGroup.Action == API_ACTION.W))
            {
                m_DbgParser.updateTaskQueue(ByPassTask, guiItem);
            }
        }
    }

    private void comboBox_show_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        ComboBox comboBox = (ComboBox)sender;
        GuiGroup guiGroup = null;
        GuiItem guiItem = null;
        long[] array = new long[1];
        string text = comboBox.Tag.ToString();
        guiGroup = GetGuiGroupByItemName(text);
        if (!m_bIsCreatingGui)
        {
            guiItem = guiGroup.FindItemByName(text);
            array[0] = comboBox.SelectedIndex + guiItem.MinValue;
            guiGroup.SetItemValue(text, array);
        }
        string text2 = "/data/cfg/" + comboBox.Text + "_cali.data";
        foreach (List<Control> item in m_PageControlArray[m_CurrPageIndex])
        {
            foreach (Control item2 in item)
            {
                if (item2.Tag.ToString() == text.Substring(0, text.LastIndexOf('_') + 1) + "FILEPATH")
                {
                    (item2 as TextBox).Text = text2;
                    break;
                }
            }
        }
    }

    private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        ComboBox comboBox = (ComboBox)sender;
        GuiGroup guiGroup = null;
        GuiItem guiItem = null;
        long[] array = new long[1];
        string text = comboBox.Tag.ToString();
        guiGroup = GetGuiGroupByItemName(text);
        if (!m_bIsCreatingGui)
        {
            guiItem = guiGroup.FindItemByName(text);
            array[0] = comboBox.SelectedIndex + guiItem.MinValue;
            guiGroup.SetItemValue(text, array);
            if (m_bAutoWrite && !m_noAutoWrite && m_DbgParser.IsConnected() && guiGroup.Action == API_ACTION.RW)
            {
                m_DbgParser.updateTaskQueue(SendApiTask, guiGroup);
            }
        }
    }

    private void comboBox_DropDownClosed(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        ComboBox comboBox = (ComboBox)sender;
        string text = comboBox.Tag.ToString();
        long[] array = new long[1];
        GuiItem guiItem = GetGuiGroupByItemName(text).FindItemByName(text);
        array[0] = comboBox.SelectedIndex + guiItem.MinValue;
        actionList.Add(new ActionStep(m_CurrPageIndex, isReset: false, text, 0, array));
        SetBackStepMode(mode: true);
        SetNextStepMode(mode: false);
    }

    private void textValueItem_TextChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        TextBox textBox = (TextBox)sender;
        GuiGroup guiGroup = null;
        string text = " ";
        int num = 27;
        if (textBox.Text.Length < num)
        {
            int num2 = num - textBox.Text.Length;
            for (int i = 0; i < num2; i++)
            {
                text += " ";
            }
        }
        guiGroup = GetGuiGroupByItemName(textBox.Tag.ToString());
        if (!m_bIsCreatingGui)
        {
            guiGroup.FindItemByName(textBox.Tag.ToString());
            long[] data = Array.ConvertAll((textBox.Text + text).ToCharArray(), (Converter<char, long>)((s) => s));
            guiGroup.SetItemValue(textBox.Tag.ToString(), data);
        }
    }

    private void CvtSrctextValueItem_TextChanged(object sender, EventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        GuiGroup guiGroup = null;
        guiGroup = GetCvtSrcGuiGroupByItemName(textBox.Tag.ToString());
        if (!m_bIsCreatingGui)
        {
            guiGroup.FindItemByName(textBox.Tag.ToString());
            long[] data = Array.ConvertAll(textBox.Text.ToCharArray(), (Converter<char, long>)((s) => s));
            guiGroup.SetItemValue(textBox.Tag.ToString(), data);
        }
    }

    private void textValueItem_NumTextChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        TextBox textBox = (TextBox)sender;
        GuiGroup guiGroup = null;
        long[] array = new long[1];
        guiGroup = GetGuiGroupByItemName(textBox.Tag.ToString());
        if (!m_bIsCreatingGui)
        {
            guiGroup.FindItemByName(textBox.Tag.ToString());
            try
            {
                array[0] = long.Parse(textBox.Text);
                guiGroup.SetItemValue(textBox.Tag.ToString(), array);
            }
            catch (Exception)
            {
            }
        }
    }

    private void CvtSrctextValueItem_NumTextChanged(object sender, EventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        GuiGroup guiGroup = null;
        long[] array = new long[1];
        guiGroup = GetCvtSrcGuiGroupByItemName(textBox.Tag.ToString());
        if (!m_bIsCreatingGui)
        {
            guiGroup.FindItemByName(textBox.Tag.ToString());
            try
            {
                array[0] = long.Parse(textBox.Text);
                guiGroup.SetItemValue(textBox.Tag.ToString(), array);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Status", MessageBoxButtons.OK);
            }
        }
    }

    private void vComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        ComboBox comboBox = (ComboBox)sender;
        GuiPage guiPage = null;
        GuiGroup guiGroup = null;
        GuiItem guiItem = null;
        long[] array = new long[1];
        string text = comboBox.Tag.ToString();
        guiPage = m_GuiPageArray[m_CurrPageIndex];
        guiGroup = GetGuiGroupByItemName(text);
        if (!m_bIsCreatingGui)
        {
            guiItem = guiGroup.FindItemByName(text);
            array[0] = comboBox.SelectedIndex + guiItem.MinValue;
            guiGroup.SetItemValue(text, array);
            guiPage.GroupIndex = (int)array[0];
        }
    }

    private void ByPassTask(object objList)
    {
        GuiItem guiItem = (GuiItem)objList;
        GuiGroup guiGroup = null;
        guiGroup = GetGuiGroupByItemName(guiItem.Tag.ToString());
        string text = guiItem.WriteByPass(m_DbgParser, guiGroup.ID);
        if (text != "")
        {
            m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text, "");
        }
    }

    private void SendApiTask(object objList)
    {
        string text = "";
        text = ((GuiGroup)objList).WriteGroup(m_DbgParser);
        if (text != "")
        {
            m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text, "");
        }
        CheckConnection();
    }

    private void SendPageTask(object objList)
    {
        GuiPage guiPage = (GuiPage)objList;
        string text = "";
        string text2 = "";
        string text3 = "";
        text = guiPage.WritePage(m_DbgParser);
        if (text.Equals(""))
        {
            text3 = "Write " + guiPage.Name + " success";
        }
        else
        {
            text3 = text;
            text2 = "Write " + guiPage.Name + " failed";
        }
        m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text2, text3);
        CheckConnection();
    }

    private void ReadPageTask(object objList)
    {
        string text = "";
        string text2 = "";
        string text3 = "";
        GuiPage guiPage = (GuiPage)objList;
        text = guiPage.ReadPage(m_DbgParser);
        if (text.Equals(""))
        {
            m_GuiPageArray[m_CurrPageIndex].bRead = true;
            if (!m_GuiPageSaved[m_CurrPageIndex].bRead)
            {
                m_GuiPageSaved[m_CurrPageIndex].UpdatePage(m_GuiPageArray[m_CurrPageIndex]);
                actionList.RemoveByPageIndex(m_CurrPageIndex);
                m_GuiPageSaved[m_CurrPageIndex].bRead = true;
            }
            if (!m_GuiPageRollback[m_CurrPageIndex].bRead)
            {
                m_GuiPageRollback[m_CurrPageIndex].UpdatePage(m_GuiPageArray[m_CurrPageIndex]);
                m_GuiPageRollback[m_CurrPageIndex].bRead = true;
            }
            InitGuiPageCache(m_CurrPageIndex);
            m_ContentPanel.Invoke(new UpdateGUIHandler(UpdateGUI));
            text3 = "Read " + guiPage.Name + " success";
        }
        else
        {
            text2 = "Read " + guiPage.Name + " failed";
            text3 = "[Error] Read Error: " + text;
        }
        m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text2, text3);
        CheckConnection();
    }

    private void SendAllTask(object objList)
    {
        string text = "";
        string text2 = "";
        string text3 = "Write all success";
        string text4 = "";
        m_DbgParser.updateTaskQueue(SendPageTask, m_GuiPageArray[1]);
        for (int i = 0; i < m_GuiPageArray.Count; i++)
        {
            text = m_GuiPageArray[i].WritePage(m_DbgParser);
            if (text != "")
            {
                text2 += text;
                text = "";
            }
        }
        if (text2 != "")
        {
            text3 = "Write all failed";
            text4 = "[Error] Write Error: " + text2;
        }
        m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text3, text4);
        CheckConnection();
    }

    private void ReadAllTask(object obj)
    {
        string text = "";
        string text2 = "";
        string text3 = "Read all success";
        string text4 = "";
        recvLengthForCaliSpeed = 0;
        int num = 0;
        int num2 = 0;
        double num3 = 0.0;
        for (int i = 0; i < m_GuiPageArray.Count; i++)
        {
            num = Environment.TickCount;
            text = m_GuiPageArray[i].ReadPage(m_DbgParser);
            num2 = Environment.TickCount - num;
            recvLengthForCaliSpeed = m_GuiPageArray[i].recvLengthForCaliSpeed;
            num3 = recvLengthForCaliSpeed != 0 && num2 != 0 ? recvLengthForCaliSpeed / (double)num2 / 1024.0 * 1000.0 : 0.0;
            m_ContentPanel.Invoke(new UpdateTransProgressByValueHandler(TransProgressByValue), i * 100 / m_GuiPageArray.Count, num3);
            if (text.Equals(""))
            {
                m_GuiPageArray[i].bRead = true;
                if (!m_GuiPageSaved[i].bRead)
                {
                    m_GuiPageSaved[i].UpdatePage(m_GuiPageArray[i]);
                    actionList.RemoveByPageIndex(i);
                    m_GuiPageSaved[i].bRead = true;
                }
                if (!m_GuiPageRollback[i].bRead)
                {
                    m_GuiPageRollback[i].UpdatePage(m_GuiPageArray[i]);
                    m_GuiPageRollback[i].bRead = true;
                }
                InitGuiPageCache(i);
            }
            else
            {
                text2 += text;
                text = "";
            }
        }
        m_ContentPanel.Invoke(new UpdateGUIHandler(UpdateGUI));
        m_ContentPanel.Invoke(new UpdateTransFinishHandler(TransFinish), text2);
        if (text2 != "")
        {
            text3 = "Read all failed";
            text4 = "[Error] Write Error: " + text2;
        }
        m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text3, text4);
        CheckConnection();
    }

    private void CalculateApiTask(object obj)
    {
        string text = "";
        string text2 = "Calculate Success";
        string text3 = "";
        GuiGroup guiGroup = (GuiGroup)obj;
        text = guiGroup.WriteGroup(m_DbgParser);
        if (text != "")
        {
            text2 = "Calculate failed";
            text3 = "[Error] Write Error: " + text;
            m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text2, text3);
        }
        else
        {
            text = guiGroup.ReadGroup(m_DbgParser);
            m_ContentPanel.Invoke(new UpdateGUIHandler(UpdateGUI));
            m_ContentPanel.Invoke(new UpdateTransFinishHandler(TransFinish), text);
            if (text != "")
            {
                text2 = text2 = "Calculate failed";
                text3 = "[Error] Write Error: " + text;
            }
            m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text2, text3);
        }
        CheckConnection();
    }

    private void CaliApiTask(object obj)
    {
        string text = "";
        string text2 = "Calculate Success";
        string text3 = "";
        GuiGroup guiGroup = (GuiGroup)obj;
        text = guiGroup.WriteGroup(m_DbgParser);
        m_ContentPanel.Invoke(new UpdateTransFinishHandler(TransFinish), text);
        if (text != "")
        {
            text2 = "Calculate failed";
            text3 = "[Error] Write Error: " + text;
            m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text2, text3);
        }
        else
        {
            if (guiGroup.Name == "CaliAWB")
            {
                guiGroup.ReadCaliAWBGroup(m_DbgParser, FILE_TRANSFER_ID.CALIBRATION_DATA);
                m_ContentPanel.Invoke(new UpdateGUIHandler(UpdateGUI));
            }
            if (guiGroup.Name == "CaliDPC" || guiGroup.Name == "CaliSDC")
            {
                guiGroup.ReadCaliDPCGroup(m_DbgParser, FILE_TRANSFER_ID.CALIBRATION_DATA);
                m_ContentPanel.Invoke(new UpdateGUIHandler(UpdateGUI));
            }
            m_ContentPanel.Invoke(new ShowMessageHandler(ShowMessage), text2, text3);
        }
        CheckConnection();
    }

    private void CaliPathApiTask(object obj)
    {
        ((GuiGroup)obj).WriteGroup(m_DbgParser);
        CheckConnection();
    }

    private void ShadingButton_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        string text = ((Button)sender).Tag.ToString();
        FormShading formShading = new FormShading(GetGuiGroupByItemName(text), m_DbgParser, text);
        formShading.Show();
        formShading.Focus();
    }

    private void CvtSrcShadingButton_Click(object sender, EventArgs e)
    {
        string text = ((Button)sender).Tag.ToString();
        FormShading formShading = new FormShading(GetCvtSrcGuiGroupByItemName(text), m_DbgParser, text);
        formShading.Show();
        formShading.Focus();
    }

    private void CvtDesShadingButton_Click(object sender, EventArgs e)
    {
        string text = ((Button)sender).Tag.ToString();
        FormShading formShading = new FormShading(GetCvtDesGuiGroupByItemName(text), m_DbgParser, text);
        formShading.Show();
        formShading.Focus();
    }

    private void HSVButton_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        Button obj = (Button)sender;
        GuiItem guiItem = null;
        string text = obj.Tag.ToString();
        GuiGroup guiGroupByItemName = GetGuiGroupByItemName(text);
        guiItem = guiGroupByItemName.FindItemByName(text);
        FormHSV formHSV = new FormHSV(guiGroupByItemName, guiItem, m_DbgParser, m_bAutoWrite);
        formHSV.Show();
        formHSV.Focus();
    }

    private void CvtSrcHSVButton_Click(object sender, EventArgs e)
    {
        Button obj = (Button)sender;
        GuiItem guiItem = null;
        string text = obj.Tag.ToString();
        GuiGroup cvtSrcGuiGroupByItemName = GetCvtSrcGuiGroupByItemName(text);
        guiItem = cvtSrcGuiGroupByItemName.FindItemByName(text);
        FormHSV formHSV = new FormHSV(cvtSrcGuiGroupByItemName, guiItem, m_DbgParser, m_bAutoWrite);
        formHSV.Show();
        formHSV.Focus();
    }

    private void CvtDesHSVButton_Click(object sender, EventArgs e)
    {
        Button obj = (Button)sender;
        GuiItem guiItem = null;
        string text = obj.Tag.ToString();
        GuiGroup cvtDesGuiGroupByItemName = GetCvtDesGuiGroupByItemName(text);
        guiItem = cvtDesGuiGroupByItemName.FindItemByName(text);
        FormHSV formHSV = new FormHSV(cvtDesGuiGroupByItemName, guiItem, m_DbgParser, m_bAutoWrite);
        formHSV.Show();
        formHSV.Focus();
    }

    private void AEButton_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        string text = ((Button)sender).Tag.ToString();
        FormAETable formAETable = new FormAETable(GetGuiGroupByItemName(text), m_DbgParser, text, m_bAutoWrite);
        formAETable.ShowDialog();
        formAETable.Focus();
    }

    private void gridButton_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        string text = ((Button)sender).Tag.ToString();
        FormTable formTable = new FormTable(GetGuiGroupByItemName(text), m_DbgParser, text, m_bAutoWrite);
        formTable.Show();
        formTable.Focus();
    }

    private void CvtSrcgridButton_Click(object sender, EventArgs e)
    {
        string text = ((Button)sender).Tag.ToString();
        FormTable formTable = new FormTable(GetCvtSrcGuiGroupByItemName(text), m_DbgParser, text, m_bAutoWrite);
        formTable.Show();
        formTable.Focus();
    }

    private void DataLineChart_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        string text = ((Button)sender).Tag.ToString();
        GuiGroup guiGroupByItemName = GetGuiGroupByItemName(text);
        GuiItem guiItem = null;
        guiItem = guiGroupByItemName.FindItemByName(text);
        FormLineChart formLineChart = new FormLineChart(guiGroupByItemName, guiItem, m_DbgParser, m_bAutoWrite);
        formLineChart.Show();
        formLineChart.Focus();
    }

    private void LutChartButton_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        string text = ((Button)sender).Tag.ToString();
        GuiGroup guiGroupByItemName = GetGuiGroupByItemName(text);
        GuiItem guiItem = null;
        guiItem = guiGroupByItemName.FindItemByName(text);
        FormLut formLut = new FormLut(guiGroupByItemName, guiItem, m_DbgParser, m_bAutoWrite);
        formLut.Show();
        formLut.Focus();
    }

    private void CvtDesgridButton_Click(object sender, EventArgs e)
    {
        string text = ((Button)sender).Tag.ToString();
        FormTable formTable = new FormTable(GetCvtDesGuiGroupByItemName(text), m_DbgParser, text, m_bAutoWrite);
        formTable.Show();
        formTable.Focus();
    }

    private void calcButton_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        Button button = (Button)sender;
        GuiGroup guiGroupByItemName = GetGuiGroupByItemName(button.Tag.ToString());
        if (m_DbgParser.IsConnected())
        {
            m_DbgParser.updateTaskQueue(CalculateApiTask, guiGroupByItemName);
            ShowProgressBar(showProgress: false, "Waiting For Calculate...");
        }
    }

    private void caliButton_Click(object sender, EventArgs e)
    {
        if (!m_bIsCreatingGui)
        {
            isFWAPIParmChange = true;
        }
        Button button = (Button)sender;
        GuiGroup guiGroupByItemName = GetGuiGroupByItemName(button.Tag.ToString());
        if (m_DbgParser.IsConnected())
        {
            if (button.Tag.ToString() == "API_CaliDBPath_CALC")
            {
                GuiGroup guiGroupByItemName2 = GetGuiGroupByItemName("API_CaliPath_CALC");
                m_DbgParser.updateTaskQueue(CaliApiTask, guiGroupByItemName);
                m_DbgParser.updateTaskQueue(CaliPathApiTask, guiGroupByItemName2);
            }
            else
            {
                m_DbgParser.updateTaskQueue(CaliApiTask, guiGroupByItemName);
            }
            ShowProgressBar(showProgress: false, "Waiting For Calculate...");
        }
    }

    private void guiItem_MouseWheel(object sender, MouseEventArgs e)
    {
        if (e is HandledMouseEventArgs handledMouseEventArgs)
        {
            handledMouseEventArgs.Handled = true;
        }
    }

    private void guiItem_MouseHover(object sender, EventArgs e)
    {
        GuiItem guiItem = null;
        string text = (sender as Control).Tag.ToString();
        guiItem = GetGuiGroupByItemName(text).FindItemByName(text);
        m_labelItemInfo.Text = guiItem.Info;
    }

    private void guiItem_Click(object sender, EventArgs e)
    {
        Label obj = sender as Label;
        obj.Focus();
        obj.Cursor = Cursors.Hand;
    }

    private void guiItem_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        Label label = sender as Label;
        if (label.ContainsFocus && e.Control && e.KeyCode == Keys.C)
        {
            Clipboard.SetText(label.Text);
            label.Cursor = Cursors.Arrow;
        }
    }

    private void CvtSrcguiItem_MouseHover(object sender, EventArgs e)
    {
        GuiItem guiItem = null;
        string text = (sender as Control).Tag.ToString();
        guiItem = GetCvtSrcGuiGroupByItemName(text).FindItemByName(text);
        m_CvtSrclabelItemInfo.Text = guiItem.Info;
    }

    private void CvtDesguiItem_MouseHover(object sender, EventArgs e)
    {
        GuiItem guiItem = null;
        string text = (sender as Control).Tag.ToString();
        guiItem = GetCvtDesGuiGroupByItemName(text).FindItemByName(text);
        m_CvtSrclabelItemInfo.Text = guiItem.Info;
    }

    private long ConvertStringToNum(string strValue)
    {
        if (strValue.StartsWith("0x"))
        {
            return Convert.ToInt64(strValue.Substring(2), 16);
        }
        if (strValue.StartsWith("-0x"))
        {
            return -Convert.ToInt64(strValue.Substring(3), 16);
        }
        return Convert.ToInt64(strValue);
    }

    private ActionStep FindDefaultStep(ActionStep step)
    {
        GuiItem guiItem = FindGuiItemByItemName(m_GuiPageSaved[m_CurrPageIndex], step.itemTag);
        return new ActionStep(step.pageIndex, isReset: false, step.itemTag, -1, guiItem.DataValue);
    }

    public void StepBack()
    {
        ActionStep actionStep = actionList.StepBack();
        if (m_CurrPageIndex != actionStep.pageIndex)
        {
            m_treePageItem.SelectedNode = m_treePageItem.Nodes[0].Nodes[actionStep.pageIndex];
        }
        if (actionStep.itemTag.StartsWith("ResetEnd"))
        {
            do
            {
                try
                {
                    actionStep = actionList.StepBack();
                    if (actionStep.isReset && actionStep.itemTag.StartsWith("API"))
                    {
                        actionStep = FindDefaultStep(actionStep);
                    }
                    if (actionStep.itemTag.StartsWith("ResetBegin"))
                    {
                        break;
                    }
                    ApplyActionStep(actionStep);
                }
                catch
                {
                }
            }
            while (!actionStep.itemTag.StartsWith("ResetBegin"));
        }
        else
        {
            if (actionStep.isReset && actionStep.itemTag.StartsWith("API"))
            {
                actionStep = FindDefaultStep(actionStep);
            }
            ApplyActionStep(actionStep);
        }
    }

    public bool IsBackStepOver()
    {
        return actionList.IsBackStepOver();
    }

    public void StepNext()
    {
        ActionStep actionStep = actionList.StepNext();
        if (m_CurrPageIndex != actionStep.pageIndex)
        {
            m_treePageItem.SelectedNode = m_treePageItem.Nodes[0].Nodes[actionStep.pageIndex];
        }
        if (actionStep.itemTag.StartsWith("ResetBegin"))
        {
            do
            {
                actionStep = actionList.StepNext();
                if (!actionStep.itemTag.StartsWith("ResetEnd"))
                {
                    ApplyActionStep(actionStep);
                    continue;
                }
                break;
            }
            while (!actionStep.itemTag.StartsWith("ResetEnd"));
        }
        else
        {
            ApplyActionStep(actionStep);
        }
    }

    public bool IsNextStepOver()
    {
        return actionList.IsNextStepOver();
    }

    private void ApplyActionStep(ActionStep step)
    {
        if (step.isReset && !step.itemTag.StartsWith("API"))
        {
            GuiGroup guiGroup = m_GuiPageSaved[step.pageIndex].FindGroupByName(step.itemTag);
            for (int i = 0; i < m_GuiPageSaved[step.pageIndex].GroupList.Count; i++)
            {
                if (m_GuiPageSaved[step.pageIndex].GroupList[i].Name == guiGroup.Name)
                {
                    SetControlGroupValue(m_PageControlArray[m_CurrPageIndex][i], guiGroup);
                    break;
                }
            }
            return;
        }
        GuiItem guiItem = FindGuiItemByItemName(m_GuiPageArray[m_CurrPageIndex], step.itemTag);
        for (int j = 0; j < guiItem.DataValue.Length; j++)
        {
            guiItem.DataValue[j] = step.dataValue[j];
        }
        foreach (List<Control> item in m_PageControlArray[m_CurrPageIndex])
        {
            foreach (Control item2 in item)
            {
                if (item2.Tag.Equals(step.itemTag))
                {
                    SetControlItemValue(item2, guiItem);
                }
            }
        }
    }

    private void GetXmlFileVer(string strFileName)
    {
        XmlDocument xmlDoc = m_xmlDoc;
        string text = "";
        string text2 = "";
        string text3 = "";
        string text4 = "";
        m_ispVersion = "";
        m_videoVersion = "";
        m_binVersion = "";
        try
        {
            xmlDoc.Load(strFileName);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("/ISP_ITEM");
            string text5 = "0.0";
            if (xmlNode == null)
            {
                xmlNode = xmlDoc.SelectSingleNode("/API_XML");
                text5 = "1.0";
            }
            m_GuiRootName = xmlNode.Attributes["Name"].Value;
            if (text5 == "1.0")
            {
                m_binVersion = "1.0";
                foreach (XmlElement childNode in xmlNode.ChildNodes)
                {
                    text = childNode.GetAttribute("Author");
                    text2 = childNode.GetAttribute("Version");
                    if (text == "ISP")
                    {
                        m_ispVersion = text2;
                    }
                    else if (text == "VIDEO")
                    {
                        m_videoVersion = text2;
                    }
                }
            }
            else
            {
                m_binVersion = "0.0";
                m_ispVersion = xmlNode.Attributes["Version"].Value;
            }
            XmlAPIVerMajor = ushort.Parse(m_ispVersion.Split('.').ElementAt(0));
            XmlAPIVerMinor = ushort.Parse(m_ispVersion.Split('.').ElementAt(1));
        }
        catch (Exception ex)
        {
            text3 = "[Error] ParseXML: " + text4 + " error " + ex.Message;
            ShowMessage(text3, "");
        }
    }

    private string GetGuiPageListFromXml(string strFileName, List<GuiPage> guiPageList)
    {
        XmlDocument xmlDoc = m_xmlDoc;
        string text = "";
        string text2 = "";
        string text3 = "";
        string text4 = "";
        string text5 = "";
        string text6 = "";
        string text7 = "";
        string text8 = "";
        string text9 = "";
        string text10 = "";
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        m_ispVersion = "";
        m_videoVersion = "";
        m_binVersion = "";
        try
        {
            guiPageList.Clear();
            m_GuiPageSaved.Clear();
            m_GuiPageRollback.Clear();
            m_GuiPageCacheSavedOne.Clear();
            m_GuiPageCacheSavedTwo.Clear();
            m_CuiPageCurrentSaved.Clear();
            xmlDoc.Load(strFileName);
            XmlNode xmlNode = xmlDoc.SelectSingleNode("/ISP_ITEM");
            string text11 = "0.0";
            if (xmlNode == null)
            {
                xmlNode = xmlDoc.SelectSingleNode("/API_XML");
                text11 = "1.0";
            }
            m_GuiRootName = xmlNode.Attributes["Name"].Value;
            if (text11 == "1.0")
            {
                m_binVersion = "1.0";
                foreach (XmlElement childNode in xmlNode.ChildNodes)
                {
                    text = childNode.GetAttribute("Author");
                    text2 = childNode.GetAttribute("Version");
                    if (text == "ISP")
                    {
                        m_ispVersion = text2;
                    }
                    else if (text == "VIDEO")
                    {
                        m_videoVersion = text2;
                    }
                    foreach (XmlElement childNode2 in childNode.ChildNodes)
                    {
                        text4 = childNode2.GetAttribute("Name");
                        text10 = childNode2.GetAttribute("Type");
                        text9 = childNode2.GetAttribute("GroupIndex");
                        short gIndex = Convert.ToInt16(text9);
                        PAGE_ACTION pAGE_ACTION = (PAGE_ACTION)Enum.Parse(value: childNode2.GetAttribute("Action"), enumType: typeof(PAGE_ACTION));
                        text9 = childNode2.GetAttribute("AutoWrite");
                        bool aw = text9 == "true";
                        guiPageList.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                        m_GuiPageSaved.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                        m_GuiPageRollback.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                        m_GuiPageCacheSavedOne.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                        m_GuiPageCacheSavedTwo.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                        m_CuiPageCurrentSaved.Add(1);
                        num2 = 0;
                        foreach (XmlElement childNode3 in childNode2.ChildNodes)
                        {
                            text4 = childNode3.GetAttribute("Name");
                            text9 = childNode3.GetAttribute("Id");
                            short id = Convert.ToInt16(text9);
                            text9 = childNode3.GetAttribute("ParamId");
                            int[] index = Array.ConvertAll(text9.Split(','), (s) => int.Parse(s));
                            text9 = childNode3.GetAttribute("Action");
                            API_ACTION action = (API_ACTION)Enum.Parse(typeof(API_ACTION), text9);
                            text9 = childNode3.GetAttribute("AutoMode");
                            int[] mode = !(text9 == "0") ? Array.ConvertAll(text9.Split('~'), (s) => int.Parse(s)) : new int[1];
                            text9 = childNode3.GetAttribute("InFile");
                            bool inFile = text9 == "true";
                            text9 = childNode3.GetAttribute("FileMode");
                            File_Mode fileMode = (File_Mode)Enum.Parse(typeof(File_Mode), text9);
                            guiPageList[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index, mode, inFile, fileMode));
                            m_GuiPageSaved[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index, mode, inFile, fileMode));
                            m_GuiPageRollback[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index, mode, inFile, fileMode));
                            m_GuiPageCacheSavedOne[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index, mode, inFile, fileMode));
                            m_GuiPageCacheSavedTwo[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index, mode, inFile, fileMode));
                            num3 = 0;
                            foreach (XmlElement childNode4 in childNode3.ChildNodes)
                            {
                                text4 = childNode4.GetAttribute("Name");
                                text5 = childNode4.GetAttribute("Text");
                                text6 = childNode4.GetAttribute("GuiType");
                                text7 = childNode4.GetAttribute("Param");
                                text7 = text7.Trim();
                                text8 = childNode4.GetAttribute("Info");
                                int xsize = Convert.ToInt32(childNode4.GetAttribute("M"));
                                int ysize = Convert.ToInt32(childNode4.GetAttribute("N"));
                                text9 = childNode4.GetAttribute("Min");
                                long min = ConvertStringToNum(text9);
                                text9 = childNode4.GetAttribute("Max");
                                long max = ConvertStringToNum(text9);
                                if (text8.Equals("") && text6 != "")
                                {
                                    text8 = text5 + " ( " + min + " ~ " + max + " )";
                                }
                                text9 = childNode4.GetAttribute("Type");
                                ITEM_TYPE itemType = text9 != "" ? (ITEM_TYPE)Enum.Parse(typeof(ITEM_TYPE), text9) : ITEM_TYPE.NULL;
                                guiPageList[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                                m_GuiPageSaved[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                                m_GuiPageRollback[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                                m_GuiPageCacheSavedOne[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                                m_GuiPageCacheSavedTwo[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                                if (childNode4.InnerText != "")
                                {
                                    guiPageList[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode4);
                                    m_GuiPageSaved[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode4);
                                    m_GuiPageRollback[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode4);
                                    m_GuiPageCacheSavedOne[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode4);
                                    m_GuiPageCacheSavedTwo[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode4);
                                }
                                num3++;
                            }
                            num2++;
                        }
                        num++;
                    }
                }
            }
            else
            {
                m_binVersion = "0.0";
                m_ispVersion = xmlNode.Attributes["Version"].Value;
                foreach (XmlElement childNode5 in xmlNode.ChildNodes)
                {
                    text4 = childNode5.GetAttribute("Name");
                    text10 = childNode5.GetAttribute("Type");
                    text9 = childNode5.GetAttribute("GroupIndex");
                    short gIndex = Convert.ToInt16(text9);
                    PAGE_ACTION pAGE_ACTION = (PAGE_ACTION)Enum.Parse(value: childNode5.GetAttribute("Action"), enumType: typeof(PAGE_ACTION));
                    text9 = childNode5.GetAttribute("AutoWrite");
                    bool aw = text9 == "true";
                    guiPageList.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                    m_GuiPageSaved.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                    m_GuiPageRollback.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                    m_GuiPageCacheSavedOne.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                    m_GuiPageCacheSavedTwo.Add(new GuiPage(text4, text10, gIndex, pAGE_ACTION, aw));
                    m_CuiPageCurrentSaved.Add(1);
                    num2 = 0;
                    foreach (XmlElement childNode6 in childNode5.ChildNodes)
                    {
                        text4 = childNode6.GetAttribute("Name");
                        text9 = childNode6.GetAttribute("Id");
                        short id = Convert.ToInt16(text9);
                        text9 = childNode6.GetAttribute("ParamId");
                        int[] index2 = Array.ConvertAll(text9.Split(','), (s) => int.Parse(s));
                        text9 = childNode6.GetAttribute("Action");
                        API_ACTION action = (API_ACTION)Enum.Parse(typeof(API_ACTION), text9);
                        text9 = childNode6.GetAttribute("AutoMode");
                        int[] mode2 = !(text9 == "0") ? Array.ConvertAll(text9.Split('~'), (s) => int.Parse(s)) : new int[1];
                        text9 = childNode6.GetAttribute("InFile");
                        bool inFile = text9 == "true";
                        text9 = childNode6.GetAttribute("FileMode");
                        File_Mode fileMode = (File_Mode)Enum.Parse(typeof(File_Mode), text9);
                        guiPageList[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index2, mode2, inFile, fileMode));
                        m_GuiPageSaved[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index2, mode2, inFile, fileMode));
                        m_GuiPageRollback[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index2, mode2, inFile, fileMode));
                        m_GuiPageCacheSavedOne[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index2, mode2, inFile, fileMode));
                        m_GuiPageCacheSavedTwo[num].GroupList.Add(new GuiGroup(text4, id, pAGE_ACTION, action, index2, mode2, inFile, fileMode));
                        num3 = 0;
                        foreach (XmlElement childNode7 in childNode6.ChildNodes)
                        {
                            text4 = childNode7.GetAttribute("Name");
                            text5 = childNode7.GetAttribute("Text");
                            text6 = childNode7.GetAttribute("GuiType");
                            text7 = childNode7.GetAttribute("Param");
                            text7 = text7.Trim();
                            text8 = childNode7.GetAttribute("Info");
                            int xsize = Convert.ToInt32(childNode7.GetAttribute("M"));
                            int ysize = Convert.ToInt32(childNode7.GetAttribute("N"));
                            text9 = childNode7.GetAttribute("Min");
                            long min = ConvertStringToNum(text9);
                            text9 = childNode7.GetAttribute("Max");
                            long max = ConvertStringToNum(text9);
                            if (text8.Equals("") && text6 != "")
                            {
                                text8 = text5 + " ( " + min + " ~ " + max + " )";
                            }
                            text9 = childNode7.GetAttribute("Type");
                            ITEM_TYPE itemType = text9 != "" ? (ITEM_TYPE)Enum.Parse(typeof(ITEM_TYPE), text9) : ITEM_TYPE.NULL;
                            guiPageList[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                            m_GuiPageSaved[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                            m_GuiPageRollback[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                            m_GuiPageCacheSavedOne[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                            m_GuiPageCacheSavedTwo[num].GroupList[num2].ItemList.Add(new GuiItem(text4, text5, text6, text7, text8, xsize, ysize, min, max, itemType));
                            if (childNode7.InnerText != "")
                            {
                                guiPageList[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode7);
                                m_GuiPageSaved[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode7);
                                m_GuiPageRollback[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode7);
                                m_GuiPageCacheSavedOne[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode7);
                                m_GuiPageCacheSavedTwo[num].GroupList[num2].ItemList[num3].ReadValueFromXml(childNode7);
                            }
                            num3++;
                        }
                        num2++;
                    }
                    num++;
                }
            }
        }
        catch (Exception ex)
        {
            text3 = "[Error] ParseXML: " + text4 + " error " + ex.Message;
            ShowMessage(text3, "");
        }
        return text3;
    }

    public string SaveXml(string strFilePath)
    {
        XmlDocument xmlDoc = m_xmlDoc;
        string result = "";
        foreach (GuiPage item in m_GuiPageArray)
        {
            item.SaveValueToXml(xmlDoc);
        }
        xmlDoc.Save(strFilePath);
        return result;
    }

    public string SaveCvtDesXml(string strFilePath)
    {
        XmlDocument xmlDoc = m_xmlDoc;
        string result = "";
        foreach (GuiPage item in m_CvtDesGuiPageArray)
        {
            item.SaveValueToXml(xmlDoc);
        }
        xmlDoc.Save(strFilePath);
        return result;
    }

    public byte[] ParseXmlToByte()
    {
        XmlDocument xmlDoc = m_xmlDoc;
        foreach (GuiPage item in m_GuiPageArray)
        {
            item.SaveValueToXml(xmlDoc);
        }
        string s = xmlDoc.InnerXml.ToString();
        return Encoding.ASCII.GetBytes(s);
    }

    public void ShowMessage(string msgBox, string msgText)
    {
        if (msgBox != "")
        {
            MessageBox.Show(msgBox, "Status", MessageBoxButtons.OK);
        }
        if (msgText != "")
        {
            m_textMsgLog.Text = msgText + Environment.NewLine + m_textMsgLog.Text;
        }
    }

    private void UpdateGUI()
    {
        m_bIsCreatingGui = true;
        if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("RGBGAMMA"))
        {
            UpdateRGBGammaControl(m_PageControlArray[m_CurrPageIndex]);
        }
        else if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("YUVGAMMA"))
        {
            UpdateYUVGammaControl(m_PageControlArray[m_CurrPageIndex]);
        }
        else if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurve"))
        {
            UpdateWDRCurveControl(m_PageControlArray[m_CurrPageIndex]);
        }
        else if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurveFull"))
        {
            UpdateWDRCurve_v2Control(m_PageControlArray[m_CurrPageIndex]);
        }
        else
        {
            UpdatePageControl(m_GuiPageArray[m_CurrPageIndex], m_PageControlArray[m_CurrPageIndex]);
        }
        m_bIsCreatingGui = false;
    }

    private void UpdateCvtSrcGUI()
    {
        m_bIsCreatingGui = true;
        if (m_CvtSrcGuiPageArray[m_CurrPageIndex].PageType.Equals("RGBGAMMA"))
        {
            UpdateRGBGammaControl(m_CvtSrcPageControlArray);
        }
        else if (m_CvtSrcGuiPageArray[m_CurrPageIndex].PageType.Equals("YUVGAMMA"))
        {
            UpdateYUVGammaControl(m_CvtSrcPageControlArray);
        }
        else if (m_CvtSrcGuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurve"))
        {
            UpdateWDRCurveControl(m_CvtSrcPageControlArray);
        }
        else if (m_CvtSrcGuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurveFull"))
        {
            UpdateWDRCurve_v2Control(m_CvtSrcPageControlArray);
        }
        else
        {
            UpdatePageControl(m_CvtSrcGuiPageArray[m_CurrPageIndex], m_CvtSrcPageControlArray);
        }
        m_bIsCreatingGui = false;
    }

    private void UpdateCvtDesGUI()
    {
        m_bIsCreatingGui = true;
        if (m_CvtDesGuiPageArray[m_CurrPageIndex].PageType.Equals("RGBGAMMA"))
        {
            UpdateRGBGammaControl(m_CvtDesPageControlArray);
        }
        else if (m_CvtDesGuiPageArray[m_CurrPageIndex].PageType.Equals("YUVGAMMA"))
        {
            UpdateYUVGammaControl(m_CvtDesPageControlArray);
        }
        else if (m_CvtDesGuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurve"))
        {
            UpdateWDRCurveControl(m_CvtDesPageControlArray);
        }
        else if (m_CvtDesGuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurveFull"))
        {
            UpdateWDRCurve_v2Control(m_CvtDesPageControlArray);
        }
        else
        {
            UpdatePageControl(m_CvtDesGuiPageArray[m_CurrPageIndex], m_CvtDesPageControlArray);
        }
        m_bIsCreatingGui = false;
    }

    private void UpdatePageControl(GuiPage page, List<List<Control>> array)
    {
        for (int i = 0; i < page.GroupList.Count; i++)
        {
            GuiGroup group = page.GroupList[i];
            List<Control> controlList = array[i];
            SetControlGroupValue(controlList, group);
        }
    }

    private void SetControlGroupValue(List<Control> controlList, GuiGroup group)
    {
        int num = -1;
        try
        {
            foreach (Control control in controlList)
            {
                while (group.ItemList[++num].Tag != control.Tag.ToString())
                {
                }
                SetControlItemValue(control, group.ItemList[num]);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(group.Name + "." + group.ItemList[num].Text + "\n" + ex.Message, "Status", MessageBoxButtons.OK);
        }
    }

    private void SetControlItemValue(Control control, GuiItem item)
    {
        try
        {
            switch (item.GuiType)
            {
                case "CheckBox":
                    (control as CheckBox).Checked = item.DataValue[0] != 0L;
                    break;
                case "ComboBox":
                    (control as ComboBox).SelectedIndex = (int)item.DataValue[0];
                    break;
                case "ComboBox_V":
                    (control as ComboBox).SelectedIndex = (int)item.DataValue[0];
                    break;
                case "TextBox":
                    (control as TextBox).Text = item.ToText().Trim();
                    break;
                case "TextBox_Num":
                    (control as TextBox).Text = item.DataValue[0].ToString();
                    break;
                case "NumScroll":
                    (control as NumericUpDown).Value = item.DataValue[0];
                    break;
                case "RadioButton_ByPass":
                    (control.Controls[0] as RadioButton).Checked = item.DataValue[0] == 1;
                    (control.Controls[1] as RadioButton).Checked = item.DataValue[0] == 0;
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(item.Text + "\n" + ex.Message, "Status", MessageBoxButtons.OK);
        }
    }

    private GuiGroup GetGuiGroupByItemName(string tagName)
    {
        for (int i = 0; i < m_GuiPageArray[m_CurrPageIndex].GroupList.Count; i++)
        {
            for (int j = 0; j < m_GuiPageArray[m_CurrPageIndex].GroupList[i].ItemList.Count; j++)
            {
                if (tagName == m_GuiPageArray[m_CurrPageIndex].GroupList[i].ItemList[j].Tag)
                {
                    return m_GuiPageArray[m_CurrPageIndex].GroupList[i];
                }
            }
        }
        return null;
    }

    private GuiGroup GetCvtSrcGuiGroupByItemName(string tagName)
    {
        for (int i = 0; i < m_CvtSrcGuiPageArray[m_CurrPageIndex].GroupList.Count; i++)
        {
            for (int j = 0; j < m_CvtSrcGuiPageArray[m_CurrPageIndex].GroupList[i].ItemList.Count; j++)
            {
                if (tagName == m_CvtSrcGuiPageArray[m_CurrPageIndex].GroupList[i].ItemList[j].Tag)
                {
                    return m_CvtSrcGuiPageArray[m_CurrPageIndex].GroupList[i];
                }
            }
        }
        return null;
    }

    private GuiGroup GetCvtDesGuiGroupByItemName(string tagName)
    {
        for (int i = 0; i < m_CvtDesGuiPageArray[m_CurrPageIndex].GroupList.Count; i++)
        {
            for (int j = 0; j < m_CvtDesGuiPageArray[m_CurrPageIndex].GroupList[i].ItemList.Count; j++)
            {
                if (tagName == m_CvtDesGuiPageArray[m_CurrPageIndex].GroupList[i].ItemList[j].Tag)
                {
                    return m_CvtDesGuiPageArray[m_CurrPageIndex].GroupList[i];
                }
            }
        }
        return null;
    }

    private GuiItem FindGuiItemByItemName(GuiPage page, string tagName)
    {
        for (int i = 0; i < page.GroupList.Count; i++)
        {
            for (int j = 0; j < page.GroupList[i].ItemList.Count; j++)
            {
                if (tagName == page.GroupList[i].ItemList[j].Tag)
                {
                    return page.GroupList[i].ItemList[j];
                }
            }
        }
        return null;
    }

    private void UpdateRGBGammaControl(List<List<Control>> container)
    {
        ((RGBGamma)container[0][0]).UpdatePage();
    }

    private void UpdateYUVGammaControl(List<List<Control>> container)
    {
        ((YUVGamma)container[0][0]).UpdatePage();
    }

    private void UpdateWDRCurveControl(List<List<Control>> container)
    {
        ((WDRCurve)container[0][0]).UpdatePage();
    }

    private void UpdateWDRCurve_v2Control(List<List<Control>> container)
    {
        ((WDRCurve_v2)container[0][0]).UpdatePage();
    }

    public void ReadPage()
    {
        if (m_DbgParser.IsConnected())
        {
            m_DbgParser.updateTaskQueue(ReadPageTask, m_GuiPageArray[m_CurrPageIndex]);
        }
    }

    public void ReadUartPage()
    {
        m_GuiPageArray[m_CurrPageIndex].ReadUartPage(m_DbgParser);
        m_ContentPanel.Invoke(new UpdateGUIHandler(UpdateGUI));
    }

    public void WritePage()
    {
        if (m_DbgParser.IsConnected())
        {
            if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("RGBGAMMA"))
            {
                (m_PageControlArray[m_CurrPageIndex][0][0] as RGBGamma).SaveGammaValue();
            }
            else if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("YUVGAMMA"))
            {
                (m_PageControlArray[m_CurrPageIndex][0][0] as YUVGamma).SaveGammaValue();
            }
            else if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurve"))
            {
                (m_PageControlArray[m_CurrPageIndex][0][0] as WDRCurve).SaveGammaValue();
            }
            else if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurveFull"))
            {
                (m_PageControlArray[m_CurrPageIndex][0][0] as WDRCurve_v2).SaveGammaValue();
            }
            m_DbgParser.updateTaskQueue(SendPageTask, m_GuiPageArray[m_CurrPageIndex]);
        }
    }

    public void WriteUartPage()
    {
        if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("RGBGAMMA"))
        {
            (m_PageControlArray[m_CurrPageIndex][0][0] as RGBGamma).SaveGammaValue();
            return;
        }
        if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("YUVGAMMA"))
        {
            (m_PageControlArray[m_CurrPageIndex][0][0] as YUVGamma).SaveGammaValue();
            return;
        }
        if (m_GuiPageArray[m_CurrPageIndex].PageType.Equals("WDRCurve"))
        {
            (m_PageControlArray[m_CurrPageIndex][0][0] as WDRCurve).SaveGammaValue();
            return;
        }
        m_GuiPageArray[m_CurrPageIndex].WriteUartPage(m_DbgParser);
        string text = "";
        text = m_GuiPageArray[m_CurrPageIndex].getmv5length(m_DbgParser);
        ShowMessage("write over", "length=" + text);
    }

    public void ReadAllPage()
    {
        if (m_DbgParser.IsConnected())
        {
            m_DbgParser.updateTaskQueue(ReadAllTask, null);
            ShowProgressBar(showProgress: true, "Recv Data...");
        }
    }

    public void WriteAllPage()
    {
        if (m_DbgParser.IsConnected())
        {
            m_DbgParser.updateTaskQueue(SendAllTask, null);
        }
    }

    public byte[] GetBinBytes(ChipID chipID, uint magicKey, ushort checksumVer)
    {
        int num = 0;
        int num2 = 0;
        byte[][] array = new byte[m_GuiPageArray.Count][];
        for (int i = 0; i < m_GuiPageArray.Count; i++)
        {
            array[i] = m_GuiPageArray[i].GetBinBytes(magicKey);
            num += array[i].Length;
        }
        byte[] array2 = new byte[num];
        for (int j = 0; j < m_GuiPageArray.Count; j++)
        {
            array[j].CopyTo(array2, num2);
            num2 += array[j].Length;
        }
        byte[] array3 = new byte[32];
        array3.Initialize();
        int value = num;
        BitConverter.GetBytes((int)chipID).CopyTo(array3, 0);
        int num3 = 0;
        uint value2 = 0u;
        ushort value3 = 0;
        uint value4 = 0u;
        num3 = int.Parse(m_ispVersion.Split('.').ElementAt(0));
        BitConverter.GetBytes(int.Parse(m_ispVersion.Split('.').ElementAt(1))).CopyTo(array3, 4);
        BitConverter.GetBytes(num3).CopyTo(array3, 6);
        BitConverter.GetBytes(value).CopyTo(array3, 8);
        BitConverter.GetBytes(CRC32.GetCRC32(array2, checksumVer)).CopyTo(array3, 12);
        BitConverter.GetBytes(magicKey).CopyTo(array3, 16);
        BitConverter.GetBytes(value2).CopyTo(array3, 20);
        BitConverter.GetBytes(checksumVer).CopyTo(array3, 24);
        BitConverter.GetBytes(value3).CopyTo(array3, 26);
        BitConverter.GetBytes(value4).CopyTo(array3, 28);
        byte[] array4 = new byte[array3.Length + num];
        array3.CopyTo(array4, 0);
        array2.CopyTo(array4, array3.Length);
        return array4;
    }

    public byte[] GetBinBytesXmlCRC(ChipID chipID, uint magicKey, uint videomagicKey, string zipPath, ushort checksumVer)
    {
        int num = 0;
        int num2 = 0;
        byte[][] array = new byte[m_GuiPageArray.Count][];
        for (int i = 0; i < m_GuiPageArray.Count; i++)
        {
            array[i] = m_GuiPageArray[i].GetBinBytes(magicKey, videomagicKey);
            num += array[i].Length;
        }
        byte[] array2 = new byte[num];
        for (int j = 0; j < m_GuiPageArray.Count; j++)
        {
            array[j].CopyTo(array2, num2);
            num2 += array[j].Length;
        }
        byte[] array3 = File.ReadAllBytes(zipPath);
        byte[] array4 = new byte[32];
        array4.Initialize();
        int value = num;
        uint value2 = 0u;
        ushort value3 = 0;
        BitConverter.GetBytes((int)chipID).CopyTo(array4, 0);
        int value4 = 0;
        int value5 = 0;
        if (m_ispVersion != "")
        {
            value4 = int.Parse(m_ispVersion.Split('.').ElementAt(0));
            value5 = int.Parse(m_ispVersion.Split('.').ElementAt(1));
        }
        BitConverter.GetBytes(value5).CopyTo(array4, 4);
        BitConverter.GetBytes(value4).CopyTo(array4, 6);
        BitConverter.GetBytes(value).CopyTo(array4, 8);
        byte[] array5 = new byte[array4.Length - 4 + array2.Length + array3.Length];
        BitConverter.GetBytes((int)chipID).CopyTo(array5, 0);
        BitConverter.GetBytes(value5).CopyTo(array5, 4);
        BitConverter.GetBytes(value4).CopyTo(array5, 6);
        BitConverter.GetBytes(value).CopyTo(array5, 8);
        BitConverter.GetBytes(magicKey).CopyTo(array5, 12);
        int value6 = 0;
        int value7 = 0;
        if (m_binVersion != "")
        {
            value6 = int.Parse(m_binVersion.Split('.').ElementAt(0));
            value7 = int.Parse(m_binVersion.Split('.').ElementAt(1));
        }
        BitConverter.GetBytes(value7).CopyTo(array5, 16);
        BitConverter.GetBytes(value6).CopyTo(array5, 18);
        int num3 = 0;
        if (m_videoVersion != "")
        {
            num3 = int.Parse(m_videoVersion.Split('.').ElementAt(0));
            BitConverter.GetBytes(int.Parse(m_videoVersion.Split('.').ElementAt(1))).CopyTo(array5, 20);
            BitConverter.GetBytes(num3).CopyTo(array5, 22);
        }
        BitConverter.GetBytes(value2).CopyTo(array5, 24);
        array2.CopyTo(array5, 28);
        array3.CopyTo(array5, 28 + array2.Length);
        BitConverter.GetBytes(CRC32.GetCRC32(array5, checksumVer)).CopyTo(array4, 12);
        BitConverter.GetBytes(magicKey).CopyTo(array4, 16);
        BitConverter.GetBytes(value7).CopyTo(array4, 20);
        BitConverter.GetBytes(value6).CopyTo(array4, 22);
        BitConverter.GetBytes(checksumVer).CopyTo(array4, 24);
        BitConverter.GetBytes(value3).CopyTo(array4, 26);
        byte[] array6 = new byte[array4.Length + num];
        array4.CopyTo(array6, 0);
        array2.CopyTo(array6, array4.Length);
        return array6;
    }

    public byte[] GetCvtDesBinBytes(ChipID chipID, uint magicKey, ushort checksumVer)
    {
        int num = 0;
        int num2 = 0;
        byte[][] array = new byte[m_CvtDesGuiPageArray.Count][];
        for (int i = 0; i < m_CvtDesGuiPageArray.Count; i++)
        {
            array[i] = m_CvtDesGuiPageArray[i].GetBinBytes(magicKey);
            num += array[i].Length;
        }
        byte[] array2 = new byte[num];
        for (int j = 0; j < m_CvtDesGuiPageArray.Count; j++)
        {
            array[j].CopyTo(array2, num2);
            num2 += array[j].Length;
        }
        byte[] array3 = new byte[32];
        array3.Initialize();
        int value = num;
        uint value2 = 0u;
        ushort value3 = 0;
        uint value4 = 0u;
        BitConverter.GetBytes((int)chipID).CopyTo(array3, 0);
        int value5 = int.Parse(m_ispVersion.Split('.').ElementAt(0));
        BitConverter.GetBytes(int.Parse(m_ispVersion.Split('.').ElementAt(1))).CopyTo(array3, 4);
        BitConverter.GetBytes(value5).CopyTo(array3, 6);
        BitConverter.GetBytes(value).CopyTo(array3, 8);
        BitConverter.GetBytes(CRC32.GetCRC32(array2, checksumVer)).CopyTo(array3, 12);
        BitConverter.GetBytes(magicKey).CopyTo(array3, 16);
        BitConverter.GetBytes(value2).CopyTo(array3, 20);
        BitConverter.GetBytes(checksumVer).CopyTo(array3, 24);
        BitConverter.GetBytes(value3).CopyTo(array3, 26);
        BitConverter.GetBytes(value4).CopyTo(array3, 28);
        byte[] array4 = new byte[array3.Length + num];
        array3.CopyTo(array4, 0);
        array2.CopyTo(array4, array3.Length);
        return array4;
    }

    public byte[] GetCvtDesBinBytesXmlCRC(ChipID chipID, uint magicKey, uint videomagicKey, string zipPath, ushort checksumVer)
    {
        int num = 0;
        int num2 = 0;
        byte[][] array = new byte[m_CvtDesGuiPageArray.Count][];
        for (int i = 0; i < m_CvtDesGuiPageArray.Count; i++)
        {
            array[i] = m_CvtDesGuiPageArray[i].GetBinBytes(magicKey, videomagicKey);
            num += array[i].Length;
        }
        byte[] array2 = new byte[num];
        for (int j = 0; j < m_CvtDesGuiPageArray.Count; j++)
        {
            array[j].CopyTo(array2, num2);
            num2 += array[j].Length;
        }
        byte[] array3 = File.ReadAllBytes(zipPath);
        byte[] array4 = new byte[32];
        array4.Initialize();
        int value = num;
        uint value2 = 0u;
        ushort value3 = 0;
        BitConverter.GetBytes((int)chipID).CopyTo(array4, 0);
        int value4 = 0;
        int value5 = 0;
        if (m_ispVersion != "")
        {
            value4 = int.Parse(m_ispVersion.Split('.').ElementAt(0));
            value5 = int.Parse(m_ispVersion.Split('.').ElementAt(1));
        }
        BitConverter.GetBytes(value5).CopyTo(array4, 4);
        BitConverter.GetBytes(value4).CopyTo(array4, 6);
        BitConverter.GetBytes(value).CopyTo(array4, 8);
        byte[] array5 = new byte[array4.Length - 4 + array2.Length + array3.Length];
        BitConverter.GetBytes((int)chipID).CopyTo(array5, 0);
        BitConverter.GetBytes(value5).CopyTo(array5, 4);
        BitConverter.GetBytes(value4).CopyTo(array5, 6);
        BitConverter.GetBytes(value).CopyTo(array5, 8);
        BitConverter.GetBytes(magicKey).CopyTo(array5, 12);
        int value6 = 0;
        int value7 = 0;
        if (m_binVersion != "")
        {
            value6 = int.Parse(m_binVersion.Split('.').ElementAt(0));
            value7 = int.Parse(m_binVersion.Split('.').ElementAt(1));
        }
        BitConverter.GetBytes(value7).CopyTo(array5, 16);
        BitConverter.GetBytes(value6).CopyTo(array5, 18);
        int num3 = 0;
        if (m_videoVersion != "")
        {
            num3 = int.Parse(m_videoVersion.Split('.').ElementAt(0));
            BitConverter.GetBytes(int.Parse(m_videoVersion.Split('.').ElementAt(1))).CopyTo(array5, 20);
            BitConverter.GetBytes(num3).CopyTo(array5, 22);
        }
        BitConverter.GetBytes(value2).CopyTo(array5, 24);
        array2.CopyTo(array5, 28);
        array3.CopyTo(array5, 28 + array2.Length);
        BitConverter.GetBytes(CRC32.GetCRC32(array5, checksumVer)).CopyTo(array4, 12);
        BitConverter.GetBytes(magicKey).CopyTo(array4, 16);
        BitConverter.GetBytes(value7).CopyTo(array4, 20);
        BitConverter.GetBytes(value6).CopyTo(array4, 22);
        BitConverter.GetBytes(checksumVer).CopyTo(array4, 24);
        BitConverter.GetBytes(value3).CopyTo(array4, 26);
        byte[] array6 = new byte[array4.Length + num];
        array4.CopyTo(array6, 0);
        array2.CopyTo(array6, array4.Length);
        return array6;
    }

    public void DecompressGZipfile(string zip, string src)
    {
        FileStream fileStream = File.OpenRead(zip);
        FileStream fileStream2 = File.Create(src);
        GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        int num;
        while ((num = gZipStream.ReadByte()) != -1)
        {
            fileStream2.WriteByte((byte)num);
        }
        gZipStream.Close();
        fileStream.Close();
        fileStream2.Close();
    }

    public void GetBinFileVer(string filePath)
    {
        BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
        BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        _ = 1835336475;
        BinAPIVerMinor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BinAPIVerMajor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        binaryReader.Close();
    }

    public void GetChipIDByXml(string xmlPath)
    {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(xmlPath);
        XmlNode xmlNode = xmlDocument.SelectSingleNode("/ISP_ITEM");
        m_binVersion = "0.0";
        if (xmlNode == null)
        {
            xmlNode = xmlDocument.SelectSingleNode("/API_XML");
            m_binVersion = "1.0";
        }
        string value = xmlNode.Attributes["Name"].Value;
        XmlChipID = (ChipID)m_DbgParser.GetChipIDByName(value);
        switch (value)
        {
            case "iNfinityI6E":
            case "Pudding":
            case "iNfinityI6B0":
            case "Ispahan":
            case "Pioneer3":
            case "Ikayaki":
                XmlChipID = ChipID.I6E;
                break;
        }
    }

    public void GetBinFileChip(string filePath)
    {
        BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
        int num = (int)(BinChipID = (ChipID)BitConverter.ToInt32(binaryReader.ReadBytes(4), 0));
        if (num == 1835336481 || num == 1835336482 || num == 1835336484)
        {
            BinChipID = ChipID.I6E;
        }
        binaryReader.Close();
    }

    public void GetCvtSrcBinFileVer(string filePath)
    {
        BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
        BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        _ = 1835336475;
        BinAPIVerMinor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BinAPIVerMajor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        binaryReader.Close();
    }

    public void LoadCvtSrcBinFile(string filePath)
    {
        BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
        BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        _ = 1835336475;
        BinAPIVerMinor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BinAPIVerMajor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        int count = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        uint checkNum = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        uint magicKey = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        m_BinChecksumVer = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        byte[] buffer = binaryReader.ReadBytes(count);
        if (CRC32.CheckCRC32(buffer, checkNum, m_BinChecksumVer))
        {
            ParseCvtSrcBinBuffer(buffer, magicKey);
        }
        else
        {
            ShowMessage("CRC32 is not match", "");
        }
        binaryReader.Close();
    }

    public int LoadCvtSrcBinXmlFile(string filePath)
    {
        int num = 0;
        string SrcPath = "";
        string ZipPath = "";
        byte[] buffer = new byte[1];
        uint magicKey = 0u;
        if ((num = ChkCRCforLoadBinXmlFile(filePath, ref SrcPath, ref ZipPath, ref buffer, ref magicKey)) == 1)
        {
            if (File.Exists(SrcPath))
            {
                File.Delete(SrcPath);
            }
            if (File.Exists(ZipPath))
            {
                File.Delete(ZipPath);
            }
            return num;
        }
        InitCvtSrcGUI(SrcPath);
        ParseCvtSrcBinBuffer(buffer, magicKey);
        if (File.Exists(SrcPath))
        {
            File.Delete(SrcPath);
        }
        if (File.Exists(ZipPath))
        {
            File.Delete(ZipPath);
        }
        return num;
    }

    public int LoadCvtSrcXmlFile(string filePath)
    {
        int result = 0;
        if (!InitCvtSrcGUI(filePath))
        {
            result = 1;
        }
        return result;
    }

    public void LoadCvtDesBinFile(string filePath)
    {
        BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
        BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        _ = 1835336475;
        BinAPIVerMinor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BinAPIVerMajor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        int count = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        uint checkNum = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        uint magicKey = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        m_BinChecksumVer = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        byte[] buffer = binaryReader.ReadBytes(count);
        if (CRC32.CheckCRC32(buffer, checkNum, m_BinChecksumVer))
        {
            ParseCvtDesBinBuffer(buffer, magicKey);
        }
        else
        {
            ShowMessage("CRC32 is not match", "");
        }
        binaryReader.Close();
    }

    public int LoadCvtDesBinXmlFile(string filePath)
    {
        int result = 0;
        BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
        BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        _ = 1835336475;
        BinAPIVerMinor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BinAPIVerMajor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        int num = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        uint checkNum = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        uint magicKey = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        m_BinChecksumVer = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        byte[] buffer = binaryReader.ReadBytes(num);
        int num2 = 32;
        if ((int)binaryReader.BaseStream.Length > num + num2)
        {
            try
            {
                int count = (int)binaryReader.BaseStream.Length - (num + num2);
                byte[] buffer2 = binaryReader.ReadBytes(count);
                string text = Application.StartupPath + "\\XmlTmp.xml";
                string text2 = Application.StartupPath + "\\XmlTmp.zip";
                BinaryWriter binaryWriter = new BinaryWriter(File.Open(text2, FileMode.Create));
                binaryWriter.Write(buffer2);
                binaryWriter.Close();
                DecompressGZipfile(text2, text);
                InitCvtDesGUI(text);
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
                if (File.Exists(text2))
                {
                    File.Delete(text2);
                }
                if (CRC32.CheckCRC32(buffer, checkNum, m_BinChecksumVer))
                {
                    ParseCvtDesBinBuffer(buffer, magicKey);
                }
                else
                {
                    ShowMessage("CRC32 is not match", "");
                    result = 1;
                }
            }
            catch
            {
                ShowMessage("Bin format (Bin+Xml) is not match", "");
                result = 1;
            }
        }
        else
        {
            result = 1;
        }
        binaryReader.Close();
        return result;
    }

    public void CheckAPIVerforEVB_Bin()
    {
        if (m_DbgParser.IsConnected())
        {
            GuiItem guiItem = null;
            GuiGroup guiGroup = null;
            GuiGroup guiGroup2 = null;
            int currPageIndex = m_CurrPageIndex;
            for (int i = 0; i < m_GuiPageArray.Count; i++)
            {
                m_CurrPageIndex = i;
                guiGroup = GetGuiGroupByItemName("Major");
                guiGroup2 = GetGuiGroupByItemName("Minor");
                if (guiGroup != null && guiGroup2 != null)
                {
                    break;
                }
            }
            if (guiGroup == null || guiGroup2 == null)
            {
                ShowMessage("Major or Minor in XML file is not found", "");
                APIVerIsMatch = false;
                return;
            }
            guiItem = GetGuiGroupByItemName("Major").FindItemByName("Major");
            EvbAPIVerMajor = (ushort)guiItem.DataValue[0];
            guiItem = GetGuiGroupByItemName("Minor").FindItemByName("Minor");
            EvbAPIVerMinor = (ushort)guiItem.DataValue[0];
            m_CurrPageIndex = currPageIndex;
            if (BinAPIVerMajor != EvbAPIVerMajor)
            {
                string msgBox = "";
                if (BinAPIVerMajor != EvbAPIVerMajor)
                {
                    msgBox = "Major API Version  is not match. Bin  Major: " + BinAPIVerMajor + ".  EVB Major: " + EvbAPIVerMajor + ".\n";
                }
                ShowMessage(msgBox, "");
                APIVerIsMatch = false;
            }
            else
            {
                APIVerIsMatch = true;
            }
        }
        else
        {
            ShowMessage("Device not connect. It Cannot Get API Version From EVB.", "");
            APIVerIsMatch = false;
        }
    }

    public void CheckAPIVerforEVB_XML()
    {
        if (m_DbgParser.IsConnected())
        {
            GuiItem guiItem = null;
            GuiGroup guiGroup = null;
            GuiGroup guiGroup2 = null;
            int currPageIndex = m_CurrPageIndex;
            for (int i = 0; i < m_GuiPageArray.Count; i++)
            {
                m_CurrPageIndex = i;
                guiGroup = GetGuiGroupByItemName("Major");
                guiGroup2 = GetGuiGroupByItemName("Minor");
                if (guiGroup != null && guiGroup2 != null)
                {
                    break;
                }
            }
            if (guiGroup == null || guiGroup2 == null)
            {
                ShowMessage("Major or Minor in XML file is not found", "");
                APIVerIsMatch = false;
                return;
            }
            guiItem = GetGuiGroupByItemName("Major").FindItemByName("Major");
            EvbAPIVerMajor = (ushort)guiItem.DataValue[0];
            guiItem = GetGuiGroupByItemName("Minor").FindItemByName("Minor");
            EvbAPIVerMinor = (ushort)guiItem.DataValue[0];
            m_CurrPageIndex = currPageIndex;
            int num = int.Parse(m_ispVersion.Split('.').ElementAt(0));
            int.Parse(m_ispVersion.Split('.').ElementAt(1));
            if (num != EvbAPIVerMajor)
            {
                APIVerIsMatch = false;
                string text = Application.StartupPath + "\\CvtXml\\I3_isp_api_" + EvbAPIVerMajor + "_" + EvbAPIVerMinor + ".xml";
                _ = Application.StartupPath + "\\I3_isp_api.xml";
                if (!File.Exists(text))
                {
                    return;
                }
                InitGUI(text);
                num = int.Parse(m_ispVersion.Split('.').ElementAt(0));
                int.Parse(m_ispVersion.Split('.').ElementAt(1));
                if (num != EvbAPIVerMajor)
                {
                    APIVerIsMatch = false;
                    string msgBox = "";
                    if (num != EvbAPIVerMajor)
                    {
                        msgBox = "Major API Version  is not match. Bin  Major: " + BinAPIVerMajor + ".  EVB Major: " + EvbAPIVerMajor + ".\n";
                    }
                    ShowMessage(msgBox, "");
                }
                else
                {
                    APIVerIsMatch = true;
                }
            }
            else
            {
                APIVerIsMatch = true;
            }
        }
        else
        {
            ShowMessage("Device not connect. It Cannot Get API Version From EVB.", "");
            APIVerIsMatch = false;
        }
    }

    public int ChkCRCforLoadBinFile(string filePath, ref byte[] buffer, ref uint magicKey)
    {
        BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
        BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        _ = 1835336475;
        BinAPIVerMinor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BinAPIVerMajor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        m_ispVersion = BinAPIVerMajor + "." + BinAPIVerMinor;
        int count = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        uint checkNum = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        magicKey = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        m_BinChecksumVer = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        buffer = binaryReader.ReadBytes(count);
        if (!CRC32.CheckCRC32(buffer, checkNum, m_BinChecksumVer))
        {
            ShowMessage("CRC32 is not match", "");
        }
        binaryReader.Close();
        return 0;
    }

    public int LoadBinFile(string filePath)
    {
        int result = 0;
        byte[] buffer = new byte[1];
        uint magicKey = 0u;
        if (ChkCRCforLoadBinFile(filePath, ref buffer, ref magicKey) == 1)
        {
            return 1;
        }
        if (m_DbgParser.IsConnected())
        {
            ChipID chipIDName = m_DbgParser.ChipIDName;
            _ = 1835336475;
            if (chipIDName >= ChipID.I3 || chipIDName == ChipID.M5 || chipIDName == ChipID.M5U)
            {
                ChipID chipID = chipIDName;
                switch (chipIDName)
                {
                    case ChipID.I6E:
                    case ChipID.I6B0:
                    case ChipID.P3:
                        chipID = ChipID.I6E;
                        break;
                    case ChipID.M5:
                    case ChipID.M5U:
                        chipID = ChipID.M5;
                        break;
                }
                if (chipIDName != ChipID.M5 && chipIDName != ChipID.M5U && chipIDName != ChipID.I6E && chipIDName != ChipID.I6B0 && chipIDName != ChipID.P3)
                {
                    GetChipIDByAPI(ref chipID);
                }
                GetBinFileChip(filePath);
                chipname = m_DbgParser.GetChipNameByID(chipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
                Binchipname = m_DbgParser.GetChipNameByID(BinChipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
                if (BinChipID != chipID)
                {
                    string text = "";
                    text = "[LoadBinFile] ChipID is not match. Bin  ChipID: " + Binchipname + ".  EVB chipID: " + chipname + ".\n";
                    ShowMessage(text, "");
                    APIVerIsMatch = false;
                    return 1;
                }
                APIVerIsMatch = true;
                short apiIDValue = m_DbgParser.GetApiIDValue(chipIDName, "ApiVersion");
                if (chipIDName == ChipID.M5 || chipIDName == ChipID.M5U)
                {
                    GetApiVersionForM5();
                }
                else
                {
                    GetApiVersionForInfinitySeries(chipID, apiIDValue);
                }
                GetBinFileVer(filePath);
                if (BinAPIVerMajor != EvbAPIVerMajor)
                {
                    string text2 = "";
                    text2 = "Major API Version  is not match. Bin  Major: " + BinAPIVerMajor + ".  EVB Major: " + EvbAPIVerMajor + ".\n";
                    ShowMessage(text2, "");
                    APIVerIsMatch = false;
                    result = 1;
                }
                else
                {
                    APIVerIsMatch = true;
                }
            }
        }
        ParseBinBuffer(buffer, magicKey);
        return result;
    }

    public int ChkCRCforLoadBinXmlFile(string filePath, ref string SrcPath, ref string ZipPath, ref byte[] buffer, ref uint magicKey)
    {
        int result = 0;
        BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
        uint value = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        _ = 1835336475;
        BinAPIVerMinor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        BinAPIVerMajor = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        m_ispVersion = BinAPIVerMajor + "." + BinAPIVerMinor;
        int num = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
        uint checkNum = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        magicKey = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        uint num2 = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        m_BinChecksumVer = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        ushort value2 = BitConverter.ToUInt16(binaryReader.ReadBytes(2), 0);
        uint value3 = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        if (num2 == 0)
        {
            buffer = binaryReader.ReadBytes(num);
            int num3 = 32;
            if ((int)binaryReader.BaseStream.Length > num + num3)
            {
                try
                {
                    int count = (int)binaryReader.BaseStream.Length - (num + num3);
                    byte[] buffer2 = binaryReader.ReadBytes(count);
                    SrcPath = Application.StartupPath + "\\XmlTmp.xml";
                    ZipPath = Application.StartupPath + "\\XmlTmp.zip";
                    BinaryWriter binaryWriter = new BinaryWriter(File.Open(ZipPath, FileMode.Create));
                    binaryWriter.Write(buffer2);
                    binaryWriter.Close();
                    DecompressGZipfile(ZipPath, SrcPath);
                    if (!CRC32.CheckCRC32(buffer, checkNum, m_BinChecksumVer))
                    {
                        ShowMessage("CRC32 is not match", "");
                        result = 1;
                    }
                }
                catch
                {
                    ShowMessage("Bin format (Bin+Xml) is not match", "");
                    result = 1;
                }
            }
            else
            {
                ShowMessage("It is not Bin+Xml file.", "");
                result = 1;
            }
        }
        else
        {
            byte[] array = new byte[binaryReader.BaseStream.Length - 4];
            BitConverter.GetBytes(value).CopyTo(array, 0);
            BitConverter.GetBytes(BinAPIVerMinor).CopyTo(array, 4);
            BitConverter.GetBytes(BinAPIVerMajor).CopyTo(array, 6);
            BitConverter.GetBytes(num).CopyTo(array, 8);
            BitConverter.GetBytes(magicKey).CopyTo(array, 12);
            BitConverter.GetBytes(num2).CopyTo(array, 16);
            BitConverter.GetBytes(m_BinChecksumVer).CopyTo(array, 20);
            BitConverter.GetBytes(value2).CopyTo(array, 22);
            BitConverter.GetBytes(value3).CopyTo(array, 24);
            buffer = binaryReader.ReadBytes(num);
            buffer.CopyTo(array, 28);
            int num4 = 32;
            if ((int)binaryReader.BaseStream.Length > num + num4)
            {
                try
                {
                    int count2 = (int)binaryReader.BaseStream.Length - (num + num4);
                    byte[] array2 = binaryReader.ReadBytes(count2);
                    array2.CopyTo(array, 28 + num);
                    if (CRC32.CheckCRC32(array, checkNum, m_BinChecksumVer))
                    {
                        SrcPath = Application.StartupPath + "\\XmlTmp.xml";
                        ZipPath = Application.StartupPath + "\\XmlTmp.zip";
                        BinaryWriter binaryWriter2 = new BinaryWriter(File.Open(ZipPath, FileMode.Create));
                        binaryWriter2.Write(array2);
                        binaryWriter2.Close();
                        DecompressGZipfile(ZipPath, SrcPath);
                    }
                    else
                    {
                        ShowMessage("CRC32 is not match", "");
                        result = 1;
                    }
                }
                catch
                {
                    ShowMessage("Bin format (Bin+Xml) is not match", "");
                    result = 1;
                }
            }
            else
            {
                ShowMessage("It is not Bin+Xml file.", "");
                result = 1;
            }
        }
        binaryReader.Close();
        return result;
    }

    private void GetChipIDByAPI(ref ChipID chipID)
    {
        byte[] pbyRcvData = null;
        int num = 0;
        if (m_DbgParser.ConnectMode == CONNECT_MODE.MODE_SOCKET)
        {
            num = m_DbgParser.ReceiveApiPacket(100, out pbyRcvData);
        }
        else if (m_DbgParser.ConnectMode == CONNECT_MODE.MODE_USB)
        {
            num = m_DbgParser.ReceiveUsbApiPacket(100, out pbyRcvData);
        }
        if (num > 0)
        {
            PacketParser.ConvertBufferToType(ref chipID, pbyRcvData);
        }
    }

    private void GetApiVersionForI3()
    {
        short apiIDValue = m_DbgParser.GetApiIDValue(ChipID.I3, "ApiVersion");
        if (m_DbgParser.ReceiveApiPacket(apiIDValue, out var pbyRcvData) > 0)
        {
            PacketParser.ConvertBufferToTypeForI3(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
        }
    }

    private void GetApiVersionForInfinitySeries(ChipID chipID, short APIVersion)
    {
        int num = 0;
        byte[] pbyRcvData;
        if (m_DbgParser.ConnectMode == CONNECT_MODE.MODE_SOCKET)
        {
            num = m_DbgParser.ReceiveApiPacket(APIVersion, out pbyRcvData);
        }
        else if (m_DbgParser.ConnectMode == CONNECT_MODE.MODE_USB)
        {
            num = m_DbgParser.ReceiveUsbApiPacket(APIVersion, out pbyRcvData);
        }
        else
        {
            if (m_DbgParser.ConnectMode != CONNECT_MODE.MODE_UART)
            {
                return;
            }
            num = m_DbgParser.ReceiveUartApiPacket(APIVersion, out pbyRcvData);
        }
        if (num > 0)
        {
            switch (chipID)
            {
                case ChipID.I3:
                    PacketParser.ConvertBufferToTypeForI3(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
                    break;
                default:
                    PacketParser.ConvertBufferToType(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
                    break;
                case ChipID.M5:
                case ChipID.M5U:
                case ChipID.I1:
                    break;
            }
        }
    }

    private void GetApiVersionForM5()
    {
        byte[] bufferInitial = new byte[100];
        short apiIDValue = m_DbgParser.GetApiIDValue(ChipID.M5, "ApiVersion");
        if (m_DbgParser.ReceiveUSBInitialApiPacket(apiIDValue, bufferInitial, out var pbyRcvData) > 0)
        {
            PacketParser.ConvertBufferToTypeForI3(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
        }
    }

    private void GetApiVersion(ChipID chipID, CONNECT_MODE mode, short APIVersion)
    {
        byte[] pbyRcvData = null;
        int num = 0;
        switch (mode)
        {
            case CONNECT_MODE.MODE_USB:
                num = m_DbgParser.ReceiveUsbApiPacket(APIVersion, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_SOCKET:
                num = m_DbgParser.ReceiveApiPacket(APIVersion, out pbyRcvData);
                break;
            case CONNECT_MODE.MODE_UART:
                num = m_DbgParser.ReceiveUartApiPacket(APIVersion, out pbyRcvData);
                break;
        }
        if (num > 0)
        {
            switch (chipID)
            {
                case ChipID.I3:
                    PacketParser.ConvertBufferToTypeForI3(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
                    break;
                default:
                    PacketParser.ConvertBufferToType(ref EvbAPIVerMajor, ref EvbAPIVerMinor, pbyRcvData);
                    break;
                case ChipID.M5:
                case ChipID.M5U:
                case ChipID.I1:
                    break;
            }
        }
    }

    public int LoadBinXmlFile(string filePath)
    {
        int num = 0;
        string SrcPath = "";
        string ZipPath = "";
        byte[] buffer = new byte[1];
        uint magicKey = 0u;
        if ((num = ChkCRCforLoadBinXmlFile(filePath, ref SrcPath, ref ZipPath, ref buffer, ref magicKey)) == 1)
        {
            if (File.Exists(SrcPath))
            {
                File.Delete(SrcPath);
            }
            if (File.Exists(ZipPath))
            {
                File.Delete(ZipPath);
            }
            APIVerIsMatch = false;
            return 1;
        }
        if (m_DbgParser.IsConnected())
        {
            ChipID chipID = ChipID.I6;
            ChipID chipIDName = m_DbgParser.ChipIDName;
            if (chipIDName >= ChipID.I3 || chipIDName == ChipID.M5 || chipIDName == ChipID.M5U)
            {
                chipID = chipIDName;
                switch (chipIDName)
                {
                    case ChipID.I6E:
                    case ChipID.I6B0:
                    case ChipID.P3:
                        chipID = ChipID.I6E;
                        break;
                    case ChipID.M5:
                    case ChipID.M5U:
                        chipID = ChipID.M5;
                        break;
                    default:
                        chipID = ChipID.I6;
                        break;
                }
                if (chipIDName != ChipID.M5 && chipIDName != ChipID.M5U)
                {
                    GetChipIDByAPI(ref chipID);
                }
                GetBinFileChip(filePath);
                chipname = m_DbgParser.GetChipNameByID(chipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
                Binchipname = m_DbgParser.GetChipNameByID(BinChipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
                if (BinChipID != chipID)
                {
                    string text = "";
                    text = "[LoadBinXmlFile] ChipID is not match. Bin  ChipID: " + Binchipname + ".  EVB chipID: " + chipname + ".\n";
                    ShowMessage(text, "");
                    if (File.Exists(SrcPath))
                    {
                        File.Delete(SrcPath);
                    }
                    if (File.Exists(ZipPath))
                    {
                        File.Delete(ZipPath);
                    }
                    APIVerIsMatch = false;
                    return 1;
                }
                APIVerIsMatch = true;
                short apiIDValue = m_DbgParser.GetApiIDValue(chipIDName, "ApiVersion");
                if (chipIDName == ChipID.M5 || chipIDName == ChipID.M5U)
                {
                    GetApiVersionForM5();
                }
                else
                {
                    GetApiVersionForInfinitySeries(chipID, apiIDValue);
                }
                GetBinFileVer(filePath);
                if (BinAPIVerMajor != EvbAPIVerMajor)
                {
                    string text2 = "";
                    text2 = "Major API Version  is not match. Bin  Major: " + BinAPIVerMajor + ".  EVB Major: " + EvbAPIVerMajor + ".\n";
                    ShowMessage(text2, "");
                    if (File.Exists(SrcPath))
                    {
                        File.Delete(SrcPath);
                    }
                    if (File.Exists(ZipPath))
                    {
                        File.Delete(ZipPath);
                    }
                    APIVerIsMatch = false;
                    num = 1;
                }
                else
                {
                    APIVerIsMatch = true;
                }
            }
        }
        InitGUI(SrcPath);
        ParseBinBuffer(buffer, magicKey);
        if (File.Exists(SrcPath))
        {
            File.Delete(SrcPath);
        }
        if (File.Exists(ZipPath))
        {
            File.Delete(ZipPath);
        }
        return num;
    }

    public int checkChipID(ChipID chipID, string filePath)
    {
        int result = 0;
        GetChipIDByAPI(ref chipID);
        GetChipIDByXml(filePath);
        if (XmlChipID != chipID)
        {
            if (chipID == ChipID.I6B0 && XmlChipID == ChipID.I6E || chipID == ChipID.I6B0 && XmlChipID == ChipID.P3 || chipID == ChipID.I6E && XmlChipID == ChipID.P3 || chipID == ChipID.I6E && XmlChipID == ChipID.I6B0 || chipID == ChipID.P3 && XmlChipID == ChipID.I6B0 || chipID == ChipID.P3 && XmlChipID == ChipID.I6E || chipID == ChipID.I7 && XmlChipID == ChipID.I6E)
            {
                APIVerIsMatch = true;
            }
            else
            {
                chipname = m_DbgParser.GetChipNameByID(chipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
                Xmlchipname = m_DbgParser.GetChipNameByID(XmlChipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
                string text = "";
                text = "[CheckChipID] ChipID is not match. Xml  ChipID: " + Xmlchipname + ".  EVB chipID: " + chipname + ".\n";
                ShowMessage(text, "");
                APIVerIsMatch = false;
                result = 1;
            }
        }
        else
        {
            APIVerIsMatch = true;
        }
        return result;
    }

    public int checkAPIVersion(string filePath, ChipID chipID, CONNECT_MODE mode)
    {
        int result = 0;
        string chipNameByID = m_DbgParser.GetChipNameByID(chipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
        short apiIDValue = m_DbgParser.GetApiIDValue(chipID, "ApiVersion");
        switch (chipID)
        {
            case ChipID.I1:
            case ChipID.I3:
                switch (mode)
                {
                    case CONNECT_MODE.MODE_SOCKET:
                        GetApiVersionForInfinitySeries(chipID, apiIDValue);
                        break;
                    case CONNECT_MODE.MODE_USB:
                        {
                            string msgBox = chipNameByID + " is not support USB Mode.\n";
                            ShowMessage(msgBox, "");
                            break;
                        }
                }
                break;
            case ChipID.I2:
                switch (mode)
                {
                    case CONNECT_MODE.MODE_SOCKET:
                        GetApiVersionForInfinitySeries(chipID, apiIDValue);
                        break;
                    case CONNECT_MODE.MODE_USB:
                        GetApiVersion(chipID, mode, apiIDValue);
                        break;
                }
                break;
            case ChipID.I5:
                switch (mode)
                {
                    case CONNECT_MODE.MODE_SOCKET:
                        GetApiVersionForInfinitySeries(chipID, apiIDValue);
                        break;
                    case CONNECT_MODE.MODE_USB:
                        GetApiVersion(chipID, mode, apiIDValue);
                        break;
                }
                break;
            default:
                switch (mode)
                {
                    case CONNECT_MODE.MODE_SOCKET:
                        GetApiVersionForInfinitySeries(chipID, apiIDValue);
                        break;
                    case CONNECT_MODE.MODE_USB:
                    case CONNECT_MODE.MODE_UART:
                        GetApiVersion(chipID, mode, apiIDValue);
                        break;
                }
                break;
        }
        GetXmlFileVer(filePath);
        if (XmlAPIVerMajor != EvbAPIVerMajor)
        {
            string text = "";
            text = "Major API Version  is not match. Xml  Major: " + XmlAPIVerMajor + ".  EVB Major: " + EvbAPIVerMajor + ".\n";
            ShowMessage(text, "");
            APIVerIsMatch = false;
            result = 1;
        }
        else
        {
            APIVerIsMatch = true;
        }
        return result;
    }

    public int LoadXmlFile(string filePath)
    {
        int result = 0;
        if (m_DbgParser.IsConnected())
        {
            ChipID chipIDName = m_DbgParser.ChipIDName;
            if (chipIDName >= ChipID.I3 || chipIDName == ChipID.M5 || chipIDName == ChipID.M5U)
            {
                ChipID chipID = chipIDName;
                switch (chipIDName)
                {
                    case ChipID.I6E:
                    case ChipID.I6B0:
                    case ChipID.P3:
                        chipID = ChipID.I6E;
                        break;
                    case ChipID.M5:
                    case ChipID.M5U:
                        chipID = ChipID.M5;
                        break;
                    default:
                        chipID = ChipID.I6;
                        break;
                }
                if (chipIDName != ChipID.M5 && chipIDName != ChipID.M5U && chipIDName != ChipID.I6E && chipIDName != ChipID.I6B0 && chipIDName != ChipID.P3)
                {
                    GetChipIDByAPI(ref chipID);
                }
                GetChipIDByXml(filePath);
                chipname = m_DbgParser.GetChipNameByID(chipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
                Xmlchipname = m_DbgParser.GetChipNameByID(XmlChipID, STRING_OPERATE_TYPE_E.TYPE_UPPER_E);
                if (XmlChipID != chipID)
                {
                    string text = "";
                    text = "[LoadXmlFile] ChipID is not match. Xml  ChipID: " + Xmlchipname + ".  EVB chipID: " + chipname + ".\n";
                    ShowMessage(text, "");
                    APIVerIsMatch = false;
                    return 1;
                }
                APIVerIsMatch = true;
                short apiIDValue = m_DbgParser.GetApiIDValue(chipID, "ApiVersion");
                if (chipIDName == ChipID.M5 || chipIDName == ChipID.M5U)
                {
                    GetApiVersionForM5();
                }
                else
                {
                    GetApiVersionForInfinitySeries(chipID, apiIDValue);
                }
                GetXmlFileVer(filePath);
                if (XmlAPIVerMajor != EvbAPIVerMajor)
                {
                    string text2 = "";
                    text2 = "Major API Version  is not match. Xml  Major: " + XmlAPIVerMajor + ".  EVB Major: " + EvbAPIVerMajor + ".\n";
                    ShowMessage(text2, "");
                    APIVerIsMatch = false;
                    result = 1;
                }
                else
                {
                    APIVerIsMatch = true;
                }
            }
        }
        InitGUI(filePath);
        return result;
    }

    private void ParseBinBuffer(byte[] buffer, uint magicKey)
    {
        while (buffer.Length != 0)
        {
            int num = BitConverter.ToInt32(buffer.Skip(8).Take(4).ToArray(), 0);
            int apiID = BitConverter.ToInt16(buffer.Skip(12).Take(2).ToArray(), 0);
            byte[] buffer2 = buffer.Skip(12).Take(num).ToArray();
            UpdateGroup(buffer2, apiID);
            buffer = buffer.Skip(12 + num).ToArray();
        }
        UpdateGUI();
    }

    private void ParseCvtDesBinBuffer(byte[] buffer, uint magicKey)
    {
        while (buffer.Length != 0)
        {
            int num = BitConverter.ToInt32(buffer.Skip(8).Take(4).ToArray(), 0);
            int apiID = BitConverter.ToInt16(buffer.Skip(12).Take(2).ToArray(), 0);
            byte[] buffer2 = buffer.Skip(12).Take(num).ToArray();
            UpdateCvtDesGroup(buffer2, apiID);
            buffer = buffer.Skip(12 + num).ToArray();
        }
        UpdateCvtDesGUI();
    }

    private void ParseCvtSrcBinBuffer(byte[] buffer, uint magicKey)
    {
        while (buffer.Length != 0)
        {
            int num = BitConverter.ToInt32(buffer.Skip(8).Take(4).ToArray(), 0);
            int apiID = BitConverter.ToInt16(buffer.Skip(12).Take(2).ToArray(), 0);
            byte[] buffer2 = buffer.Skip(12).Take(num).ToArray();
            UpdateCvtSrcGroup(buffer2, apiID);
            buffer = buffer.Skip(12 + num).ToArray();
        }
        UpdateCvtSrcGUI();
    }

    private void UpdateGroup(byte[] buffer, int apiID)
    {
        foreach (GuiPage item in m_GuiPageArray)
        {
            foreach (GuiGroup group in item.GroupList)
            {
                if (group.ID == apiID)
                {
                    group.UpdateGroup(buffer);
                    return;
                }
            }
        }
    }

    private void UpdateCvtDesGroup(byte[] buffer, int apiID)
    {
        foreach (GuiPage item in m_CvtDesGuiPageArray)
        {
            foreach (GuiGroup group in item.GroupList)
            {
                if (group.ID == apiID)
                {
                    group.UpdateGroup(buffer);
                    return;
                }
            }
        }
    }

    private void UpdateCvtSrcGroup(byte[] buffer, int apiID)
    {
        foreach (GuiPage item in m_CvtSrcGuiPageArray)
        {
            foreach (GuiGroup group in item.GroupList)
            {
                if (group.ID == apiID)
                {
                    group.UpdateGroup(buffer);
                    return;
                }
            }
        }
    }

    public string ParseBufferToXml(byte[] buffer)
    {
        return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
    }

    public int GetOBCRGB(ref int R, ref int Gr, ref int Gb, ref int B)
    {
        int num = -1;
        for (int i = 0; i < m_GuiPageArray.Count; i++)
        {
            if (m_GuiPageArray[i].Name == "OBC")
            {
                num = i;
            }
        }
        if (num == -1)
        {
            return 0;
        }
        for (int j = 0; j < m_GuiPageArray[num].GroupList[0].ItemList.Count; j++)
        {
            if (m_GuiPageArray[num].GroupList[0].ItemList[j].Text == "Manual.Value.R")
            {
                R = (int)m_GuiPageArray[num].GroupList[0].ItemList[j].DataValue[0];
            }
            else if (m_GuiPageArray[num].GroupList[0].ItemList[j].Text == "Manual.Value.GR")
            {
                Gr = (int)m_GuiPageArray[num].GroupList[0].ItemList[j].DataValue[0];
            }
            else if (m_GuiPageArray[num].GroupList[0].ItemList[j].Text == "Manual.Value.GB")
            {
                Gb = (int)m_GuiPageArray[num].GroupList[0].ItemList[j].DataValue[0];
            }
            else if (m_GuiPageArray[num].GroupList[0].ItemList[j].Text == "Manual.Value.B")
            {
                B = (int)m_GuiPageArray[num].GroupList[0].ItemList[j].DataValue[0];
            }
        }
        return 1;
    }

    public void InitGuiPageCache(int idx)
    {
        if (!m_GuiPageCacheSavedOne[idx].bRead)
        {
            m_GuiPageCacheSavedOne[idx].bRead = true;
            m_GuiPageCacheSavedOne[idx].UpdatePage(m_GuiPageArray[idx]);
        }
        if (!m_GuiPageCacheSavedTwo[idx].bRead)
        {
            m_GuiPageCacheSavedTwo[idx].bRead = true;
            m_GuiPageCacheSavedTwo[idx].UpdatePage(m_GuiPageArray[idx]);
        }
    }

    public void OneParam()
    {
        _ = m_xmlDoc;
        if (m_CurrPageIndex >= 0 && m_CurrPageIndex < m_GuiPageCacheSavedOne.Count() && m_CuiPageCurrentSaved[m_CurrPageIndex] == 2)
        {
            m_GuiPageCacheSavedTwo[m_CurrPageIndex].UpdatePage(m_GuiPageArray[m_CurrPageIndex]);
            m_GuiPageArray[m_CurrPageIndex].UpdatePage(m_GuiPageCacheSavedOne[m_CurrPageIndex]);
            UpdatePageControl(m_GuiPageArray[m_CurrPageIndex], m_PageControlArray[m_CurrPageIndex]);
            m_CuiPageCurrentSaved[m_CurrPageIndex] = 1;
        }
    }

    public void TwoParam()
    {
        if (m_CurrPageIndex >= 0 && m_CurrPageIndex < m_GuiPageCacheSavedOne.Count() && m_CuiPageCurrentSaved[m_CurrPageIndex] == 1)
        {
            m_GuiPageCacheSavedOne[m_CurrPageIndex].UpdatePage(m_GuiPageArray[m_CurrPageIndex]);
            m_GuiPageArray[m_CurrPageIndex].UpdatePage(m_GuiPageCacheSavedTwo[m_CurrPageIndex]);
            UpdatePageControl(m_GuiPageArray[m_CurrPageIndex], m_PageControlArray[m_CurrPageIndex]);
            m_CuiPageCurrentSaved[m_CurrPageIndex] = 2;
        }
    }
}
