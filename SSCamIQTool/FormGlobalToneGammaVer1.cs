using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSCamIQTool.SSCamIQTool;

public class FormGlobalToneGammaVer1 : Form
{
    public bool m_chartSelectR;

    public bool m_chartSelectRGB;

    public bool m_chartSelectR_C;

    public bool m_chartSelectRGB_C;

    private int unknowN;

    private int m_nFeatureIdxR;

    private int m_nFeatureIdxR_c;

    private int m_nGammaLength = 33;

    private int m_nGammaInterval = 2048;

    private int nYMax = 4096;

    private short m_nGammaValueMax;

    private List<Point> m_ptFeatureR = new List<Point>();

    private List<Point> m_ptFeatureRControl = new List<Point>();

    private string m_strCtrlPtYes = "Ctrl Pt: Yes";

    private string m_strCtrlPtNo = "Ctrl Pt: No";

    private int diff_left_control_x;

    private int diff_left_control_y;

    private int diff_right_control_x;

    private int diff_right_control_y;

    private int FindPoint_index = 0;

    private int FindPoint;

    private int sectionvalue = 1;

    private int GammaLength = 65535;

    private int GammaNumber = 33;

    private int NumberOfGroup = 16;

    private static int[] xnode_Auto = new int[528];

    private static int[] ynode_Auto = new int[528];

    private static int[] xnode_Manual = new int[33];

    private static int[] ynode_Manual = new int[33];

    private bool autoMode;

    private static GuiGroup gammaGroup;

    private static GuiGroup gammaGroup_temp;

    private static GuiItem enableItem;

    private static GuiItem opTypeItem;

    private int itemR = 1;

    private double new_Y;

    public static long[] gammainitialvalueR;

    private static int[] RecordGlobalToneGammaIndex_Auto = new int[528];

    public static long[] gammainitialvalueR_Manual;

    private static int[] RecordGlobalToneGammaIndex_Manual = new int[33];

    private static Series seiresGamma;

    private GenGlobalToneCurve1 m_objGlobalTone = new GenGlobalToneCurve1();

    private long[] GlobalToneParam_Auto = new long[144];

    private long[] GlobalToneParam_Manual = new long[9];

    private IContainer components = null;

    private GroupBox groupBox1_GenParam;

    private Button button1_GenParam;

    private Chart chartSimulationGamma;

    private Label GammalabelPosition;

    private GroupBox GammagroupBoxApi2;

    private Label GammalabelOpType;

    private ComboBox GammacomboBoxIndex;

    private ComboBox GammacomboBoxOpType;

    private Label label3;

    private CheckBox GammacheckBoxEnable;

    private GroupBox GammagroupFileMenu;

    private Button GammabtnLoad;

    private Button GammabtnSave;

    private Chart chartGamma;

    public int Index { get; set; }

    public int Index_Old { get; set; }

    public int OpMode { get; set; }

    public int GammaEnable { get; set; }

    private void GammacomboBoxIndex_SelectedIndexChanged(object sender, EventArgs e)
    {
        Index = GammacomboBoxIndex.SelectedIndex;
        if (Index != -1)
        {
            UpdateGammaValue(Index);
        }
    }

    private void GammacomboBoxOpType_SelectedIndexChanged(object sender, EventArgs e)
    {
        OpMode = GammacomboBoxOpType.SelectedIndex;
        opTypeItem.DataValue[0] = OpMode;
        if (OpMode == 1)
        {
            GammacomboBoxIndex.Enabled = false;
            itemR = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GammaLut");
            UpdateGammaValue(0);
        }
        else
        {
            GammacomboBoxIndex.Enabled = true;
            itemR = GetItemFieldIdx("API_WDRCurveFull_AUTO_GammaLut");
            UpdateGammaValue(Index);
        }
    }

    private void GammacheckBoxEnable_CheckedChanged(object sender, EventArgs e)
    {
        GammaEnable = GammacheckBoxEnable.Checked ? 1 : 0;
        enableItem.DataValue[0] = GammaEnable;
        if (GammaEnable == 0)
        {
            chartGamma.Enabled = false;
        }
        else
        {
            chartGamma.Enabled = true;
        }
    }

    public void UpdatePage()
    {
        int num = 33;
        int num2 = 16;
        GammacheckBoxEnable.Checked = enableItem.DataValue[0] != 0L;
        GammacomboBoxOpType.SelectedIndex = (int)opTypeItem.DataValue[0];
        if (OpMode == 0)
        {
            if (Index != Index_Old)
            {
                GammacomboBoxIndex.SelectedIndex = Index_Old;
                GammacomboBoxIndex_SelectedIndexChanged(new ComboBox(), new EventArgs());
            }
            else
            {
                GammacomboBoxIndex_SelectedIndexChanged(new ComboBox(), new EventArgs());
            }
            for (int i = 0; i < num2 * num; i++)
            {
                gammaGroup_temp.ItemList[itemR].DataValue[i] = gammaGroup.ItemList[itemR].DataValue[i];
            }
        }
        else
        {
            UpdateGammaValue(0);
            for (int j = 0; j < num; j++)
            {
                gammaGroup_temp.ItemList[itemR].DataValue[j] = gammaGroup.ItemList[itemR].DataValue[j];
            }
        }
    }

    public void UpdateGammaValue(int index)
    {
        if (OpMode == 0)
        {
            setRedGammaArray(gammainitialvalueR, index);
        }
        else
        {
            setRedGammaArray(gammainitialvalueR_Manual, index);
        }
    }

    public void SaveGammaValue_temp()
    {
        getRedGammaArray(out var pGammaR);
        if (OpMode == 0 && GammaEnable == 1)
        {
            pGammaR.CopyTo(gammainitialvalueR, Index * pGammaR.Length);
        }
        else if (OpMode == 1 && GammaEnable == 1)
        {
            gammainitialvalueR_Manual = pGammaR;
        }
    }

