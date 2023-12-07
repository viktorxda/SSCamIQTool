using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSCamIQTool.SSCamIQTool;

public class WDRCurve : UserControl
{
    public bool m_chartSelectR;

    public bool m_chartSelectRGB;

    public bool m_chartSelectR_C;

    public bool m_chartSelectRGB_C;

    private int unknowN;

    private int m_nFeatureIdxR;

    private int m_nFeatureIdxR_c;

    private int m_nGammaLength = 256;

    private short m_nGammaValueMax;

    private List<Point> m_ptFeatureR = new List<Point>();

    private List<Point> m_ptFeatureRControl = new List<Point>();

    private string m_strCtrlPtYes = "Ctrl Pt: Yes";

    private string m_strCtrlPtNo = "Ctrl Pt: No";

    private int diff_left_control_x;

    private int diff_left_control_y;

    private int diff_right_control_x;

    private int diff_right_control_y;

    private bool autoMode;

    private GuiGroup gammaGroup;

    private GuiGroup gammaGroup_temp;

    private GuiItem enableItem;

    private GuiItem opTypeItem;

    private int itemR;

    private double new_Y;

    public long[] gammainitialvalueR;

    public long[] gammainitialvalueR_Manual;

    private IContainer components = null;

    private Chart chartGamma;

    private Label labelPosition;

    private Button btnSave;

    private Button btnLoad;

    private Button btnRedset;

    private NumericUpDown numUpDown_Red_Y;

    private NumericUpDown numUpDown_Red_X;

    private Label labelColorR;

    private Label label9;

    private Label label8;

    private Label label5;

    private CheckBox checkBox_R;

    private Label sectionlabel;

    private TrackBar sectiontrackBar;

    private Button btnResetRCurve;

    private Label lbIsCtrlPtR;

    private GroupBox groupRCurve;

    private GroupBox groupFileMenu;

    private Label labelIndex;

    private ComboBox comboBoxIndex;

    private CheckBox checkBoxEnable;

    private ComboBox comboBoxOpType;

    private Label labelOpType;

    private GroupBox groupBoxApi2;

