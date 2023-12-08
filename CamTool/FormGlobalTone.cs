using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSCamIQTool.CamTool;

public class FormGlobalTone : Form
{
    public bool m_chartSelectR;

    public bool m_chartSelectRGB;

    public bool m_chartSelectR_C;

    public bool m_chartSelectRGB_C;

    private int unknowN;

    private int m_nFeatureIdxR;

    private int m_nFeatureIdxR_c;

    private readonly int m_nGammaLength = 32;

    private readonly short m_nGammaValueMax;

    private readonly List<Point> m_ptFeatureR = new();

    private readonly List<Point> m_ptFeatureRControl = new();

    private readonly string m_strCtrlPtYes = "Ctrl Pt: Yes";

    private readonly string m_strCtrlPtNo = "Ctrl Pt: No";

    private int diff_left_control_x;

    private int diff_left_control_y;

    private int diff_right_control_x;

    private int diff_right_control_y;

    private readonly int FindPoint_index = 0;

    private int FindPoint;

    private int DeletePoint;

    private int sectionvalue = 1;

    private readonly int GammaLength = 65535;

    private readonly int FCurveNumber = 32;

    private readonly int NumberOfGroup = 16;

    private static readonly int[] xnode_Auto = new int[512];

    private static readonly int[] ynode_Auto = new int[512];

    private static readonly int[] xnode_Manual = new int[32];

    private static readonly int[] ynode_Manual = new int[32];

    private readonly byte SftSize = 31;

    private readonly double[] GlobalToneSftValue = new double[496];

    private readonly double[] GlobalToneSftValue_Manual = new double[31];

    private static long[] GlobalToneSft_Auto = new long[1];

    private long[] GlobalToneSft = new long[31];

    private static long[] GlobalToneSft_Manual = new long[1];

    private readonly bool autoMode;

    private static GuiGroup gammaGroup;

    private static GuiGroup gammaGroup_temp;

    private static GuiItem enableItem;

    private static GuiItem opTypeItem;

    private int itemR;

    private double new_Y;

    public static long[] globaltoneinitialvalueR;

    public static long[] globaltoneinitialvalueR_xnode;

    private static readonly int[] RecordGlobalToneIndex_Auto = new int[512];

    public static long[] globaltoneinitialvalueR_Manual;

    public static long[] globaltoneinitialvalueR_Manual_xnode;

    private static readonly int[] RecordGlobalToneIndex_Manual = new int[32];

    private static Series seiresGamma;

    private readonly IContainer components = null;

    private Chart chartWDR;

    private GroupBox groupFileMenu;

    private Button btnLoad;

    private Button btnSave;

    private GroupBox groupBoxApi2;

    private Label labelOpType;

    private ComboBox comboBoxIndex;

    private ComboBox comboBoxOpType;

    private Label labelIndex;

    private CheckBox checkBoxEnable;

    private Label labelPosition;

    private Button SetGlobalToneSft;

    private GroupBox groupBox1;

    private Chart chartSimulatioWDR;

    public int Index { get; set; }

    public int Index_Old { get; set; }

    public int OpMode { get; set; }

    public int GammaEnable { get; set; }

    private void comboBoxIndex_SelectedIndexChanged(object sender, EventArgs e)
    {
        Index = comboBoxIndex.SelectedIndex;
        UpdateGammaValue(Index);
    }

    private void comboBoxOpType_SelectedIndexChanged(object sender, EventArgs e)
    {
        OpMode = comboBoxOpType.SelectedIndex;
        opTypeItem.DataValue[0] = OpMode;
        if (OpMode == 1)
        {
            comboBoxIndex.Enabled = false;
            itemR = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GlobalToneLut");
            UpdateGammaValue(0);
        }
        else
        {
            comboBoxIndex.Enabled = true;
            itemR = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneLut");
            UpdateGammaValue(Index);
        }
    }

    private void checkBoxEnable_CheckedChanged(object sender, EventArgs e)
    {
        GammaEnable = checkBoxEnable.Checked ? 1 : 0;
        enableItem.DataValue[0] = GammaEnable;
        chartWDR.Enabled = GammaEnable != 0;
    }