    private static void AppExceptionShow(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.ExceptionObject.ToString());
    }

    public FormGlobalToneGammaVer1(GuiGroup group)
    {
        AppDomain.CurrentDomain.UnhandledException += AppExceptionShow;
        InitializeComponent();
        gammaGroup = group;
        gammaGroup_temp = group;
        short num = (short)(gammaGroup.ItemList[GetItemFieldIdx("API_WDRCurveFull_AUTO_GammaLut")].MaxValue + 1);
        if (num > m_nGammaValueMax)
        {
            m_nGammaValueMax = num;
        }
        InitChart();
        InitGenParam();
        autoMode = group.autoMode.Length > 1;
        if (autoMode)
        {
            OpMode = 0;
        }
        else
        {
            OpMode = 1;
        }
        if (OpMode == 0)
        {
            enableItem = gammaGroup.ItemList[GetItemFieldIdx("API_WDRCurveFull_ENABLE_Gamma")];
            opTypeItem = gammaGroup.ItemList[GetItemFieldIdx("API_WDRCurveFull_OP_TYPE_Gamma")];
            itemR = GetItemFieldIdx("API_WDRCurveFull_AUTO_GammaLut");
        }
        else
        {
            GammagroupBoxApi2.Visible = false;
            itemR = GetItemFieldIdx("API_WDRCurveFull_ENABLE_Gamma");
        }
        initialGammaValue();
        UpdatePage();
        GammacheckBoxEnable_CheckedChanged(new CheckBox(), new EventArgs());
        GammacomboBoxIndex.Text = "index 0";
    }

    public FormGlobalToneGammaVer1()
    {
    }

    public void InitChart()
    {
        int num = 33;
        GetGammaWithOB(m_nGammaValueMax, m_nGammaValueMax, 0m, out var pGammaArray);
        chartGamma.ChartAreas[0].AxisX.Minimum = 0.0;
        chartGamma.ChartAreas[0].AxisX.Maximum = num - 1;
        chartGamma.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
        chartGamma.ChartAreas[0].AxisX.LabelStyle.Format = "D1";
        chartGamma.ChartAreas[0].AxisY.Minimum = 0.0;
        chartGamma.ChartAreas[0].AxisY.Maximum = nYMax;
        chartGamma.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
        chartGamma.ChartAreas[0].AxisY.LabelStyle.Format = "D1";
        chartGamma.Legends[0].Enabled = false;
        chartSimulationGamma.ChartAreas[0].AxisX.Minimum = 0.0;
        chartSimulationGamma.ChartAreas[0].AxisX.Maximum = GammaLength + 1;
        chartSimulationGamma.ChartAreas[0].AxisX.Interval = (GammaLength + 1) / 16;
        chartSimulationGamma.ChartAreas[0].AxisX.LabelStyle.Format = "D1";
        chartSimulationGamma.ChartAreas[0].AxisY.Minimum = 0.0;
        chartSimulationGamma.ChartAreas[0].AxisY.Maximum = nYMax;
        chartSimulationGamma.ChartAreas[0].AxisY.Interval = nYMax / 16;
        chartSimulationGamma.ChartAreas[0].AxisY.LabelStyle.Format = "D1";
        chartSimulationGamma.Legends[0].Enabled = false;
        if (pGammaArray != null)
        {
            chartGamma.Series[0].Points.Clear();
            for (int i = 0; i < pGammaArray.Length; i++)
            {
                chartGamma.Series["SeriesGamma"].Points.AddXY(i, pGammaArray[i].Y);
            }
            Series seiresPoints = chartGamma.Series["SeriesGammaFeature"];
            Series seiresLines = chartGamma.Series["SeriesGamma"];
            initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        }
        for (int j = 0; j < 512; j++)
        {
            chartGamma.Series.Add("SeriesBezierControl" + j);
            chartGamma.Series["SeriesBezierControl" + j].ChartType = SeriesChartType.Spline;
            chartGamma.Series["SeriesBezierControl" + j].BorderWidth = 1;
            chartGamma.Series["SeriesBezierControl" + j].Color = Color.Gray;
        }
    }

    public void clearControlLine()
    {
        for (int i = 0; i < 512; i++)
        {
            chartGamma.Series["SeriesBezierControl" + i].Points.Clear();
        }
    }

    public void InitCtrolChart(double Pm1, double Pm2, double P0, double P3, double T1, double T2, ref double P1, ref double P2)
    {
        double num = 0.0;
        double num2 = 0.0;
        double num3 = 1.0 - T1;
        double num4 = 1.0 - T2;
        double num5 = (Pm1 - P0 * num3 * num3 * num3 - P3 * T1 * T1 * T1) / (3.0 * T1 * num3);
        double num6 = (Pm2 - P0 * num4 * num4 * num4 - P3 * T2 * T2 * T2) / (3.0 * num4 * T2);
        num2 = (num5 * num4 - num6 * num3) / (T1 * num4 - T2 * num3);
        num = (num5 - num2 * T1) / num3;
        P1 = num;
        P2 = num2;
    }

    public void InitBezierControl(Series seiresRLine, Series seiresRPoints, List<Point> m_ptFeatureR, List<Point> m_ptFeatureRControl)
    {
        m_ptFeatureRControl.Clear();
        for (int i = 0; i < m_ptFeatureR.Count; i++)
        {
            if (i == 0)
            {
                double num = 0.0;
                double num2 = (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) / 2;
                double num3 = (m_ptFeatureR[i + 1].Y - m_ptFeatureR[i].Y) / 2;
                num = m_ptFeatureR[i].X + num2;
                double num4 = m_ptFeatureR[i].Y + num3;
                int num5 = (int)num;
                int num6 = (int)num4;
                m_ptFeatureRControl.Add(new Point(num5, num6));
                continue;
            }
            if (i == m_ptFeatureR.Count - 1)
            {
                double num7 = 0.0;
                double num8 = (m_ptFeatureR[i].X - m_ptFeatureR[i - 1].X) / 2;
                double num9 = (m_ptFeatureR[i].Y - m_ptFeatureR[i - 1].Y) / 2;
                num7 = m_ptFeatureR[i].X - num8;
                double num10 = m_ptFeatureR[i].Y - num9;
                int num11 = (int)num7;
                int num12 = (int)num10;
                m_ptFeatureRControl.Add(new Point(num11, num12));
                continue;
            }
            double num13 = 0.0;
            double num14 = 0.0;
            double num15 = 0.0;
            double num16 = 0.0;
            double num17 = 0.0;
            double num18 = 0.0;
            if (m_ptFeatureR[i].Y > m_ptFeatureR[i + 1].Y)
            {
                num17 = (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) / 2;
                num18 = (m_ptFeatureR[i].Y - m_ptFeatureR[i + 1].Y) / 2;
                num13 = m_ptFeatureR[i].X - num17;
                num15 = m_ptFeatureR[i].Y + num18;
                num14 = m_ptFeatureR[i].X + num17;
                num16 = m_ptFeatureR[i].Y - num18;
            }
            else
            {
                num17 = (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) / 2;
                num18 = (m_ptFeatureR[i + 1].Y - m_ptFeatureR[i].Y) / 2;
                num13 = m_ptFeatureR[i].X - num17;
                num15 = m_ptFeatureR[i].Y - num18;
                num14 = m_ptFeatureR[i].X + num17;
                num16 = m_ptFeatureR[i].Y + num18;
            }
            int num19 = (int)num13;
            int num20 = (int)num14;
            int num21 = (int)num15;
            int num22 = (int)num16;
            m_ptFeatureRControl.Add(new Point(num19, num21));
            m_ptFeatureRControl.Add(new Point(num20, num22));
        }
        clearControlLine();
        for (int j = 0; j < m_ptFeatureRControl.Count; j++)
        {
            int num23 = 0;
            int num24 = 0;
            int num25 = 0;
            int num26 = 0;
            float a = 0f;
            float b = 0f;
            int num27 = 0;
            int num28 = 0;
            if (j % 2 == 0)
            {
                num28 = j / 2;
                num23 = m_ptFeatureR[num28].X;
                num24 = m_ptFeatureRControl[j].X;
                num25 = m_ptFeatureR[num28].Y;
                num26 = m_ptFeatureRControl[j].Y;
                LineFunction(num23, num24, num25, num26, ref a, ref b);
                for (int k = num23; k <= num24; k++)
                {
                    num27 = (int)(a * k + b);
                    chartGamma.Series["SeriesBezierControl" + j].Points.AddXY(k, num27);
                }
            }
            else
            {
                num28 = (j - 1) / 2 + 1;
                num23 = m_ptFeatureRControl[j].X;
                num24 = m_ptFeatureR[num28].X;
                num25 = m_ptFeatureRControl[j].Y;
                num26 = m_ptFeatureR[num28].Y;
                LineFunction(num23, num24, num25, num26, ref a, ref b);
                for (int l = num23; l <= num24; l++)
                {
                    num27 = (int)(a * l + b);
                    chartGamma.Series["SeriesBezierControl" + j].Points.AddXY(l, num27);
                }
            }
        }
    }

    public void AddBezierControl(Series seiresRLine, Series seiresRPoints, List<Point> m_ptFeatureR, List<Point> m_ptFeatureRControl)
    {
        for (int i = 0; i < m_ptFeatureR.Count; i++)
        {
            if (i != FindPoint_index)
            {
                continue;
            }
            if (i == 0)
            {
                double num = 0.0;
                double num2 = (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) / 2;
                double num3 = (m_ptFeatureR[i + 1].Y - m_ptFeatureR[i].Y) / 2;
                num = m_ptFeatureR[i].X + num2;
                double num4 = m_ptFeatureR[i].Y + num3;
                int num5 = (int)num;
                int num6 = (int)num4;
                m_ptFeatureRControl.Insert(0, new Point(num5, num6));
                continue;
            }
            if (i == m_ptFeatureR.Count - 1)
            {
                double num7 = 0.0;
                double num8 = (m_ptFeatureR[i].X - m_ptFeatureR[i - 1].X) / 2;
                double num9 = (m_ptFeatureR[i].Y - m_ptFeatureR[i - 1].Y) / 2;
                num7 = m_ptFeatureR[i].X - num8;
                double num10 = m_ptFeatureR[i].Y - num9;
                int num11 = (int)num7;
                int num12 = (int)num10;
                m_ptFeatureRControl.Insert(2 * (m_ptFeatureR.Count - 1) - 1, new Point(num11, num12));
                continue;
            }
            double num13 = 0.0;
            double num14 = 0.0;
            double num15 = 0.0;
            double num16 = 0.0;
            double num17 = 0.0;
            double num18 = 0.0;
            if (m_ptFeatureR[i].Y > m_ptFeatureR[i + 1].Y)
            {
                num17 = (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) / 2;
                num18 = (m_ptFeatureR[i].Y - m_ptFeatureR[i + 1].Y) / 2;
                num13 = m_ptFeatureR[i].X - num17;
                num15 = m_ptFeatureR[i].Y + num18;
                num14 = m_ptFeatureR[i].X + num17;
                num16 = m_ptFeatureR[i].Y - num18;
            }
            else
            {
                num17 = (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) / 2;
                num18 = (m_ptFeatureR[i + 1].Y - m_ptFeatureR[i].Y) / 2;
                num13 = m_ptFeatureR[i].X - num17;
                num15 = m_ptFeatureR[i].Y - num18;
                num14 = m_ptFeatureR[i].X + num17;
                num16 = m_ptFeatureR[i].Y + num18;
            }
            int num19 = (int)num13;
            int num20 = (int)num14;
            int num21 = (int)num15;
            int num22 = (int)num16;
            m_ptFeatureRControl.Insert(2 * i - 1, new Point(num19, num21));
            m_ptFeatureRControl.Insert(2 * i, new Point(num20, num22));
        }
        clearControlLine();
        for (int j = 0; j < m_ptFeatureRControl.Count; j++)
        {
            int num23 = 0;
            int num24 = 0;
            int num25 = 0;
            int num26 = 0;
            float a = 0f;
            float b = 0f;
            int num27 = 0;
            int num28 = 0;
            if (j % 2 == 0)
            {
                num28 = j / 2;
                num23 = m_ptFeatureR[num28].X;
                num24 = m_ptFeatureRControl[j].X;
                num25 = m_ptFeatureR[num28].Y;
                num26 = m_ptFeatureRControl[j].Y;
                LineFunction(num23, num24, num25, num26, ref a, ref b);
                for (int k = num23; k <= num24; k++)
                {
                    num27 = (int)(a * k + b);
                    chartGamma.Series["SeriesBezierControl" + j].Points.AddXY(k, num27);
                }
            }
            else
            {
                num28 = (j - 1) / 2 + 1;
                num23 = m_ptFeatureRControl[j].X;
                num24 = m_ptFeatureR[num28].X;
                num25 = m_ptFeatureRControl[j].Y;
                num26 = m_ptFeatureR[num28].Y;
                LineFunction(num23, num24, num25, num26, ref a, ref b);
                for (int l = num23; l <= num24; l++)
                {
                    num27 = (int)(a * l + b);
                    chartGamma.Series["SeriesBezierControl" + j].Points.AddXY(l, num27);
                }
            }
        }
    }

    public void LineFunction(int x1, int x2, int y1, int y2, ref float a, ref float b)
    {
        a = (y1 - y2) / (float)(x1 - x2 + 1E-10);
        b = y1 - a * x1;
    }

    public void drawControlLine(List<Point> m_ptFeature, List<Point> m_ptFeatureControl)
    {
        for (int i = 0; i < m_ptFeatureControl.Count; i++)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            float a = 0f;
            float b = 0f;
            int num5 = 0;
            int num6 = 0;
            if (i % 2 == 0)
            {
                num6 = i / 2;
                num = m_ptFeature[num6].X;
                num2 = m_ptFeatureControl[i].X;
                num3 = m_ptFeature[num6].Y;
                num4 = m_ptFeatureControl[i].Y;
                LineFunction(num, num2, num3, num4, ref a, ref b);
                for (int j = num; j <= num2; j++)
                {
                    num5 = (int)(a * j + b);
                    chartGamma.Series["SeriesBezierControl" + i].Points.AddXY(j, num5);
                }
            }
            else
            {
                num6 = (i - 1) / 2 + 1;
                num = m_ptFeatureControl[i].X;
                num2 = m_ptFeature[num6].X;
                num3 = m_ptFeatureControl[i].Y;
                num4 = m_ptFeature[num6].Y;
                LineFunction(num, num2, num3, num4, ref a, ref b);
                for (int k = num; k <= num2; k++)
                {
                    num5 = (int)(a * k + b);
                    chartGamma.Series["SeriesBezierControl" + i].Points.AddXY(k, num5);
                }
            }
        }
    }

    private void initialAddSection(ref Series seiresPoints, ref Series seiresLines, List<Point> pFeaturePoint)
    {
        int nGammaLength = m_nGammaLength;
        int num = 1;
        seiresPoints.Points.Clear();
        num = 33;
        for (int i = 0; i < nGammaLength; i += nGammaLength / num)
        {
            pFeaturePoint.Add(new Point((int)seiresLines.Points[i].XValue, (int)seiresLines.Points[i].YValues[0]));
        }
        seiresPoints.Points.Clear();
        for (int j = 0; j < pFeaturePoint.Count; j++)
        {
            seiresPoints.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
        }
    }

    private void RGBGamma_Load(object sender, EventArgs e)
    {
    }

    private void checkBoxSyncRGB_CheckedChanged(object sender, EventArgs e)
    {
        RGBsameLineBack();
        chartGamma.Series["SeriesGamma"].Color = Color.Red;
        clearControlLine();
    }

    private void GetGammaWithOB(decimal nSourceMax, decimal nOutputMax, decimal nObValue, out Point[] pGammaArray)
    {
        decimal[] array = new decimal[m_nGammaLength];
        pGammaArray = new Point[m_nGammaLength];
        decimal num = (nSourceMax - 1m) / (array.Length - 1);
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = num * i;
            pGammaArray[i].X = (int)array[i];
        }
        for (int j = 0; j < pGammaArray.Length; j++)
        {
            pGammaArray[j].Y = (int)((array[j] - nObValue) * nOutputMax / (nSourceMax - 1m - nObValue));
            if (pGammaArray[j].Y < 0)
            {
                pGammaArray[j].Y = 0;
            }
            else if (pGammaArray[j].Y >= nOutputMax)
            {
                pGammaArray[j].Y = (int)nOutputMax - 1;
            }
        }
    }

    protected void BezierCurveFirstModify(int nFeatureIdx, Series seiresCurve, List<Point> ptFeature, ref double choosepointfinalY)
    {
        double num = 0.0;
        if (nFeatureIdx < 1)
        {
            return;
        }
        double num2 = ptFeature[nFeatureIdx - 1].X;
        double num3 = ptFeature[nFeatureIdx].X;
        double num4 = ptFeature[nFeatureIdx - 1].Y;
        double num5 = ptFeature[nFeatureIdx].Y;
        double num6 = 0.0;
        double num7 = 0.0;
        for (int i = ptFeature[nFeatureIdx - 1].X; i <= ptFeature[nFeatureIdx].X; i++)
        {
            num = (i - num2) / (num3 - num2);
            num6 = (int)((1.0 - num) * num4 + num * num5);
            num7 = (int)((1.0 - num) * num2 + num * num3);
            seiresCurve.Points[i].YValues[0] = num6;
            seiresCurve.Points[i].XValue = num7;
            if (seiresCurve.Points[i].YValues[0] > m_nGammaValueMax - 1)
            {
                seiresCurve.Points[i].YValues[0] = m_nGammaValueMax - 1;
            }
            if (seiresCurve.Points[i].YValues[0] < 0.0)
            {
                seiresCurve.Points[i].YValues[0] = 0.0;
            }
        }
        choosepointfinalY = seiresCurve.Points[(int)num3].YValues[0];
    }

    protected void BezierCurveModify(int nFeatureIdx, Series seiresCurve, List<Point> ptFeature, ref double choosepointfinalY)
    {
        double num = 0.0;
        if (nFeatureIdx >= 1 && nFeatureIdx < ptFeature.Count - 1)
        {
            double num2 = ptFeature[nFeatureIdx - 1].X;
            double num3 = ptFeature[nFeatureIdx].X;
            double num4 = ptFeature[nFeatureIdx + 1].X;
            double num5 = ptFeature[nFeatureIdx - 1].Y;
            double num6 = ptFeature[nFeatureIdx].Y;
            double num7 = ptFeature[nFeatureIdx + 1].Y;
            double num8 = 0.0;
            double num9 = 0.0;
            for (int i = ptFeature[nFeatureIdx - 1].X; i <= ptFeature[nFeatureIdx + 1].X; i++)
            {
                num = (i - num2) / (num4 - num2);
                num8 = (int)((1.0 - num) * (1.0 - num) * num5 + 2.0 * (1.0 - num) * num * num6 + num * num * num7);
                num9 = (int)((1.0 - num) * (1.0 - num) * num2 + 2.0 * (1.0 - num) * num * num3 + num * num * num4);
                seiresCurve.Points[i].YValues[0] = num8;
                seiresCurve.Points[i].XValue = num9;
                if (seiresCurve.Points[i].YValues[0] > m_nGammaValueMax - 1)
                {
                    seiresCurve.Points[i].YValues[0] = m_nGammaValueMax - 1;
                }
                if (seiresCurve.Points[i].YValues[0] < 0.0)
                {
                    seiresCurve.Points[i].YValues[0] = 0.0;
                }
            }
            choosepointfinalY = seiresCurve.Points[(int)num3].YValues[0];
        }
        else
        {
            CurveFittingModify(nFeatureIdx, seiresCurve, ptFeature);
        }
    }

    public void cubicBezier(double p_y0, double p_y1, double p_y2, double p_y3, double p_x0, double p_x1, double p_x2, double p_x3, int x1, int x2, Series seiresCurve)
    {
        int num = x2 - x1;
        double num2 = 0.0;
        for (int i = 0; i < num; i++)
        {
            if (x1 + i >= 33)
            {
                continue;
            }
            num2 = i / (double)num;
            double num3 = p_y0 * (1.0 - num2) * (1.0 - num2) * (1.0 - num2) + 3.0 * p_y1 * num2 * (1.0 - num2) * (1.0 - num2) + 3.0 * p_y2 * num2 * num2 * (1.0 - num2) + p_y3 * num2 * num2 * num2;
            double xValue = p_x0 * (1.0 - num2) * (1.0 - num2) * (1.0 - num2) + 3.0 * p_x1 * num2 * (1.0 - num2) * (1.0 - num2) + 3.0 * p_x2 * num2 * num2 * (1.0 - num2) + p_x3 * num2 * num2 * num2;
            if (x1 + i == 0)
            {
                if (num3 > seiresCurve.Points[x1 + i + 1].YValues[0])
                {
                    seiresCurve.Points[x1 + i].YValues[0] = seiresCurve.Points[x1 + i + 1].YValues[0];
                }
                else
                {
                    seiresCurve.Points[x1 + i].YValues[0] = num3;
                }
            }
            else if (num3 < seiresCurve.Points[x1 + i - 1].YValues[0])
            {
                seiresCurve.Points[x1 + i].YValues[0] = seiresCurve.Points[x1 + i - 1].YValues[0];
            }
            else if (num3 > seiresCurve.Points[x1 + i + 1].YValues[0])
            {
                seiresCurve.Points[x1 + i].YValues[0] = seiresCurve.Points[x1 + i + 1].YValues[0];
            }
            else
            {
                seiresCurve.Points[x1 + i].YValues[0] = num3;
            }
            seiresCurve.Points[x1 + i].XValue = xValue;
        }
    }

    protected void BezierCurveBelongModify(int nFeatureIdx, Series seiresCurve, List<Point> ptFeature, ref double choosepointfinalY)
    {
        double num = ptFeature[nFeatureIdx - 1].X;
        double num2 = ptFeature[nFeatureIdx].X;
        double num3 = ptFeature[nFeatureIdx + 1].X;
        double num4 = ptFeature[nFeatureIdx - 1].Y;
        double num5 = ptFeature[nFeatureIdx].Y;
        double num6 = ptFeature[nFeatureIdx + 1].Y;
        double num7 = 0.0;
        double num8 = 0.0;
        double num9 = 0.0;
        double num10 = (num2 - num) / (num3 - num);
        num9 = (num5 - (1.0 - num10) * (1.0 - num10) * num4 - num10 * num10 * num6) / (2.0 * (1.0 - num10) * num10);
        double num11 = (num2 - (1.0 - num10) * (1.0 - num10) * num - num10 * num10 * num3) / (2.0 * (1.0 - num10) * num10);
        num5 = num9;
        num2 = num11;
        if (nFeatureIdx >= 1 && nFeatureIdx < ptFeature.Count - 1)
        {
            for (int i = ptFeature[nFeatureIdx - 1].X; i < ptFeature[nFeatureIdx + 1].X; i++)
            {
                num10 = (i - num) / (num3 - num);
                num7 = (int)((1.0 - num10) * (1.0 - num10) * num4 + 2.0 * (1.0 - num10) * num10 * num5 + num10 * num10 * num6);
                num8 = (int)((1.0 - num10) * (1.0 - num10) * num + 2.0 * (1.0 - num10) * num10 * num2 + num10 * num10 * num3);
                seiresCurve.Points[i].YValues[0] = num7;
                seiresCurve.Points[i].XValue = num8;
                if (seiresCurve.Points[i].YValues[0] > m_nGammaValueMax - 1)
                {
                    seiresCurve.Points[i].YValues[0] = m_nGammaValueMax - 1;
                }
                if (seiresCurve.Points[i].YValues[0] < 0.0)
                {
                    seiresCurve.Points[i].YValues[0] = 0.0;
                }
            }
            choosepointfinalY = seiresCurve.Points[(int)num2].YValues[0];
        }
        else
        {
            CurveFittingModify(nFeatureIdx, seiresCurve, ptFeature);
        }
    }

    protected void CurveFittingModify(int nFeatureIdx, Series seiresCurve, List<Point> ptFeature)
    {
        int num = 1;
        int num2 = 0;
        int num3 = ptFeature.Count - 1;
        double[,] array = new double[6, 7];
        double[] array2 = new double[6];
        if (num == 1)
        {
            unknowN = 5;
        }
        else
        {
            unknowN = 3;
        }
        for (int i = 0; i < unknowN + 1; i++)
        {
            for (int j = 0; j < unknowN + 2; j++)
            {
                array[i, j] = 0.0;
            }
        }
        if (num == 1)
        {
            if (nFeatureIdx >= 2)
            {
                num2 = nFeatureIdx - 2;
            }
            else
            {
                unknowN--;
            }
            if (ptFeature.Count - nFeatureIdx >= 3)
            {
                num3 = nFeatureIdx + 2;
            }
            else
            {
                unknowN--;
            }
        }
        else
        {
            if (nFeatureIdx >= 1)
            {
                num2 = nFeatureIdx - 1;
            }
            else
            {
                unknowN--;
            }
            if (ptFeature.Count - nFeatureIdx >= 2)
            {
                num3 = nFeatureIdx + 1;
            }
            else
            {
                unknowN--;
            }
        }
        for (int k = num2; k <= num3; k++)
        {
            array[k - num2 + 1, 1] = 1.0;
            for (int l = 2; l < unknowN + 1; l++)
            {
                array[k - num2 + 1, l] = Math.Pow(ptFeature[k].X, l - 1);
            }
            array[k - num2 + 1, unknowN + 1] = ptFeature[k].Y;
        }
        gaussian(array);
        substitute(array, array2);
        for (int m = ptFeature[num2].X + 1; m <= ptFeature[num3].X; m++)
        {
            seiresCurve.Points[m].YValues[0] = 0.0;
            seiresCurve.Points[m].YValues[0] += array2[1];
            for (int n = 2; n < unknowN + 1; n++)
            {
                seiresCurve.Points[m].YValues[0] += Math.Pow(m, n - 1) * array2[n];
            }
            if (seiresCurve.Points[m].YValues[0] > m_nGammaValueMax - 1)
            {
                seiresCurve.Points[m].YValues[0] = m_nGammaValueMax - 1;
            }
            if (seiresCurve.Points[m].YValues[0] < 0.0)
            {
                seiresCurve.Points[m].YValues[0] = 0.0;
            }
        }
    }

    protected void gaussian(double[,] Array)
    {
        for (int i = 1; i <= unknowN; i++)
        {
            int num = i;
            for (int j = i + 1; j <= unknowN; j++)
            {
                if (Math.Abs(Array[j, i]) > Math.Abs(Array[num, i]))
                {
                    num = j;
                }
            }
            for (int k = i; k <= unknowN + 1; k++)
            {
                double num2 = Array[i, k];
                Array[i, k] = Array[num, k];
                Array[num, k] = num2;
            }
            for (int j = i + 1; j <= unknowN; j++)
            {
                for (int k = unknowN + 1; k >= i; k--)
                {
                    if (Array[i, i] != 0.0)
                    {
                        Array[j, k] -= Array[i, k] * Array[j, i] / Array[i, i];
                    }
                }
            }
        }
    }

    protected void substitute(double[,] Array, double[] answer)
    {
        for (int i = 1; i <= 1; i++)
        {
            answer[i] = 0.0;
        }
        if (Array[unknowN, unknowN] != 0.0)
        {
            answer[unknowN] = Array[unknowN, unknowN + 1] / Array[unknowN, unknowN];
        }
        for (int num = unknowN; num >= 1; num--)
        {
            double num2 = 0.0;
            for (int i = num + 1; i <= unknowN; i++)
            {
                num2 += Array[num, i] * answer[i];
            }
            if (Array[num, num] != 0.0)
            {
                answer[num] = (Array[num, unknowN + 1] - num2) / Array[num, num];
            }
        }
    }

    private void chartGamma_MouseDown(object sender, MouseEventArgs e)
    {
        if (RestoreView(sender, e) == 1)
        {
            return;
        }
        try
        {
            bool bFindPoint = false;
            bool bFindPoint_c = false;
            Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartGamma, e.X, e.Y);
            if (axisValuesFromMouse == null)
            {
                return;
            }
            Point ptSelect = new Point((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            Series pCurve = chartGamma.Series["SeriesGamma"];
            AddFeaturePoint(pCurve, m_ptFeatureR, m_ptFeatureRControl, ptSelect, ref m_nFeatureIdxR, ref m_nFeatureIdxR_c, ref bFindPoint, ref bFindPoint_c);
            if (bFindPoint)
            {
                m_chartSelectR = true;
                Series seiresRLine = chartGamma.Series["SeriesGamma"];
                Series seiresRPoints = chartGamma.Series["SeriesGammaFeature"];
                if (FindPoint == 1)
                {
                    AddBezierControl(seiresRLine, seiresRPoints, m_ptFeatureR, m_ptFeatureRControl);
                    FindPoint = 0;
                }
                if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
                {
                    diff_left_control_x = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].X - m_ptFeatureR[m_nFeatureIdxR].X;
                    diff_left_control_y = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].Y - m_ptFeatureR[m_nFeatureIdxR].Y;
                    diff_right_control_x = m_ptFeatureRControl[m_nFeatureIdxR * 2].X - m_ptFeatureR[m_nFeatureIdxR].X;
                    diff_right_control_y = m_ptFeatureRControl[m_nFeatureIdxR * 2].Y - m_ptFeatureR[m_nFeatureIdxR].Y;
                }
                else if (m_nFeatureIdxR == 0)
                {
                    diff_right_control_x = m_ptFeatureRControl[m_nFeatureIdxR * 2].X - m_ptFeatureR[m_nFeatureIdxR].X;
                    diff_right_control_y = m_ptFeatureRControl[m_nFeatureIdxR * 2].Y - m_ptFeatureR[m_nFeatureIdxR].Y;
                }
                else if (m_nFeatureIdxR == m_ptFeatureR.Count - 1)
                {
                    diff_left_control_x = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].X - m_ptFeatureR[m_nFeatureIdxR].X;
                    diff_left_control_y = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].Y - m_ptFeatureR[m_nFeatureIdxR].Y;
                }
            }
            else if (bFindPoint_c)
            {
                m_chartSelectR_C = true;
            }
        }
        catch
        {
            Console.WriteLine("out of range");
        }
    }

    private void chartGamma_MouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartGamma, e.X, e.Y);
            if (!m_chartSelectR || axisValuesFromMouse == null)
            {
                return;
            }
            Series series = chartGamma.Series["SeriesGamma"];
            if (m_nFeatureIdxR == 0)
            {
                if (axisValuesFromMouse.Item2 > m_ptFeatureR[m_nFeatureIdxR + 1].Y)
                {
                    m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y);
                }
                else
                {
                    m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, (int)axisValuesFromMouse.Item2);
                }
            }
            else if (m_nFeatureIdxR == GammaNumber - 1)
            {
                if (axisValuesFromMouse.Item2 < m_ptFeatureR[m_nFeatureIdxR - 1].Y)
                {
                    m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y);
                }
                else if (axisValuesFromMouse.Item2 > nYMax)
                {
                    m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, nYMax);
                }
                else
                {
                    m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, (int)axisValuesFromMouse.Item2);
                }
            }
            else if (axisValuesFromMouse.Item2 > m_ptFeatureR[m_nFeatureIdxR + 1].Y)
            {
                m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y);
            }
            else if (axisValuesFromMouse.Item2 < m_ptFeatureR[m_nFeatureIdxR - 1].Y)
            {
                m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y);
            }
            else
            {
                m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, (int)axisValuesFromMouse.Item2);
            }
            if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
            {
                if (m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y < m_ptFeatureR[m_nFeatureIdxR - 1].Y)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y);
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y);
                }
                else if (m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y > m_ptFeatureR[m_nFeatureIdxR + 1].Y)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y);
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y);
                }
                else
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y);
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR].Y + diff_right_control_y);
                }
            }
            else if (m_nFeatureIdxR == 0)
            {
                if (m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y > m_ptFeatureR[m_nFeatureIdxR + 1].Y)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y);
                }
                else
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y);
                }
            }
            else if (m_nFeatureIdxR == m_ptFeatureR.Count - 1)
            {
                if (m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y < m_ptFeatureR[m_nFeatureIdxR - 1].Y)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y);
                }
                else if (m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y > nYMax)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, nYMax);
                }
                else
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y);
                }
            }
            clearControlLine();
            _ = chartGamma.Series["SeriesGamma"];
            _ = chartGamma.Series["SeriesGammaFeature"];
            if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
            {
                double num = 0.0;
                double num2 = 0.0;
                double num3 = 0.0;
                double num4 = 0.0;
                double num5 = 0.0;
                double num6 = 0.0;
                double num7 = 0.0;
                double num8 = 0.0;
                int num9 = 0;
                int num10 = 0;
                num = m_ptFeatureR[m_nFeatureIdxR - 1].Y;
                num2 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 2].Y;
                num3 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].Y;
                num4 = m_ptFeatureR[m_nFeatureIdxR].Y;
                num5 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                num6 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 2].X;
                num7 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].X;
                num8 = m_ptFeatureR[m_nFeatureIdxR].X;
                num9 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                num10 = m_ptFeatureR[m_nFeatureIdxR].X;
                cubicBezier(num, num2, num3, num4, num5, num6, num7, num8, num9, num10, series);
                double num11 = 0.0;
                double num12 = 0.0;
                double num13 = 0.0;
                double num14 = 0.0;
                double num15 = 0.0;
                double num16 = 0.0;
                double num17 = 0.0;
                double num18 = 0.0;
                int num19 = 0;
                int num20 = 0;
                num11 = m_ptFeatureR[m_nFeatureIdxR].Y;
                num12 = m_ptFeatureRControl[m_nFeatureIdxR * 2].Y;
                num13 = m_ptFeatureRControl[m_nFeatureIdxR * 2 + 1].Y;
                num14 = m_ptFeatureR[m_nFeatureIdxR + 1].Y;
                num15 = m_ptFeatureR[m_nFeatureIdxR].X;
                num16 = m_ptFeatureRControl[m_nFeatureIdxR * 2].X;
                num17 = m_ptFeatureRControl[m_nFeatureIdxR * 2 + 1].X;
                num18 = m_ptFeatureR[m_nFeatureIdxR + 1].X;
                num19 = m_ptFeatureR[m_nFeatureIdxR].X;
                num20 = m_ptFeatureR[m_nFeatureIdxR + 1].X;
                cubicBezier(num11, num12, num13, num14, num15, num16, num17, num18, num19, num20, series);
            }
            else if (m_nFeatureIdxR == 0)
            {
                double num21 = 0.0;
                double num22 = 0.0;
                double num23 = 0.0;
                double num24 = 0.0;
                double num25 = 0.0;
                double num26 = 0.0;
                double num27 = 0.0;
                double num28 = 0.0;
                int num29 = 0;
                int num30 = 0;
                num21 = m_ptFeatureR[m_nFeatureIdxR].Y;
                num22 = m_ptFeatureRControl[m_nFeatureIdxR * 2].Y;
                num23 = m_ptFeatureRControl[m_nFeatureIdxR * 2 + 1].Y;
                num24 = m_ptFeatureR[m_nFeatureIdxR + 1].Y;
                num25 = m_ptFeatureR[m_nFeatureIdxR].X;
                num26 = m_ptFeatureRControl[m_nFeatureIdxR * 2].X;
                num27 = m_ptFeatureRControl[m_nFeatureIdxR * 2 + 1].X;
                num28 = m_ptFeatureR[m_nFeatureIdxR + 1].X;
                num29 = m_ptFeatureR[m_nFeatureIdxR].X;
                num30 = m_ptFeatureR[m_nFeatureIdxR + 1].X;
                cubicBezier(num21, num22, num23, num24, num25, num26, num27, num28, num29, num30, series);
            }
            else if (m_nFeatureIdxR == m_ptFeatureR.Count - 1)
            {
                double num31 = 0.0;
                double num32 = 0.0;
                double num33 = 0.0;
                double num34 = 0.0;
                double num35 = 0.0;
                double num36 = 0.0;
                double num37 = 0.0;
                double num38 = 0.0;
                int num39 = 0;
                int num40 = 0;
                num31 = m_ptFeatureR[m_nFeatureIdxR - 1].Y;
                num32 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 2].Y;
                num33 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].Y;
                num34 = m_ptFeatureR[m_nFeatureIdxR].Y;
                num35 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                num36 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 2].X;
                num37 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].X;
                num38 = m_ptFeatureR[m_nFeatureIdxR].X;
                num39 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                num40 = m_ptFeatureR[m_nFeatureIdxR].X;
                cubicBezier(num31, num32, num33, num34, num35, num36, num37, num38, num39, num40, series);
                series.Points[m_nFeatureIdxR].YValues[0] = m_ptFeatureR[m_nFeatureIdxR].Y;
                series.Points[m_nFeatureIdxR].XValue = m_ptFeatureR[m_nFeatureIdxR].X;
            }
            chartGamma.Series["SeriesGammaFeature"].Points.Clear();
            for (int i = 0; i < m_ptFeatureR.Count; i++)
            {
                chartGamma.Series["SeriesGammaFeature"].Points.AddXY(m_ptFeatureR[i].X, m_ptFeatureR[i].Y);
            }
        }
        catch
        {
            Console.WriteLine("Out of range!");
        }
    }

    private void chartGamma_MouseUp(object sender, MouseEventArgs e)
    {
        try
        {
            if (m_chartSelectR)
            {
                seiresGamma = chartGamma.Series["SeriesGamma"];
                Series seriesPoint = chartGamma.Series["SeriesGammaFeature"];
                increasingLineModify(m_nFeatureIdxR, seiresGamma, m_ptFeatureR, seriesPoint);
                clearControlLine();
            }
            else if (m_chartSelectRGB)
            {
                Series seiresCurve = chartGamma.Series["SeriesGamma"];
                Series seriesPoint2 = chartGamma.Series["SeriesGammaFeature"];
                increasingLineModify(m_nFeatureIdxR, seiresCurve, m_ptFeatureR, seriesPoint2);
                clearControlLine();
            }
            if (m_chartSelectR_C)
            {
                clearControlLine();
            }
            else if (m_chartSelectRGB_C)
            {
                clearControlLine();
            }
            m_chartSelectR = false;
            m_chartSelectR_C = false;
            m_chartSelectRGB_C = false;
            chartGamma.Cursor = Cursors.Cross;
            SaveGammaValue_temp();
            RecomputeGurve(0);
        }
        catch
        {
            Console.WriteLine("Out of range!");
        }
    }

    private Tuple<double, double> GetAxisValuesFromMouse(ref Chart chartShow, int x, int y)
    {
        ChartArea chartArea = chartShow.ChartAreas[0];
        double num = 0.0;
        double num2 = 0.0;
        int num3 = 10;
        int num4 = 10;
        try
        {
            if (num3 < x && x < chartShow.Size.Width - num3)
            {
                num = chartArea.AxisX.PixelPositionToValue(x);
            }
            if (num4 < y && y < chartShow.Size.Height - num4)
            {
                num2 = chartArea.AxisY.PixelPositionToValue(y);
            }
        }
        catch
        {
            _ = "Position Error!" + Environment.NewLine + "Pos: (" + x + "," + y + "), Size:" + chartShow.Size.Width + "x" + chartShow.Size.Height;
        }
        GammalabelPosition.Text = "x=" + (int)num + " y=" + (int)num2;
        return new Tuple<double, double>(num, num2);
    }

    public void Max(double[] array, ref double y)
    {
        double num = 0.0;
        num = array[0];
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] >= num)
            {
                num = array[i];
            }
        }
        y = num;
    }

    public void Min(double[] array, ref double y)
    {
        double num = 0.0;
        num = array[0];
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] <= num)
            {
                num = array[i];
            }
        }
        y = num;
    }

    protected void increasingLineModify(int nFeatureIdx, Series seiresCurve, List<Point> ptFeature, Series seriesPoint)
    {
        if (nFeatureIdx >= 2 && nFeatureIdx < ptFeature.Count - 2)
        {
            double num = ptFeature[nFeatureIdx - 1].X;
            double num2 = ptFeature[nFeatureIdx].X;
            double num3 = ptFeature[nFeatureIdx + 1].X;
            double num4 = ptFeature[nFeatureIdx - 1].Y;
            double num5 = ptFeature[nFeatureIdx].Y;
            double num6 = ptFeature[nFeatureIdx + 1].Y;
            double num7 = (num2 - num) / (num3 - num);
            _ = (num5 - (1.0 - num7) * (1.0 - num7) * num4 - num7 * num7 * num6) / (2.0 * (1.0 - num7) * num7);
            int num8 = 0;
            int num9 = 0;
            double num10 = 0.0;
            double num11 = 0.0;
            double[] array = new double[ptFeature[nFeatureIdx - 1].X - ptFeature[nFeatureIdx - 2].X + 1];
            double[] array2 = new double[ptFeature[nFeatureIdx + 2].X - ptFeature[nFeatureIdx + 1].X + 1];
            for (int i = ptFeature[nFeatureIdx - 2].X; i <= ptFeature[nFeatureIdx - 1].X; i++)
            {
                array[num8] = seiresCurve.Points[i].YValues[0];
                num8++;
            }
            Max(array, ref num10);
            for (int j = ptFeature[nFeatureIdx + 1].X; j <= ptFeature[nFeatureIdx + 2].X; j++)
            {
                array2[num9] = seiresCurve.Points[j].YValues[0];
                num9++;
            }
            Min(array2, ref num11);
            for (int k = ptFeature[nFeatureIdx - 1].X; k <= ptFeature[nFeatureIdx + 1].X; k++)
            {
                if (k == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point((int)seiresCurve.Points[k].XValue, (int)seiresCurve.Points[k].YValues[0]);
                }
            }
            seriesPoint.Points.Clear();
            for (int l = 0; l < ptFeature.Count; l++)
            {
                seriesPoint.Points.AddXY(ptFeature[l].X, ptFeature[l].Y);
            }
        }
        else if (nFeatureIdx == 0 && ptFeature.Count >= 4)
        {
            double[] array3 = new double[ptFeature[nFeatureIdx + 2].X - ptFeature[nFeatureIdx + 1].X + 1];
            int num12 = 0;
            double num13 = 0.0;
            for (int m = ptFeature[nFeatureIdx + 1].X; m <= ptFeature[nFeatureIdx + 2].X; m++)
            {
                array3[num12] = seiresCurve.Points[m].YValues[0];
                num12++;
            }
            Min(array3, ref num13);
            for (int n = ptFeature[nFeatureIdx].X; n <= ptFeature[nFeatureIdx + 1].X; n++)
            {
                if (n == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point((int)seiresCurve.Points[n].XValue, (int)seiresCurve.Points[n].YValues[0]);
                }
            }
            for (int num14 = ptFeature[nFeatureIdx].X; num14 <= ptFeature[nFeatureIdx + 1].X; num14++)
            {
                if (num14 == ptFeature[nFeatureIdx + 1].X)
                {
                    ptFeature[nFeatureIdx + 1] = new Point((int)seiresCurve.Points[num14].XValue, (int)seiresCurve.Points[num14].YValues[0]);
                }
            }
            for (int num15 = ptFeature[nFeatureIdx + 1].X; num15 <= ptFeature[nFeatureIdx + 2].X; num15++)
            {
                if (num15 == ptFeature[nFeatureIdx + 2].X)
                {
                    ptFeature[nFeatureIdx + 2] = new Point((int)seiresCurve.Points[num15].XValue, (int)seiresCurve.Points[num15].YValues[0]);
                }
            }
            seriesPoint.Points.Clear();
            for (int num16 = 0; num16 < ptFeature.Count; num16++)
            {
                seriesPoint.Points.AddXY(ptFeature[num16].X, ptFeature[num16].Y);
            }
        }
        else if (nFeatureIdx == 1 && ptFeature.Count >= 4)
        {
            double[] array4 = new double[ptFeature[nFeatureIdx + 2].X - ptFeature[nFeatureIdx + 1].X + 1];
            int num17 = 0;
            double num18 = 0.0;
            for (int num19 = ptFeature[nFeatureIdx + 1].X; num19 <= ptFeature[nFeatureIdx + 2].X; num19++)
            {
                array4[num17] = seiresCurve.Points[num19].YValues[0];
                num17++;
            }
            Min(array4, ref num18);
            for (int num20 = ptFeature[nFeatureIdx - 1].X; num20 <= ptFeature[nFeatureIdx + 1].X; num20++)
            {
                if (num20 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point((int)seiresCurve.Points[num20].XValue, (int)seiresCurve.Points[num20].YValues[0]);
                }
            }
            seriesPoint.Points.Clear();
            for (int num21 = 0; num21 < ptFeature.Count; num21++)
            {
                seriesPoint.Points.AddXY(ptFeature[num21].X, ptFeature[num21].Y);
            }
        }
        else if (nFeatureIdx == ptFeature.Count - 2 && ptFeature.Count >= 4)
        {
            int num22 = 0;
            double num23 = 0.0;
            double[] array5 = new double[ptFeature[nFeatureIdx - 1].X - ptFeature[nFeatureIdx - 2].X + 1];
            for (int num24 = ptFeature[nFeatureIdx - 2].X; num24 <= ptFeature[nFeatureIdx - 1].X; num24++)
            {
                array5[num22] = seiresCurve.Points[num24].YValues[0];
                num22++;
            }
            Max(array5, ref num23);
            for (int num25 = ptFeature[nFeatureIdx - 1].X; num25 <= ptFeature[nFeatureIdx + 1].X; num25++)
            {
                if (num25 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point((int)seiresCurve.Points[num25].XValue, (int)seiresCurve.Points[num25].YValues[0]);
                }
            }
            num22 = 0;
            double[] array6 = new double[ptFeature[nFeatureIdx].X - ptFeature[nFeatureIdx - 1].X + 1];
            for (int num26 = ptFeature[nFeatureIdx - 1].X; num26 <= ptFeature[nFeatureIdx].X; num26++)
            {
                array6[num22] = seiresCurve.Points[num26].YValues[0];
                num22++;
            }
            Max(array6, ref num23);
            for (int num27 = ptFeature[nFeatureIdx].X; num27 <= ptFeature[nFeatureIdx + 1].X; num27++)
            {
                if (num27 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point((int)seiresCurve.Points[num27].XValue, (int)seiresCurve.Points[num27].YValues[0]);
                }
            }
            for (int num28 = ptFeature[nFeatureIdx].X; num28 <= ptFeature[nFeatureIdx + 1].X; num28++)
            {
                if (num28 == ptFeature[nFeatureIdx + 1].X)
                {
                    ptFeature[nFeatureIdx + 1] = new Point((int)seiresCurve.Points[num28].XValue, (int)seiresCurve.Points[num28].YValues[0]);
                }
            }
            seriesPoint.Points.Clear();
            for (int num29 = 0; num29 < ptFeature.Count; num29++)
            {
                seriesPoint.Points.AddXY(ptFeature[num29].X, ptFeature[num29].Y);
            }
        }
        else
        {
            if (nFeatureIdx != ptFeature.Count - 1 || ptFeature.Count < 4)
            {
                return;
            }
            int num30 = 0;
            double num31 = 0.0;
            double[] array7 = new double[ptFeature[nFeatureIdx - 1].X - ptFeature[nFeatureIdx - 2].X + 1];
            for (int num32 = ptFeature[nFeatureIdx - 2].X; num32 <= ptFeature[nFeatureIdx - 1].X; num32++)
            {
                array7[num30] = seiresCurve.Points[num32].YValues[0];
                num30++;
            }
            Max(array7, ref num31);
            for (int num33 = ptFeature[nFeatureIdx - 1].X; num33 <= ptFeature[nFeatureIdx].X; num33++)
            {
                if (num33 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point((int)seiresCurve.Points[num33].XValue, (int)seiresCurve.Points[num33].YValues[0]);
                }
            }
            num30 = 0;
            double[] array8 = new double[ptFeature[nFeatureIdx - 2].X - ptFeature[nFeatureIdx - 3].X + 1];
            for (int num34 = ptFeature[nFeatureIdx - 3].X; num34 <= ptFeature[nFeatureIdx - 2].X; num34++)
            {
                array8[num30] = seiresCurve.Points[num34].YValues[0];
                num30++;
            }
            Max(array8, ref num31);
            for (int num35 = ptFeature[nFeatureIdx - 2].X; num35 <= ptFeature[nFeatureIdx - 1].X; num35++)
            {
                if (num35 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point((int)seiresCurve.Points[num35].XValue, (int)seiresCurve.Points[num35].YValues[0]);
                }
            }
            double[] array9 = new double[ptFeature[nFeatureIdx].X - ptFeature[nFeatureIdx - 1].X + 1];
            int num36 = 0;
            double num37 = 0.0;
            for (int num38 = ptFeature[nFeatureIdx - 1].X; num38 <= ptFeature[nFeatureIdx].X; num38++)
            {
                array9[num36] = seiresCurve.Points[num38].YValues[0];
                num36++;
            }
            Min(array9, ref num37);
            for (int num39 = ptFeature[nFeatureIdx - 2].X; num39 <= ptFeature[nFeatureIdx - 1].X; num39++)
            {
                if (num39 == ptFeature[nFeatureIdx - 1].X)
                {
                    ptFeature[nFeatureIdx - 1] = new Point((int)seiresCurve.Points[num39].XValue, (int)seiresCurve.Points[num39].YValues[0]);
                }
            }
            for (int num40 = ptFeature[nFeatureIdx - 2].X; num40 <= ptFeature[nFeatureIdx - 1].X; num40++)
            {
                if (num40 == ptFeature[nFeatureIdx - 1].X)
                {
                    ptFeature[nFeatureIdx - 1] = new Point((int)seiresCurve.Points[num40].XValue, (int)seiresCurve.Points[num40].YValues[0]);
                }
            }
            for (int num41 = ptFeature[nFeatureIdx - 1].X; num41 <= ptFeature[nFeatureIdx].X; num41++)
            {
                if (num41 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point((int)seiresCurve.Points[num41].XValue, (int)seiresCurve.Points[num41].YValues[0]);
                }
            }
            seriesPoint.Points.Clear();
            for (int num42 = 0; num42 < ptFeature.Count; num42++)
            {
                seriesPoint.Points.AddXY(ptFeature[num42].X, ptFeature[num42].Y);
            }
        }
    }

    private void RGBsameLineBack()
    {
    }

    public void SeperateSection(ref Series seiresPoints, ref Series seiresLines, List<Point> pFeaturePoint)
    {
        seiresPoints.Points.Clear();
        sectionvalue = 33;
        for (int i = 0; i < m_nGammaLength; i += m_nGammaLength / sectionvalue)
        {
            pFeaturePoint.Add(new Point((int)seiresLines.Points[i].XValue, (int)seiresLines.Points[i].YValues[0]));
        }
        pFeaturePoint.Add(new Point((int)seiresLines.Points[m_nGammaLength - 1].XValue, (int)seiresLines.Points[m_nGammaLength - 1].YValues[0]));
        for (int j = 0; j < pFeaturePoint.Count; j++)
        {
            seiresPoints.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
        }
    }

    private void GammasectiontrackBar_Scroll(object sender, EventArgs e)
    {
        m_ptFeatureR.Clear();
        Series seiresPoints = chartGamma.Series["SeriesGammaFeature"];
        Series seiresLines = chartGamma.Series["SeriesGamma"];
        SeperateSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        Series seiresRLine = chartGamma.Series["SeriesGamma"];
        Series seiresRPoints = chartGamma.Series["SeriesGammaFeature"];
        InitBezierControl(seiresRLine, seiresRPoints, m_ptFeatureR, m_ptFeatureRControl);
    }

    private static void Sort(int[] numbers, int left, int right)
    {
        if (left >= right)
        {
            return;
        }
        int num = numbers[(left + right) / 2];
        int num2 = left - 1;
        int num3 = right + 1;
        while (true)
        {
            if (numbers[++num2] >= num)
            {
                while (numbers[--num3] > num)
                {
                }
                if (num2 >= num3)
                {
                    break;
                }
                Swap(numbers, num2, num3);
            }
        }
        Sort(numbers, left, num2 - 1);
        Sort(numbers, num3 + 1, right);
    }

    private static void Swap(int[] numbers, int i, int j)
    {
        int num = numbers[i];
        numbers[i] = numbers[j];
        numbers[j] = num;
    }

    public void getRedGammaArray(out long[] pGammaR)
    {
        pGammaR = new long[33];
        for (int i = 0; i < 33; i++)
        {
            pGammaR[i] = (int)chartGamma.Series["SeriesGamma"].Points[i].YValues[0];
        }
    }

    public void setRedGammaArray(long[] pGamma, int index)
    {
        chartGamma.Series["SeriesGamma"].Points.Clear();
        chartGamma.Series["SeriesGammaFeature"].Points.Clear();
        chartSimulationGamma.Series["SimulationGamma"].Points.Clear();
        chartSimulationGamma.Series["SimulationGammaFeature"].Points.Clear();
        m_ptFeatureR.Clear();
        int num = 0;
        for (int i = 33 * index; i < 33 * (index + 1); i++)
        {
            chartGamma.Series["SeriesGamma"].Points.AddXY(num, pGamma[i]);
            num++;
            if (OpMode == 0)
            {
                chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Auto[i], pGamma[i]);
                chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Auto[i], pGamma[i]);
            }
            else
            {
                chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Manual[i], pGamma[i]);
                chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Manual[i], pGamma[i]);
            }
        }
        Series seiresPoints = chartGamma.Series["SeriesGammaFeature"];
        Series seiresLines = chartGamma.Series["SeriesGamma"];
        initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        InitBezierControl(seiresLines, seiresPoints, m_ptFeatureR, m_ptFeatureRControl);
    }

    private void GammabtnLoad_Click(object sender, EventArgs e)
    {
        int[] array = new int[65];
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Title = "Open the text file you wish";
        openFileDialog.InitialDirectory = "D:";
        openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        chartGamma.Series["SeriesGamma"].Points.Clear();
        chartGamma.Series["SeriesGammaFeature"].Points.Clear();
        chartSimulationGamma.Series["SimulationGamma"].Points.Clear();
        chartSimulationGamma.Series["SimulationGammaFeature"].Points.Clear();
        m_ptFeatureR.Clear();
        StreamReader streamReader = File.OpenText(openFileDialog.FileName);
        streamReader.ReadLine();
        string text = "0";
        int num = 0;
        while (!streamReader.EndOfStream)
        {
            text = streamReader.ReadLine();
            array[num] = Convert.ToInt16(text);
            num++;
        }
        if (OpMode == 0)
        {
            for (int i = 0; i < GammaNumber * NumberOfGroup; i++)
            {
                if (i % GammaNumber != 0)
                {
                    xnode_Auto[i] = xnode_Auto[i - 1] + 2048;
                }
                else
                {
                    xnode_Auto[i] = 0;
                }
            }
        }
        else
        {
            for (int j = 0; j < GammaNumber; j++)
            {
                if (j % GammaNumber != 0)
                {
                    xnode_Manual[j] = xnode_Manual[j - 1] + 2048;
                }
                else
                {
                    xnode_Manual[j] = 0;
                }
            }
        }
        for (int k = 0; k < 33; k++)
        {
            m_ptFeatureR.Add(new Point(k, array[k]));
            chartGamma.Series["SeriesGamma"].Points.AddXY(k, array[k]);
            chartGamma.Series["SeriesGammaFeature"].Points.AddXY(k, array[k]);
            if (OpMode == 0)
            {
                ynode_Auto[Index * GammaNumber + k] = array[k];
                chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Auto[Index * GammaNumber + k], array[k]);
                chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Auto[Index * GammaNumber + k], array[k]);
            }
            else
            {
                ynode_Manual[k] = array[k];
                chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Manual[k], array[k]);
                chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Manual[k], array[k]);
            }
        }
        UpdateLineControl();
        SaveGammaValue_temp();
        streamReader.Close();
    }

    public void UpdateLineControl()
    {
        m_ptFeatureR.Clear();
        Series seiresPoints = chartGamma.Series["SeriesGammaFeature"];
        Series seiresLines = chartGamma.Series["SeriesGamma"];
        SeperateSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        Series seiresRLine = chartGamma.Series["SeriesGamma"];
        Series seiresRPoints = chartGamma.Series["SeriesGammaFeature"];
        InitBezierControl(seiresRLine, seiresRPoints, m_ptFeatureR, m_ptFeatureRControl);
    }

    private void GammabtnSave_Click(object sender, EventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        saveFileDialog.FilterIndex = 1;
        saveFileDialog.RestoreDirectory = true;
        Stream stream;
        if (saveFileDialog.ShowDialog() != DialogResult.OK || (stream = saveFileDialog.OpenFile()) == null)
        {
            return;
        }
        using (StreamWriter streamWriter = new StreamWriter(stream))
        {
            streamWriter.WriteLine("GlobalToneGamma:1~33");
            for (int i = 0; i < 33; i++)
            {
                if (OpMode == 0)
                {
                    streamWriter.WriteLine((int)chartGamma.Series["SeriesGamma"].Points[i].YValues[0]);
                }
                else
                {
                    streamWriter.WriteLine((int)chartGamma.Series["SeriesGamma"].Points[i].YValues[0]);
                }
            }
        }
        stream.Close();
        MessageBox.Show("Save GlobalToneGamma Data OK");
    }

    private void CheckControlPoint(List<Point> pFeaturePoint, int ptX, Label labelCtrlPt)
    {
        int nFeatureIdx = -1;
        IsFeaturePoint(pFeaturePoint, ptX, ref nFeatureIdx);
        if (nFeatureIdx >= 0)
        {
            labelCtrlPt.Text = m_strCtrlPtYes;
        }
        else
        {
            labelCtrlPt.Text = m_strCtrlPtNo;
        }
    }

    private void CheckControlPoint_y(List<Point> pFeaturePoint, int ptY, Label labelCtrlPt)
    {
        int nFeatureIdx = -1;
        IsFeaturePoint_y(pFeaturePoint, ptY, ref nFeatureIdx);
        if (nFeatureIdx >= 0)
        {
            labelCtrlPt.Text = m_strCtrlPtYes;
        }
        else
        {
            labelCtrlPt.Text = m_strCtrlPtNo;
        }
    }

    private void IsFeaturePoint(List<Point> pFeaturePoint, int ptX, ref int nFeatureIdx)
    {
        nFeatureIdx = -1;
        for (int i = 0; i < pFeaturePoint.Count; i++)
        {
            if (pFeaturePoint[i].X == ptX)
            {
                nFeatureIdx = i;
                break;
            }
        }
    }

    private void IsFeaturePoint_y(List<Point> pFeaturePoint, int ptY, ref int nFeatureIdx)
    {
        nFeatureIdx = -1;
        for (int i = 0; i < pFeaturePoint.Count; i++)
        {
            if (pFeaturePoint[i].Y == ptY)
            {
                nFeatureIdx = i;
                break;
            }
        }
    }

    private void AddFeaturePointByXY(Series pCurve, Series seiresPointsfor_c, Series seiresControl, List<Point> pFeaturePoint, Point ptSelect, ref int nFeatureIdx)
    {
        nFeatureIdx = -1;
        IsFeaturePoint(pFeaturePoint, ptSelect.X, ref nFeatureIdx);
        IsFeaturePoint_y(pFeaturePoint, ptSelect.Y, ref nFeatureIdx);
        if (nFeatureIdx >= 0)
        {
            pFeaturePoint[nFeatureIdx] = new Point(ptSelect.X, ptSelect.Y);
        }
        else
        {
            pFeaturePoint.Add(new Point(ptSelect.X, ptSelect.Y));
            pFeaturePoint.Sort((px, py) => px.X.CompareTo(py.X));
            for (int i = 0; i < pFeaturePoint.Count; i++)
            {
                if (pFeaturePoint[i].X == ptSelect.X)
                {
                    nFeatureIdx = i;
                }
            }
        }
        if (nFeatureIdx >= 0)
        {
            if (nFeatureIdx >= 1 && nFeatureIdx < pFeaturePoint.Count - 1)
            {
                BezierCurveBelongModify(nFeatureIdx, pCurve, pFeaturePoint, ref new_Y);
                InitBezierControl(pCurve, seiresPointsfor_c, pFeaturePoint, m_ptFeatureRControl);
            }
            else
            {
                pFeaturePoint[nFeatureIdx] = new Point(pFeaturePoint[nFeatureIdx].X, ptSelect.Y);
                CurveFittingModify(nFeatureIdx, pCurve, pFeaturePoint);
                InitBezierControl(pCurve, seiresPointsfor_c, pFeaturePoint, m_ptFeatureRControl);
            }
        }
    }

    private void AddFeaturePoint(Series pCurve, List<Point> pFeaturePoint, List<Point> pFeaturePoint_c, Point ptSelect, ref int nFeatureIdx, ref int nFeatureIdx_c, ref bool bFindPoint, ref bool bFindPoint_c)
    {
        int num = 1;
        int num2 = 18;
        bFindPoint = false;
        bFindPoint_c = false;
        for (int i = 0; i < pFeaturePoint_c.Count; i++)
        {
            if (pFeaturePoint_c[i].X > ptSelect.X - num && pFeaturePoint_c[i].X < ptSelect.X + num && pFeaturePoint_c[i].Y > ptSelect.Y - num2 && pFeaturePoint_c[i].Y < ptSelect.Y + num2)
            {
                bFindPoint_c = true;
                nFeatureIdx_c = i;
                chartGamma.Cursor = Cursors.NoMoveVert;
                break;
            }
        }
        for (int j = 0; j < pFeaturePoint.Count; j++)
        {
            if (pFeaturePoint[j].X > ptSelect.X - num && pFeaturePoint[j].X < ptSelect.X + num && pFeaturePoint[j].Y > ptSelect.Y - num2 && pFeaturePoint[j].Y < ptSelect.Y + num2)
            {
                bFindPoint = true;
                nFeatureIdx = j;
                chartGamma.Cursor = Cursors.NoMove2D;
                break;
            }
        }
    }

    private void RemoveFeaturePoint(Series pCurve, Series SeriesGammaFeature, List<Point> pFeaturePoint, Point ptSelect, ref bool bFindPoint)
    {
        int num = 5;
        int num2 = 0;
        bFindPoint = false;
        if (pFeaturePoint.Count <= 2)
        {
            return;
        }
        for (int i = 0; i < pFeaturePoint.Count; i++)
        {
            if (pFeaturePoint[i].X > ptSelect.X - num && pFeaturePoint[i].X < ptSelect.X + num && pFeaturePoint[i].Y > ptSelect.Y - num && pFeaturePoint[i].Y < ptSelect.Y + num)
            {
                num2 = i;
                bFindPoint = true;
                pFeaturePoint.RemoveAt(i);
                double choosepointfinalY = 0.0;
                BezierCurveFirstModify(num2, pCurve, pFeaturePoint, ref choosepointfinalY);
                SeriesGammaFeature.Points.Clear();
                for (int j = 0; j < pFeaturePoint.Count; j++)
                {
                    SeriesGammaFeature.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
                }
                break;
            }
        }
    }

    private void ResetCurve(string strGammaName)
    {
        int num = 0;
        GetGammaWithOB(1024m, 1024m, 0m, out var pGammaArray);
        if (pGammaArray != null)
        {
            chartGamma.Series[strGammaName].Points.Clear();
            for (num = 0; num < pGammaArray.Length; num++)
            {
                chartGamma.Series[strGammaName].Points.AddXY(num, pGammaArray[num].Y);
            }
            List<Point> ptFeatureR = m_ptFeatureR;
            for (int i = 0; i < ptFeatureR.Count; i++)
            {
                ptFeatureR[i] = new Point(ptFeatureR[i].X, (int)chartGamma.Series[strGammaName].Points[ptFeatureR[i].X].YValues[0]);
            }
            string name = strGammaName.Replace("Gamma", "Feature");
            Series series = chartGamma.Series[name];
            series.Points.Clear();
            for (int j = 0; j < ptFeatureR.Count; j++)
            {
                series.Points.AddXY(ptFeatureR[j].X, ptFeatureR[j].Y);
            }
        }
    }

    private void initialGammavalue(Series seiresCurve, int index, Series seiresPoint, long[] gammavalue, List<Point> ptFeature, List<Point> ptFeatureControl)
    {
        seiresCurve.Points.Clear();
        seiresPoint.Points.Clear();
        ptFeature.Clear();
        int num = 0;
        for (int i = 33 * index; i < 33 * (index + 1); i++)
        {
            seiresCurve.Points.AddXY(num, gammavalue[i]);
            num++;
        }
        initialAddSection(ref seiresPoint, ref seiresCurve, ptFeature);
        InitBezierControl(seiresCurve, seiresPoint, ptFeature, ptFeatureControl);
    }

    private void btnResetRCurve_Click(object sender, EventArgs e)
    {
        try
        {
            OpMode = GammacomboBoxOpType.SelectedIndex;
            opTypeItem.DataValue[0] = OpMode;
            if (OpMode == 1)
            {
                GammacomboBoxIndex.Enabled = false;
                itemR = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GammaLut");
                int index = 0;
                Series seiresCurve = chartGamma.Series["SeriesGamma"];
                Series seiresPoint = chartGamma.Series["SeriesGammaFeature"];
                initialGammavalue(seiresCurve, index, seiresPoint, gammainitialvalueR_Manual, m_ptFeatureR, m_ptFeatureRControl);
            }
            else
            {
                GammacomboBoxIndex.Enabled = true;
                itemR = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneSft");
                int num = GammacomboBoxIndex.SelectedIndex;
                if (num < 0)
                {
                    num = 0;
                }
                Series seiresCurve2 = chartGamma.Series["SeriesGamma"];
                Series seiresPoint2 = chartGamma.Series["SeriesGammaFeature"];
                initialGammavalue(seiresCurve2, num, seiresPoint2, gammainitialvalueR, m_ptFeatureR, m_ptFeatureRControl);
            }
            SaveGammaValue_temp();
        }
        catch
        {
            ResetCurve("SeriesGamma");
            Series seiresRLine = chartGamma.Series["SeriesGamma"];
            Series seiresRPoints = chartGamma.Series["SeriesGammaFeature"];
            InitBezierControl(seiresRLine, seiresRPoints, m_ptFeatureR, m_ptFeatureRControl);
        }
    }

    private void initialGammaValue()
    {
        _ = gammaGroup.ItemList[itemR].DataValue.Length;
        gammainitialvalueR = new long[33 * NumberOfGroup];
        for (int i = 0; i < GammaNumber * NumberOfGroup; i++)
        {
            if (i % GammaNumber != 0)
            {
                xnode_Auto[i] = xnode_Auto[i - 1] + m_nGammaInterval;
            }
            else
            {
                xnode_Auto[i] = 0;
            }
        }
        for (int j = 0; j < GammaNumber * NumberOfGroup; j++)
        {
            ynode_Auto[j] = Convert.ToInt32(gammaGroup.ItemList[itemR].DataValue[j]);
        }
        for (int k = 0; k < NumberOfGroup; k++)
        {
            for (int l = 0; l < GammaNumber; l++)
            {
                gammainitialvalueR[l + k * GammaNumber] = ynode_Auto[l + k * GammaNumber];
            }
        }
        itemR = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GammaLut");
        gammainitialvalueR_Manual = new long[33];
        for (int m = 0; m < GammaNumber; m++)
        {
            if (m % GammaNumber != 0)
            {
                xnode_Manual[m] = xnode_Manual[m - 1] + m_nGammaInterval;
            }
            else
            {
                xnode_Manual[m] = 0;
            }
        }
        for (int n = 0; n < GammaNumber; n++)
        {
            ynode_Manual[n] = Convert.ToInt32(gammaGroup.ItemList[itemR].DataValue[n]);
        }
        for (int num = 0; num < GammaNumber; num++)
        {
            gammainitialvalueR_Manual[num] = ynode_Manual[num];
        }
    }

    public void SaveGlobalToneGamma()
    {
        int itemFieldIdx = GetItemFieldIdx("API_WDRCurveFull_ENABLE_Gamma");
        int itemFieldIdx2 = GetItemFieldIdx("API_WDRCurveFull_OP_TYPE_Gamma");
        int itemFieldIdx3 = GetItemFieldIdx("API_WDRCurveFull_AUTO_GammaLut");
        int itemFieldIdx4 = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GammaLut");
        TakePoint();
        gammaGroup.ItemList[itemFieldIdx] = enableItem;
        gammaGroup.ItemList[itemFieldIdx2] = opTypeItem;
        for (int i = 0; i < gammaGroup.ItemList[itemFieldIdx3].DataValue.Length; i++)
        {
            gammaGroup.ItemList[itemFieldIdx3].DataValue[i] = ynode_Auto[i];
        }
        for (int j = 0; j < gammaGroup.ItemList[itemFieldIdx4].DataValue.Length; j++)
        {
            gammaGroup.ItemList[itemFieldIdx4].DataValue[j] = ynode_Manual[j];
        }
    }

    public void ReadPage()
    {
        int itemFieldIdx = GetItemFieldIdx("API_WDRCurveFull_ENABLE_Gamma");
        int itemFieldIdx2 = GetItemFieldIdx("API_WDRCurveFull_OP_TYPE_Gamma");
        int itemFieldIdx3 = GetItemFieldIdx("API_WDRCurveFull_AUTO_GammaLut");
        int itemFieldIdx4 = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GammaLut");
        enableItem = gammaGroup.ItemList[itemFieldIdx];
        opTypeItem = gammaGroup.ItemList[itemFieldIdx2];
        for (int i = 0; i < GammaNumber * NumberOfGroup; i++)
        {
            if (i % GammaNumber != 0)
            {
                xnode_Auto[i] = xnode_Auto[i - 1] + 2048;
            }
            else
            {
                xnode_Auto[i] = 0;
            }
        }
        for (int j = 0; j < GammaNumber; j++)
        {
            if (j % GammaNumber != 0)
            {
                xnode_Manual[j] = xnode_Manual[j - 1] + 2048;
            }
            else
            {
                xnode_Manual[j] = 0;
            }
        }
        for (int k = 0; k < gammaGroup.ItemList[itemFieldIdx3].DataValue.Length; k++)
        {
            ynode_Auto[k] = Convert.ToInt32(gammaGroup.ItemList[itemFieldIdx3].DataValue[k]);
            gammainitialvalueR[k] = ynode_Auto[k];
        }
        for (int l = 0; l < gammaGroup.ItemList[itemFieldIdx4].DataValue.Length; l++)
        {
            ynode_Manual[l] = Convert.ToInt32(gammaGroup.ItemList[itemFieldIdx4].DataValue[l]);
            gammainitialvalueR_Manual[l] = ynode_Manual[l];
        }
        chartGamma.Series["SeriesGamma"].Points.Clear();
        chartGamma.Series["SeriesGammaFeature"].Points.Clear();
        chartSimulationGamma.Series["SimulationGamma"].Points.Clear();
        chartSimulationGamma.Series["SimulationGammaFeature"].Points.Clear();
        m_ptFeatureR.Clear();
        if (OpMode == 1)
        {
            for (int m = 0; m < 33; m++)
            {
                m_ptFeatureR.Add(new Point(m, ynode_Manual[m]));
                chartGamma.Series["SeriesGamma"].Points.AddXY(m, ynode_Manual[m]);
                chartGamma.Series["SeriesGammaFeature"].Points.AddXY(m, ynode_Manual[m]);
                chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Manual[m], ynode_Manual[m]);
                chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Manual[m], ynode_Manual[m]);
            }
        }
        else
        {
            for (int n = 0; n < 33; n++)
            {
                m_ptFeatureR.Add(new Point(n, ynode_Auto[Index * GammaNumber + n]));
                chartGamma.Series["SeriesGamma"].Points.AddXY(n, ynode_Auto[Index * GammaNumber + n]);
                chartGamma.Series["SeriesGammaFeature"].Points.AddXY(n, ynode_Auto[Index * GammaNumber + n]);
                chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Auto[Index * GammaNumber + n], ynode_Auto[Index * GammaNumber + n]);
                chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Auto[Index * GammaNumber + n], ynode_Auto[Index * GammaNumber + n]);
            }
        }
    }

    private void TakePoint()
    {
        for (int i = 0; i < NumberOfGroup; i++)
        {
            for (int j = 0; j < GammaNumber; j++)
            {
                ynode_Auto[j + i * GammaNumber] = Convert.ToInt32(gammainitialvalueR[j + i * GammaNumber]);
            }
        }
        for (int k = 0; k < GammaNumber; k++)
        {
            ynode_Manual[k] = Convert.ToInt32(gammainitialvalueR_Manual[k]);
        }
    }

    private void RecomputeGurve(int all)
    {
        switch (all)
        {
            case 1:
                {
                    long[] array = new long[9];
                    for (int i = 0; i < NumberOfGroup; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            array[j] = GlobalToneParam_Auto[i * 9 + j];
                        }
                        GenGlobalToneCurve1 genGlobalToneCurve = new GenGlobalToneCurve1();
                        genGlobalToneCurve.m_genParam = array;
                        genGlobalToneCurve.update();
                        for (int k = 0; k < 33; k++)
                        {
                            ynode_Auto[i * 33 + k] = (int)genGlobalToneCurve.m_genPoint[k];
                            gammainitialvalueR[i * 33 + k] = genGlobalToneCurve.m_genPoint[k];
                        }
                    }
                    for (int l = 0; l < 9; l++)
                    {
                        array[l] = GlobalToneParam_Manual[l];
                    }
                    GenGlobalToneCurve1 genGlobalToneCurve2 = new GenGlobalToneCurve1();
                    genGlobalToneCurve2.m_genParam = array;
                    genGlobalToneCurve2.update();
                    for (int m = 0; m < 33; m++)
                    {
                        ynode_Manual[m] = (int)genGlobalToneCurve2.m_genPoint[m];
                        gammainitialvalueR_Manual[m] = genGlobalToneCurve2.m_genPoint[m];
                    }
                    chartGamma.Series["SeriesGamma"].Points.Clear();
                    chartGamma.Series["SeriesGammaFeature"].Points.Clear();
                    chartSimulationGamma.Series["SimulationGamma"].Points.Clear();
                    chartSimulationGamma.Series["SimulationGammaFeature"].Points.Clear();
                    m_ptFeatureR.Clear();
                    if (OpMode == 1)
                    {
                        for (int n = 0; n < 33; n++)
                        {
                            m_ptFeatureR.Add(new Point(n, ynode_Manual[n]));
                            chartGamma.Series["SeriesGamma"].Points.AddXY(n, ynode_Manual[n]);
                            chartGamma.Series["SeriesGammaFeature"].Points.AddXY(n, ynode_Manual[n]);
                            chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Manual[n], ynode_Manual[n]);
                            chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Manual[n], ynode_Manual[n]);
                        }
                    }
                    else
                    {
                        for (int num = 0; num < 33; num++)
                        {
                            m_ptFeatureR.Add(new Point(num, ynode_Auto[Index * GammaNumber + num]));
                            chartGamma.Series["SeriesGamma"].Points.AddXY(num, ynode_Auto[Index * GammaNumber + num]);
                            chartGamma.Series["SeriesGammaFeature"].Points.AddXY(num, ynode_Auto[Index * GammaNumber + num]);
                            chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Auto[Index * GammaNumber + num], ynode_Auto[Index * GammaNumber + num]);
                            chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Auto[Index * GammaNumber + num], ynode_Auto[Index * GammaNumber + num]);
                        }
                    }
                    break;
                }
            case 0:
                RenewSimulation();
                break;
        }
    }

    private void RenewSimulation()
    {
        for (int i = 0; i < GammaNumber * NumberOfGroup; i++)
        {
            if (i % GammaNumber != 0)
            {
                xnode_Auto[i] = xnode_Auto[i - 1] + m_nGammaInterval;
            }
            else
            {
                xnode_Auto[i] = 0;
            }
        }
        for (int j = 0; j < GammaNumber; j++)
        {
            if (j % GammaNumber != 0)
            {
                xnode_Manual[j] = xnode_Manual[j - 1] + m_nGammaInterval;
            }
            else
            {
                xnode_Manual[j] = 0;
            }
        }
        for (int k = 0; k < NumberOfGroup; k++)
        {
            for (int l = 0; l < GammaNumber; l++)
            {
                ynode_Auto[l + k * GammaNumber] = Convert.ToInt32(gammainitialvalueR[l + k * GammaNumber]);
            }
        }
        for (int m = 0; m < GammaNumber; m++)
        {
            ynode_Manual[m] = Convert.ToInt32(gammainitialvalueR_Manual[m]);
        }
        chartSimulationGamma.Series["SimulationGamma"].Points.Clear();
        chartSimulationGamma.Series["SimulationGammaFeature"].Points.Clear();
        if (OpMode == 1)
        {
            for (int n = 0; n < 33; n++)
            {
                chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Manual[n], ynode_Manual[n]);
                chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Manual[n], ynode_Manual[n]);
            }
        }
        else
        {
            for (int num = 0; num < 33; num++)
            {
                chartSimulationGamma.Series["SimulationGamma"].Points.AddXY(xnode_Auto[Index * GammaNumber + num], ynode_Auto[Index * GammaNumber + num]);
                chartSimulationGamma.Series["SimulationGammaFeature"].Points.AddXY(xnode_Auto[Index * GammaNumber + num], ynode_Auto[Index * GammaNumber + num]);
            }
        }
    }

    private void SetChartScaleXY(bool enable)
    {
        chartGamma.ChartAreas[0].CursorX.IsUserSelectionEnabled = enable;
        chartGamma.ChartAreas[0].CursorY.IsUserSelectionEnabled = enable;
    }

    private int RestoreView(object sender, MouseEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            Chart chart = sender as Chart;
            if (e.Button == MouseButtons.Right)
            {
                chart.ChartAreas[0].AxisX.ScaleView.ZoomReset(1);
                chart.ChartAreas[0].AxisY.ScaleView.ZoomReset(1);
                return 1;
            }
        }
        return 0;
    }

    private void InitChartScale()
    {
    }

    protected override bool ProcessKeyPreview(ref Message msg)
    {
        int num = 256;
        int num2 = 257;
        if (((int)msg.WParam & 0xFFFF & 0x11) == 17)
        {
            if (msg.Msg == num)
            {
                SetChartScaleXY(enable: true);
            }
            else if (msg.Msg == num2)
            {
                SetChartScaleXY(enable: false);
            }
        }
        return base.ProcessKeyPreview(ref msg);
    }

    internal void SetWinFormFocus(object sender, EventArgs e)
    {
        GammacomboBoxIndex.Focus();
    }

    private void InitGenParam()
    {
        long[] array = GlobalToneParam_Manual = new long[9] { 492L, 4096L, 492L, 4096L, 1024L, 1024L, 1024L, 2470L, 2048L };
        for (int i = 0; i < NumberOfGroup; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                GlobalToneParam_Auto[i * 9 + j] = array[j];
            }
        }
        for (int k = 0; k < 9; k++)
        {
            GlobalToneParam_Manual[k] = array[k];
        }
    }

    private void button1_GenParam_Click(object sender, EventArgs e)
    {
        FormGlobalToneSetParam formGlobalToneSetParam = new FormGlobalToneSetParam();
        formGlobalToneSetParam.Text = "FormCurve1SetParam";
        formGlobalToneSetParam.SetSelfDlgParam(2, "SetParam", ref m_objGlobalTone.m_genParamTitle, ref m_objGlobalTone.m_genParamMin, ref m_objGlobalTone.m_genParamMax);
        if (OpMode == 1)
        {
            for (int i = 0; i < 9; i++)
            {
                m_objGlobalTone.m_genParam[i] = GlobalToneParam_Manual[i];
            }
        }
        else
        {
            for (int j = 0; j < 9; j++)
            {
                m_objGlobalTone.m_genParam[j] = GlobalToneParam_Auto[Index * 9 + j];
            }
        }
        formGlobalToneSetParam.SftSize_Changed = (byte)m_objGlobalTone.m_genParam.Length;
        formGlobalToneSetParam.GlobalToneSft_Changed = m_objGlobalTone.m_genParam;
        formGlobalToneSetParam.ShowDialog();
        if (formGlobalToneSetParam.DialogResult == DialogResult.OK)
        {
            m_objGlobalTone.m_genParam = formGlobalToneSetParam.GlobalToneSft_Changed;
            if (OpMode == 1)
            {
                for (int k = 0; k < 9; k++)
                {
                    GlobalToneParam_Manual[k] = m_objGlobalTone.m_genParam[k];
                }
            }
            else
            {
                for (int l = 0; l < 9; l++)
                {
                    GlobalToneParam_Auto[Index * 9 + l] = m_objGlobalTone.m_genParam[l];
                }
            }
            RecomputeGurve(1);
            formGlobalToneSetParam.Dispose();
        }
        else
        {
            formGlobalToneSetParam.Dispose();
        }
    }

    private void chartGamma_SelectionRangeChanging(object sender, CursorEventArgs e)
    {
        e.NewSelectionStart = (int)e.NewSelectionStart;
        e.NewSelectionEnd = (int)e.NewSelectionEnd;
    }

    private int GetItemFieldIdx(string type)
    {
        for (int i = 0; i < gammaGroup.ItemList.Count; i++)
        {
            if (gammaGroup.ItemList[i].Tag.Equals(type))
            {
                return i;
            }
        }
        return -1;
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
        ChartArea chartArea = new ChartArea();
        Legend legend = new Legend();
        Series series = new Series();
        Series series2 = new Series();
        ChartArea chartArea2 = new ChartArea();
        Legend legend2 = new Legend();
        Series series3 = new Series();
        Series series4 = new Series();
        groupBox1_GenParam = new GroupBox();
        button1_GenParam = new Button();
        chartSimulationGamma = new Chart();
        GammalabelPosition = new Label();
        GammagroupBoxApi2 = new GroupBox();
        GammalabelOpType = new Label();
        GammacomboBoxIndex = new ComboBox();
        GammacomboBoxOpType = new ComboBox();
        label3 = new Label();
        GammacheckBoxEnable = new CheckBox();
        GammagroupFileMenu = new GroupBox();
        GammabtnLoad = new Button();
        GammabtnSave = new Button();
        chartGamma = new Chart();
        groupBox1_GenParam.SuspendLayout();
        ((ISupportInitialize)chartSimulationGamma).BeginInit();
        GammagroupBoxApi2.SuspendLayout();
        GammagroupFileMenu.SuspendLayout();
        ((ISupportInitialize)chartGamma).BeginInit();
        SuspendLayout();
        groupBox1_GenParam.Controls.Add(button1_GenParam);
        groupBox1_GenParam.Location = new Point(569, 319);
        groupBox1_GenParam.Name = "groupBox1_GenParam";
        groupBox1_GenParam.Size = new Size(245, 84);
        groupBox1_GenParam.TabIndex = 169;
        groupBox1_GenParam.TabStop = false;
        groupBox1_GenParam.Text = "Generate Curve";
        button1_GenParam.Location = new Point(20, 33);
        button1_GenParam.Name = "button1_GenParam";
        button1_GenParam.Size = new Size(75, 23);
        button1_GenParam.TabIndex = 0;
        button1_GenParam.Text = "Set Param";
        button1_GenParam.UseVisualStyleBackColor = true;
        button1_GenParam.Click += new EventHandler(button1_GenParam_Click);
        chartArea.Name = "ChartArea1";
        chartSimulationGamma.ChartAreas.Add(chartArea);
        chartSimulationGamma.Cursor = Cursors.Cross;
        legend.Name = "Legend1";
        chartSimulationGamma.Legends.Add(legend);
        chartSimulationGamma.Location = new Point(12, 393);
        chartSimulationGamma.Name = "chartSimulationGamma";
        series.BorderWidth = 2;
        series.ChartArea = "ChartArea1";
        series.ChartType = SeriesChartType.Spline;
        series.Color = Color.Red;
        series.Legend = "Legend1";
        series.MarkerSize = 7;
        series.Name = "SimulationGamma";
        series2.BorderColor = Color.FromArgb(0, 192, 0);
        series2.BorderWidth = 2;
        series2.ChartArea = "ChartArea1";
        series2.ChartType = SeriesChartType.Point;
        series2.Color = Color.Fuchsia;
        series2.Legend = "Legend1";
        series2.MarkerColor = Color.Green;
        series2.MarkerSize = 7;
        series2.MarkerStyle = MarkerStyle.Circle;
        series2.Name = "SimulationGammaFeature";
        series2.YValuesPerPoint = 2;
        chartSimulationGamma.Series.Add(series);
        chartSimulationGamma.Series.Add(series2);
        chartSimulationGamma.Size = new Size(520, 244);
        chartSimulationGamma.TabIndex = 168;
        chartSimulationGamma.Text = "chart1";
        chartSimulationGamma.Visible = false;
        GammalabelPosition.Location = new Point(10, 378);
        GammalabelPosition.Name = "GammalabelPosition";
        GammalabelPosition.Size = new Size(101, 12);
        GammalabelPosition.TabIndex = 167;
        GammalabelPosition.Text = "Position:Out of chart";
        GammagroupBoxApi2.Controls.Add(GammalabelOpType);
        GammagroupBoxApi2.Controls.Add(GammacomboBoxIndex);
        GammagroupBoxApi2.Controls.Add(GammacomboBoxOpType);
        GammagroupBoxApi2.Controls.Add(label3);
        GammagroupBoxApi2.Controls.Add(GammacheckBoxEnable);
        GammagroupBoxApi2.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        GammagroupBoxApi2.Location = new Point(569, 165);
        GammagroupBoxApi2.Name = "GammagroupBoxApi2";
        GammagroupBoxApi2.Size = new Size(232, 141);
        GammagroupBoxApi2.TabIndex = 166;
        GammagroupBoxApi2.TabStop = false;
        GammagroupBoxApi2.Text = "Option";
        GammalabelOpType.AutoSize = true;
        GammalabelOpType.Location = new Point(95, 40);
        GammalabelOpType.Name = "GammalabelOpType";
        GammalabelOpType.Size = new Size(55, 15);
        GammalabelOpType.TabIndex = 138;
        GammalabelOpType.Text = "OpType :";
        GammacomboBoxIndex.DropDownStyle = ComboBoxStyle.DropDownList;
        GammacomboBoxIndex.FormattingEnabled = true;
        GammacomboBoxIndex.Items.AddRange(new object[16]
        {
            "index 0", "index 1", "index 2", "index 3", "index 4", "index 5", "index 6", "index 7", "index 8", "index 9",
            "index 10", "index 11", "index 12", "index 13", "index 14", "index 15"
        });
        GammacomboBoxIndex.Text = "index 0";
        GammacomboBoxIndex.Location = new Point(102, 88);
        GammacomboBoxIndex.Name = "GammacomboBoxIndex";
        GammacomboBoxIndex.Size = new Size(121, 23);
        GammacomboBoxIndex.TabIndex = 134;
        GammacomboBoxIndex.SelectedIndexChanged += new EventHandler(GammacomboBoxIndex_SelectedIndexChanged);
        GammacomboBoxOpType.DropDownStyle = ComboBoxStyle.DropDownList;
        GammacomboBoxOpType.FormattingEnabled = true;
        GammacomboBoxOpType.Items.AddRange(new object[2] { "Auto", "Manual" });
        GammacomboBoxOpType.Location = new Point(160, 37);
        GammacomboBoxOpType.Name = "GammacomboBoxOpType";
        GammacomboBoxOpType.Size = new Size(63, 23);
        GammacomboBoxOpType.TabIndex = 137;
        GammacomboBoxOpType.SelectedIndexChanged += new EventHandler(GammacomboBoxOpType_SelectedIndexChanged);
        label3.AutoSize = true;
        label3.Location = new Point(17, 91);
        label3.Name = "label3";
        label3.Size = new Size(70, 15);
        label3.TabIndex = 135;
        label3.Text = "Select Index :";
        GammacheckBoxEnable.AutoSize = true;
        GammacheckBoxEnable.Location = new Point(20, 39);
        GammacheckBoxEnable.Name = "GammacheckBoxEnable";
        GammacheckBoxEnable.Size = new Size(58, 19);
        GammacheckBoxEnable.TabIndex = 136;
        GammacheckBoxEnable.Text = "Enable";
        GammacheckBoxEnable.UseVisualStyleBackColor = true;
        GammacheckBoxEnable.CheckedChanged += new EventHandler(GammacheckBoxEnable_CheckedChanged);
        GammagroupFileMenu.Controls.Add(GammabtnLoad);
        GammagroupFileMenu.Controls.Add(GammabtnSave);
        GammagroupFileMenu.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        GammagroupFileMenu.Location = new Point(614, 28);
        GammagroupFileMenu.Name = "GammagroupFileMenu";
        GammagroupFileMenu.Size = new Size(187, 131);
        GammagroupFileMenu.TabIndex = 165;
        GammagroupFileMenu.TabStop = false;
        GammagroupFileMenu.Text = "File";
        GammabtnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        GammabtnLoad.BackColor = Color.LightCyan;
        GammabtnLoad.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        GammabtnLoad.Location = new Point(49, 40);
        GammabtnLoad.Name = "GammabtnLoad";
        GammabtnLoad.Size = new Size(90, 23);
        GammabtnLoad.TabIndex = 132;
        GammabtnLoad.Text = "Load";
        GammabtnLoad.UseVisualStyleBackColor = false;
        GammabtnLoad.Click += new EventHandler(GammabtnLoad_Click);
        GammabtnSave.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        GammabtnSave.BackColor = Color.LightCyan;
        GammabtnSave.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        GammabtnSave.Location = new Point(49, 88);
        GammabtnSave.Name = "GammabtnSave";
        GammabtnSave.Size = new Size(90, 23);
        GammabtnSave.TabIndex = 133;
        GammabtnSave.Text = "Save";
        GammabtnSave.UseVisualStyleBackColor = false;
        GammabtnSave.Click += new EventHandler(GammabtnSave_Click);
        chartGamma.BackImageWrapMode = ChartImageWrapMode.TileFlipXY;
        chartArea2.Name = "ChartArea1";
        chartGamma.ChartAreas.Add(chartArea2);
        chartGamma.Cursor = Cursors.Cross;
        legend2.Name = "Legend1";
        chartGamma.Legends.Add(legend2);
        chartGamma.Location = new Point(12, 28);
        chartGamma.Name = "chartGamma";
        series3.BorderWidth = 2;
        series3.ChartArea = "ChartArea1";
        series3.ChartType = SeriesChartType.Line;
        series3.Color = Color.Red;
        series3.Legend = "Legend1";
        series3.MarkerSize = 7;
        series3.Name = "SeriesGamma";
        series4.BorderColor = Color.FromArgb(0, 192, 0);
        series4.BorderWidth = 2;
        series4.ChartArea = "ChartArea1";
        series4.ChartType = SeriesChartType.Point;
        series4.Color = Color.Fuchsia;
        series4.Legend = "Legend1";
        series4.MarkerColor = Color.Green;
        series4.MarkerSize = 7;
        series4.MarkerStyle = MarkerStyle.Circle;
        series4.Name = "SeriesGammaFeature";
        series4.YValuesPerPoint = 2;
        chartGamma.Series.Add(series3);
        chartGamma.Series.Add(series4);
        chartGamma.Size = new Size(520, 347);
        chartGamma.TabIndex = 164;
        chartGamma.Text = "chart1";
        chartGamma.SelectionRangeChanging += new EventHandler<CursorEventArgs>(chartGamma_SelectionRangeChanging);
        chartGamma.MouseDown += new MouseEventHandler(chartGamma_MouseDown);
        chartGamma.MouseMove += new MouseEventHandler(chartGamma_MouseMove);
        chartGamma.MouseUp += new MouseEventHandler(chartGamma_MouseUp);
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.ControlLightLight;
        ClientSize = new Size(989, 664);
        Controls.Add(groupBox1_GenParam);
        Controls.Add(chartSimulationGamma);
        Controls.Add(GammalabelPosition);
        Controls.Add(GammagroupBoxApi2);
        Controls.Add(GammagroupFileMenu);
        Controls.Add(chartGamma);
        Name = "FormGlobalToneGammaVer1";
        Text = "FormGlobalToneGammaVer1";
        groupBox1_GenParam.ResumeLayout(false);
        ((ISupportInitialize)chartSimulationGamma).EndInit();
        GammagroupBoxApi2.ResumeLayout(false);
        GammagroupBoxApi2.PerformLayout();
        GammagroupFileMenu.ResumeLayout(false);
        ((ISupportInitialize)chartGamma).EndInit();
        ResumeLayout(false);
    }
}