    private GroupBox groupBoxControl;

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
            itemR = 3;
            UpdateGammaValue(0);
        }
        else
        {
            comboBoxIndex.Enabled = true;
            itemR = 2;
            UpdateGammaValue(Index);
        }
    }

    private void checkBoxEnable_CheckedChanged(object sender, EventArgs e)
    {
        GammaEnable = checkBoxEnable.Checked ? 1 : 0;
        enableItem.DataValue[0] = GammaEnable;
        if (GammaEnable == 0)
        {
            chartGamma.Enabled = false;
            groupBoxControl.Enabled = false;
        }
        else
        {
            chartGamma.Enabled = true;
            groupBoxControl.Enabled = true;
            checkBoxSyncRGB_CheckedChanged(new CheckBox(), new EventArgs());
        }
    }

    public void UpdatePage()
    {
        int num = 256;
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
            setRedGammaArray(gammaGroup.ItemList[itemR].DataValue, index);
        }
        else
        {
            setRedGammaArray(gammaGroup.ItemList[itemR].DataValue, index);
        }
    }

    public void SaveGammaValue()
    {
        int num = 256;
        int num2 = 16;
        Index_Old = Index;
        if (OpMode == 0 && GammaEnable == 1)
        {
            for (int i = 0; i < num2 * num; i++)
            {
                gammaGroup.ItemList[itemR].DataValue[i] = gammaGroup_temp.ItemList[itemR].DataValue[i];
            }
        }
        else if (OpMode == 1 && GammaEnable == 1)
        {
            for (int j = 0; j < num; j++)
            {
                gammaGroup.ItemList[itemR].DataValue[j] = gammaGroup_temp.ItemList[itemR].DataValue[j];
            }
        }
    }

    public void SaveGammaValue_temp()
    {
        getRedGammaArray(out var pGammaR);
        if (OpMode == 0 && GammaEnable == 1)
        {
            pGammaR.CopyTo(gammaGroup_temp.ItemList[itemR].DataValue, Index * pGammaR.Length);
        }
        else if (OpMode == 1 && GammaEnable == 1)
        {
            gammaGroup_temp.ItemList[itemR].DataValue = pGammaR;
        }
    }

    public WDRCurve(GuiGroup group)
    {
        InitializeComponent();
        gammaGroup = group;
        gammaGroup_temp = group;
        short num = (short)(gammaGroup.ItemList[2].MaxValue + 1);
        if (num > m_nGammaValueMax)
        {
            m_nGammaValueMax = num;
        }
        InitChart();
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
            enableItem = gammaGroup.ItemList[0];
            opTypeItem = gammaGroup.ItemList[1];
            itemR = 2;
        }
        else
        {
            groupBoxApi2.Visible = false;
            itemR = 0;
        }
        initialGammaValue();
        UpdatePage();
        checkBoxEnable_CheckedChanged(new CheckBox(), new EventArgs());
    }

    public void InitChart()
    {
        int nGammaValueMax = m_nGammaValueMax;
        int num = 256;
        GetGammaWithOB(m_nGammaValueMax, m_nGammaValueMax, 0m, out var pGammaArray);
        chartGamma.ChartAreas[0].AxisX.Minimum = 0.0;
        chartGamma.ChartAreas[0].AxisX.Maximum = num;
        chartGamma.ChartAreas[0].AxisX.Interval = num / 16;
        chartGamma.ChartAreas[0].AxisX.LabelStyle.Format = "D1";
        chartGamma.ChartAreas[0].AxisY.Minimum = 0.0;
        chartGamma.ChartAreas[0].AxisY.Maximum = nGammaValueMax;
        chartGamma.ChartAreas[0].AxisY.Interval = nGammaValueMax / 16;
        chartGamma.ChartAreas[0].AxisY.LabelStyle.Format = "D1";
        chartGamma.Legends[0].Enabled = false;
        if (pGammaArray != null)
        {
            chartGamma.Series[0].Points.Clear();
            for (int i = 0; i < pGammaArray.Length; i++)
            {
                chartGamma.Series["SeriesGammaR"].Points.AddXY(i, pGammaArray[i].Y);
            }
            Series seiresPoints = chartGamma.Series["SeriesFeatureR"];
            Series seiresLines = chartGamma.Series["SeriesGammaR"];
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

    public void InitBezierControl(Series seiresRLine, Series seiresRPoints, Series seiresControl, List<Point> m_ptFeatureR, List<Point> m_ptFeatureRControl)
    {
        seiresControl.Points.Clear();
        m_ptFeatureRControl.Clear();
        for (int i = 0; i < m_ptFeatureR.Count - 1; i++)
        {
            double P = 0.0;
            double P2 = 0.0;
            double num = 0.3;
            double num2 = 0.6;
            double num3 = m_ptFeatureR[i].X + (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) * num;
            double num4 = m_ptFeatureR[i].X + (m_ptFeatureR[i + 1].X - m_ptFeatureR[i].X) * num2;
            double p = m_ptFeatureR[i].X;
            double p2 = m_ptFeatureR[i + 1].X;
            InitCtrolChart(num3, num4, p, p2, num, num2, ref P, ref P2);
            double P3 = 0.0;
            double P4 = 0.0;
            double t = 0.3;
            double t2 = 0.6;
            double pm = seiresRLine.Points[(int)num3].YValues[0];
            double pm2 = seiresRLine.Points[(int)num4].YValues[0];
            double p3 = m_ptFeatureR[i].Y;
            double p4 = m_ptFeatureR[i + 1].Y;
            InitCtrolChart(pm, pm2, p3, p4, t, t2, ref P3, ref P4);
            int num5 = (int)P;
            int num6 = (int)P2;
            int num7 = (int)P3;
            int num8 = (int)P4;
            m_ptFeatureRControl.Add(new Point(num5, num7));
            m_ptFeatureRControl.Add(new Point(num6, num8));
        }
        for (int j = 0; j < m_ptFeatureRControl.Count; j++)
        {
            seiresControl.Points.AddXY(m_ptFeatureRControl[j].X, m_ptFeatureRControl[j].Y);
        }
        clearControlLine();
        for (int k = 0; k < m_ptFeatureRControl.Count; k++)
        {
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            float a = 0f;
            float b = 0f;
            int num13 = 0;
            int num14 = 0;
            if (k % 2 == 0)
            {
                num14 = k / 2;
                num9 = m_ptFeatureR[num14].X;
                num10 = m_ptFeatureRControl[k].X;
                num11 = m_ptFeatureR[num14].Y;
                num12 = m_ptFeatureRControl[k].Y;
                LineFunction(num9, num10, num11, num12, ref a, ref b);
                for (int l = num9; l <= num10; l++)
                {
                    num13 = (int)(a * l + b);
                    chartGamma.Series["SeriesBezierControl" + k].Points.AddXY(l, num13);
                }
            }
            else
            {
                num14 = (k - 1) / 2 + 1;
                num9 = m_ptFeatureRControl[k].X;
                num10 = m_ptFeatureR[num14].X;
                num11 = m_ptFeatureRControl[k].Y;
                num12 = m_ptFeatureR[num14].Y;
                LineFunction(num9, num10, num11, num12, ref a, ref b);
                for (int m = num9; m <= num10; m++)
                {
                    num13 = (int)(a * m + b);
                    chartGamma.Series["SeriesBezierControl" + k].Points.AddXY(m, num13);
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
        for (int i = 0; i < sectiontrackBar.Value; i++)
        {
            num *= 2;
        }
        for (int j = 0; j < nGammaLength; j += nGammaLength / num)
        {
            pFeaturePoint.Add(new Point(j, (int)seiresLines.Points[j].YValues[0]));
        }
        pFeaturePoint.Add(new Point(nGammaLength - 1, (int)seiresLines.Points[nGammaLength - 1].YValues[0]));
        seiresPoints.Points.Clear();
        for (int k = 0; k < pFeaturePoint.Count; k++)
        {
            seiresPoints.Points.AddXY(pFeaturePoint[k].X, pFeaturePoint[k].Y);
        }
    }

    private void RGBGamma_Load(object sender, EventArgs e)
    {
    }

    private void checkBoxSyncRGB_CheckedChanged(object sender, EventArgs e)
    {
        RGBsameLineBack();
        chartGamma.Series["SeriesGammaR"].Color = Color.Red;
        labelColorR.BackColor = Color.Red;
        groupRCurve.Text = "Red";
        clearControlLine();
        drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
    }

    private void checkBox_R_CheckedChanged(object sender, EventArgs e)
    {
        chartGamma.Series["SeriesGammaR"].Enabled = checkBox_R.Checked;
        chartGamma.Series["SeriesFeatureR"].Enabled = checkBox_R.Checked;
        chartGamma.Series["SeriesControlR"].Enabled = checkBox_R.Checked;
        numUpDown_Red_X.Enabled = checkBox_R.Checked;
        numUpDown_Red_Y.Enabled = checkBox_R.Checked;
        btnRedset.Enabled = checkBox_R.Checked;
        btnResetRCurve.Enabled = checkBox_R.Checked;
        if (!checkBox_R.Checked)
        {
            clearControlLine();
            return;
        }
        clearControlLine();
        drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
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
        for (int i = ptFeature[nFeatureIdx - 1].X; i <= ptFeature[nFeatureIdx].X; i++)
        {
            num = (i - num2) / (num3 - num2);
            num6 = (int)((1.0 - num) * num4 + num * num5);
            seiresCurve.Points[i].YValues[0] = num6;
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
            for (int i = ptFeature[nFeatureIdx - 1].X; i <= ptFeature[nFeatureIdx + 1].X; i++)
            {
                num = (i - num2) / (num4 - num2);
                num8 = (int)((1.0 - num) * (1.0 - num) * num5 + 2.0 * (1.0 - num) * num * num6 + num * num * num7);
                seiresCurve.Points[i].YValues[0] = num8;
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

    public void cubicBezier(double p0, double p1, double p2, double p3, int x1, int x2, Series seiresCurve)
    {
        int num = x2 - x1;
        double num2 = 0.0;
        for (int i = 0; i < num; i++)
        {
            if (x1 + i < 256)
            {
                num2 = i / (double)num;
                double num3 = p0 * (1.0 - num2) * (1.0 - num2) * (1.0 - num2) + 3.0 * p1 * num2 * (1.0 - num2) * (1.0 - num2) + 3.0 * p2 * num2 * num2 * (1.0 - num2) + p3 * num2 * num2 * num2;
                seiresCurve.Points[x1 + i].YValues[0] = num3;
            }
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
        double num8 = (num2 - num) / (num3 - num);
        num5 = (num5 - (1.0 - num8) * (1.0 - num8) * num4 - num8 * num8 * num6) / (2.0 * (1.0 - num8) * num8);
        if (nFeatureIdx >= 1 && nFeatureIdx < ptFeature.Count - 1)
        {
            for (int i = ptFeature[nFeatureIdx - 1].X; i < ptFeature[nFeatureIdx + 1].X; i++)
            {
                num8 = (i - num) / (num3 - num);
                num7 = (int)((1.0 - num8) * (1.0 - num8) * num4 + 2.0 * (1.0 - num8) * num8 * num5 + num8 * num8 * num6);
                seiresCurve.Points[i].YValues[0] = num7;
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
            if (e.Button == MouseButtons.Left)
            {
                if (!checkBox_R.Checked)
                {
                    return;
                }
                Series pCurve = chartGamma.Series["SeriesGammaR"];
                AddFeaturePoint(pCurve, m_ptFeatureR, m_ptFeatureRControl, ptSelect, ref m_nFeatureIdxR, ref m_nFeatureIdxR_c, ref bFindPoint, ref bFindPoint_c);
                if (bFindPoint)
                {
                    m_chartSelectR = true;
                    Series seiresRLine = chartGamma.Series["SeriesGammaR"];
                    Series seiresRPoints = chartGamma.Series["SeriesFeatureR"];
                    Series seiresControl = chartGamma.Series["SeriesControlR"];
                    InitBezierControl(seiresRLine, seiresRPoints, seiresControl, m_ptFeatureR, m_ptFeatureRControl);
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
            else if (e.Button == MouseButtons.Right)
            {
                axisValuesFromMouse = GetAxisValuesFromMouse(ref chartGamma, e.X, e.Y);
                if (checkBox_R.Checked)
                {
                    ptSelect = new Point((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
                    Series pCurve = chartGamma.Series["SeriesGammaR"];
                    Series series = chartGamma.Series["SeriesFeatureR"];
                    RemoveFeaturePoint(pCurve, series, m_ptFeatureR, ptSelect, ref bFindPoint);
                    Series seiresControl2 = chartGamma.Series["SeriesControlR"];
                    InitBezierControl(pCurve, series, seiresControl2, m_ptFeatureR, m_ptFeatureRControl);
                }
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
            if (m_chartSelectR && axisValuesFromMouse != null)
            {
                Series seiresCurve = chartGamma.Series["SeriesGammaR"];
                m_ptFeatureR[m_nFeatureIdxR] = new Point((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
                if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
                {
                    if ((int)axisValuesFromMouse.Item1 <= m_ptFeatureR[m_nFeatureIdxR - 1].X)
                    {
                        m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR - 1].X + 1, (int)axisValuesFromMouse.Item2);
                    }
                    else if ((int)axisValuesFromMouse.Item1 >= m_ptFeatureR[m_nFeatureIdxR + 1].X)
                    {
                        m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR + 1].X - 1, (int)axisValuesFromMouse.Item2);
                    }
                }
                if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point((int)axisValuesFromMouse.Item1 + diff_left_control_x, (int)axisValuesFromMouse.Item2 + diff_left_control_y);
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point((int)axisValuesFromMouse.Item1 + diff_right_control_x, (int)axisValuesFromMouse.Item2 + diff_right_control_y);
                }
                else if (m_nFeatureIdxR == 0)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point((int)axisValuesFromMouse.Item1 + diff_right_control_x, (int)axisValuesFromMouse.Item2 + diff_right_control_y);
                }
                else if (m_nFeatureIdxR == m_ptFeatureR.Count - 1)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point((int)axisValuesFromMouse.Item1 + diff_left_control_x, (int)axisValuesFromMouse.Item2 + diff_left_control_y);
                }
                Series series = chartGamma.Series["SeriesControlR"];
                series.Points.Clear();
                for (int i = 0; i < m_ptFeatureRControl.Count; i++)
                {
                    series.Points.AddXY(m_ptFeatureRControl[i].X, m_ptFeatureRControl[i].Y);
                }
                clearControlLine();
                drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
                _ = chartGamma.Series["SeriesGammaR"];
                _ = chartGamma.Series["SeriesFeatureR"];
                if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
                {
                    double num = 0.0;
                    double num2 = 0.0;
                    double num3 = 0.0;
                    double num4 = 0.0;
                    int num5 = 0;
                    int num6 = 0;
                    num = m_ptFeatureR[m_nFeatureIdxR - 1].Y;
                    num2 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 2].Y;
                    num3 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].Y;
                    num4 = m_ptFeatureR[m_nFeatureIdxR].Y;
                    num5 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                    num6 = m_ptFeatureR[m_nFeatureIdxR].X;
                    cubicBezier(num, num2, num3, num4, num5, num6, seiresCurve);
                    double num7 = 0.0;
                    double num8 = 0.0;
                    double num9 = 0.0;
                    double num10 = 0.0;
                    int num11 = 0;
                    int num12 = 0;
                    num7 = m_ptFeatureR[m_nFeatureIdxR].Y;
                    num8 = m_ptFeatureRControl[m_nFeatureIdxR * 2].Y;
                    num9 = m_ptFeatureRControl[m_nFeatureIdxR * 2 + 1].Y;
                    num10 = m_ptFeatureR[m_nFeatureIdxR + 1].Y;
                    num11 = m_ptFeatureR[m_nFeatureIdxR].X;
                    num12 = m_ptFeatureR[m_nFeatureIdxR + 1].X;
                    cubicBezier(num7, num8, num9, num10, num11, num12, seiresCurve);
                }
                else if (m_nFeatureIdxR == 0)
                {
                    double num13 = 0.0;
                    double num14 = 0.0;
                    double num15 = 0.0;
                    double num16 = 0.0;
                    int num17 = 0;
                    int num18 = 0;
                    num13 = m_ptFeatureR[m_nFeatureIdxR].Y;
                    num14 = m_ptFeatureRControl[m_nFeatureIdxR * 2].Y;
                    num15 = m_ptFeatureRControl[m_nFeatureIdxR * 2 + 1].Y;
                    num16 = m_ptFeatureR[m_nFeatureIdxR + 1].Y;
                    num17 = m_ptFeatureR[m_nFeatureIdxR].X;
                    num18 = m_ptFeatureR[m_nFeatureIdxR + 1].X;
                    cubicBezier(num13, num14, num15, num16, num17, num18, seiresCurve);
                }
                else if (m_nFeatureIdxR == m_ptFeatureR.Count - 1)
                {
                    double num19 = 0.0;
                    double num20 = 0.0;
                    double num21 = 0.0;
                    double num22 = 0.0;
                    int num23 = 0;
                    int num24 = 0;
                    num19 = m_ptFeatureR[m_nFeatureIdxR - 1].Y;
                    num20 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 2].Y;
                    num21 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].Y;
                    num22 = m_ptFeatureR[m_nFeatureIdxR].Y;
                    num23 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                    num24 = m_ptFeatureR[m_nFeatureIdxR].X;
                    cubicBezier(num19, num20, num21, num22, num23, num24, seiresCurve);
                }
                chartGamma.Series["SeriesFeatureR"].Points.Clear();
                for (int j = 0; j < m_ptFeatureR.Count; j++)
                {
                    chartGamma.Series["SeriesFeatureR"].Points.AddXY(m_ptFeatureR[j].X, m_ptFeatureR[j].Y);
                }
            }
            if (m_chartSelectR_C && axisValuesFromMouse != null)
            {
                m_ptFeatureRControl[m_nFeatureIdxR_c] = new Point((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
                if (m_nFeatureIdxR_c % 2 == 0)
                {
                    int num25 = m_ptFeatureR[m_nFeatureIdxR_c / 2].X;
                    if ((int)axisValuesFromMouse.Item1 <= num25)
                    {
                        m_ptFeatureRControl[m_nFeatureIdxR_c] = new Point(num25 + 1, (int)axisValuesFromMouse.Item2);
                    }
                }
                else
                {
                    int num25 = m_ptFeatureR[(m_nFeatureIdxR_c + 1) / 2].X;
                    if ((int)axisValuesFromMouse.Item1 >= num25)
                    {
                        m_ptFeatureRControl[m_nFeatureIdxR_c] = new Point(num25 - 1, (int)axisValuesFromMouse.Item2);
                    }
                }
                int num26 = 0;
                int num27 = 0;
                int num28 = 0;
                int num29 = 0;
                int num30 = 0;
                int num31 = 0;
                if (m_nFeatureIdxR_c != 0 && m_nFeatureIdxR_c != m_ptFeatureRControl.Count - 1)
                {
                    if (m_nFeatureIdxR_c % 2 == 0)
                    {
                        num26 = m_ptFeatureRControl[m_nFeatureIdxR_c - 1].X;
                        num27 = m_ptFeatureRControl[m_nFeatureIdxR_c - 1].Y;
                        num28 = m_ptFeatureR[m_nFeatureIdxR_c / 2].X;
                        num29 = m_ptFeatureR[m_nFeatureIdxR_c / 2].Y;
                        num30 = m_ptFeatureRControl[m_nFeatureIdxR_c].X;
                        num31 = m_ptFeatureRControl[m_nFeatureIdxR_c].Y;
                        if (num30 - num28 != 0)
                        {
                            num26 = num28 - (num30 - num28);
                            num27 = num29 - (num31 - num29);
                        }
                        m_ptFeatureRControl[m_nFeatureIdxR_c - 1] = new Point(num26, num27);
                    }
                    else
                    {
                        num26 = m_ptFeatureRControl[m_nFeatureIdxR_c].X;
                        num27 = m_ptFeatureRControl[m_nFeatureIdxR_c].Y;
                        num28 = m_ptFeatureR[(m_nFeatureIdxR_c + 1) / 2].X;
                        num29 = m_ptFeatureR[(m_nFeatureIdxR_c + 1) / 2].Y;
                        num30 = m_ptFeatureRControl[m_nFeatureIdxR_c + 1].X;
                        num31 = m_ptFeatureRControl[m_nFeatureIdxR_c + 1].Y;
                        if (num28 - num26 != 0)
                        {
                            num30 = num28 + (num28 - num26);
                            num31 = num29 + (num29 - num27);
                        }
                        m_ptFeatureRControl[m_nFeatureIdxR_c + 1] = new Point(num30, num31);
                    }
                }
                Series series2 = chartGamma.Series["SeriesControlR"];
                Series seiresCurve2 = chartGamma.Series["SeriesGammaR"];
                series2.Points.Clear();
                for (int k = 0; k < m_ptFeatureRControl.Count; k++)
                {
                    series2.Points.AddXY(m_ptFeatureRControl[k].X, m_ptFeatureRControl[k].Y);
                }
                int num32 = m_nFeatureIdxR_c / 2;
                double num33 = 0.0;
                double num34 = 0.0;
                double num35 = 0.0;
                double num36 = 0.0;
                int num37 = 0;
                int num38 = 0;
                double num39 = 0.0;
                double num40 = 0.0;
                double num41 = 0.0;
                double num42 = 0.0;
                int num43 = 0;
                int num44 = 0;
                if (m_nFeatureIdxR_c % 2 == 0)
                {
                    num33 = m_ptFeatureR[num32].Y;
                    num34 = m_ptFeatureRControl[m_nFeatureIdxR_c].Y;
                    num35 = m_ptFeatureRControl[m_nFeatureIdxR_c + 1].Y;
                    num36 = m_ptFeatureR[num32 + 1].Y;
                    num37 = m_ptFeatureR[num32].X;
                    num38 = m_ptFeatureR[num32 + 1].X;
                    cubicBezier(num33, num34, num35, num36, num37, num38, seiresCurve2);
                    if (m_nFeatureIdxR_c != 0)
                    {
                        num39 = m_ptFeatureR[num32 - 1].Y;
                        num40 = m_ptFeatureRControl[m_nFeatureIdxR_c - 2].Y;
                        num41 = m_ptFeatureRControl[m_nFeatureIdxR_c - 1].Y;
                        num42 = m_ptFeatureR[num32].Y;
                        num43 = m_ptFeatureR[num32 - 1].X;
                        num44 = m_ptFeatureR[num32].X;
                        cubicBezier(num39, num40, num41, num42, num43, num44, seiresCurve2);
                    }
                }
                else
                {
                    num33 = m_ptFeatureR[num32].Y;
                    num34 = m_ptFeatureRControl[m_nFeatureIdxR_c - 1].Y;
                    num35 = m_ptFeatureRControl[m_nFeatureIdxR_c].Y;
                    num36 = m_ptFeatureR[num32 + 1].Y;
                    num37 = m_ptFeatureR[num32].X;
                    num38 = m_ptFeatureR[num32 + 1].X;
                    cubicBezier(num33, num34, num35, num36, num37, num38, seiresCurve2);
                    if (m_nFeatureIdxR_c != m_ptFeatureRControl.Count - 1)
                    {
                        num39 = m_ptFeatureR[num32 + 1].Y;
                        num40 = m_ptFeatureRControl[m_nFeatureIdxR_c + 1].Y;
                        num41 = m_ptFeatureRControl[m_nFeatureIdxR_c + 2].Y;
                        num42 = m_ptFeatureR[num32 + 2].Y;
                        num43 = m_ptFeatureR[num32 + 1].X;
                        num44 = m_ptFeatureR[num32 + 2].X;
                        cubicBezier(num39, num40, num41, num42, num43, num44, seiresCurve2);
                    }
                }
                clearControlLine();
                drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
            }
            if (m_chartSelectRGB && axisValuesFromMouse != null)
            {
                Series seiresCurve3 = chartGamma.Series["SeriesGammaR"];
                m_ptFeatureR[m_nFeatureIdxR] = new Point((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
                if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
                {
                    if ((int)axisValuesFromMouse.Item1 <= m_ptFeatureR[m_nFeatureIdxR - 1].X)
                    {
                        m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR - 1].X + 1, (int)axisValuesFromMouse.Item2);
                    }
                    else if ((int)axisValuesFromMouse.Item1 >= m_ptFeatureR[m_nFeatureIdxR + 1].X)
                    {
                        m_ptFeatureR[m_nFeatureIdxR] = new Point(m_ptFeatureR[m_nFeatureIdxR + 1].X - 1, (int)axisValuesFromMouse.Item2);
                    }
                }
                if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point((int)axisValuesFromMouse.Item1 + diff_left_control_x, (int)axisValuesFromMouse.Item2 + diff_left_control_y);
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point((int)axisValuesFromMouse.Item1 + diff_right_control_x, (int)axisValuesFromMouse.Item2 + diff_right_control_y);
                }
                else if (m_nFeatureIdxR == 0)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2] = new Point((int)axisValuesFromMouse.Item1 + diff_right_control_x, (int)axisValuesFromMouse.Item2 + diff_right_control_y);
                }
                else if (m_nFeatureIdxR == m_ptFeatureR.Count - 1)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1] = new Point((int)axisValuesFromMouse.Item1 + diff_left_control_x, (int)axisValuesFromMouse.Item2 + diff_left_control_y);
                }
                Series series3 = chartGamma.Series["SeriesControlR"];
                series3.Points.Clear();
                for (int l = 0; l < m_ptFeatureRControl.Count; l++)
                {
                    series3.Points.AddXY(m_ptFeatureRControl[l].X, m_ptFeatureRControl[l].Y);
                }
                clearControlLine();
                drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
                _ = chartGamma.Series["SeriesGammaR"];
                _ = chartGamma.Series["SeriesFeatureR"];
                if (m_nFeatureIdxR != 0 && m_nFeatureIdxR != m_ptFeatureR.Count - 1)
                {
                    double num45 = 0.0;
                    double num46 = 0.0;
                    double num47 = 0.0;
                    double num48 = 0.0;
                    int num49 = 0;
                    int num50 = 0;
                    num45 = m_ptFeatureR[m_nFeatureIdxR - 1].Y;
                    num46 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 2].Y;
                    num47 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].Y;
                    num48 = m_ptFeatureR[m_nFeatureIdxR].Y;
                    num49 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                    num50 = m_ptFeatureR[m_nFeatureIdxR].X;
                    cubicBezier(num45, num46, num47, num48, num49, num50, seiresCurve3);
                    double num51 = 0.0;
                    double num52 = 0.0;
                    double num53 = 0.0;
                    double num54 = 0.0;
                    int num55 = 0;
                    int num56 = 0;
                    num51 = m_ptFeatureR[m_nFeatureIdxR].Y;
                    num52 = m_ptFeatureRControl[m_nFeatureIdxR * 2].Y;
                    num53 = m_ptFeatureRControl[m_nFeatureIdxR * 2 + 1].Y;
                    num54 = m_ptFeatureR[m_nFeatureIdxR + 1].Y;
                    num55 = m_ptFeatureR[m_nFeatureIdxR].X;
                    num56 = m_ptFeatureR[m_nFeatureIdxR + 1].X;
                    cubicBezier(num51, num52, num53, num54, num55, num56, seiresCurve3);
                }
                else if (m_nFeatureIdxR == 0)
                {
                    double num57 = 0.0;
                    double num58 = 0.0;
                    double num59 = 0.0;
                    double num60 = 0.0;
                    int num61 = 0;
                    int num62 = 0;
                    num57 = m_ptFeatureR[m_nFeatureIdxR].Y;
                    num58 = m_ptFeatureRControl[m_nFeatureIdxR * 2].Y;
                    num59 = m_ptFeatureRControl[m_nFeatureIdxR * 2 + 1].Y;
                    num60 = m_ptFeatureR[m_nFeatureIdxR + 1].Y;
                    num61 = m_ptFeatureR[m_nFeatureIdxR].X;
                    num62 = m_ptFeatureR[m_nFeatureIdxR + 1].X;
                    cubicBezier(num57, num58, num59, num60, num61, num62, seiresCurve3);
                }
                else if (m_nFeatureIdxR == m_ptFeatureR.Count - 1)
                {
                    double num63 = 0.0;
                    double num64 = 0.0;
                    double num65 = 0.0;
                    double num66 = 0.0;
                    int num67 = 0;
                    int num68 = 0;
                    num63 = m_ptFeatureR[m_nFeatureIdxR - 1].Y;
                    num64 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 2].Y;
                    num65 = m_ptFeatureRControl[m_nFeatureIdxR * 2 - 1].Y;
                    num66 = m_ptFeatureR[m_nFeatureIdxR].Y;
                    num67 = m_ptFeatureR[m_nFeatureIdxR - 1].X;
                    num68 = m_ptFeatureR[m_nFeatureIdxR].X;
                    cubicBezier(num63, num64, num65, num66, num67, num68, seiresCurve3);
                }
                chartGamma.Series["SeriesFeatureR"].Points.Clear();
                chartGamma.Series["SeriesFeatureG"].Points.Clear();
                chartGamma.Series["SeriesFeatureB"].Points.Clear();
                for (int m = 0; m < m_ptFeatureR.Count; m++)
                {
                    chartGamma.Series["SeriesFeatureR"].Points.AddXY(m_ptFeatureR[m].X, m_ptFeatureR[m].Y);
                    chartGamma.Series["SeriesFeatureG"].Points.AddXY(m_ptFeatureR[m].X, m_ptFeatureR[m].Y);
                    chartGamma.Series["SeriesFeatureB"].Points.AddXY(m_ptFeatureR[m].X, m_ptFeatureR[m].Y);
                }
            }
            if (!m_chartSelectRGB_C || axisValuesFromMouse == null)
            {
                return;
            }
            m_ptFeatureRControl[m_nFeatureIdxR_c] = new Point((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
            if (m_nFeatureIdxR_c % 2 == 0)
            {
                int num69 = m_ptFeatureR[m_nFeatureIdxR_c / 2].X;
                if ((int)axisValuesFromMouse.Item1 <= num69)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR_c] = new Point(num69 + 1, (int)axisValuesFromMouse.Item2);
                }
            }
            else
            {
                int num69 = m_ptFeatureR[(m_nFeatureIdxR_c + 1) / 2].X;
                if ((int)axisValuesFromMouse.Item1 >= num69)
                {
                    m_ptFeatureRControl[m_nFeatureIdxR_c] = new Point(num69 - 1, (int)axisValuesFromMouse.Item2);
                }
            }
            int num70 = 0;
            int num71 = 0;
            int num72 = 0;
            int num73 = 0;
            int num74 = 0;
            int num75 = 0;
            if (m_nFeatureIdxR_c != 0 && m_nFeatureIdxR_c != m_ptFeatureRControl.Count - 1)
            {
                if (m_nFeatureIdxR_c % 2 == 0)
                {
                    num70 = m_ptFeatureRControl[m_nFeatureIdxR_c - 1].X;
                    num71 = m_ptFeatureRControl[m_nFeatureIdxR_c - 1].Y;
                    num72 = m_ptFeatureR[m_nFeatureIdxR_c / 2].X;
                    num73 = m_ptFeatureR[m_nFeatureIdxR_c / 2].Y;
                    num74 = m_ptFeatureRControl[m_nFeatureIdxR_c].X;
                    num75 = m_ptFeatureRControl[m_nFeatureIdxR_c].Y;
                    if (num74 - num72 != 0)
                    {
                        num70 = num72 - (num74 - num72);
                        num71 = num73 - (num75 - num73);
                    }
                    m_ptFeatureRControl[m_nFeatureIdxR_c - 1] = new Point(num70, num71);
                }
                else
                {
                    num70 = m_ptFeatureRControl[m_nFeatureIdxR_c].X;
                    num71 = m_ptFeatureRControl[m_nFeatureIdxR_c].Y;
                    num72 = m_ptFeatureR[(m_nFeatureIdxR_c + 1) / 2].X;
                    num73 = m_ptFeatureR[(m_nFeatureIdxR_c + 1) / 2].Y;
                    num74 = m_ptFeatureRControl[m_nFeatureIdxR_c + 1].X;
                    num75 = m_ptFeatureRControl[m_nFeatureIdxR_c + 1].Y;
                    if (num72 - num70 != 0)
                    {
                        num74 = num72 + (num72 - num70);
                        num75 = num73 + (num73 - num71);
                    }
                    m_ptFeatureRControl[m_nFeatureIdxR_c + 1] = new Point(num74, num75);
                }
            }
            Series series4 = chartGamma.Series["SeriesControlR"];
            Series seiresCurve4 = chartGamma.Series["SeriesGammaR"];
            series4.Points.Clear();
            for (int n = 0; n < m_ptFeatureRControl.Count; n++)
            {
                series4.Points.AddXY(m_ptFeatureRControl[n].X, m_ptFeatureRControl[n].Y);
            }
            int num76 = m_nFeatureIdxR_c / 2;
            double num77 = 0.0;
            double num78 = 0.0;
            double num79 = 0.0;
            double num80 = 0.0;
            int num81 = 0;
            int num82 = 0;
            double num83 = 0.0;
            double num84 = 0.0;
            double num85 = 0.0;
            double num86 = 0.0;
            int num87 = 0;
            int num88 = 0;
            if (m_nFeatureIdxR_c % 2 == 0)
            {
                num77 = m_ptFeatureR[num76].Y;
                num78 = m_ptFeatureRControl[m_nFeatureIdxR_c].Y;
                num79 = m_ptFeatureRControl[m_nFeatureIdxR_c + 1].Y;
                num80 = m_ptFeatureR[num76 + 1].Y;
                num81 = m_ptFeatureR[num76].X;
                num82 = m_ptFeatureR[num76 + 1].X;
                cubicBezier(num77, num78, num79, num80, num81, num82, seiresCurve4);
                if (m_nFeatureIdxR_c != 0)
                {
                    num83 = m_ptFeatureR[num76 - 1].Y;
                    num84 = m_ptFeatureRControl[m_nFeatureIdxR_c - 2].Y;
                    num85 = m_ptFeatureRControl[m_nFeatureIdxR_c - 1].Y;
                    num86 = m_ptFeatureR[num76].Y;
                    num87 = m_ptFeatureR[num76 - 1].X;
                    num88 = m_ptFeatureR[num76].X;
                    cubicBezier(num83, num84, num85, num86, num87, num88, seiresCurve4);
                }
            }
            else
            {
                num77 = m_ptFeatureR[num76].Y;
                num78 = m_ptFeatureRControl[m_nFeatureIdxR_c - 1].Y;
                num79 = m_ptFeatureRControl[m_nFeatureIdxR_c].Y;
                num80 = m_ptFeatureR[num76 + 1].Y;
                num81 = m_ptFeatureR[num76].X;
                num82 = m_ptFeatureR[num76 + 1].X;
                cubicBezier(num77, num78, num79, num80, num81, num82, seiresCurve4);
                if (m_nFeatureIdxR_c != m_ptFeatureRControl.Count - 1)
                {
                    num83 = m_ptFeatureR[num76 + 1].Y;
                    num84 = m_ptFeatureRControl[m_nFeatureIdxR_c + 1].Y;
                    num85 = m_ptFeatureRControl[m_nFeatureIdxR_c + 2].Y;
                    num86 = m_ptFeatureR[num76 + 2].Y;
                    num87 = m_ptFeatureR[num76 + 1].X;
                    num88 = m_ptFeatureR[num76 + 2].X;
                    cubicBezier(num83, num84, num85, num86, num87, num88, seiresCurve4);
                }
            }
            clearControlLine();
            drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
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
                _ = chartGamma.Series["SeriesGammaR"];
                _ = chartGamma.Series["SeriesFeatureR"];
                clearControlLine();
                drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
            }
            else if (m_chartSelectRGB)
            {
                Series seiresCurve = chartGamma.Series["SeriesGammaR"];
                Series seriesPoint = chartGamma.Series["SeriesFeatureR"];
                increasingLineModify(m_nFeatureIdxR, seiresCurve, m_ptFeatureR, seriesPoint);
                clearControlLine();
                drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
            }
            if (m_chartSelectR_C)
            {
                clearControlLine();
                drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
            }
            else if (m_chartSelectRGB_C)
            {
                clearControlLine();
                drawControlLine(m_ptFeatureR, m_ptFeatureRControl);
            }
            m_chartSelectR = false;
            m_chartSelectR_C = false;
            m_chartSelectRGB_C = false;
            chartGamma.Series["SeriesChoosePoint"].Points.Clear();
            chartGamma.Cursor = Cursors.Cross;
            SaveGammaValue_temp();
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
                if (seiresCurve.Points[k].YValues[0] >= num11)
                {
                    seiresCurve.Points[k].YValues[0] = num11;
                }
                if (seiresCurve.Points[k].YValues[0] <= num10)
                {
                    seiresCurve.Points[k].YValues[0] = num10;
                }
                if (k == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point(ptFeature[nFeatureIdx].X, (int)seiresCurve.Points[k].YValues[0]);
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
                if (seiresCurve.Points[n].YValues[0] >= num13)
                {
                    seiresCurve.Points[n].YValues[0] = num13;
                }
                if (n == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point(ptFeature[nFeatureIdx].X, (int)seiresCurve.Points[n].YValues[0]);
                }
            }
            for (int num14 = ptFeature[nFeatureIdx].X; num14 <= ptFeature[nFeatureIdx + 1].X; num14++)
            {
                if (seiresCurve.Points[num14 + 1].YValues[0] <= seiresCurve.Points[num14].YValues[0])
                {
                    seiresCurve.Points[num14 + 1].YValues[0] = seiresCurve.Points[num14].YValues[0];
                }
                if (num14 == ptFeature[nFeatureIdx + 1].X)
                {
                    ptFeature[nFeatureIdx + 1] = new Point(ptFeature[nFeatureIdx + 1].X, (int)seiresCurve.Points[num14].YValues[0]);
                }
            }
            for (int num15 = ptFeature[nFeatureIdx + 1].X; num15 <= ptFeature[nFeatureIdx + 2].X; num15++)
            {
                if (seiresCurve.Points[num15].YValues[0] <= seiresCurve.Points[num15 - 1].YValues[0])
                {
                    seiresCurve.Points[num15].YValues[0] = seiresCurve.Points[num15 - 1].YValues[0];
                }
                if (num15 == ptFeature[nFeatureIdx + 2].X)
                {
                    ptFeature[nFeatureIdx + 2] = new Point(ptFeature[nFeatureIdx + 2].X, (int)seiresCurve.Points[num15].YValues[0]);
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
                if (seiresCurve.Points[num20].YValues[0] >= num18)
                {
                    seiresCurve.Points[num20].YValues[0] = num18;
                }
                if (num20 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point(ptFeature[nFeatureIdx].X, (int)seiresCurve.Points[num20].YValues[0]);
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
                if (seiresCurve.Points[num25].YValues[0] <= num23)
                {
                    seiresCurve.Points[num25].YValues[0] = num23;
                }
                if (num25 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point(ptFeature[nFeatureIdx].X, (int)seiresCurve.Points[num25].YValues[0]);
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
                if (seiresCurve.Points[num27].YValues[0] <= num23)
                {
                    seiresCurve.Points[num27].YValues[0] = num23;
                }
                if (num27 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point(ptFeature[nFeatureIdx].X, (int)seiresCurve.Points[num27].YValues[0]);
                }
            }
            for (int num28 = ptFeature[nFeatureIdx].X; num28 <= ptFeature[nFeatureIdx + 1].X; num28++)
            {
                if (seiresCurve.Points[num28].YValues[0] <= seiresCurve.Points[num28 - 1].YValues[0])
                {
                    seiresCurve.Points[num28].YValues[0] = seiresCurve.Points[num28 - 1].YValues[0];
                }
                if (num28 == ptFeature[nFeatureIdx + 1].X)
                {
                    ptFeature[nFeatureIdx + 1] = new Point(ptFeature[nFeatureIdx + 1].X, (int)seiresCurve.Points[num28].YValues[0]);
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
                if (seiresCurve.Points[num33].YValues[0] <= num31)
                {
                    seiresCurve.Points[num33].YValues[0] = num31;
                }
                if (num33 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point(ptFeature[nFeatureIdx].X, (int)seiresCurve.Points[num33].YValues[0]);
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
                if (seiresCurve.Points[num35].YValues[0] <= num31)
                {
                    seiresCurve.Points[num35].YValues[0] = num31;
                }
                if (num35 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point(ptFeature[nFeatureIdx].X, (int)seiresCurve.Points[num35].YValues[0]);
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
                if (seiresCurve.Points[num39].YValues[0] >= num37)
                {
                    seiresCurve.Points[num39].YValues[0] = num37;
                }
                if (num39 == ptFeature[nFeatureIdx - 1].X)
                {
                    ptFeature[nFeatureIdx - 1] = new Point(ptFeature[nFeatureIdx - 1].X, (int)seiresCurve.Points[num39].YValues[0]);
                }
            }
            for (int num40 = ptFeature[nFeatureIdx - 2].X; num40 <= ptFeature[nFeatureIdx - 1].X; num40++)
            {
                if (seiresCurve.Points[num40].YValues[0] <= seiresCurve.Points[num40 - 1].YValues[0])
                {
                    seiresCurve.Points[num40].YValues[0] = seiresCurve.Points[num40 - 1].YValues[0];
                }
                if (num40 == ptFeature[nFeatureIdx - 1].X)
                {
                    ptFeature[nFeatureIdx - 1] = new Point(ptFeature[nFeatureIdx - 1].X, (int)seiresCurve.Points[num40].YValues[0]);
                }
            }
            for (int num41 = ptFeature[nFeatureIdx - 1].X; num41 <= ptFeature[nFeatureIdx].X; num41++)
            {
                if (seiresCurve.Points[num41].YValues[0] <= seiresCurve.Points[num41 - 1].YValues[0])
                {
                    seiresCurve.Points[num41].YValues[0] = seiresCurve.Points[num41 - 1].YValues[0];
                }
                if (num41 == ptFeature[nFeatureIdx].X)
                {
                    ptFeature[nFeatureIdx] = new Point(ptFeature[nFeatureIdx].X, (int)seiresCurve.Points[num41].YValues[0]);
                }
            }
            seriesPoint.Points.Clear();
            for (int num42 = 0; num42 < ptFeature.Count; num42++)
            {
                seriesPoint.Points.AddXY(ptFeature[num42].X, ptFeature[num42].Y);
            }
        }
    }

    private void RGBsameLineSet()
    {
        chartGamma.Series["SeriesGammaG"].Points.Clear();
        chartGamma.Series["SeriesGammaB"].Points.Clear();
        chartGamma.Series["SeriesFeatureG"].Points.Clear();
        chartGamma.Series["SeriesFeatureB"].Points.Clear();
        for (int i = 0; i < m_nGammaLength; i++)
        {
            chartGamma.Series["SeriesGammaG"].Points.AddXY(i, chartGamma.Series["SeriesGammaR"].Points[i].YValues[0]);
            chartGamma.Series["SeriesGammaB"].Points.AddXY(i, chartGamma.Series["SeriesGammaR"].Points[i].YValues[0]);
        }
        chartGamma.Series["SeriesGammaG"].Enabled = false;
        chartGamma.Series["SeriesGammaB"].Enabled = false;
        chartGamma.Series["SeriesControlG"].Points.Clear();
        chartGamma.Series["SeriesControlB"].Points.Clear();
        for (int j = 0; j < m_ptFeatureRControl.Count; j++)
        {
            chartGamma.Series["SeriesControlG"].Points.AddXY(m_ptFeatureRControl[j].X, m_ptFeatureRControl[j].Y);
            chartGamma.Series["SeriesControlB"].Points.AddXY(m_ptFeatureRControl[j].X, m_ptFeatureRControl[j].Y);
        }
        chartGamma.Series["SeriesControlG"].Enabled = false;
        chartGamma.Series["SeriesControlB"].Enabled = false;
    }

    private void RGBsameLineBack()
    {
    }

    public void SeperateSection(ref Series seiresPoints, ref Series seiresLines, List<Point> pFeaturePoint)
    {
        int num = 1;
        seiresPoints.Points.Clear();
        for (int i = 0; i < sectiontrackBar.Value; i++)
        {
            num *= 2;
        }
        for (int j = 0; j < m_nGammaLength; j += m_nGammaLength / num)
        {
            pFeaturePoint.Add(new Point(j, (int)seiresLines.Points[j].YValues[0]));
        }
        pFeaturePoint.Add(new Point(m_nGammaLength - 1, (int)seiresLines.Points[m_nGammaLength - 1].YValues[0]));
        for (int k = 0; k < pFeaturePoint.Count; k++)
        {
            seiresPoints.Points.AddXY(pFeaturePoint[k].X, pFeaturePoint[k].Y);
        }
        sectionlabel.Text = num + " Section";
    }

    private void sectiontrackBar_Scroll(object sender, EventArgs e)
    {
        if (checkBox_R.Checked)
        {
            m_ptFeatureR.Clear();
            Series seiresPoints = chartGamma.Series["SeriesFeatureR"];
            Series seiresLines = chartGamma.Series["SeriesGammaR"];
            SeperateSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
            Series seiresRLine = chartGamma.Series["SeriesGammaR"];
            Series seiresRPoints = chartGamma.Series["SeriesFeatureR"];
            Series seiresControl = chartGamma.Series["SeriesControlR"];
            InitBezierControl(seiresRLine, seiresRPoints, seiresControl, m_ptFeatureR, m_ptFeatureRControl);
        }
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

    private void btnRedadd_Click(object sender, EventArgs e)
    {
        chartGamma.Series["SeriesFeatureR"].Points.AddXY(numUpDown_Red_X.Value, chartGamma.Series["SeriesGammaR"].Points[(int)numUpDown_Red_X.Value].YValues[0]);
    }

    private void btnRedset_Click(object sender, EventArgs e)
    {
        Series series = chartGamma.Series["SeriesGammaR"];
        Series seiresPointsfor_c = chartGamma.Series["SeriesFeatureR"];
        Series seiresControl = chartGamma.Series["SeriesControlR"];
        Point ptSelect = new Point((int)numUpDown_Red_X.Value, (int)numUpDown_Red_Y.Value);
        AddFeaturePointByXY(series, seiresPointsfor_c, seiresControl, m_ptFeatureR, ptSelect, ref m_nFeatureIdxR);
        chartGamma.Series["SeriesFeatureR"].Points.Clear();
        for (int i = 0; i < m_ptFeatureR.Count; i++)
        {
            chartGamma.Series["SeriesFeatureR"].Points.AddXY(m_ptFeatureR[i].X, m_ptFeatureR[i].Y);
        }
        CheckControlPoint(m_ptFeatureR, ptSelect.X, lbIsCtrlPtR);
        Series seriesPoint = chartGamma.Series["SeriesFeatureR"];
        increasingLineModify(m_nFeatureIdxR, series, m_ptFeatureR, seriesPoint);
        SaveGammaValue_temp();
    }

    public void getRedGammaArray(out long[] pGammaR)
    {
        pGammaR = new long[256];
        for (int i = 0; i < 256; i++)
        {
            pGammaR[i] = (int)chartGamma.Series["SeriesGammaR"].Points[i].YValues[0];
        }
    }

    public void setRedGammaArray(long[] pGamma, int index)
    {
        checkBox_R.Checked = true;
        chartGamma.Series["SeriesGammaR"].Points.Clear();
        chartGamma.Series["SeriesFeatureR"].Points.Clear();
        m_ptFeatureR.Clear();
        int num = 0;
        for (int i = 256 * index; i < 256 * (index + 1); i++)
        {
            chartGamma.Series["SeriesGammaR"].Points.AddXY(num, pGamma[i]);
            num++;
        }
        Series seiresPoints = chartGamma.Series["SeriesFeatureR"];
        Series seiresLines = chartGamma.Series["SeriesGammaR"];
        initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
        Series seiresControl = chartGamma.Series["SeriesControlR"];
        InitBezierControl(seiresLines, seiresPoints, seiresControl, m_ptFeatureR, m_ptFeatureRControl);
    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
        int[] array = new int[769];
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Title = "Open the text file you wish";
        openFileDialog.InitialDirectory = "D:";
        openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        chartGamma.Series["SeriesGammaR"].Points.Clear();
        chartGamma.Series["SeriesFeatureR"].Points.Clear();
        chartGamma.Series["SeriesControlR"].Points.Clear();
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
        for (int i = 0; i < 768; i++)
        {
            if (i >= 0 && i <= 255)
            {
                chartGamma.Series["SeriesGammaR"].Points.AddXY(i, array[i]);
            }
        }
        m_ptFeatureR.Clear();
        m_ptFeatureR.Add(new Point(0, (int)chartGamma.Series["SeriesGammaR"].Points[0].YValues[0]));
        m_ptFeatureR.Add(new Point(255, (int)chartGamma.Series["SeriesGammaR"].Points[255].YValues[0]));
        chartGamma.Series["SeriesFeatureR"].Points.AddXY(0.0, chartGamma.Series["SeriesGammaR"].Points[0].YValues[0]);
        chartGamma.Series["SeriesFeatureR"].Points.AddXY(255.0, chartGamma.Series["SeriesGammaR"].Points[255].YValues[0]);
        UpdateLineControl();
        SaveGammaValue_temp();
        streamReader.Close();
    }

    public void UpdateLineControl()
    {
        if (checkBox_R.Checked)
        {
            m_ptFeatureR.Clear();
            Series seiresPoints = chartGamma.Series["SeriesFeatureR"];
            Series seiresLines = chartGamma.Series["SeriesGammaR"];
            SeperateSection(ref seiresPoints, ref seiresLines, m_ptFeatureR);
            Series seiresRLine = chartGamma.Series["SeriesGammaR"];
            Series seiresRPoints = chartGamma.Series["SeriesFeatureR"];
            Series seiresControl = chartGamma.Series["SeriesControlR"];
            InitBezierControl(seiresRLine, seiresRPoints, seiresControl, m_ptFeatureR, m_ptFeatureRControl);
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
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
            streamWriter.WriteLine("Fcurve:1~256");
            for (int i = 0; i < 256; i++)
            {
                streamWriter.WriteLine((int)chartGamma.Series["SeriesGammaR"].Points[i].YValues[0]);
            }
        }
        stream.Close();
        MessageBox.Show("Save WDRCurve Data OK");
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

    private void numUpDown_Red_X_ValueChanged(object sender, EventArgs e)
    {
        int num = (int)numUpDown_Red_X.Value;
        int num2 = (int)chartGamma.Series["SeriesGammaR"].Points[num].YValues[0];
        try
        {
            numUpDown_Red_Y.Value = num2;
        }
        catch
        {
            if (num2 > numUpDown_Red_Y.Maximum)
            {
                numUpDown_Red_Y.Value = numUpDown_Red_Y.Maximum;
            }
            else if (num2 < numUpDown_Red_Y.Minimum)
            {
                numUpDown_Red_Y.Value = numUpDown_Red_Y.Minimum;
            }
        }
        CheckControlPoint(m_ptFeatureR, num, lbIsCtrlPtR);
    }

    private void numUpDown_Red_Y_ValueChanged(object sender, EventArgs e)
    {
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

    private void AddFeaturePointByXY(Series pCurve, Series seiresPointsfor_c, Series seiresControl, List<Point> pFeaturePoint, Point ptSelect, ref int nFeatureIdx)
    {
        nFeatureIdx = -1;
        IsFeaturePoint(pFeaturePoint, ptSelect.X, ref nFeatureIdx);
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
                InitBezierControl(pCurve, seiresPointsfor_c, seiresControl, pFeaturePoint, m_ptFeatureRControl);
            }
            else
            {
                pFeaturePoint[nFeatureIdx] = new Point(pFeaturePoint[nFeatureIdx].X, ptSelect.Y);
                CurveFittingModify(nFeatureIdx, pCurve, pFeaturePoint);
                InitBezierControl(pCurve, seiresPointsfor_c, seiresControl, pFeaturePoint, m_ptFeatureRControl);
            }
        }
    }

    private void AddFeaturePoint(Series pCurve, List<Point> pFeaturePoint, List<Point> pFeaturePoint_c, Point ptSelect, ref int nFeatureIdx, ref int nFeatureIdx_c, ref bool bFindPoint, ref bool bFindPoint_c)
    {
        int num = 5;
        int num2 = 12;
        int num3 = 5;
        int num4 = 18;
        int num5 = 0;
        bFindPoint = false;
        bFindPoint_c = false;
        for (int i = 0; i < pFeaturePoint_c.Count; i++)
        {
            if (pFeaturePoint_c[i].X > ptSelect.X - num3 && pFeaturePoint_c[i].X < ptSelect.X + num3 && pFeaturePoint_c[i].Y > ptSelect.Y - num4 && pFeaturePoint_c[i].Y < ptSelect.Y + num4)
            {
                bFindPoint_c = true;
                nFeatureIdx_c = i;
                chartGamma.Cursor = Cursors.NoMoveVert;
                break;
            }
        }
        for (int j = 0; j < pFeaturePoint.Count; j++)
        {
            if (pFeaturePoint[j].X > ptSelect.X - num3 && pFeaturePoint[j].X < ptSelect.X + num3 && pFeaturePoint[j].Y > ptSelect.Y - num4 && pFeaturePoint[j].Y < ptSelect.Y + num4)
            {
                bFindPoint = true;
                nFeatureIdx = j;
                chartGamma.Cursor = Cursors.NoMove2D;
                break;
            }
        }
        if (bFindPoint || bFindPoint_c)
        {
            return;
        }
        for (int k = 0; k < pCurve.Points.Count; k++)
        {
            if (!(pCurve.Points[k].XValue > ptSelect.X - num) || !(pCurve.Points[k].XValue < ptSelect.X + num) || !(pCurve.Points[k].YValues[0] > ptSelect.Y - num2) || !(pCurve.Points[k].YValues[0] < ptSelect.Y + num2))
            {
                continue;
            }
            num5 = k;
            pFeaturePoint.Add(new Point(num5, (int)pCurve.Points[num5].YValues[0]));
            pFeaturePoint.Sort((px, py) => px.X.CompareTo(py.X));
            for (int l = 0; l < pCurve.Points.Count; l++)
            {
                if (pFeaturePoint[l].X == num5)
                {
                    bFindPoint = true;
                    nFeatureIdx = l;
                    double choosepointfinalY = 0.0;
                    BezierCurveModify(nFeatureIdx, pCurve, pFeaturePoint, ref choosepointfinalY);
                    break;
                }
            }
            break;
        }
    }

    private void RemoveFeaturePoint(Series pCurve, Series seriesFeature, List<Point> pFeaturePoint, Point ptSelect, ref bool bFindPoint)
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
                seriesFeature.Points.Clear();
                for (int j = 0; j < pFeaturePoint.Count; j++)
                {
                    seriesFeature.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
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

    private void initialGammavalue(Series seiresCurve, int index, Series seiresPoint, long[] gammavalue, Series seiresControl, List<Point> ptFeature, List<Point> ptFeatureControl)
    {
        seiresCurve.Points.Clear();
        seiresPoint.Points.Clear();
        ptFeature.Clear();
        int num = 0;
        for (int i = 256 * index; i < 256 * (index + 1); i++)
        {
            seiresCurve.Points.AddXY(num, gammavalue[i]);
            num++;
        }
        initialAddSection(ref seiresPoint, ref seiresCurve, ptFeature);
        InitBezierControl(seiresCurve, seiresPoint, seiresControl, ptFeature, ptFeatureControl);
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
                itemR = 3;
                int index = 0;
                Series seiresCurve = chartGamma.Series["SeriesGammaR"];
                Series seiresPoint = chartGamma.Series["SeriesFeatureR"];
                Series seiresControl = chartGamma.Series["SeriesControlR"];
                initialGammavalue(seiresCurve, index, seiresPoint, gammainitialvalueR_Manual, seiresControl, m_ptFeatureR, m_ptFeatureRControl);
            }
            else
            {
                comboBoxIndex.Enabled = true;
                itemR = 2;
                int num = comboBoxIndex.SelectedIndex;
                if (num < 0)
                {
                    num = 0;
                }
                Series seiresCurve2 = chartGamma.Series["SeriesGammaR"];
                Series seiresPoint2 = chartGamma.Series["SeriesFeatureR"];
                Series seiresControl2 = chartGamma.Series["SeriesControlR"];
                initialGammavalue(seiresCurve2, num, seiresPoint2, gammainitialvalueR, seiresControl2, m_ptFeatureR, m_ptFeatureRControl);
            }
            SaveGammaValue_temp();
        }
        catch
        {
            ResetCurve("SeriesGammaR");
            Series seiresRLine = chartGamma.Series["SeriesGammaR"];
            Series seiresRPoints = chartGamma.Series["SeriesFeatureR"];
            Series seiresControl3 = chartGamma.Series["SeriesControlR"];
            InitBezierControl(seiresRLine, seiresRPoints, seiresControl3, m_ptFeatureR, m_ptFeatureRControl);
        }
    }

    private void initialGammaValue()
    {
        int num = gammaGroup.ItemList[itemR].DataValue.Length;
        gammainitialvalueR = new long[num];
        for (int i = 0; i < num; i++)
        {
            gammainitialvalueR[i] = gammaGroup.ItemList[itemR].DataValue[i];
        }
        itemR = 3;
        num = gammaGroup.ItemList[itemR].DataValue.Length;
        gammainitialvalueR_Manual = new long[num];
        for (int j = 0; j < num; j++)
        {
            gammainitialvalueR_Manual[j] = gammaGroup.ItemList[itemR].DataValue[j];
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
        ChartArea chartArea = new ChartArea();
        Legend legend = new Legend();
        Series series = new Series();
        Series series2 = new Series();
        Series series3 = new Series();
        Series series4 = new Series();
        chartGamma = new Chart();
        labelPosition = new Label();
        btnSave = new Button();
        btnLoad = new Button();
        btnRedset = new Button();
        numUpDown_Red_Y = new NumericUpDown();
        numUpDown_Red_X = new NumericUpDown();
        labelColorR = new Label();
        label9 = new Label();
        label8 = new Label();
        label5 = new Label();
        checkBox_R = new CheckBox();
        sectionlabel = new Label();
        sectiontrackBar = new TrackBar();
        btnResetRCurve = new Button();
        lbIsCtrlPtR = new Label();
        groupRCurve = new GroupBox();
        groupFileMenu = new GroupBox();
        labelOpType = new Label();
        comboBoxOpType = new ComboBox();
        checkBoxEnable = new CheckBox();
        labelIndex = new Label();
        comboBoxIndex = new ComboBox();
        groupBoxApi2 = new GroupBox();
        groupBoxControl = new GroupBox();
        ((ISupportInitialize)chartGamma).BeginInit();
        ((ISupportInitialize)numUpDown_Red_Y).BeginInit();
        ((ISupportInitialize)numUpDown_Red_X).BeginInit();
        ((ISupportInitialize)sectiontrackBar).BeginInit();
        groupRCurve.SuspendLayout();
        groupFileMenu.SuspendLayout();
        groupBoxApi2.SuspendLayout();
        groupBoxControl.SuspendLayout();
        SuspendLayout();
        chartArea.Name = "ChartArea1";
        chartGamma.ChartAreas.Add(chartArea);
        chartGamma.Cursor = Cursors.Cross;
        legend.Name = "Legend1";
        chartGamma.Legends.Add(legend);
        chartGamma.Location = new Point(25, 168);
        chartGamma.Name = "chartGamma";
        series.BorderWidth = 2;
        series.ChartArea = "ChartArea1";
        series.ChartType = SeriesChartType.Spline;
        series.Color = Color.Red;
        series.Legend = "Legend1";
        series.MarkerSize = 7;
        series.Name = "SeriesGammaR";
        series2.BorderColor = Color.FromArgb(0, 192, 0);
        series2.BorderWidth = 2;
        series2.ChartArea = "ChartArea1";
        series2.ChartType = SeriesChartType.Point;
        series2.Color = Color.Fuchsia;
        series2.Legend = "Legend1";
        series2.MarkerColor = Color.Green;
        series2.MarkerSize = 7;
        series2.MarkerStyle = MarkerStyle.Circle;
        series2.Name = "SeriesFeatureR";
        series2.YValuesPerPoint = 2;
        series3.ChartArea = "ChartArea1";
        series3.ChartType = SeriesChartType.FastPoint;
        series3.Legend = "Legend1";
        series3.MarkerColor = Color.FromArgb(255, 128, 255);
        series3.MarkerSize = 7;
        series3.MarkerStyle = MarkerStyle.Cross;
        series3.Name = "SeriesChoosePoint";
        series4.ChartArea = "ChartArea1";
        series4.ChartType = SeriesChartType.Point;
        series4.Legend = "Legend1";
        series4.MarkerColor = Color.Black;
        series4.MarkerSize = 8;
        series4.MarkerStyle = MarkerStyle.Diamond;
        series4.Name = "SeriesControlR";
        chartGamma.Series.Add(series);
        chartGamma.Series.Add(series2);
        chartGamma.Series.Add(series3);
        chartGamma.Series.Add(series4);
        chartGamma.Size = new Size(520, 374);
        chartGamma.TabIndex = 140;
        chartGamma.Text = "chart1";
        chartGamma.MouseDown += new MouseEventHandler(chartGamma_MouseDown);
        chartGamma.MouseMove += new MouseEventHandler(chartGamma_MouseMove);
        chartGamma.MouseUp += new MouseEventHandler(chartGamma_MouseUp);
        labelPosition.Location = new Point(234, 559);
        labelPosition.Name = "labelPosition";
        labelPosition.Size = new Size(101, 12);
        labelPosition.TabIndex = 134;
        labelPosition.Text = "Position:Out of chart";
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
        btnRedset.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnRedset.BackColor = Color.LightCyan;
        btnRedset.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnRedset.Location = new Point(120, 73);
        btnRedset.MinimumSize = new Size(70, 23);
        btnRedset.Name = "btnRedset";
        btnRedset.Size = new Size(70, 23);
        btnRedset.TabIndex = 127;
        btnRedset.Text = "Set";
        btnRedset.UseVisualStyleBackColor = false;
        btnRedset.Click += new EventHandler(btnRedset_Click);
        numUpDown_Red_Y.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        numUpDown_Red_Y.Location = new Point(33, 73);
        numUpDown_Red_Y.Maximum = new decimal(new int[4] { 4095, 0, 0, 0 });
        numUpDown_Red_Y.MinimumSize = new Size(60, 0);
        numUpDown_Red_Y.Name = "numUpDown_Red_Y";
        numUpDown_Red_Y.Size = new Size(60, 21);
        numUpDown_Red_Y.TabIndex = 120;
        numUpDown_Red_Y.ValueChanged += new EventHandler(numUpDown_Red_Y_ValueChanged);
        numUpDown_Red_X.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        numUpDown_Red_X.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        numUpDown_Red_X.Location = new Point(33, 45);
        numUpDown_Red_X.Maximum = new decimal(new int[4] { 255, 0, 0, 0 });
        numUpDown_Red_X.MinimumSize = new Size(30, 0);
        numUpDown_Red_X.Name = "numUpDown_Red_X";
        numUpDown_Red_X.Size = new Size(60, 21);
        numUpDown_Red_X.TabIndex = 119;
        numUpDown_Red_X.ValueChanged += new EventHandler(numUpDown_Red_X_ValueChanged);
        labelColorR.AutoSize = true;
        labelColorR.BackColor = Color.Red;
        labelColorR.Location = new Point(171, 24);
        labelColorR.Name = "labelColorR";
        labelColorR.Size = new Size(16, 15);
        labelColorR.TabIndex = 118;
        labelColorR.Text = "   ";
        label9.AutoSize = true;
        label9.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label9.Location = new Point(10, 73);
        label9.Name = "label9";
        label9.Size = new Size(17, 15);
        label9.TabIndex = 113;
        label9.Text = "Y:";
        label8.AutoSize = true;
        label8.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label8.Location = new Point(11, 49);
        label8.Name = "label8";
        label8.Size = new Size(19, 15);
        label8.TabIndex = 112;
        label8.Text = "X:";
        label5.AutoSize = true;
        label5.BackColor = Color.Red;
        label5.Font = new Font("Times New Roman", 10f);
        label5.Location = new Point(221, 74);
        label5.Name = "label5";
        label5.Size = new Size(0, 14);
        label5.TabIndex = 109;
        checkBox_R.AutoSize = true;
        checkBox_R.BackColor = SystemColors.Window;
        checkBox_R.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        checkBox_R.Location = new Point(13, 21);
        checkBox_R.Name = "checkBox_R";
        checkBox_R.Size = new Size(62, 19);
        checkBox_R.TabIndex = 107;
        checkBox_R.Text = "FCurve";
        checkBox_R.UseVisualStyleBackColor = false;
        checkBox_R.CheckedChanged += new EventHandler(checkBox_R_CheckedChanged);
        sectionlabel.AutoSize = true;
        sectionlabel.BackColor = SystemColors.Window;
        sectionlabel.Font = new Font("Cambria", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        sectionlabel.Location = new Point(84, 112);
        sectionlabel.Name = "sectionlabel";
        sectionlabel.Size = new Size(61, 14);
        sectionlabel.TabIndex = 139;
        sectionlabel.Text = "16 Section";
        sectiontrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        sectiontrackBar.BackColor = SystemColors.Window;
        sectiontrackBar.LargeChange = 1;
        sectiontrackBar.Location = new Point(20, 64);
        sectiontrackBar.Maximum = 8;
        sectiontrackBar.Name = "sectiontrackBar";
        sectiontrackBar.Size = new Size(192, 45);
        sectiontrackBar.TabIndex = 138;
        sectiontrackBar.TickStyle = TickStyle.None;
        sectiontrackBar.Value = 4;
        sectiontrackBar.Scroll += new EventHandler(sectiontrackBar_Scroll);
        btnResetRCurve.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnResetRCurve.BackColor = Color.LightCyan;
        btnResetRCurve.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnResetRCurve.Location = new Point(69, 102);
        btnResetRCurve.MinimumSize = new Size(70, 23);
        btnResetRCurve.Name = "btnResetRCurve";
        btnResetRCurve.Size = new Size(70, 23);
        btnResetRCurve.TabIndex = 147;
        btnResetRCurve.Text = "Reset";
        btnResetRCurve.UseVisualStyleBackColor = false;
        btnResetRCurve.Click += new EventHandler(btnResetRCurve_Click);
        lbIsCtrlPtR.AutoSize = true;
        lbIsCtrlPtR.Location = new Point(121, 49);
        lbIsCtrlPtR.Name = "lbIsCtrlPtR";
        lbIsCtrlPtR.Size = new Size(63, 15);
        lbIsCtrlPtR.TabIndex = 150;
        lbIsCtrlPtR.Text = "Ctrl Pt: Yes";
        groupRCurve.Controls.Add(numUpDown_Red_X);
        groupRCurve.Controls.Add(lbIsCtrlPtR);
        groupRCurve.Controls.Add(label8);
        groupRCurve.Controls.Add(btnResetRCurve);
        groupRCurve.Controls.Add(numUpDown_Red_Y);
        groupRCurve.Controls.Add(label9);
        groupRCurve.Controls.Add(checkBox_R);
        groupRCurve.Controls.Add(labelColorR);
        groupRCurve.Controls.Add(btnRedset);
        groupRCurve.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupRCurve.Location = new Point(12, 12);
        groupRCurve.Name = "groupRCurve";
        groupRCurve.Size = new Size(209, 131);
        groupRCurve.TabIndex = 151;
        groupRCurve.TabStop = false;
        groupRCurve.Text = "FCurve";
        groupFileMenu.Controls.Add(btnLoad);
        groupFileMenu.Controls.Add(btnSave);
        groupFileMenu.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupFileMenu.Location = new Point(614, 12);
        groupFileMenu.Name = "groupFileMenu";
        groupFileMenu.Size = new Size(187, 131);
        groupFileMenu.TabIndex = 154;
        groupFileMenu.TabStop = false;
        groupFileMenu.Text = "File";
        labelOpType.AutoSize = true;
        labelOpType.Location = new Point(95, 40);
        labelOpType.Name = "labelOpType";
        labelOpType.Size = new Size(55, 15);
        labelOpType.TabIndex = 138;
        labelOpType.Text = "OpType :";
        comboBoxOpType.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxOpType.FormattingEnabled = true;
        comboBoxOpType.Items.AddRange(new object[2] { "Auto", "Manual" });
        comboBoxOpType.Location = new Point(160, 37);
        comboBoxOpType.Name = "comboBoxOpType";
        comboBoxOpType.Size = new Size(63, 23);
        comboBoxOpType.TabIndex = 137;
        comboBoxOpType.SelectedIndexChanged += new EventHandler(comboBoxOpType_SelectedIndexChanged);
        checkBoxEnable.AutoSize = true;
        checkBoxEnable.Location = new Point(20, 39);
        checkBoxEnable.Name = "checkBoxEnable";
        checkBoxEnable.Size = new Size(58, 19);
        checkBoxEnable.TabIndex = 136;
        checkBoxEnable.Text = "Enable";
        checkBoxEnable.UseVisualStyleBackColor = true;
        checkBoxEnable.CheckedChanged += new EventHandler(checkBoxEnable_CheckedChanged);
        labelIndex.AutoSize = true;
        labelIndex.Location = new Point(17, 91);
        labelIndex.Name = "labelIndex";
        labelIndex.Size = new Size(70, 15);
        labelIndex.TabIndex = 135;
        labelIndex.Text = "Select Index :";
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
        groupBoxApi2.Controls.Add(labelOpType);
        groupBoxApi2.Controls.Add(comboBoxIndex);
        groupBoxApi2.Controls.Add(comboBoxOpType);
        groupBoxApi2.Controls.Add(labelIndex);
        groupBoxApi2.Controls.Add(checkBoxEnable);
        groupBoxApi2.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupBoxApi2.Location = new Point(569, 339);
        groupBoxApi2.Name = "groupBoxApi2";
        groupBoxApi2.Size = new Size(232, 141);
        groupBoxApi2.TabIndex = 155;
        groupBoxApi2.TabStop = false;
        groupBoxApi2.Text = "Option";
        groupBoxControl.Controls.Add(sectiontrackBar);
        groupBoxControl.Controls.Add(sectionlabel);
        groupBoxControl.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupBoxControl.Location = new Point(569, 168);
        groupBoxControl.Name = "groupBoxControl";
        groupBoxControl.Size = new Size(232, 140);
        groupBoxControl.TabIndex = 156;
        groupBoxControl.TabStop = false;
        groupBoxControl.Text = "Control";
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.Window;
        Controls.Add(groupBoxControl);
        Controls.Add(groupBoxApi2);
        Controls.Add(groupFileMenu);
        Controls.Add(groupRCurve);
        Controls.Add(chartGamma);
        Controls.Add(labelPosition);
        Controls.Add(label5);
        ForeColor = SystemColors.ControlText;
        Name = "WDRCurve";
        Size = new Size(1024, 720);
        Load += new EventHandler(RGBGamma_Load);
        ((ISupportInitialize)chartGamma).EndInit();
        ((ISupportInitialize)numUpDown_Red_Y).EndInit();
        ((ISupportInitialize)numUpDown_Red_X).EndInit();
        ((ISupportInitialize)sectiontrackBar).EndInit();
        groupRCurve.ResumeLayout(false);
        groupRCurve.PerformLayout();
        groupFileMenu.ResumeLayout(false);
        groupBoxApi2.ResumeLayout(false);
        groupBoxApi2.PerformLayout();
        groupBoxControl.ResumeLayout(false);
        groupBoxControl.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