    public void UpdatePage()
    {
        int num = 32;
        int num2 = 16;
        checkBoxEnable.Checked = enableItem.DataValue[0] != 0L;
        comboBoxOpType.SelectedIndex = (int)opTypeItem.DataValue[0];
        if (OpMode == 0)
        {
            if (Index != Index_Old)
            {
                comboBoxIndex.SelectedIndex = Index_Old;
                comboBoxIndex_SelectedIndexChanged(new ComboBox(), new EventArgs());
            }
            else
            {
                comboBoxIndex_SelectedIndexChanged(new ComboBox(), new EventArgs());
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
            setRedGammaArray(globaltoneinitialvalueR, index);
        }
        else
        {
            setRedGammaArray(globaltoneinitialvalueR_Manual, index);
        }
    }

    public void SaveGammaValue_temp()
    {
        getRedGammaArray(out long[] pGammaR);
        if (OpMode == 0 && GammaEnable == 1)
        {
            pGammaR.CopyTo(globaltoneinitialvalueR, Index * pGammaR.Length);
        }
        else if (OpMode == 1 && GammaEnable == 1)
        {
            globaltoneinitialvalueR_Manual = pGammaR;
        }
    }

    public FormGlobalTone(GuiGroup group)
    {
        InitializeComponent();
        gammaGroup = group;
        gammaGroup_temp = group;
        short num = (short)(gammaGroup.ItemList[GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneLut")].MaxValue + 1);
        if (num > m_nGammaValueMax)
        {
            m_nGammaValueMax = num;
        }
        InitChart();
        autoMode = group.autoMode.Length > 1;
        OpMode = autoMode ? 0 : 1;
        if (OpMode == 0)
        {
            enableItem = gammaGroup.ItemList[GetItemFieldIdx("API_WDRCurveFull_ENABLE_GlobalTone")];
            opTypeItem = gammaGroup.ItemList[GetItemFieldIdx("API_WDRCurveFull_OP_TYPE_GlobalTone")];
            itemR = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneLut");
        }
        else
        {
            groupBoxApi2.Visible = false;
            itemR = GetItemFieldIdx("API_WDRCurveFull_ENABLE_GlobalTone");
        }
        InitGlobalToneSft();
        initialGammaValue();
        UpdatePage();
        checkBoxEnable_CheckedChanged(new CheckBox(), new EventArgs());
    }

    public FormGlobalTone()
    {
    }

    public void InitChart()
    {
        int nGammaValueMax = m_nGammaValueMax;
        int num = 32;
        GetGammaWithOB(m_nGammaValueMax, m_nGammaValueMax, 0m, out Point[] pGammaArray);
        chartWDR.ChartAreas[0].AxisX.Minimum = 0.0;
        chartWDR.ChartAreas[0].AxisX.Maximum = num - 1;
        chartWDR.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
        chartWDR.ChartAreas[0].AxisX.LabelStyle.Format = "D1";
        chartWDR.ChartAreas[0].AxisY.Minimum = 0.0;
        chartWDR.ChartAreas[0].AxisY.Maximum = nGammaValueMax + 256;
        chartWDR.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
        chartWDR.ChartAreas[0].AxisY.LabelStyle.Format = "D1";
        chartWDR.Legends[0].Enabled = false;
        chartSimulatioWDR.ChartAreas[0].AxisX.Minimum = 0.0;
        chartSimulatioWDR.ChartAreas[0].AxisX.Maximum = GammaLength + 1;
        chartSimulatioWDR.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
        chartSimulatioWDR.ChartAreas[0].AxisX.LabelStyle.Format = "D1";
        chartSimulatioWDR.ChartAreas[0].AxisY.Minimum = 0.0;
        chartSimulatioWDR.ChartAreas[0].AxisY.Maximum = nGammaValueMax + 256;
        chartSimulatioWDR.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
        chartSimulatioWDR.ChartAreas[0].AxisY.LabelStyle.Format = "D1";
        chartSimulatioWDR.Legends[0].Enabled = false;
        if (pGammaArray != null)
        {
            chartWDR.Series[0].Points.Clear();
            for (int i = 0; i < pGammaArray.Length; i++)
            {
                _ = chartWDR.Series["SeriesWDR"].Points.AddXY(i, pGammaArray[i].Y);
            }
            Series seiresPoints = chartWDR.Series["SeriesWDRFeature"];
            Series seiresLines = chartWDR.Series["SeriesWDR"];
            initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        }
        for (int j = 0; j < 512; j++)
        {
            _ = chartWDR.Series.Add("SeriesBezierControl" + j);
            chartWDR.Series["SeriesBezierControl" + j].ChartType = SeriesChartType.Spline;
            chartWDR.Series["SeriesBezierControl" + j].BorderWidth = 1;
            chartWDR.Series["SeriesBezierControl" + j].Color = Color.Gray;
        }
    }

    public void clearControlLine()
    {
        for (int i = 0; i < 512; i++)
        {
            chartWDR.Series["SeriesBezierControl" + i].Points.Clear();
        }
    }

    public void InitCtrolChart(double Pm1, double Pm2, double P0, double P3, double T1, double T2, ref double P1, ref double P2)
    {
        double num3 = 1.0 - T1;
        double num4 = 1.0 - T2;
        double num5 = (Pm1 - (P0 * num3 * num3 * num3) - (P3 * T1 * T1 * T1)) / (3.0 * T1 * num3);
        double num6 = (Pm2 - (P0 * num4 * num4 * num4) - (P3 * T2 * T2 * T2)) / (3.0 * num4 * T2);
        double num2 = ((num5 * num4) - (num6 * num3)) / ((T1 * num4) - (T2 * num3));
        double num = (num5 - (num2 * T1)) / num3;
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
                double num2 = (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) / 2;
                double num3 = (m_ptFeatureR[i + 1].Y - m_ptFeatureR[i].Y) / 2;
                double num = m_ptFeatureR[i].X + num2;
                double num4 = m_ptFeatureR[i].Y + num3;
                int num5 = (int)num;
                int num6 = (int)num4;
                m_ptFeatureRControl.Add(new Point(num5, num6));
                continue;
            }
            if (i == m_ptFeatureR.Count - 1)
            {
                double num8 = (m_ptFeatureR[i].X - m_ptFeatureR[i - 1].X) / 2;
                double num9 = (m_ptFeatureR[i].Y - m_ptFeatureR[i - 1].Y) / 2;
                double num7 = m_ptFeatureR[i].X - num8;
                double num10 = m_ptFeatureR[i].Y - num9;
                int num11 = (int)num7;
                int num12 = (int)num10;
                m_ptFeatureRControl.Add(new Point(num11, num12));
                continue;
            }

            double num13;
            double num14;
            double num15;
            double num16;
            double num17;
            double num18;
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
            float a = 0f;
            float b = 0f;
            int num23;
            int num24;
            int num25;
            int num26;
            int num27;
            int num28;
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
                    num27 = (int)((a * k) + b);
                    _ = chartWDR.Series["SeriesBezierControl" + j].Points.AddXY(k, num27);
                }
            }
            else
            {
                num28 = ((j - 1) / 2) + 1;
                num23 = m_ptFeatureRControl[j].X;
                num24 = m_ptFeatureR[num28].X;
                num25 = m_ptFeatureRControl[j].Y;
                num26 = m_ptFeatureR[num28].Y;
                LineFunction(num23, num24, num25, num26, ref a, ref b);
                for (int l = num23; l <= num24; l++)
                {
                    num27 = (int)((a * l) + b);
                    _ = chartWDR.Series["SeriesBezierControl" + j].Points.AddXY(l, num27);
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
                double num2 = (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) / 2;
                double num3 = (m_ptFeatureR[i + 1].Y - m_ptFeatureR[i].Y) / 2;
                double num = m_ptFeatureR[i].X + num2;
                double num4 = m_ptFeatureR[i].Y + num3;
                int num5 = (int)num;
                int num6 = (int)num4;
                m_ptFeatureRControl.Insert(0, new Point(num5, num6));
                continue;
            }
            if (i == m_ptFeatureR.Count - 1)
            {
                double num8 = (m_ptFeatureR[i].X - m_ptFeatureR[i - 1].X) / 2;
                double num9 = (m_ptFeatureR[i].Y - m_ptFeatureR[i - 1].Y) / 2;
                double num7 = m_ptFeatureR[i].X - num8;
                double num10 = m_ptFeatureR[i].Y - num9;
                int num11 = (int)num7;
                int num12 = (int)num10;
                m_ptFeatureRControl.Insert((2 * (m_ptFeatureR.Count - 1)) - 1, new Point(num11, num12));
                continue;
            }

            double num13;
            double num14;
            double num15;
            double num16;
            double num17;
            double num18;
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
            m_ptFeatureRControl.Insert((2 * i) - 1, new Point(num19, num21));
            m_ptFeatureRControl.Insert(2 * i, new Point(num20, num22));
        }
        clearControlLine();
        for (int j = 0; j < m_ptFeatureRControl.Count; j++)
        {
            float a = 0f;
            float b = 0f;
            int num23;
            int num24;
            int num25;
            int num26;
            int num27;
            int num28;
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
                    num27 = (int)((a * k) + b);
                    _ = chartWDR.Series["SeriesBezierControl" + j].Points.AddXY(k, num27);
                }
            }
            else
            {
                num28 = ((j - 1) / 2) + 1;
                num23 = m_ptFeatureRControl[j].X;
                num24 = m_ptFeatureR[num28].X;
                num25 = m_ptFeatureRControl[j].Y;
                num26 = m_ptFeatureR[num28].Y;
                LineFunction(num23, num24, num25, num26, ref a, ref b);
                for (int l = num23; l <= num24; l++)
                {
                    num27 = (int)((a * l) + b);
                    _ = chartWDR.Series["SeriesBezierControl" + j].Points.AddXY(l, num27);
                }
            }
        }
    }

    public void RemoveBezierControl(Series seiresRLine, Series seiresRPoints, List<Point> m_ptFeatureR, List<Point> m_ptFeatureRControl)
    {
        if (DeletePoint == 0)
        {
            m_ptFeatureRControl.RemoveAt(0);
        }
        else if (DeletePoint == m_ptFeatureR.Count)
        {
            m_ptFeatureRControl.RemoveAt((2 * m_ptFeatureR.Count) - 1);
        }
        else
        {
            m_ptFeatureRControl.RemoveAt((2 * (DeletePoint - 1)) + 1);
            m_ptFeatureRControl.RemoveAt((2 * (DeletePoint - 1)) + 1);
        }
        clearControlLine();
        for (int i = 0; i < m_ptFeatureRControl.Count; i++)
        {
            float a = 0f;
            float b = 0f;
            int num;
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            if (i % 2 == 0)
            {
                num6 = i / 2;
                num = m_ptFeatureR[num6].X;
                num2 = m_ptFeatureRControl[i].X;
                num3 = m_ptFeatureR[num6].Y;
                num4 = m_ptFeatureRControl[i].Y;
                LineFunction(num, num2, num3, num4, ref a, ref b);
                for (int j = num; j <= num2; j++)
                {
                    num5 = (int)((a * j) + b);
                    _ = chartWDR.Series["SeriesBezierControl" + i].Points.AddXY(j, num5);
                }
            }
            else
            {
                num6 = ((i - 1) / 2) + 1;
                num = m_ptFeatureRControl[i].X;
                num2 = m_ptFeatureR[num6].X;
                num3 = m_ptFeatureRControl[i].Y;
                num4 = m_ptFeatureR[num6].Y;
                LineFunction(num, num2, num3, num4, ref a, ref b);
                for (int k = num; k <= num2; k++)
                {
                    num5 = (int)((a * k) + b);
                    _ = chartWDR.Series["SeriesBezierControl" + i].Points.AddXY(k, num5);
                }
            }
        }
    }

    public void LineFunction(int x1, int x2, int y1, int y2, ref float a, ref float b)
    {
        a = (y1 - y2) / (float)(x1 - x2 + 1E-10);
        b = y1 - (a * x1);
    }

    public void drawControlLine(List<Point> m_ptFeature, List<Point> m_ptFeatureControl)
    {
        for (int i = 0; i < m_ptFeatureControl.Count; i++)
        {
            float a = 0f;
            float b = 0f;
            int num;
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
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
                    num5 = (int)((a * j) + b);
                    _ = chartWDR.Series["SeriesBezierControl" + i].Points.AddXY(j, num5);
                }
            }
            else
            {
                num6 = ((i - 1) / 2) + 1;
                num = m_ptFeatureControl[i].X;
                num2 = m_ptFeature[num6].X;
                num3 = m_ptFeatureControl[i].Y;
                num4 = m_ptFeature[num6].Y;
                LineFunction(num, num2, num3, num4, ref a, ref b);
                for (int k = num; k <= num2; k++)
                {
                    num5 = (int)((a * k) + b);
                    _ = chartWDR.Series["SeriesBezierControl" + i].Points.AddXY(k, num5);
                }
            }
        }
    }

    private void initialAddSection(ref Series seiresPoints, ref Series seiresLines, List<Point> pFeaturePoint)
    {
        int nGammaLength = m_nGammaLength;
        seiresPoints.Points.Clear();
        int num = 32;
        for (int i = 0; i < nGammaLength; i += nGammaLength / num)
        {
            pFeaturePoint.Add(new Point((int)seiresLines.Points[i].XValue, (int)seiresLines.Points[i].YValues[0]));
        }
        seiresPoints.Points.Clear();
        for (int j = 0; j < pFeaturePoint.Count; j++)
        {
            _ = seiresPoints.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
        }
    }

    private void RGBGamma_Load(object sender, EventArgs e)
    {
    }

    private void checkBoxSyncRGB_CheckedChanged(object sender, EventArgs e)
    {
        RGBsameLineBack();
        chartWDR.Series["SeriesWDR"].Color = Color.Red;
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
        if (nFeatureIdx < 1)
        {
            return;
        }
        double num2 = ptFeature[nFeatureIdx - 1].X;
        double num3 = ptFeature[nFeatureIdx].X;
        double num4 = ptFeature[nFeatureIdx - 1].Y;
        double num5 = ptFeature[nFeatureIdx].Y;
        for (int i = ptFeature[nFeatureIdx - 1].X; i <= ptFeature[nFeatureIdx].X; i++)
        {
            double num = (i - num2) / (num3 - num2);
            double num6 = (int)(((1.0 - num) * num4) + (num * num5));
            double num7 = (int)(((1.0 - num) * num2) + (num * num3));
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
        if (nFeatureIdx >= 1 && nFeatureIdx < ptFeature.Count - 1)
        {
            double num2 = ptFeature[nFeatureIdx - 1].X;
            double num3 = ptFeature[nFeatureIdx].X;
            double num4 = ptFeature[nFeatureIdx + 1].X;
            double num5 = ptFeature[nFeatureIdx - 1].Y;
            double num6 = ptFeature[nFeatureIdx].Y;
            double num7 = ptFeature[nFeatureIdx + 1].Y;
            for (int i = ptFeature[nFeatureIdx - 1].X; i <= ptFeature[nFeatureIdx + 1].X; i++)
            {
                double num = (i - num2) / (num4 - num2);
                double num8 = (int)(((1.0 - num) * (1.0 - num) * num5) + (2.0 * (1.0 - num) * num * num6) + (num * num * num7));
                double num9 = (int)(((1.0 - num) * (1.0 - num) * num2) + (2.0 * (1.0 - num) * num * num3) + (num * num * num4));
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
        for (int i = 0; i < num; i++)
        {
            if (x1 + i >= 32)
            {
                continue;
            }
            double num2 = i / (double)num;
            double num3 = (p_y0 * (1.0 - num2) * (1.0 - num2) * (1.0 - num2)) + (3.0 * p_y1 * num2 * (1.0 - num2) * (1.0 - num2)) + (3.0 * p_y2 * num2 * num2 * (1.0 - num2)) + (p_y3 * num2 * num2 * num2);
            double xValue = (p_x0 * (1.0 - num2) * (1.0 - num2) * (1.0 - num2)) + (3.0 * p_x1 * num2 * (1.0 - num2) * (1.0 - num2)) + (3.0 * p_x2 * num2 * num2 * (1.0 - num2)) + (p_x3 * num2 * num2 * num2);
            seiresCurve.Points[x1 + i].YValues[0] = x1 + i == 0
                ? num3 > seiresCurve.Points[x1 + i + 1].YValues[0] ? seiresCurve.Points[x1 + i + 1].YValues[0] : num3
                : num3 < seiresCurve.Points[x1 + i - 1].YValues[0]
                    ? seiresCurve.Points[x1 + i - 1].YValues[0]
                    : num3 > seiresCurve.Points[x1 + i + 1].YValues[0] ? seiresCurve.Points[x1 + i + 1].YValues[0] : num3;
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
        double num10 = (num2 - num) / (num3 - num);
        double num9 = (num5 - ((1.0 - num10) * (1.0 - num10) * num4) - (num10 * num10 * num6)) / (2.0 * (1.0 - num10) * num10);
        double num11 = (num2 - ((1.0 - num10) * (1.0 - num10) * num) - (num10 * num10 * num3)) / (2.0 * (1.0 - num10) * num10);
        num5 = num9;
        num2 = num11;
        if (nFeatureIdx >= 1 && nFeatureIdx < ptFeature.Count - 1)
        {
            for (int i = ptFeature[nFeatureIdx - 1].X; i < ptFeature[nFeatureIdx + 1].X; i++)
            {
                num10 = (i - num) / (num3 - num);
                double num7 = (int)(((1.0 - num10) * (1.0 - num10) * num4) + (2.0 * (1.0 - num10) * num10 * num5) + (num10 * num10 * num6));
                double num8 = (int)(((1.0 - num10) * (1.0 - num10) * num) + (2.0 * (1.0 - num10) * num10 * num2) + (num10 * num10 * num3));
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
        unknowN = num == 1 ? 5 : 3;
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
                (Array[num, k], Array[i, k]) = (Array[i, k], Array[num, k]);
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

    private void chartWDR_MouseDown(object sender, MouseEventArgs e)
    {
        if (RestoreView(sender, e) == 1)
        {
            return;
        }
        try
        {
            bool bFindPoint = false;
            bool bFindPoint_c = false;
            Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartWDR, e.X, e.Y);
            if (axisValuesFromMouse == null)
            {
                return;
            }
            Point ptSelect = new((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            Series pCurve = chartWDR.Series["SeriesWDR"];
            AddFeaturePoint(pCurve, m_ptFeatureR, m_ptFeatureRControl, ptSelect, ref m_nFeatureIdxR, ref m_nFeatureIdxR_c, ref bFindPoint, ref bFindPoint_c);
            if (bFindPoint)
            {
                m_chartSelectR = true;
                Series seiresRLine = chartWDR.Series["SeriesWDR"];
                Series seiresRPoints = chartWDR.Series["SeriesWDRFeature"];
                if (FindPoint == 1)
                {
                    AddBezierControl(seiresRLine, seiresRPoints, m_ptFeatureR, m_ptFeatureRControl);
                    FindPoint = 0;
                }
                if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
                {
                    diff_left_control_x = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1].X - m_ptFeatureR[m_nFeatureIdxR].X;
                    diff_left_control_y = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1].Y - m_ptFeatureR[m_nFeatureIdxR].Y;
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
                    diff_left_control_x = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1].X - m_ptFeatureR[m_nFeatureIdxR].X;
                    diff_left_control_y = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1].Y - m_ptFeatureR[m_nFeatureIdxR].Y;
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

    private void chartWDR_MouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartWDR, e.X, e.Y);
            if (!m_chartSelectR || axisValuesFromMouse == null)
            {
                return;
            }
            Series series = chartWDR.Series["SeriesWDR"];
            m_ptFeatureR[m_nFeatureIdxR] = m_nFeatureIdxR == 0
                ? axisValuesFromMouse.Item2 > m_ptFeatureR[m_nFeatureIdxR + 1].Y
                    ? new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y)
                    : new Point(m_ptFeatureR[m_nFeatureIdxR].X, (int)axisValuesFromMouse.Item2)
                : m_nFeatureIdxR == FCurveNumber - 1
                    ? axisValuesFromMouse.Item2 < m_ptFeatureR[m_nFeatureIdxR - 1].Y
                                    ? new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y)
                                    : new Point(m_ptFeatureR[m_nFeatureIdxR].X, (int)axisValuesFromMouse.Item2)
                    : axisValuesFromMouse.Item2 > m_ptFeatureR[m_nFeatureIdxR + 1].Y
                                    ? new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y)
                                    : axisValuesFromMouse.Item2 < m_ptFeatureR[m_nFeatureIdxR - 1].Y
                                                    ? new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y)
                                                    : new Point(m_ptFeatureR[m_nFeatureIdxR].X, (int)axisValuesFromMouse.Item2);
            if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
            {
                if (m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y < m_ptFeatureR[m_nFeatureIdxR - 1].Y)
                {
                    m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y);
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y);
                }
                else if (m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y > m_ptFeatureR[m_nFeatureIdxR + 1].Y)
                {
                    m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y);
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y);
                }
                else
                {
                    m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y);
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR].Y + diff_right_control_y);
                }
            }
            else if (m_nFeatureIdxR == 0)
            {
                m_ptFeatureRControl[m_nFeatureIdxR * 2] = m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y > m_ptFeatureR[m_nFeatureIdxR + 1].Y
                    ? new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR + 1].Y)
                    : new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y);
            }
            else if (m_nFeatureIdxR == m_ptFeatureR.Count - 1)
            {
                m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1] = m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y < m_ptFeatureR[m_nFeatureIdxR - 1].Y
                    ? new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR - 1].Y)
                    : new Point(m_ptFeatureR[m_nFeatureIdxR].X, m_ptFeatureR[m_nFeatureIdxR].Y + diff_left_control_y);
            }
            clearControlLine();
            _ = chartWDR.Series["SeriesWDR"];
            _ = chartWDR.Series["SeriesWDRFeature"];
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
                num2 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 2].Y;
                num3 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1].Y;
                num4 = m_ptFeatureR[m_nFeatureIdxR].Y;
                num5 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                num6 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 2].X;
                num7 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1].X;
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
                num13 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) + 1].Y;
                num14 = m_ptFeatureR[m_nFeatureIdxR + 1].Y;
                num15 = m_ptFeatureR[m_nFeatureIdxR].X;
                num16 = m_ptFeatureRControl[m_nFeatureIdxR * 2].X;
                num17 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) + 1].X;
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
                num23 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) + 1].Y;
                num24 = m_ptFeatureR[m_nFeatureIdxR + 1].Y;
                num25 = m_ptFeatureR[m_nFeatureIdxR].X;
                num26 = m_ptFeatureRControl[m_nFeatureIdxR * 2].X;
                num27 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) + 1].X;
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
                num32 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 2].Y;
                num33 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1].Y;
                num34 = m_ptFeatureR[m_nFeatureIdxR].Y;
                num35 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                num36 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 2].X;
                num37 = m_ptFeatureRControl[(m_nFeatureIdxR * 2) - 1].X;
                num38 = m_ptFeatureR[m_nFeatureIdxR].X;
                num39 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                num40 = m_ptFeatureR[m_nFeatureIdxR].X;
                cubicBezier(num31, num32, num33, num34, num35, num36, num37, num38, num39, num40, series);
                series.Points[m_nFeatureIdxR].YValues[0] = m_ptFeatureR[m_nFeatureIdxR].Y;
                series.Points[m_nFeatureIdxR].XValue = m_ptFeatureR[m_nFeatureIdxR].X;
            }
            chartWDR.Series["SeriesWDRFeature"].Points.Clear();
            for (int i = 0; i < m_ptFeatureR.Count; i++)
            {
                _ = chartWDR.Series["SeriesWDRFeature"].Points.AddXY(m_ptFeatureR[i].X, m_ptFeatureR[i].Y);
            }
        }
        catch
        {
            Console.WriteLine("Out of range!");
        }
    }

    private void chartWDR_MouseUp(object sender, MouseEventArgs e)
    {
        try
        {
            if (m_chartSelectR)
            {
                seiresGamma = chartWDR.Series["SeriesWDR"];
                Series seriesPoint = chartWDR.Series["SeriesWDRFeature"];
                increasingLineModify(m_nFeatureIdxR, seiresGamma, m_ptFeatureR, seriesPoint);
                clearControlLine();
            }
            else if (m_chartSelectRGB)
            {
                Series seiresCurve = chartWDR.Series["SeriesWDR"];
                Series seriesPoint2 = chartWDR.Series["SeriesWDRFeature"];
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
            chartWDR.Cursor = Cursors.Cross;
            SaveGammaValue_temp();
            RenewSimulation();
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
        labelPosition.Text = "x=" + (int)num + " y=" + (int)num2;
        return new Tuple<double, double>(num, num2);
    }

    public void Max(double[] array, ref double y)
    {
        double num = array[0];
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
        double num = array[0];
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
            _ = (num5 - ((1.0 - num7) * (1.0 - num7) * num4) - (num7 * num7 * num6)) / (2.0 * (1.0 - num7) * num7);
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
                _ = seriesPoint.Points.AddXY(ptFeature[l].X, ptFeature[l].Y);
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
                _ = seriesPoint.Points.AddXY(ptFeature[num16].X, ptFeature[num16].Y);
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
                _ = seriesPoint.Points.AddXY(ptFeature[num21].X, ptFeature[num21].Y);
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
                _ = seriesPoint.Points.AddXY(ptFeature[num29].X, ptFeature[num29].Y);
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
                _ = seriesPoint.Points.AddXY(ptFeature[num42].X, ptFeature[num42].Y);
            }
        }
    }

    private void RGBsameLineBack()
    {
    }

    public void SeperateSection(ref Series seiresPoints, ref Series seiresLines, List<Point> pFeaturePoint)
    {
        seiresPoints.Points.Clear();
        sectionvalue = 32;
        for (int i = 0; i < m_nGammaLength; i += m_nGammaLength / sectionvalue)
        {
            pFeaturePoint.Add(new Point((int)seiresLines.Points[i].XValue, (int)seiresLines.Points[i].YValues[0]));
        }
        pFeaturePoint.Add(new Point((int)seiresLines.Points[m_nGammaLength - 1].XValue, (int)seiresLines.Points[m_nGammaLength - 1].YValues[0]));
        for (int j = 0; j < pFeaturePoint.Count; j++)
        {
            _ = seiresPoints.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
        }
    }

    private void sectiontrackBar_Scroll(object sender, EventArgs e)
    {
        m_ptFeatureR.Clear();
        Series seiresPoints = chartWDR.Series["SeriesWDRFeature"];
        Series seiresLines = chartWDR.Series["SeriesWDR"];
        SeperateSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        Series seiresRLine = chartWDR.Series["SeriesWDR"];
        Series seiresRPoints = chartWDR.Series["SeriesWDRFeature"];
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
        (numbers[j], numbers[i]) = (numbers[i], numbers[j]);
    }

    public void getRedGammaArray(out long[] pGammaR)
    {
        pGammaR = new long[32];
        for (int i = 0; i < 32; i++)
        {
            pGammaR[i] = (int)chartWDR.Series["SeriesWDR"].Points[i].YValues[0];
        }
    }

    public void setRedGammaArray(long[] pGamma, int index)
    {
        chartWDR.Series["SeriesWDR"].Points.Clear();
        chartWDR.Series["SeriesWDRFeature"].Points.Clear();
        chartSimulatioWDR.Series["SimulationWDR"].Points.Clear();
        chartSimulatioWDR.Series["SimulationWDRFeature"].Points.Clear();
        m_ptFeatureR.Clear();
        int num = 0;
        for (int i = 32 * index; i < 32 * (index + 1); i++)
        {
            _ = chartWDR.Series["SeriesWDR"].Points.AddXY(num, pGamma[i]);
            num++;
            if (OpMode == 0)
            {
                _ = chartSimulatioWDR.Series["SimulationWDR"].Points.AddXY(xnode_Auto[i], pGamma[i]);
                _ = chartSimulatioWDR.Series["SimulationWDRFeature"].Points.AddXY(xnode_Auto[i], pGamma[i]);
            }
            else
            {
                _ = chartSimulatioWDR.Series["SimulationWDR"].Points.AddXY(xnode_Manual[i], pGamma[i]);
                _ = chartSimulatioWDR.Series["SimulationWDRFeature"].Points.AddXY(xnode_Manual[i], pGamma[i]);
            }
        }
        Series seiresPoints = chartWDR.Series["SeriesWDRFeature"];
        Series seiresLines = chartWDR.Series["SeriesWDR"];
        initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        InitBezierControl(seiresLines, seiresPoints, m_ptFeatureR, m_ptFeatureRControl);
    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
        int[] array = new int[65];
        OpenFileDialog openFileDialog = new()
        {
            Title = "Open the text file you wish",
            InitialDirectory = "D:",
            Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        chartWDR.Series["SeriesWDR"].Points.Clear();
        chartWDR.Series["SeriesWDRFeature"].Points.Clear();
        chartSimulatioWDR.Series["SimulationWDR"].Points.Clear();
        chartSimulatioWDR.Series["SimulationWDRFeature"].Points.Clear();
        m_ptFeatureR.Clear();
        StreamReader streamReader = File.OpenText(openFileDialog.FileName);
        _ = streamReader.ReadLine();
        int num = 0;
        int num2 = 0;
        while (!streamReader.EndOfStream)
        {
            string text = streamReader.ReadLine();
            if (text != "GlobalToneShift:1~31")
            {
                array[num] = Convert.ToInt16(text);
                num++;
            }
        }
        if (OpMode == 0)
        {
            for (int i = 0; i < 31; i++)
            {
                GlobalToneSft_Auto[(Index * SftSize) + i] = array[i + FCurveNumber];
            }
            for (int j = 0; j < FCurveNumber * NumberOfGroup; j++)
            {
                if (j % FCurveNumber != 0)
                {
                    GlobalToneSftValue[num2] = Math.Pow(2.0, GlobalToneSft_Auto[num2]);
                    xnode_Auto[j] = xnode_Auto[j - 1] + Convert.ToInt32(Math.Pow(2.0, GlobalToneSft_Auto[num2]));
                    num2++;
                }
                else
                {
                    xnode_Auto[j] = 0;
                }
            }
        }
        else
        {
            for (int k = 0; k < 31; k++)
            {
                GlobalToneSft_Manual[k] = array[k + FCurveNumber];
            }
            for (int l = 0; l < FCurveNumber; l++)
            {
                if (l % FCurveNumber != 0)
                {
                    GlobalToneSftValue_Manual[num2] = Math.Pow(2.0, GlobalToneSft_Manual[num2]);
                    xnode_Manual[l] = xnode_Manual[l - 1] + Convert.ToInt32(Math.Pow(2.0, GlobalToneSft_Manual[num2]));
                    num2++;
                }
                else
                {
                    xnode_Manual[l] = 0;
                }
            }
        }
        for (int m = 0; m < 32; m++)
        {
            if (OpMode == 0)
            {
                if (xnode_Auto[(Index * FCurveNumber) + m] > GammaLength)
                {
                    array[m] += array[m - 1];
                }
                m_ptFeatureR.Add(new Point(m, array[m]));
                _ = chartWDR.Series["SeriesWDR"].Points.AddXY(m, array[m]);
                _ = chartWDR.Series["SeriesWDRFeature"].Points.AddXY(m, array[m]);
                ynode_Auto[(Index * FCurveNumber) + m] = array[m];
                _ = chartSimulatioWDR.Series["SimulationWDR"].Points.AddXY(xnode_Auto[(Index * FCurveNumber) + m], array[m]);
                _ = chartSimulatioWDR.Series["SimulationWDRFeature"].Points.AddXY(xnode_Auto[(Index * FCurveNumber) + m], array[m]);
            }
            else
            {
                if (xnode_Manual[m] > GammaLength)
                {
                    array[m] += array[m - 1];
                }
                m_ptFeatureR.Add(new Point(m, array[m]));
                _ = chartWDR.Series["SeriesWDR"].Points.AddXY(m, array[m]);
                _ = chartWDR.Series["SeriesWDRFeature"].Points.AddXY(m, array[m]);
                ynode_Manual[m] = array[m];
                _ = chartSimulatioWDR.Series["SimulationWDR"].Points.AddXY(xnode_Manual[m], array[m]);
                _ = chartSimulatioWDR.Series["SimulationWDRFeature"].Points.AddXY(xnode_Manual[m], array[m]);
            }
        }
        SaveGammaValue_temp();
        streamReader.Close();
    }

    public void UpdateLineControl()
    {
        m_ptFeatureR.Clear();
        Series seiresPoints = chartWDR.Series["SeriesWDRFeature"];
        Series seiresLines = chartWDR.Series["SeriesWDR"];
        SeperateSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        Series seiresRLine = chartWDR.Series["SeriesWDR"];
        Series seiresRPoints = chartWDR.Series["SeriesWDRFeature"];
        InitBezierControl(seiresRLine, seiresRPoints, m_ptFeatureR, m_ptFeatureRControl);
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        SaveFileDialog saveFileDialog = new()
        {
            Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
            FilterIndex = 1,
            RestoreDirectory = true
        };
        Stream stream;
        if (saveFileDialog.ShowDialog() != DialogResult.OK || (stream = saveFileDialog.OpenFile()) == null)
        {
            return;
        }
        using (StreamWriter streamWriter = new(stream))
        {
            streamWriter.WriteLine("GlobalTone:1~32");
            for (int i = 0; i < 32; i++)
            {
                if (OpMode == 0)
                {
                    if (xnode_Auto[(Index * FCurveNumber) + i] > GammaLength)
                    {
                        streamWriter.WriteLine((int)(chartWDR.Series["SeriesWDR"].Points[i].YValues[0] - chartWDR.Series["SeriesWDR"].Points[i - 1].YValues[0]));
                    }
                    else
                    {
                        streamWriter.WriteLine((int)chartWDR.Series["SeriesWDR"].Points[i].YValues[0]);
                    }
                }
                else if (xnode_Manual[i] > GammaLength)
                {
                    streamWriter.WriteLine((int)(chartWDR.Series["SeriesWDR"].Points[i].YValues[0] - chartWDR.Series["SeriesWDR"].Points[i - 1].YValues[0]));
                }
                else
                {
                    streamWriter.WriteLine((int)chartWDR.Series["SeriesWDR"].Points[i].YValues[0]);
                }
            }
            streamWriter.WriteLine("GlobalToneShift:1~31");
            if (OpMode == 0)
            {
                for (int j = 0; j < 31; j++)
                {
                    streamWriter.WriteLine(GlobalToneSft_Auto[(Index * SftSize) + j]);
                }
            }
            else
            {
                for (int k = 0; k < 31; k++)
                {
                    streamWriter.WriteLine(GlobalToneSft_Manual[k]);
                }
            }
        }
        stream.Close();
        _ = MessageBox.Show("Save GlobalTone Data OK");
    }

    private void CheckControlPoint(List<Point> pFeaturePoint, int ptX, Label labelCtrlPt)
    {
        int nFeatureIdx = -1;
        IsFeaturePoint(pFeaturePoint, ptX, ref nFeatureIdx);
        labelCtrlPt.Text = nFeatureIdx >= 0 ? m_strCtrlPtYes : m_strCtrlPtNo;
    }

    private void CheckControlPoint_y(List<Point> pFeaturePoint, int ptY, Label labelCtrlPt)
    {
        int nFeatureIdx = -1;
        IsFeaturePoint_y(pFeaturePoint, ptY, ref nFeatureIdx);
        labelCtrlPt.Text = nFeatureIdx >= 0 ? m_strCtrlPtYes : m_strCtrlPtNo;
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
                chartWDR.Cursor = Cursors.NoMoveVert;
                break;
            }
        }
        for (int j = 0; j < pFeaturePoint.Count; j++)
        {
            if (pFeaturePoint[j].X > ptSelect.X - num && pFeaturePoint[j].X < ptSelect.X + num && pFeaturePoint[j].Y > ptSelect.Y - num2 && pFeaturePoint[j].Y < ptSelect.Y + num2)
            {
                bFindPoint = true;
                nFeatureIdx = j;
                chartWDR.Cursor = Cursors.NoMove2D;
                break;
            }
        }
    }

    private void RemoveFeaturePoint(Series pCurve, Series SeriesWDRFeature, List<Point> pFeaturePoint, Point ptSelect, ref bool bFindPoint)
    {
        int num = 5;
        bFindPoint = false;
        if (pFeaturePoint.Count <= 2)
        {
            return;
        }
        for (int i = 0; i < pFeaturePoint.Count; i++)
        {
            if (pFeaturePoint[i].X > ptSelect.X - num && pFeaturePoint[i].X < ptSelect.X + num && pFeaturePoint[i].Y > ptSelect.Y - num && pFeaturePoint[i].Y < ptSelect.Y + num)
            {
                int num2 = i;
                bFindPoint = true;
                pFeaturePoint.RemoveAt(i);
                DeletePoint = i;
                double choosepointfinalY = 0.0;
                BezierCurveFirstModify(num2, pCurve, pFeaturePoint, ref choosepointfinalY);
                SeriesWDRFeature.Points.Clear();
                for (int j = 0; j < pFeaturePoint.Count; j++)
                {
                    _ = SeriesWDRFeature.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
                }
                break;
            }
        }
    }

    private void ResetCurve(string strGammaName)
    {
        GetGammaWithOB(1024m, 1024m, 0m, out Point[] pGammaArray);
        if (pGammaArray != null)
        {
            chartWDR.Series[strGammaName].Points.Clear();
            int num;
            for (num = 0; num < pGammaArray.Length; num++)
            {
                _ = chartWDR.Series[strGammaName].Points.AddXY(num, pGammaArray[num].Y);
            }
            List<Point> ptFeatureR = m_ptFeatureR;
            for (int i = 0; i < ptFeatureR.Count; i++)
            {
                ptFeatureR[i] = new Point(ptFeatureR[i].X, (int)chartWDR.Series[strGammaName].Points[ptFeatureR[i].X].YValues[0]);
            }
            string name = strGammaName.Replace("Gamma", "Feature");
            Series series = chartWDR.Series[name];
            series.Points.Clear();
            for (int j = 0; j < ptFeatureR.Count; j++)
            {
                _ = series.Points.AddXY(ptFeatureR[j].X, ptFeatureR[j].Y);
            }
        }
    }

    private void initialGammavalue(Series seiresCurve, int index, Series seiresPoint, long[] gammavalue, List<Point> ptFeature, List<Point> ptFeatureControl)
    {
        seiresCurve.Points.Clear();
        seiresPoint.Points.Clear();
        ptFeature.Clear();
        int num = 0;
        for (int i = 32 * index; i < 32 * (index + 1); i++)
        {
            _ = seiresCurve.Points.AddXY(num, gammavalue[i]);
            num++;
        }
        initialAddSection(ref seiresPoint, ref seiresCurve, ptFeature);
        InitBezierControl(seiresCurve, seiresPoint, ptFeature, ptFeatureControl);
    }

    private void btnResetRCurve_Click(object sender, EventArgs e)
    {
        try
        {
            OpMode = comboBoxOpType.SelectedIndex;
            opTypeItem.DataValue[0] = OpMode;
            if (OpMode == 1)
            {
                comboBoxIndex.Enabled = false;
                itemR = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GlobalToneLut");
                int index = 0;
                Series seiresCurve = chartWDR.Series["SeriesWDR"];
                Series seiresPoint = chartWDR.Series["SeriesWDRFeature"];
                initialGammavalue(seiresCurve, index, seiresPoint, globaltoneinitialvalueR_Manual, m_ptFeatureR, m_ptFeatureRControl);
            }
            else
            {
                comboBoxIndex.Enabled = true;
                itemR = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneLut");
                int num = comboBoxIndex.SelectedIndex;
                if (num < 0)
                {
                    num = 0;
                }
                Series seiresCurve2 = chartWDR.Series["SeriesWDR"];
                Series seiresPoint2 = chartWDR.Series["SeriesWDRFeature"];
                initialGammavalue(seiresCurve2, num, seiresPoint2, globaltoneinitialvalueR, m_ptFeatureR, m_ptFeatureRControl);
            }
            SaveGammaValue_temp();
        }
        catch
        {
            ResetCurve("SeriesWDR");
            Series seiresRLine = chartWDR.Series["SeriesWDR"];
            Series seiresRPoints = chartWDR.Series["SeriesWDRFeature"];
            InitBezierControl(seiresRLine, seiresRPoints, m_ptFeatureR, m_ptFeatureRControl);
        }
    }

    private void initialGammaValue()
    {
        _ = gammaGroup.ItemList[itemR].DataValue.Length;
        int num = 0;
        globaltoneinitialvalueR = new long[32 * NumberOfGroup];
        for (int i = 0; i < FCurveNumber * NumberOfGroup; i++)
        {
            if (i % FCurveNumber != 0)
            {
                GlobalToneSftValue[num] = Math.Pow(2.0, GlobalToneSft_Auto[num]);
                xnode_Auto[i] = xnode_Auto[i - 1] + Convert.ToInt32(Math.Pow(2.0, GlobalToneSft_Auto[num]));
                num++;
            }
            else
            {
                xnode_Auto[i] = 0;
            }
        }
        for (int j = 0; j < FCurveNumber * NumberOfGroup; j++)
        {
            ynode_Auto[j] = Convert.ToInt32(gammaGroup.ItemList[itemR].DataValue[j]);
        }
        for (int k = 0; k < NumberOfGroup; k++)
        {
            for (int l = 0; l < FCurveNumber; l++)
            {
                globaltoneinitialvalueR[l + (k * FCurveNumber)] = xnode_Auto[l + (k * FCurveNumber)] > GammaLength
                    ? ynode_Auto[l + (k * FCurveNumber)] + ynode_Auto[l + (k * FCurveNumber) - 1]
                    : ynode_Auto[l + (k * FCurveNumber)];
            }
        }
        itemR = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GlobalToneLut");
        num = 0;
        globaltoneinitialvalueR_Manual = new long[32];
        for (int m = 0; m < FCurveNumber; m++)
        {
            if (m % FCurveNumber != 0)
            {
                GlobalToneSftValue_Manual[num] = Math.Pow(2.0, GlobalToneSft_Manual[num]);
                xnode_Manual[m] = xnode_Manual[m - 1] + Convert.ToInt32(Math.Pow(2.0, GlobalToneSft_Manual[num]));
                num++;
            }
            else
            {
                xnode_Manual[m] = 0;
            }
        }
        for (int n = 0; n < FCurveNumber; n++)
        {
            ynode_Manual[n] = Convert.ToInt32(gammaGroup.ItemList[itemR].DataValue[n]);
        }
        for (int num2 = 0; num2 < FCurveNumber; num2++)
        {
            globaltoneinitialvalueR_Manual[num2] = ynode_Manual[num2];
            globaltoneinitialvalueR_Manual[num2] = xnode_Manual[num2] > GammaLength ? ynode_Manual[num2] + ynode_Manual[num2 - 1] : ynode_Manual[num2];
        }
    }

    private void SetGlobalToneSft_Click(object sender, EventArgs e)
    {
        SetDeltaFormat();
    }

    private void SetDeltaFormat()
    {
        FormGlobalToneSft formGlobalToneSft = new();
        if (OpMode == 1)
        {
            for (int i = 0; i < SftSize; i++)
            {
                GlobalToneSft[i] = GlobalToneSft_Manual[i];
            }
        }
        else
        {
            for (int j = 0; j < SftSize; j++)
            {
                GlobalToneSft[j] = GlobalToneSft_Auto[(Index * SftSize) + j];
            }
        }
        formGlobalToneSft.SftSize_Changed = SftSize;
        formGlobalToneSft.GlobalToneSft_Changed = GlobalToneSft;
        _ = formGlobalToneSft.ShowDialog();
        if (formGlobalToneSft.DialogResult == DialogResult.OK)
        {
            GlobalToneSft = formGlobalToneSft.GlobalToneSft_Changed;
            formGlobalToneSft.Dispose();
            if (OpMode == 1)
            {
                for (int k = 0; k < SftSize; k++)
                {
                    GlobalToneSft_Manual[k] = GlobalToneSft[k];
                }
            }
            else
            {
                for (int l = 0; l < SftSize; l++)
                {
                    GlobalToneSft_Auto[(Index * SftSize) + l] = GlobalToneSft[l];
                }
            }
            RenewSimulation();
        }
        else
        {
            formGlobalToneSft.Dispose();
        }
    }

    private void InitGlobalToneSft()
    {
        int itemFieldIdx = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneSft");
        int itemFieldIdx2 = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GlobalToneSft");
        int num = gammaGroup.ItemList[itemFieldIdx].DataValue.Length;
        int num2 = gammaGroup.ItemList[itemFieldIdx2].DataValue.Length;
        Array.Resize(ref GlobalToneSft_Auto, num);
        for (int i = 0; i < num; i++)
        {
            GlobalToneSft_Auto[i] = gammaGroup.ItemList[itemFieldIdx].DataValue[i];
        }
        Array.Resize(ref GlobalToneSft_Manual, num2);
        for (int j = 0; j < num2; j++)
        {
            GlobalToneSft_Manual[j] = gammaGroup.ItemList[itemFieldIdx2].DataValue[j];
        }
    }

    public void SaveGlobalToneValue()
    {
        int itemFieldIdx = GetItemFieldIdx("API_WDRCurveFull_ENABLE_GlobalTone");
        int itemFieldIdx2 = GetItemFieldIdx("API_WDRCurveFull_OP_TYPE_GlobalTone");
        int itemFieldIdx3 = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneSft");
        int itemFieldIdx4 = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneLut");
        int itemFieldIdx5 = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GlobalToneSft");
        int itemFieldIdx6 = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GlobalToneLut");
        TakePoint();
        gammaGroup.ItemList[itemFieldIdx] = enableItem;
        gammaGroup.ItemList[itemFieldIdx2] = opTypeItem;
        for (int i = 0; i < gammaGroup.ItemList[itemFieldIdx3].DataValue.Length; i++)
        {
            gammaGroup.ItemList[itemFieldIdx3].DataValue[i] = GlobalToneSft_Auto[i];
        }
        for (int j = 0; j < gammaGroup.ItemList[itemFieldIdx4].DataValue.Length; j++)
        {
            gammaGroup.ItemList[itemFieldIdx4].DataValue[j] = ynode_Auto[j];
        }
        for (int k = 0; k < gammaGroup.ItemList[itemFieldIdx5].DataValue.Length; k++)
        {
            gammaGroup.ItemList[itemFieldIdx5].DataValue[k] = GlobalToneSft_Manual[k];
        }
        for (int l = 0; l < gammaGroup.ItemList[itemFieldIdx6].DataValue.Length; l++)
        {
            gammaGroup.ItemList[itemFieldIdx6].DataValue[l] = ynode_Manual[l];
        }
    }

    public void ReadPage()
    {
        int itemFieldIdx = GetItemFieldIdx("API_WDRCurveFull_ENABLE_GlobalTone");
        int itemFieldIdx2 = GetItemFieldIdx("API_WDRCurveFull_OP_TYPE_GlobalTone");
        int itemFieldIdx3 = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneSft");
        int itemFieldIdx4 = GetItemFieldIdx("API_WDRCurveFull_AUTO_GlobalToneLut");
        int itemFieldIdx5 = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GlobalToneSft");
        int itemFieldIdx6 = GetItemFieldIdx("API_WDRCurveFull_MANUAL_GlobalToneLut");
        int num = 0;
        enableItem = gammaGroup.ItemList[itemFieldIdx];
        opTypeItem = gammaGroup.ItemList[itemFieldIdx2];
        for (int i = 0; i < gammaGroup.ItemList[itemFieldIdx3].DataValue.Length; i++)
        {
            GlobalToneSft_Auto[i] = gammaGroup.ItemList[itemFieldIdx3].DataValue[i];
        }
        for (int j = 0; j < FCurveNumber * NumberOfGroup; j++)
        {
            if (j % FCurveNumber != 0)
            {
                GlobalToneSftValue[num] = Math.Pow(2.0, GlobalToneSft_Auto[num]);
                xnode_Auto[j] = xnode_Auto[j - 1] + Convert.ToInt32(Math.Pow(2.0, GlobalToneSft_Auto[num]));
                num++;
            }
            else
            {
                xnode_Auto[j] = 0;
            }
        }
        num = 0;
        for (int k = 0; k < gammaGroup.ItemList[itemFieldIdx5].DataValue.Length; k++)
        {
            GlobalToneSft_Manual[k] = gammaGroup.ItemList[itemFieldIdx5].DataValue[k];
        }
        for (int l = 0; l < FCurveNumber; l++)
        {
            if (l % FCurveNumber != 0)
            {
                GlobalToneSftValue_Manual[num] = Math.Pow(2.0, GlobalToneSft_Manual[num]);
                xnode_Manual[l] = xnode_Manual[l - 1] + Convert.ToInt32(Math.Pow(2.0, GlobalToneSft_Manual[num]));
                num++;
            }
            else
            {
                xnode_Manual[l] = 0;
            }
        }
        for (int m = 0; m < gammaGroup.ItemList[itemFieldIdx4].DataValue.Length; m++)
        {
            ynode_Auto[m] = Convert.ToInt32(gammaGroup.ItemList[itemFieldIdx4].DataValue[m]);
            if (xnode_Auto[m] > GammaLength && ynode_Auto[m] < ynode_Auto[m - 1])
            {
                ynode_Auto[m] += ynode_Auto[m - 1];
            }
            globaltoneinitialvalueR[m] = ynode_Auto[m];
        }
        for (int n = 0; n < gammaGroup.ItemList[itemFieldIdx6].DataValue.Length; n++)
        {
            ynode_Manual[n] = Convert.ToInt32(gammaGroup.ItemList[itemFieldIdx6].DataValue[n]);
            if (xnode_Manual[n] > GammaLength && ynode_Manual[n] < ynode_Manual[n - 1])
            {
                ynode_Manual[n] += ynode_Manual[n - 1];
            }
            globaltoneinitialvalueR_Manual[n] = ynode_Manual[n];
        }
        chartWDR.Series["SeriesWDR"].Points.Clear();
        chartWDR.Series["SeriesWDRFeature"].Points.Clear();
        chartSimulatioWDR.Series["SimulationWDR"].Points.Clear();
        chartSimulatioWDR.Series["SimulationWDRFeature"].Points.Clear();
        m_ptFeatureR.Clear();
        if (OpMode == 1)
        {
            for (int num2 = 0; num2 < 32; num2++)
            {
                m_ptFeatureR.Add(new Point(num2, ynode_Manual[num2]));
                _ = chartWDR.Series["SeriesWDR"].Points.AddXY(num2, ynode_Manual[num2]);
                _ = chartWDR.Series["SeriesWDRFeature"].Points.AddXY(num2, ynode_Manual[num2]);
                _ = chartSimulatioWDR.Series["SimulationWDR"].Points.AddXY(xnode_Manual[num2], ynode_Manual[num2]);
                _ = chartSimulatioWDR.Series["SimulationWDRFeature"].Points.AddXY(xnode_Manual[num2], ynode_Manual[num2]);
            }
        }
        else
        {
            for (int num3 = 0; num3 < 32; num3++)
            {
                m_ptFeatureR.Add(new Point(num3, ynode_Auto[(Index * FCurveNumber) + num3]));
                _ = chartWDR.Series["SeriesWDR"].Points.AddXY(num3, ynode_Auto[(Index * FCurveNumber) + num3]);
                _ = chartWDR.Series["SeriesWDRFeature"].Points.AddXY(num3, ynode_Auto[(Index * FCurveNumber) + num3]);
                _ = chartSimulatioWDR.Series["SimulationWDR"].Points.AddXY(xnode_Auto[(Index * FCurveNumber) + num3], ynode_Auto[(Index * FCurveNumber) + num3]);
                _ = chartSimulatioWDR.Series["SimulationWDRFeature"].Points.AddXY(xnode_Auto[(Index * FCurveNumber) + num3], ynode_Auto[(Index * FCurveNumber) + num3]);
            }
        }
    }

    private void TakePoint()
    {
        for (int i = 0; i < NumberOfGroup; i++)
        {
            for (int j = 0; j < FCurveNumber; j++)
            {
                ynode_Auto[j + (i * FCurveNumber)] = Convert.ToInt32(globaltoneinitialvalueR[j + (i * FCurveNumber)]);
                if (xnode_Auto[j + (i * FCurveNumber)] > GammaLength)
                {
                    ynode_Auto[j + (i * FCurveNumber)] = ynode_Auto[j + (i * FCurveNumber)] - ynode_Auto[j + (i * FCurveNumber) - 1];
                }
            }
        }
        for (int k = 0; k < FCurveNumber; k++)
        {
            ynode_Manual[k] = Convert.ToInt32(globaltoneinitialvalueR_Manual[k]);
            if (xnode_Manual[k] > GammaLength)
            {
                ynode_Manual[k] -= ynode_Manual[k - 1];
            }
        }
    }

    private void RenewSimulation()
    {
        int num = 0;
        for (int i = 0; i < FCurveNumber * NumberOfGroup; i++)
        {
            if (i % FCurveNumber != 0)
            {
                GlobalToneSftValue[num] = Math.Pow(2.0, GlobalToneSft_Auto[num]);
                xnode_Auto[i] = xnode_Auto[i - 1] + Convert.ToInt32(Math.Pow(2.0, GlobalToneSft_Auto[num]));
                num++;
            }
            else
            {
                xnode_Auto[i] = 0;
            }
        }
        num = 0;
        for (int j = 0; j < FCurveNumber; j++)
        {
            if (j % FCurveNumber != 0)
            {
                GlobalToneSftValue_Manual[num] = Math.Pow(2.0, GlobalToneSft_Manual[num]);
                xnode_Manual[j] = xnode_Manual[j - 1] + Convert.ToInt32(Math.Pow(2.0, GlobalToneSft_Manual[num]));
                num++;
            }
            else
            {
                xnode_Manual[j] = 0;
            }
        }
        for (int k = 0; k < NumberOfGroup; k++)
        {
            for (int l = 0; l < FCurveNumber; l++)
            {
                ynode_Auto[l + (k * FCurveNumber)] = Convert.ToInt32(globaltoneinitialvalueR[l + (k * FCurveNumber)]);
            }
        }
        for (int m = 0; m < FCurveNumber; m++)
        {
            ynode_Manual[m] = Convert.ToInt32(globaltoneinitialvalueR_Manual[m]);
        }
        chartSimulatioWDR.Series["SimulationWDR"].Points.Clear();
        chartSimulatioWDR.Series["SimulationWDRFeature"].Points.Clear();
        if (OpMode == 1)
        {
            for (int n = 0; n < 32; n++)
            {
                _ = chartSimulatioWDR.Series["SimulationWDR"].Points.AddXY(xnode_Manual[n], ynode_Manual[n]);
                _ = chartSimulatioWDR.Series["SimulationWDRFeature"].Points.AddXY(xnode_Manual[n], ynode_Manual[n]);
            }
        }
        else
        {
            for (int num2 = 0; num2 < 32; num2++)
            {
                _ = chartSimulatioWDR.Series["SimulationWDR"].Points.AddXY(xnode_Auto[(Index * FCurveNumber) + num2], ynode_Auto[(Index * FCurveNumber) + num2]);
                _ = chartSimulatioWDR.Series["SimulationWDRFeature"].Points.AddXY(xnode_Auto[(Index * FCurveNumber) + num2], ynode_Auto[(Index * FCurveNumber) + num2]);
            }
        }
    }

    internal void SetWinFormFocus(object sender, EventArgs e)
    {
        _ = comboBoxOpType.Focus();
        _ = btnLoad.Focus();
    }

    private void SetChartScaleXY(bool enable)
    {
        chartWDR.ChartAreas[0].CursorX.IsUserSelectionEnabled = enable;
        chartWDR.ChartAreas[0].CursorY.IsUserSelectionEnabled = enable;
        chartSimulatioWDR.ChartAreas[0].CursorX.IsUserSelectionEnabled = enable;
        chartSimulatioWDR.ChartAreas[0].CursorY.IsUserSelectionEnabled = enable;
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

    private void chartSimulatioWDR_MouseDown(object sender, MouseEventArgs e)
    {
        _ = RestoreView(sender, e);
        _ = 1;
    }

    private void chartWDR_SelectionRangeChanging(object sender, CursorEventArgs e)
    {
        e.NewSelectionStart = (int)e.NewSelectionStart;
        e.NewSelectionEnd = (int)e.NewSelectionEnd;
    }

    private void chartSimulatioWDR_SelectionRangeChanging(object sender, CursorEventArgs e)
    {
        e.NewSelectionStart = (int)e.NewSelectionStart;
        e.NewSelectionEnd = (int)e.NewSelectionEnd;
    }

    private void FormGlobalTone_MouseEnter(object sender, EventArgs e)
    {
        if (!btnLoad.Focused)
        {
            _ = btnLoad.Focus();
        }
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
        ChartArea chartArea = new();
        Legend legend = new();
        Series series = new();
        Series series2 = new();
        ChartArea chartArea2 = new();
        Legend legend2 = new();
        Series series3 = new();
        Series series4 = new();
        chartWDR = new Chart();
        groupFileMenu = new GroupBox();
        btnLoad = new Button();
        btnSave = new Button();
        groupBoxApi2 = new GroupBox();
        labelOpType = new Label();
        comboBoxIndex = new ComboBox();
        comboBoxOpType = new ComboBox();
        labelIndex = new Label();
        checkBoxEnable = new CheckBox();
        labelPosition = new Label();
        SetGlobalToneSft = new Button();
        groupBox1 = new GroupBox();
        chartSimulatioWDR = new Chart();
        ((ISupportInitialize)chartWDR).BeginInit();
        groupFileMenu.SuspendLayout();
        groupBoxApi2.SuspendLayout();
        groupBox1.SuspendLayout();
        ((ISupportInitialize)chartSimulatioWDR).BeginInit();
        SuspendLayout();
        chartArea.Name = "ChartArea1";
        chartWDR.ChartAreas.Add(chartArea);
        chartWDR.Cursor = Cursors.Cross;
        legend.Name = "Legend1";
        chartWDR.Legends.Add(legend);
        chartWDR.Location = new Point(12, 12);
        chartWDR.Name = "chartWDR";
        series.BorderWidth = 2;
        series.ChartArea = "ChartArea1";
        series.ChartType = SeriesChartType.Line;
        series.Color = Color.Red;
        series.Legend = "Legend1";
        series.MarkerSize = 7;
        series.Name = "SeriesWDR";
        series2.BorderColor = Color.FromArgb(0, 192, 0);
        series2.BorderWidth = 2;
        series2.ChartArea = "ChartArea1";
        series2.ChartType = SeriesChartType.Point;
        series2.Color = Color.Fuchsia;
        series2.Legend = "Legend1";
        series2.MarkerColor = Color.Green;
        series2.MarkerSize = 7;
        series2.MarkerStyle = MarkerStyle.Circle;
        series2.Name = "SeriesWDRFeature";
        series2.YValuesPerPoint = 2;
        chartWDR.Series.Add(series);
        chartWDR.Series.Add(series2);
        chartWDR.Size = new Size(520, 347);
        chartWDR.TabIndex = 142;
        chartWDR.Text = "chart1";
        chartWDR.SelectionRangeChanging += new EventHandler<CursorEventArgs>(chartWDR_SelectionRangeChanging);
        chartWDR.MouseDown += new MouseEventHandler(chartWDR_MouseDown);
        chartWDR.MouseMove += new MouseEventHandler(chartWDR_MouseMove);
        chartWDR.MouseUp += new MouseEventHandler(chartWDR_MouseUp);
        groupFileMenu.Controls.Add(btnLoad);
        groupFileMenu.Controls.Add(btnSave);
        groupFileMenu.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupFileMenu.Location = new Point(614, 12);
        groupFileMenu.Name = "groupFileMenu";
        groupFileMenu.Size = new Size(187, 131);
        groupFileMenu.TabIndex = 156;
        groupFileMenu.TabStop = false;
        groupFileMenu.Text = "File";
        btnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnLoad.BackColor = Color.LightCyan;
        btnLoad.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnLoad.Location = new Point(49, 40);
        btnLoad.Name = "btnLoad";
        btnLoad.Size = new Size(90, 23);
        btnLoad.TabIndex = 132;
        btnLoad.Text = "Load";
        btnLoad.UseVisualStyleBackColor = false;
        btnLoad.Click += new EventHandler(btnLoad_Click);
        btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnSave.BackColor = Color.LightCyan;
        btnSave.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnSave.Location = new Point(49, 88);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(90, 23);
        btnSave.TabIndex = 133;
        btnSave.Text = "Save";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += new EventHandler(btnSave_Click);
        groupBoxApi2.Controls.Add(labelOpType);
        groupBoxApi2.Controls.Add(comboBoxIndex);
        groupBoxApi2.Controls.Add(comboBoxOpType);
        groupBoxApi2.Controls.Add(labelIndex);
        groupBoxApi2.Controls.Add(checkBoxEnable);
        groupBoxApi2.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupBoxApi2.Location = new Point(569, 149);
        groupBoxApi2.Name = "groupBoxApi2";
        groupBoxApi2.Size = new Size(232, 141);
        groupBoxApi2.TabIndex = 159;
        groupBoxApi2.TabStop = false;
        groupBoxApi2.Text = "Option";
        labelOpType.AutoSize = true;
        labelOpType.Location = new Point(95, 40);
        labelOpType.Name = "labelOpType";
        labelOpType.Size = new Size(55, 15);
        labelOpType.TabIndex = 138;
        labelOpType.Text = "OpType :";
        comboBoxIndex.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxIndex.FormattingEnabled = true;
        comboBoxIndex.Items.AddRange(new object[16]
        {
            "index 0", "index 1", "index 2", "index 3", "index 4", "index 5", "index 6", "index 7", "index 8", "index 9",
            "index 10", "index 11", "index 12", "index 13", "index 14", "index 15"
        });
        comboBoxIndex.Location = new Point(102, 88);
        comboBoxIndex.Name = "comboBoxIndex";
        comboBoxIndex.Size = new Size(121, 23);
        comboBoxIndex.TabIndex = 134;
        comboBoxIndex.SelectedIndex = 0;
        comboBoxIndex.SelectedIndexChanged += new EventHandler(comboBoxIndex_SelectedIndexChanged);
        comboBoxOpType.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxOpType.FormattingEnabled = true;
        comboBoxOpType.Items.AddRange(new object[2] { "Auto", "Manual" });
        comboBoxOpType.Location = new Point(160, 37);
        comboBoxOpType.Name = "comboBoxOpType";
        comboBoxOpType.Size = new Size(63, 23);
        comboBoxOpType.TabIndex = 137;
        comboBoxOpType.SelectedIndexChanged += new EventHandler(comboBoxOpType_SelectedIndexChanged);
        labelIndex.AutoSize = true;
        labelIndex.Location = new Point(17, 91);
        labelIndex.Name = "labelIndex";
        labelIndex.Size = new Size(70, 15);
        labelIndex.TabIndex = 135;
        labelIndex.Text = "Select Index :";
        checkBoxEnable.AutoSize = true;
        checkBoxEnable.Location = new Point(20, 39);
        checkBoxEnable.Name = "checkBoxEnable";
        checkBoxEnable.Size = new Size(58, 19);
        checkBoxEnable.TabIndex = 136;
        checkBoxEnable.Text = "Enable";
        checkBoxEnable.UseVisualStyleBackColor = true;
        checkBoxEnable.CheckedChanged += new EventHandler(checkBoxEnable_CheckedChanged);
        labelPosition.Location = new Point(10, 362);
        labelPosition.Name = "labelPosition";
        labelPosition.Size = new Size(101, 12);
        labelPosition.TabIndex = 160;
        labelPosition.Text = "Position:Out of chart";
        SetGlobalToneSft.BackColor = Color.LightCyan;
        SetGlobalToneSft.ForeColor = SystemColors.ControlText;
        SetGlobalToneSft.ImageKey = "(無)";
        SetGlobalToneSft.Location = new Point(49, 40);
        SetGlobalToneSft.Name = "SetGlobalToneSft";
        SetGlobalToneSft.Size = new Size(90, 23);
        SetGlobalToneSft.TabIndex = 161;
        SetGlobalToneSft.Text = "Set Shift";
        SetGlobalToneSft.UseVisualStyleBackColor = false;
        SetGlobalToneSft.Click += new EventHandler(SetGlobalToneSft_Click);
        groupBox1.Controls.Add(SetGlobalToneSft);
        groupBox1.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupBox1.Location = new Point(614, 296);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new Size(187, 96);
        groupBox1.TabIndex = 162;
        groupBox1.TabStop = false;
        groupBox1.Text = "Parameter";
        chartArea2.Name = "ChartArea1";
        chartSimulatioWDR.ChartAreas.Add(chartArea2);
        chartSimulatioWDR.Cursor = Cursors.Cross;
        legend2.Name = "Legend1";
        chartSimulatioWDR.Legends.Add(legend2);
        chartSimulatioWDR.Location = new Point(12, 377);
        chartSimulatioWDR.Name = "chartSimulatioWDR";
        series3.BorderWidth = 2;
        series3.ChartArea = "ChartArea1";
        series3.ChartType = SeriesChartType.Line;
        series3.Color = Color.Red;
        series3.Legend = "Legend1";
        series3.MarkerSize = 7;
        series3.Name = "SimulationWDR";
        series4.ChartArea = "ChartArea1";
        series4.ChartType = SeriesChartType.Point;
        series4.Legend = "Legend1";
        series4.Name = "SimulationWDRFeature";
        chartSimulatioWDR.Series.Add(series3);
        chartSimulatioWDR.Series.Add(series4);
        chartSimulatioWDR.Size = new Size(520, 244);
        chartSimulatioWDR.TabIndex = 163;
        chartSimulatioWDR.Text = "chartSimulatioWDR";
        chartSimulatioWDR.SelectionRangeChanging += new EventHandler<CursorEventArgs>(chartSimulatioWDR_SelectionRangeChanging);
        chartSimulatioWDR.MouseDown += new MouseEventHandler(chartSimulatioWDR_MouseDown);
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.ControlLightLight;
        ClientSize = new Size(989, 664);
        Controls.Add(chartSimulatioWDR);
        Controls.Add(groupBox1);
        Controls.Add(labelPosition);
        Controls.Add(groupBoxApi2);
        Controls.Add(groupFileMenu);
        Controls.Add(chartWDR);
        Name = "FormGlobalTone";
        Text = "Form1";
        MouseEnter += new EventHandler(FormGlobalTone_MouseEnter);
        ((ISupportInitialize)chartWDR).EndInit();
        groupFileMenu.ResumeLayout(false);
        groupBoxApi2.ResumeLayout(false);
        groupBoxApi2.PerformLayout();
        groupBox1.ResumeLayout(false);
        ((ISupportInitialize)chartSimulatioWDR).EndInit();
        ResumeLayout(false);
    }
}
