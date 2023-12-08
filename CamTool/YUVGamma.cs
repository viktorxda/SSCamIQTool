using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSCamIQTool.CamTool;

public class YUVGamma : UserControl
{
    public bool m_chartSelectY;

    public bool m_chartSelectU;

    public bool m_chartSelectV;

    private int unknowN;

    private int m_nFeatureIdxY;

    private int m_nFeatureIdxU;

    private int m_nFeatureIdxV;

    private readonly int m_nYLength = 256;

    private readonly int m_nYMax = 1024;

    private readonly int m_nUVLength = 128;

    private readonly int m_nUVMax = 512;

    private readonly List<Point> m_ptFeatureY = new();

    private readonly List<Point> m_ptFeatureU = new();

    private readonly List<Point> m_ptFeatureV = new();

    private readonly string m_strCtrlPtYes = "Ctrl Pt: Yes";

    private readonly string m_strCtrlPtNo = "Ctrl Pt: No";

    private readonly bool autoMode;

    private readonly GuiGroup gammaGroup;

    private readonly GuiGroup gammaGroup_temp;

    private readonly GuiItem enableItem;

    private readonly GuiItem opTypeItem;

    private int itemR;

    private readonly IContainer components = null;

    private CheckBox checkBox_V;

    private Chart chartY;

    private Chart chartUV;

    private Label labelPosition;

    private Button btnSave;

    private Button btnLoad;

    private Button btnVset;

    private Button btnUset;

    private Button btnYset;

    private NumericUpDown numUpDown_V_Y;

    private NumericUpDown numUpDown_V_X;

    private NumericUpDown numUpDown_U_Y;

    private NumericUpDown numUpDown_U_X;

    private NumericUpDown numUpDown_Y_Y;

    private NumericUpDown numUpDown_Y_X;

    private Label labelColorY;

    private Label label13;

    private Label label12;

    private Label label11;

    private Label label10;

    private Label label9;

    private Label label8;

    private Label labelColorV;

    private Label labelColorU;

    private Label label5;

    private CheckBox checkBox_U;

    private CheckBox checkBox_Y;

    private Label sectionlabel;

    private TrackBar sectiontrackBar;

    private Button btnResetCurveY;

    private Button btnResetCurveU;

    private Button btnResetCurveV;

    private Label lbIsCtrlPtY;

    private GroupBox groupCurveY;

    private GroupBox groupCurveU;

    private Label lbIsCtrlPtU;

    private GroupBox groupCurveV;

    private Label lbIsCtrlPtV;

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
            itemR = 5;
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
            groupCurveY.Enabled = false;
            groupCurveU.Enabled = false;
            groupCurveV.Enabled = false;
            chartY.Enabled = false;
            chartUV.Enabled = false;
            groupBoxControl.Enabled = false;
        }
        else
        {
            groupCurveY.Enabled = true;
            groupCurveU.Enabled = true;
            groupCurveV.Enabled = true;
            chartY.Enabled = true;
            chartUV.Enabled = true;
            groupBoxControl.Enabled = true;
        }
    }

    public void UpdatePage()
    {
        int num = 16;
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
            for (int i = 0; i < num * m_nYLength; i++)
            {
                gammaGroup_temp.ItemList[itemR].DataValue[i] = gammaGroup.ItemList[itemR].DataValue[i];
            }
            for (int j = 0; j < num * m_nUVLength; j++)
            {
                gammaGroup_temp.ItemList[itemR + 1].DataValue[j] = gammaGroup.ItemList[itemR + 1].DataValue[j];
                gammaGroup_temp.ItemList[itemR + 2].DataValue[j] = gammaGroup.ItemList[itemR + 2].DataValue[j];
            }
        }
        else
        {
            UpdateGammaValue(0);
            for (int k = 0; k < m_nYLength; k++)
            {
                gammaGroup_temp.ItemList[itemR].DataValue[k] = gammaGroup.ItemList[itemR].DataValue[k];
            }
            for (int l = 0; l < m_nUVLength; l++)
            {
                gammaGroup_temp.ItemList[itemR + 1].DataValue[l] = gammaGroup.ItemList[itemR + 1].DataValue[l];
                gammaGroup_temp.ItemList[itemR + 2].DataValue[l] = gammaGroup.ItemList[itemR + 2].DataValue[l];
            }
        }
    }

    public void UpdateGammaValue(int index)
    {
        if (OpMode == 0)
        {
            setYGammaArray(gammaGroup.ItemList[itemR].DataValue, index);
            setUGammaArray(gammaGroup.ItemList[itemR + 1].DataValue, index);
            setVGammaArray(gammaGroup.ItemList[itemR + 2].DataValue, index);
        }
        else
        {
            setYGammaArray(gammaGroup.ItemList[itemR].DataValue, index);
            setUGammaArray(gammaGroup.ItemList[itemR + 1].DataValue, index);
            setVGammaArray(gammaGroup.ItemList[itemR + 2].DataValue, index);
        }
    }

    public void SaveGammaValue()
    {
        int num = 16;
        Index_Old = Index;
        if (OpMode == 0 && GammaEnable == 1)
        {
            for (int i = 0; i < num * m_nYLength; i++)
            {
                gammaGroup.ItemList[itemR].DataValue[i] = gammaGroup_temp.ItemList[itemR].DataValue[i];
            }
            for (int j = 0; j < num * m_nUVLength; j++)
            {
                gammaGroup.ItemList[itemR + 1].DataValue[j] = gammaGroup_temp.ItemList[itemR + 1].DataValue[j];
                gammaGroup.ItemList[itemR + 2].DataValue[j] = gammaGroup_temp.ItemList[itemR + 2].DataValue[j];
            }
        }
        else if (OpMode == 1 && GammaEnable == 1)
        {
            for (int k = 0; k < m_nYLength; k++)
            {
                gammaGroup.ItemList[itemR].DataValue[k] = gammaGroup_temp.ItemList[itemR].DataValue[k];
            }
            for (int l = 0; l < m_nUVLength; l++)
            {
                gammaGroup.ItemList[itemR + 1].DataValue[l] = gammaGroup_temp.ItemList[itemR + 1].DataValue[l];
                gammaGroup.ItemList[itemR + 2].DataValue[l] = gammaGroup_temp.ItemList[itemR + 2].DataValue[l];
            }
        }
    }

    public void SaveGammaValue_temp()
    {
        getYGammaArray(out long[] pGammaY);
        getUGammaArray(out long[] pGammaU);
        getVGammaArray(out long[] pGammaV);
        if (OpMode == 0 && GammaEnable == 1)
        {
            pGammaY.CopyTo(gammaGroup_temp.ItemList[itemR].DataValue, Index * pGammaY.Length);
            pGammaU.CopyTo(gammaGroup_temp.ItemList[itemR + 1].DataValue, Index * pGammaU.Length);
            pGammaV.CopyTo(gammaGroup_temp.ItemList[itemR + 2].DataValue, Index * pGammaV.Length);
        }
        else if (OpMode == 1 && GammaEnable == 1)
        {
            gammaGroup_temp.ItemList[itemR].DataValue = pGammaY;
            gammaGroup_temp.ItemList[itemR + 1].DataValue = pGammaU;
            gammaGroup_temp.ItemList[itemR + 2].DataValue = pGammaV;
        }
    }

    public YUVGamma(GuiGroup group)
    {
        InitializeComponent();
        gammaGroup = group;
        gammaGroup_temp = group;
        InitChart();
        autoMode = group.autoMode.Length > 1;
        OpMode = autoMode ? 0 : 1;
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
        UpdatePage();
        checkBoxEnable_CheckedChanged(new CheckBox(), new EventArgs());
    }

    private void InitChart()
    {
        GetGammaWithOB(m_nYMax, m_nYMax, 0m, m_nYLength, out Point[] pGammaArray);
        GetGammaWithOB(m_nUVMax, m_nUVMax, 0m, m_nUVLength, out Point[] pGammaArray2);
        chartY.ChartAreas[0].AxisX.Interval = m_nYLength / 16;
        chartY.ChartAreas[0].AxisX.Minimum = 0.0;
        chartY.ChartAreas[0].AxisX.LabelStyle.Format = "D2";
        chartY.ChartAreas[0].AxisY.Maximum = m_nYMax;
        chartY.ChartAreas[0].AxisY.Minimum = -64.0;
        chartY.ChartAreas[0].AxisY.Interval = m_nYMax / 16;
        chartY.Legends[0].Enabled = false;
        chartUV.ChartAreas[0].AxisX.Interval = m_nUVLength / 8;
        chartUV.ChartAreas[0].AxisX.Minimum = -m_nUVLength;
        chartUV.ChartAreas[0].AxisX.LabelStyle.Format = "D2";
        chartUV.ChartAreas[0].AxisY.Maximum = m_nUVMax;
        chartUV.ChartAreas[0].AxisY.Minimum = -m_nUVMax;
        chartUV.ChartAreas[0].AxisY.Interval = m_nUVMax / 8;
        chartUV.Legends[0].Enabled = false;
        if (pGammaArray != null)
        {
            chartY.Series[0].Points.Clear();
            for (int i = 0; i < pGammaArray.Length; i++)
            {
                _ = chartY.Series["SeriesGammaY"].Points.AddXY(i, pGammaArray[i].Y);
            }
            Series seiresPoints = chartY.Series["SeriesFeatureY"];
            Series seiresLines = chartY.Series["SeriesGammaY"];
            initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureY);
        }
        if (pGammaArray2 != null)
        {
            chartUV.Series[0].Points.Clear();
            for (int j = 0; j < pGammaArray2.Length; j++)
            {
                _ = chartUV.Series["SeriesGammaU"].Points.AddXY(j, pGammaArray2[j].Y);
                chartUV.Series["SeriesGammaU"].Points[j].Color = Color.FromArgb(0, 0, j * 2);
                _ = chartUV.Series["SeriesGammaUr"].Points.AddXY(-j, -pGammaArray2[j].Y);
                chartUV.Series["SeriesGammaUr"].Points[j].Color = Color.FromArgb(2 * j, 2 * j, 0);
                _ = chartUV.Series["SeriesGammaV"].Points.AddXY(j, pGammaArray2[j].Y);
                chartUV.Series["SeriesGammaV"].Points[j].Color = Color.FromArgb(2 * j, 0, 2 * j);
                _ = chartUV.Series["SeriesGammaVr"].Points.AddXY(-j, -pGammaArray2[j].Y);
                chartUV.Series["SeriesGammaVr"].Points[j].Color = Color.FromArgb(0, 2 * j, 0);
            }
            Series seiresPoints2 = chartUV.Series["SeriesFeatureU"];
            Series seiresLines2 = chartUV.Series["SeriesGammaU"];
            Series seiresPoints3 = chartUV.Series["SeriesFeatureV"];
            Series seiresLines3 = chartUV.Series["SeriesGammaV"];
            initialAddSection(ref seiresPoints2, ref seiresLines2, m_ptFeatureU);
            initialAddSection(ref seiresPoints3, ref seiresLines3, m_ptFeatureV);
        }
    }

    private void initialAddSection(ref Series seiresPoints, ref Series seiresLines, List<Point> pFeaturePoint)
    {
        int count = seiresLines.Points.Count;
        for (int i = 0; i < count; i += count / 16)
        {
            pFeaturePoint.Add(new Point(i, (int)seiresLines.Points[i].YValues[0]));
        }
        pFeaturePoint.Add(new Point(count - 1, (int)seiresLines.Points[count - 1].YValues[0]));
        seiresPoints.Points.Clear();
        for (int j = 0; j < pFeaturePoint.Count; j++)
        {
            _ = seiresPoints.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
        }
    }

    private void RGBGamma_Load(object sender, EventArgs e)
    {
    }

    private void checkBox_Y_CheckedChanged(object sender, EventArgs e)
    {
        chartY.Series["SeriesGammaY"].Enabled = checkBox_Y.Checked;
        chartY.Series["SeriesFeatureY"].Enabled = checkBox_Y.Checked;
        numUpDown_Y_X.Enabled = checkBox_Y.Checked;
        numUpDown_Y_Y.Enabled = checkBox_Y.Checked;
        btnYset.Enabled = checkBox_Y.Checked;
        btnResetCurveY.Enabled = checkBox_Y.Checked;
    }

    private void checkBox_U_CheckedChanged(object sender, EventArgs e)
    {
        chartUV.Series["SeriesGammaU"].Enabled = checkBox_U.Checked;
        chartUV.Series["SeriesFeatureU"].Enabled = checkBox_U.Checked;
        numUpDown_U_X.Enabled = checkBox_U.Checked;
        numUpDown_U_Y.Enabled = checkBox_U.Checked;
        btnUset.Enabled = checkBox_U.Checked;
        btnResetCurveU.Enabled = checkBox_U.Checked;
    }

    private void checkBox_V_CheckedChanged(object sender, EventArgs e)
    {
        chartUV.Series["SeriesGammaV"].Enabled = checkBox_V.Checked;
        chartUV.Series["SeriesFeatureV"].Enabled = checkBox_V.Checked;
        numUpDown_V_X.Enabled = checkBox_V.Checked;
        numUpDown_V_Y.Enabled = checkBox_V.Checked;
        btnVset.Enabled = checkBox_V.Checked;
        btnResetCurveV.Enabled = checkBox_V.Checked;
    }

    private void GetGammaWithOB(decimal nSourceMax, decimal nOutputMax, decimal nObValue, int length, out Point[] pGammaArray)
    {
        decimal[] array = new decimal[length];
        pGammaArray = new Point[length];
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
            if (seiresCurve.Points[m].YValues[0] > 1023.0)
            {
                seiresCurve.Points[m].YValues[0] = 1023.0;
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

    private void ReversSeires(Series series)
    {
        Series series2 = chartUV.Series[series.Name + "r"];
        for (int i = 0; i < series.Points.Count; i++)
        {
            series2.Points[i].YValues[0] = 0.0 - series.Points[i].YValues[0];
        }
    }

    private void chartY_MouseMove(object sender, MouseEventArgs e)
    {
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartY, e.X, e.Y);
        if (m_chartSelectY && axisValuesFromMouse != null)
        {
            Series seiresCurve = chartY.Series["SeriesGammaY"];
            m_ptFeatureY[m_nFeatureIdxY] = new Point(m_ptFeatureY[m_nFeatureIdxY].X, (int)axisValuesFromMouse.Item2);
            CurveFittingModify(m_nFeatureIdxY, seiresCurve, m_ptFeatureY);
            chartY.Series["SeriesFeatureY"].Points.Clear();
            for (int i = 0; i < m_ptFeatureY.Count; i++)
            {
                _ = chartY.Series["SeriesFeatureY"].Points.AddXY(m_ptFeatureY[i].X, m_ptFeatureY[i].Y);
            }
        }
    }

    private void chartUV_MouseMove(object sender, MouseEventArgs e)
    {
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartUV, e.X, e.Y);
        if (m_chartSelectU && axisValuesFromMouse != null)
        {
            Series series = chartUV.Series["SeriesGammaU"];
            m_ptFeatureU[m_nFeatureIdxU] = new Point(m_ptFeatureU[m_nFeatureIdxU].X, (int)axisValuesFromMouse.Item2);
            CurveFittingModify(m_nFeatureIdxU, series, m_ptFeatureU);
            ReversSeires(series);
            chartUV.Series["SeriesFeatureU"].Points.Clear();
            for (int i = 0; i < m_ptFeatureU.Count; i++)
            {
                _ = chartUV.Series["SeriesFeatureU"].Points.AddXY(m_ptFeatureU[i].X, m_ptFeatureU[i].Y);
            }
        }
        if (m_chartSelectV && axisValuesFromMouse != null)
        {
            Series series2 = chartUV.Series["SeriesGammaV"];
            m_ptFeatureV[m_nFeatureIdxV] = new Point(m_ptFeatureV[m_nFeatureIdxV].X, (int)axisValuesFromMouse.Item2);
            CurveFittingModify(m_nFeatureIdxV, series2, m_ptFeatureV);
            ReversSeires(series2);
            chartUV.Series["SeriesFeatureV"].Points.Clear();
            for (int j = 0; j < m_ptFeatureV.Count; j++)
            {
                _ = chartUV.Series["SeriesFeatureV"].Points.AddXY(m_ptFeatureV[j].X, m_ptFeatureV[j].Y);
            }
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

    private void chartY_MouseDown(object sender, MouseEventArgs e)
    {
        if (RestoreView(sender, e) == 1)
        {
            return;
        }
        bool bFindPoint = false;
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartY, e.X, e.Y);
        if (axisValuesFromMouse == null)
        {
            return;
        }
        Point ptSelect = new((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
        if (e.Button == MouseButtons.Left)
        {
            if (checkBox_U.Checked)
            {
                Series pCurve = chartY.Series["SeriesGammaY"];
                AddFeaturePoint(pCurve, m_ptFeatureY, ptSelect, ref m_nFeatureIdxY, ref bFindPoint);
                if (bFindPoint)
                {
                    m_chartSelectY = true;
                }
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            axisValuesFromMouse = GetAxisValuesFromMouse(ref chartY, e.X, e.Y);
            if (checkBox_Y.Checked)
            {
                ptSelect = new Point((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
                Series pCurve = chartY.Series["SeriesGammaY"];
                Series seriesFeature = chartY.Series["SeriesFeatureY"];
                RemoveFeaturePoint(pCurve, seriesFeature, m_ptFeatureY, ptSelect, ref bFindPoint);
            }
        }
    }

    private void chartUV_MouseDown(object sender, MouseEventArgs e)
    {
        if (RestoreView(sender, e) == 1)
        {
            return;
        }
        bool bFindPoint = false;
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartUV, e.X, e.Y);
        if (axisValuesFromMouse == null)
        {
            return;
        }
        Point ptSelect = new((int)axisValuesFromMouse.Item1, (int)axisValuesFromMouse.Item2);
        if (e.Button == MouseButtons.Left)
        {
            if (checkBox_U.Checked)
            {
                Series pCurve = chartUV.Series["SeriesGammaU"];
                AddFeaturePoint(pCurve, m_ptFeatureU, ptSelect, ref m_nFeatureIdxU, ref bFindPoint);
                if (bFindPoint)
                {
                    m_chartSelectU = true;
                }
            }
            if (checkBox_V.Checked && !bFindPoint)
            {
                Series pCurve = chartUV.Series["SeriesGammaV"];
                AddFeaturePoint(pCurve, m_ptFeatureV, ptSelect, ref m_nFeatureIdxV, ref bFindPoint);
                if (bFindPoint)
                {
                    m_chartSelectV = true;
                }
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            _ = GetAxisValuesFromMouse(ref chartY, e.X, e.Y);
            if (checkBox_U.Checked)
            {
                Series pCurve = chartUV.Series["SeriesGammaU"];
                Series seriesFeature = chartUV.Series["SeriesFeatureU"];
                RemoveFeaturePoint(pCurve, seriesFeature, m_ptFeatureU, ptSelect, ref bFindPoint);
            }
            if (checkBox_V.Checked)
            {
                Series pCurve = chartUV.Series["SeriesGammaV"];
                Series seriesFeature = chartUV.Series["SeriesFeatureV"];
                RemoveFeaturePoint(pCurve, seriesFeature, m_ptFeatureV, ptSelect, ref bFindPoint);
            }
        }
    }

    private void chartY_MouseUp(object sender, MouseEventArgs e)
    {
        m_chartSelectY = false;
        chartY.Cursor = Cursors.Cross;
        SaveGammaValue_temp();
    }

    private void chartUV_MouseUp(object sender, MouseEventArgs e)
    {
        m_chartSelectU = false;
        m_chartSelectV = false;
        chartUV.Cursor = Cursors.Cross;
        SaveGammaValue_temp();
    }

    public void SeperateSection(ref Series seiresPoints, ref Series seiresLines, List<Point> pFeaturePoint)
    {
        int num = 1;
        int count = seiresLines.Points.Count;
        seiresPoints.Points.Clear();
        for (int i = 0; i < sectiontrackBar.Value; i++)
        {
            num *= 2;
        }
        for (int j = 0; j < count; j += count / num)
        {
            pFeaturePoint.Add(new Point(j, (int)seiresLines.Points[j].YValues[0]));
        }
        pFeaturePoint.Add(new Point(count - 1, (int)seiresLines.Points[count - 1].YValues[0]));
        for (int k = 0; k < pFeaturePoint.Count; k++)
        {
            _ = seiresPoints.Points.AddXY(pFeaturePoint[k].X, pFeaturePoint[k].Y);
        }
        sectionlabel.Text = num + " Section";
    }

    public void SeperateSectionUV(ref Series seiresPoints, ref Series seiresLines, List<Point> pFeaturePoint)
    {
        int num = 1;
        int count = seiresLines.Points.Count;
        seiresPoints.Points.Clear();
        for (int i = 0; i < sectiontrackBar.Value; i++)
        {
            num *= 2;
        }
        sectionlabel.Text = num + " Section";
        if (num == 256)
        {
            num = 128;
        }
        for (int j = 0; j < count; j += count / num)
        {
            pFeaturePoint.Add(new Point(j, (int)seiresLines.Points[j].YValues[0]));
        }
        pFeaturePoint.Add(new Point(count - 1, (int)seiresLines.Points[count - 1].YValues[0]));
        for (int k = 0; k < pFeaturePoint.Count; k++)
        {
            _ = seiresPoints.Points.AddXY(pFeaturePoint[k].X, pFeaturePoint[k].Y);
        }
    }

    private void sectiontrackBar_Scroll(object sender, EventArgs e)
    {
        if (checkBox_Y.Checked)
        {
            m_ptFeatureY.Clear();
            Series seiresPoints = chartY.Series["SeriesFeatureY"];
            Series seiresLines = chartY.Series["SeriesGammaY"];
            SeperateSection(ref seiresPoints, ref seiresLines, m_ptFeatureY);
        }
        if (checkBox_U.Checked)
        {
            m_ptFeatureU.Clear();
            Series seiresPoints2 = chartUV.Series["SeriesFeatureU"];
            Series seiresLines2 = chartUV.Series["SeriesGammaU"];
            SeperateSectionUV(ref seiresPoints2, ref seiresLines2, m_ptFeatureU);
        }
        if (checkBox_V.Checked)
        {
            m_ptFeatureV.Clear();
            Series seiresPoints3 = chartUV.Series["SeriesFeatureV"];
            Series seiresLines3 = chartUV.Series["SeriesGammaV"];
            SeperateSectionUV(ref seiresPoints3, ref seiresLines3, m_ptFeatureV);
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
        (numbers[j], numbers[i]) = (numbers[i], numbers[j]);
    }

    private void btnYset_Click(object sender, EventArgs e)
    {
        Series pCurve = chartY.Series["SeriesGammaY"];
        Point ptSelect = new((int)numUpDown_Y_X.Value, (int)numUpDown_Y_Y.Value);
        AddFeaturePointByXY(pCurve, m_ptFeatureY, ptSelect, ref m_nFeatureIdxY);
        chartY.Series["SeriesFeatureY"].Points.Clear();
        for (int i = 0; i < m_ptFeatureY.Count; i++)
        {
            _ = chartY.Series["SeriesFeatureY"].Points.AddXY(m_ptFeatureY[i].X, m_ptFeatureY[i].Y);
        }
        CheckControlPoint(m_ptFeatureY, ptSelect.X, lbIsCtrlPtY);
    }

    private void btnUset_Click(object sender, EventArgs e)
    {
        Series pCurve = chartUV.Series["SeriesGammaU"];
        Point ptSelect = new((int)numUpDown_U_X.Value, (int)numUpDown_U_Y.Value);
        AddFeaturePointByXY(pCurve, m_ptFeatureU, ptSelect, ref m_nFeatureIdxU);
        chartUV.Series["SeriesFeatureU"].Points.Clear();
        for (int i = 0; i < m_ptFeatureU.Count; i++)
        {
            _ = chartUV.Series["SeriesFeatureU"].Points.AddXY(m_ptFeatureU[i].X, m_ptFeatureU[i].Y);
        }
        CheckControlPoint(m_ptFeatureU, ptSelect.X, lbIsCtrlPtU);
    }

    private void btnVset_Click(object sender, EventArgs e)
    {
        Series pCurve = chartUV.Series["SeriesGammaV"];
        Point ptSelect = new((int)numUpDown_V_X.Value, (int)numUpDown_V_Y.Value);
        AddFeaturePointByXY(pCurve, m_ptFeatureV, ptSelect, ref m_nFeatureIdxV);
        chartUV.Series["SeriesFeatureV"].Points.Clear();
        for (int i = 0; i < m_ptFeatureV.Count; i++)
        {
            _ = chartUV.Series["SeriesFeatureV"].Points.AddXY(m_ptFeatureV[i].X, m_ptFeatureV[i].Y);
        }
        CheckControlPoint(m_ptFeatureV, ptSelect.X, lbIsCtrlPtV);
    }

    public void getYGammaArray(out long[] pGammaY)
    {
        pGammaY = new long[m_nYLength];
        for (int i = 0; i < pGammaY.Length; i++)
        {
            pGammaY[i] = (int)chartY.Series["SeriesGammaY"].Points[i].YValues[0];
        }
    }

    public void getUGammaArray(out long[] pGammaU)
    {
        pGammaU = new long[m_nUVLength];
        for (int i = 0; i < pGammaU.Length; i++)
        {
            pGammaU[i] = (int)chartUV.Series["SeriesGammaU"].Points[i].YValues[0];
        }
    }

    public void getVGammaArray(out long[] pGammaV)
    {
        pGammaV = new long[m_nUVLength];
        for (int i = 0; i < pGammaV.Length; i++)
        {
            pGammaV[i] = (int)chartUV.Series["SeriesGammaV"].Points[i].YValues[0];
        }
    }

    public void setYGammaArray(long[] pGamma, int index)
    {
        checkBox_Y.Checked = true;
        chartY.Series["SeriesFeatureY"].Points.Clear();
        m_ptFeatureY.Clear();
        int num = 0;
        for (int i = m_nYLength * index; i < m_nYLength * (index + 1); i++)
        {
            chartY.Series["SeriesGammaY"].Points[num].YValues[0] = pGamma[i];
            num++;
        }
        Series seiresPoints = chartY.Series["SeriesFeatureY"];
        Series seiresLines = chartY.Series["SeriesGammaY"];
        initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureY);
    }

    public void setUGammaArray(long[] pGamma, int index)
    {
        checkBox_U.Checked = true;
        chartUV.Series["SeriesFeatureU"].Points.Clear();
        m_ptFeatureU.Clear();
        int num = 0;
        for (int i = m_nUVLength * index; i < m_nUVLength * (index + 1); i++)
        {
            chartUV.Series["SeriesGammaU"].Points[num].YValues[0] = pGamma[i];
            num++;
        }
        ReversSeires(chartUV.Series["SeriesGammaU"]);
        Series seiresPoints = chartUV.Series["SeriesFeatureU"];
        Series seiresLines = chartUV.Series["SeriesGammaU"];
        initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureU);
    }

    public void setVGammaArray(long[] pGamma, int index)
    {
        checkBox_V.Checked = true;
        chartUV.Series["SeriesFeatureV"].Points.Clear();
        m_ptFeatureV.Clear();
        int num = 0;
        for (int i = m_nUVLength * index; i < m_nUVLength * (index + 1); i++)
        {
            chartUV.Series["SeriesGammaV"].Points[num].YValues[0] = pGamma[i];
            num++;
        }
        ReversSeires(chartUV.Series["SeriesGammaV"]);
        Series seiresPoints = chartUV.Series["SeriesFeatureV"];
        Series seiresLines = chartUV.Series["SeriesGammaV"];
        initialAddSection(ref seiresPoints, ref seiresLines, m_ptFeatureV);
    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
        int[] array = new int[769];
        chartY.Series["SeriesFeatureY"].Points.Clear();
        chartUV.Series["SeriesFeatureU"].Points.Clear();
        chartUV.Series["SeriesFeatureV"].Points.Clear();
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
        StreamReader streamReader = File.OpenText(openFileDialog.FileName);
        _ = streamReader.ReadLine();
        int num = 0;
        while (!streamReader.EndOfStream)
        {
            string text = streamReader.ReadLine();
            array[num] = Convert.ToInt16(text);
            num++;
        }
        for (int i = 0; i < m_nYLength + (m_nUVLength * 2); i++)
        {
            if (i >= 0 && i < m_nYLength)
            {
                chartY.Series["SeriesGammaY"].Points[i].YValues[0] = array[i];
            }
            else if (i >= m_nYLength && i < m_nYLength + m_nUVLength)
            {
                chartUV.Series["SeriesGammaU"].Points[i - m_nYLength].YValues[0] = array[i];
            }
            else if (i >= m_nYLength + m_nUVLength && i < m_nYLength + (m_nUVLength * 2))
            {
                chartUV.Series["SeriesGammaV"].Points[i - m_nYLength - m_nUVLength].YValues[0] = array[i];
            }
        }
        ReversSeires(chartUV.Series["SeriesGammaU"]);
        ReversSeires(chartUV.Series["SeriesGammaV"]);
        m_ptFeatureY.Clear();
        m_ptFeatureY.Add(new Point(0, (int)chartY.Series["SeriesGammaY"].Points[0].YValues[0]));
        m_ptFeatureY.Add(new Point(m_nYLength - 1, (int)chartY.Series["SeriesGammaY"].Points[m_nYLength - 1].YValues[0]));
        _ = chartY.Series["SeriesFeatureY"].Points.AddXY(0.0, chartY.Series["SeriesGammaY"].Points[0].YValues[0]);
        _ = chartY.Series["SeriesFeatureY"].Points.AddXY(m_nYLength - 1, chartY.Series["SeriesGammaY"].Points[m_nYLength - 1].YValues[0]);
        m_ptFeatureU.Clear();
        m_ptFeatureU.Add(new Point(0, (int)chartUV.Series["SeriesGammaU"].Points[0].YValues[0]));
        m_ptFeatureU.Add(new Point(m_nUVLength - 1, (int)chartUV.Series["SeriesGammaU"].Points[m_nUVLength - 1].YValues[0]));
        _ = chartUV.Series["SeriesFeatureU"].Points.AddXY(0.0, chartUV.Series["SeriesGammaU"].Points[0].YValues[0]);
        _ = chartUV.Series["SeriesFeatureU"].Points.AddXY(m_nUVLength - 1, chartUV.Series["SeriesGammaU"].Points[m_nUVLength - 1].YValues[0]);
        m_ptFeatureV.Clear();
        m_ptFeatureV.Add(new Point(0, (int)chartUV.Series["SeriesGammaV"].Points[0].YValues[0]));
        m_ptFeatureV.Add(new Point(m_nUVLength - 1, (int)chartUV.Series["SeriesGammaV"].Points[m_nUVLength - 1].YValues[0]));
        _ = chartUV.Series["SeriesFeatureV"].Points.AddXY(0.0, chartUV.Series["SeriesGammaV"].Points[0].YValues[0]);
        _ = chartUV.Series["SeriesFeatureV"].Points.AddXY(m_nUVLength - 1, chartUV.Series["SeriesGammaV"].Points[m_nUVLength - 1].YValues[0]);
        SaveGammaValue_temp();
        streamReader.Close();
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
            streamWriter.WriteLine("pixel red:1~256 green:257~512 blue:513~768");
            for (int i = 0; i < m_nYLength; i++)
            {
                streamWriter.WriteLine((int)chartY.Series["SeriesGammaY"].Points[i].YValues[0]);
            }
            for (int j = 0; j < m_nUVLength; j++)
            {
                streamWriter.WriteLine((int)chartUV.Series["SeriesGammaU"].Points[j].YValues[0]);
            }
            for (int k = 0; k < m_nUVLength; k++)
            {
                streamWriter.WriteLine((int)chartUV.Series["SeriesGammaV"].Points[k].YValues[0]);
            }
        }
        stream.Close();
        _ = MessageBox.Show("Save Gamma Data OK");
    }

    private void CheckControlPoint(List<Point> pFeaturePoint, int ptX, Label labelCtrlPt)
    {
        int nFeatureIdx = -1;
        IsFeaturePoint(pFeaturePoint, ptX, ref nFeatureIdx);
        labelCtrlPt.Text = nFeatureIdx >= 0 ? m_strCtrlPtYes : m_strCtrlPtNo;
    }

    private void numUpDown_Y_X_ValueChanged(object sender, EventArgs e)
    {
        int num = (int)numUpDown_Y_X.Value;
        numUpDown_Y_Y.Value = (int)chartY.Series["SeriesGammaY"].Points[num].YValues[0];
        CheckControlPoint(m_ptFeatureY, num, lbIsCtrlPtY);
    }

    private void numUpDown_Y_Y_ValueChanged(object sender, EventArgs e)
    {
    }

    private void numUpDown_U_X_ValueChanged(object sender, EventArgs e)
    {
        int num = (int)numUpDown_U_X.Value;
        numUpDown_U_Y.Value = (int)chartUV.Series["SeriesGammaU"].Points[num].YValues[0];
        CheckControlPoint(m_ptFeatureU, num, lbIsCtrlPtU);
    }

    private void numUpDown_U_Y_ValueChanged(object sender, EventArgs e)
    {
    }

    private void numUpDown_V_X_ValueChanged(object sender, EventArgs e)
    {
        int num = (int)numUpDown_V_X.Value;
        numUpDown_V_Y.Value = (int)chartUV.Series["SeriesGammaV"].Points[num].YValues[0];
        CheckControlPoint(m_ptFeatureV, num, lbIsCtrlPtV);
    }

    private void numUpDown_Y_ValueChanged(object sender, EventArgs e)
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

    private void AddFeaturePointByXY(Series pCurve, List<Point> pFeaturePoint, Point ptSelect, ref int nFeatureIdx)
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
            CurveFittingModify(nFeatureIdx, pCurve, pFeaturePoint);
        }
    }

    private void AddFeaturePoint(Series pCurve, List<Point> pFeaturePoint, Point ptSelect, ref int nFeatureIdx, ref bool bFindPoint)
    {
        int num = 5;
        int num2 = 12;
        int num3 = 5;
        int num4 = 18;
        int num5 = 0;
        bFindPoint = false;
        for (int i = 0; i < pFeaturePoint.Count; i++)
        {
            if (pFeaturePoint[i].X > ptSelect.X - num3 && pFeaturePoint[i].X < ptSelect.X + num3 && pFeaturePoint[i].Y > ptSelect.Y - num4 && pFeaturePoint[i].Y < ptSelect.Y + num4)
            {
                bFindPoint = true;
                nFeatureIdx = i;
                if (pCurve.ChartArea == "ChartArea1")
                {
                    chartY.Cursor = Cursors.NoMove2D;
                }
                else
                {
                    chartUV.Cursor = Cursors.NoMove2D;
                }
                break;
            }
        }
        if (bFindPoint)
        {
            return;
        }
        for (int j = 0; j < pCurve.Points.Count; j++)
        {
            if (!(pCurve.Points[j].XValue > ptSelect.X - num) || !(pCurve.Points[j].XValue < ptSelect.X + num) || !(pCurve.Points[j].YValues[0] > ptSelect.Y - num2) || !(pCurve.Points[j].YValues[0] < ptSelect.Y + num2))
            {
                continue;
            }
            num5 = j;
            pFeaturePoint.Add(new Point(num5, (int)pCurve.Points[num5].YValues[0]));
            pFeaturePoint.Sort((px, py) => px.X.CompareTo(py.X));
            for (int k = 0; k < pCurve.Points.Count; k++)
            {
                if (pFeaturePoint[k].X == num5)
                {
                    bFindPoint = true;
                    nFeatureIdx = k;
                    CurveFittingModify(nFeatureIdx, pCurve, pFeaturePoint);
                    break;
                }
            }
            break;
        }
    }

    private void RemoveFeaturePoint(Series pCurve, Series seriesFeature, List<Point> pFeaturePoint, Point ptSelect, ref bool bFindPoint)
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
                CurveFittingModify(num2, pCurve, pFeaturePoint);
                seriesFeature.Points.Clear();
                for (int j = 0; j < pFeaturePoint.Count; j++)
                {
                    _ = seriesFeature.Points.AddXY(pFeaturePoint[j].X, pFeaturePoint[j].Y);
                }
                break;
            }
        }
    }

    private void ResetCurveY(string strGammaName)
    {
        GetGammaWithOB(m_nYMax, m_nYMax, 0m, m_nYLength, out Point[] pGammaArray);
        if (pGammaArray != null)
        {
            int num;
            for (num = 0; num < pGammaArray.Length; num++)
            {
                chartY.Series[strGammaName].Points[num].YValues[0] = pGammaArray[num].Y;
            }
            List<Point> ptFeatureY = m_ptFeatureY;
            for (int i = 0; i < ptFeatureY.Count; i++)
            {
                ptFeatureY[i] = new Point(ptFeatureY[i].X, (int)chartY.Series[strGammaName].Points[ptFeatureY[i].X].YValues[0]);
            }
            string name = strGammaName.Replace("Gamma", "Feature");
            Series series = chartY.Series[name];
            series.Points.Clear();
            for (int j = 0; j < ptFeatureY.Count; j++)
            {
                _ = series.Points.AddXY(ptFeatureY[j].X, ptFeatureY[j].Y);
            }
        }
    }

    private void ResetCurveUV(string strGammaName)
    {
        GetGammaWithOB(m_nUVMax, m_nUVMax, 0m, m_nUVLength, out Point[] pGammaArray);
        if (pGammaArray != null)
        {
            int num;
            for (num = 0; num < pGammaArray.Length; num++)
            {
                chartUV.Series[strGammaName].Points[num].YValues[0] = pGammaArray[num].Y;
            }
            ReversSeires(chartUV.Series[strGammaName]);
            List<Point> list = !(strGammaName == "SeriesGammaU") ? m_ptFeatureV : m_ptFeatureU;
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = new Point(list[i].X, (int)chartUV.Series[strGammaName].Points[list[i].X].YValues[0]);
            }
            string name = strGammaName.Replace("Gamma", "Feature");
            Series series = chartUV.Series[name];
            series.Points.Clear();
            for (int j = 0; j < list.Count; j++)
            {
                _ = series.Points.AddXY(list[j].X, list[j].Y);
            }
        }
    }

    private void btnResetCurveY_Click(object sender, EventArgs e)
    {
        ResetCurveY("SeriesGammaY");
        SaveGammaValue_temp();
    }

    private void btnResetCurveU_Click(object sender, EventArgs e)
    {
        ResetCurveUV("SeriesGammaU");
        SaveGammaValue_temp();
    }

    private void btnResetCurveV_Click(object sender, EventArgs e)
    {
        ResetCurveUV("SeriesGammaV");
        SaveGammaValue_temp();
    }

    private void SetChartScaleXY(bool enable)
    {
        chartY.ChartAreas[0].CursorX.IsUserSelectionEnabled = enable;
        chartY.ChartAreas[0].CursorY.IsUserSelectionEnabled = enable;
        chartUV.ChartAreas[0].CursorX.IsUserSelectionEnabled = enable;
        chartUV.ChartAreas[0].CursorY.IsUserSelectionEnabled = enable;
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
        chartY.ChartAreas[0].CursorX.Interval = 0.0;
        chartY.ChartAreas[0].CursorX.IsUserEnabled = true;
        chartY.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
        chartY.ChartAreas[0].AxisX.ScrollBar.Size = 10.0;
        chartY.ChartAreas[0].CursorY.Interval = 0.0;
        chartY.ChartAreas[0].CursorY.IsUserEnabled = true;
        chartY.ChartAreas[0].CursorY.IsUserSelectionEnabled = false;
        chartY.ChartAreas[0].AxisY.ScrollBar.Size = 10.0;
        chartUV.ChartAreas[0].CursorX.Interval = 0.0;
        chartUV.ChartAreas[0].CursorX.IsUserEnabled = true;
        chartUV.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
        chartUV.ChartAreas[0].AxisX.ScrollBar.Size = 10.0;
        chartUV.ChartAreas[0].CursorY.Interval = 0.0;
        chartUV.ChartAreas[0].CursorY.IsUserEnabled = true;
        chartUV.ChartAreas[0].CursorY.IsUserSelectionEnabled = false;
        chartUV.ChartAreas[0].AxisY.ScrollBar.Size = 10.0;
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

    private void chartY_SelectionRangeChanging(object sender, CursorEventArgs e)
    {
        e.NewSelectionEnd = (int)e.NewSelectionEnd;
        e.NewSelectionStart = (int)e.NewSelectionStart;
    }

    private void chartUV_SelectionRangeChanging(object sender, CursorEventArgs e)
    {
        e.NewSelectionEnd = (int)e.NewSelectionEnd;
        e.NewSelectionStart = (int)e.NewSelectionStart;
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
        Series series5 = new();
        Series series6 = new();
        Series series7 = new();
        Series series8 = new();
        checkBox_V = new CheckBox();
        chartY = new Chart();
        chartUV = new Chart();
        labelPosition = new Label();
        btnSave = new Button();
        btnLoad = new Button();
        btnVset = new Button();
        btnUset = new Button();
        btnYset = new Button();
        numUpDown_V_Y = new NumericUpDown();
        numUpDown_V_X = new NumericUpDown();
        numUpDown_U_Y = new NumericUpDown();
        numUpDown_U_X = new NumericUpDown();
        numUpDown_Y_Y = new NumericUpDown();
        numUpDown_Y_X = new NumericUpDown();
        labelColorY = new Label();
        label13 = new Label();
        label12 = new Label();
        label11 = new Label();
        label10 = new Label();
        label9 = new Label();
        label8 = new Label();
        labelColorV = new Label();
        labelColorU = new Label();
        label5 = new Label();
        checkBox_U = new CheckBox();
        checkBox_Y = new CheckBox();
        sectionlabel = new Label();
        sectiontrackBar = new TrackBar();
        btnResetCurveY = new Button();
        btnResetCurveU = new Button();
        btnResetCurveV = new Button();
        lbIsCtrlPtY = new Label();
        groupCurveY = new GroupBox();
        groupCurveU = new GroupBox();
        lbIsCtrlPtU = new Label();
        groupCurveV = new GroupBox();
        lbIsCtrlPtV = new Label();
        groupFileMenu = new GroupBox();
        labelOpType = new Label();
        comboBoxOpType = new ComboBox();
        checkBoxEnable = new CheckBox();
        labelIndex = new Label();
        comboBoxIndex = new ComboBox();
        groupBoxApi2 = new GroupBox();
        groupBoxControl = new GroupBox();
        ((ISupportInitialize)chartY).BeginInit();
        ((ISupportInitialize)chartUV).BeginInit();
        ((ISupportInitialize)numUpDown_V_Y).BeginInit();
        ((ISupportInitialize)numUpDown_V_X).BeginInit();
        ((ISupportInitialize)numUpDown_U_Y).BeginInit();
        ((ISupportInitialize)numUpDown_U_X).BeginInit();
        ((ISupportInitialize)numUpDown_Y_Y).BeginInit();
        ((ISupportInitialize)numUpDown_Y_X).BeginInit();
        ((ISupportInitialize)sectiontrackBar).BeginInit();
        groupCurveY.SuspendLayout();
        groupCurveU.SuspendLayout();
        groupCurveV.SuspendLayout();
        groupFileMenu.SuspendLayout();
        groupBoxApi2.SuspendLayout();
        groupBoxControl.SuspendLayout();
        SuspendLayout();
        checkBox_V.AutoSize = true;
        checkBox_V.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        checkBox_V.Location = new Point(16, 23);
        checkBox_V.Name = "checkBox_V";
        checkBox_V.Size = new Size(78, 18);
        checkBox_V.TabIndex = 146;
        checkBox_V.Text = "Gamma V";
        checkBox_V.UseVisualStyleBackColor = true;
        checkBox_V.CheckedChanged += new EventHandler(checkBox_V_CheckedChanged);
        chartArea.AxisY.MajorTickMark.Interval = 0.0;
        chartArea.AxisY.MajorTickMark.IntervalOffset = 0.0;
        chartArea.AxisY.MajorTickMark.IntervalOffsetType = DateTimeIntervalType.Auto;
        chartArea.AxisY.MajorTickMark.IntervalType = DateTimeIntervalType.Auto;
        chartArea.Name = "ChartArea1";
        chartY.ChartAreas.Add(chartArea);
        chartY.Cursor = Cursors.Cross;
        legend.Name = "Legend1";
        chartY.Legends.Add(legend);
        chartY.Location = new Point(25, 168);
        chartY.Name = "chartY";
        series.BorderWidth = 2;
        series.ChartArea = "ChartArea1";
        series.ChartType = SeriesChartType.Spline;
        series.Color = Color.Gray;
        series.Legend = "Legend1";
        series.Name = "SeriesGammaY";
        series2.BorderColor = Color.Gray;
        series2.BorderWidth = 2;
        series2.ChartArea = "ChartArea1";
        series2.ChartType = SeriesChartType.Point;
        series2.Color = Color.FromArgb(64, 64, 64);
        series2.Legend = "Legend1";
        series2.Name = "SeriesFeatureY";
        chartY.Series.Add(series);
        chartY.Series.Add(series2);
        chartY.Size = new Size(520, 374);
        chartY.TabIndex = 140;
        chartY.Text = "chartY";
        chartY.SelectionRangeChanging += new EventHandler<CursorEventArgs>(chartY_SelectionRangeChanging);
        chartY.MouseDown += new MouseEventHandler(chartY_MouseDown);
        chartY.MouseMove += new MouseEventHandler(chartY_MouseMove);
        chartY.MouseUp += new MouseEventHandler(chartY_MouseUp);
        chartArea2.Name = "ChartArea2";
        chartUV.ChartAreas.Add(chartArea2);
        chartUV.Cursor = Cursors.Cross;
        legend2.Name = "Legend2";
        chartUV.Legends.Add(legend2);
        chartUV.Location = new Point(551, 168);
        chartUV.Name = "chartUV";
        series3.BorderWidth = 2;
        series3.ChartArea = "ChartArea2";
        series3.ChartType = SeriesChartType.Spline;
        series3.Color = Color.FromArgb(255, 128, 255);
        series3.Legend = "Legend2";
        series3.Name = "SeriesGammaV";
        series4.BorderColor = Color.FromArgb(255, 192, 255);
        series4.BorderWidth = 2;
        series4.ChartArea = "ChartArea2";
        series4.ChartType = SeriesChartType.Point;
        series4.Color = Color.Fuchsia;
        series4.Legend = "Legend2";
        series4.MarkerSize = 6;
        series4.MarkerStyle = MarkerStyle.Circle;
        series4.Name = "SeriesFeatureV";
        series4.YValuesPerPoint = 2;
        series5.BorderWidth = 2;
        series5.ChartArea = "ChartArea2";
        series5.ChartType = SeriesChartType.Spline;
        series5.Legend = "Legend2";
        series5.Name = "SeriesGammaU";
        series6.BorderColor = Color.FromArgb(192, 0, 192);
        series6.BorderWidth = 2;
        series6.ChartArea = "ChartArea2";
        series6.ChartType = SeriesChartType.Point;
        series6.Color = Color.Purple;
        series6.Legend = "Legend2";
        series6.Name = "SeriesFeatureU";
        series7.BorderWidth = 2;
        series7.ChartArea = "ChartArea2";
        series7.ChartType = SeriesChartType.Spline;
        series7.Legend = "Legend2";
        series7.Name = "SeriesGammaVr";
        series8.BorderWidth = 2;
        series8.ChartArea = "ChartArea2";
        series8.ChartType = SeriesChartType.Spline;
        series8.Legend = "Legend2";
        series8.Name = "SeriesGammaUr";
        chartUV.Series.Add(series3);
        chartUV.Series.Add(series4);
        chartUV.Series.Add(series5);
        chartUV.Series.Add(series6);
        chartUV.Series.Add(series7);
        chartUV.Series.Add(series8);
        chartUV.Size = new Size(520, 374);
        chartUV.TabIndex = 157;
        chartUV.Text = "chartUV";
        chartUV.SelectionRangeChanging += new EventHandler<CursorEventArgs>(chartUV_SelectionRangeChanging);
        chartUV.MouseDown += new MouseEventHandler(chartUV_MouseDown);
        chartUV.MouseMove += new MouseEventHandler(chartUV_MouseMove);
        chartUV.MouseUp += new MouseEventHandler(chartUV_MouseUp);
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
        btnVset.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnVset.BackColor = Color.LightCyan;
        btnVset.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnVset.Location = new Point(123, 73);
        btnVset.MinimumSize = new Size(37, 0);
        btnVset.Name = "btnVset";
        btnVset.Size = new Size(70, 23);
        btnVset.TabIndex = 131;
        btnVset.Text = "Set";
        btnVset.UseVisualStyleBackColor = false;
        btnVset.Click += new EventHandler(btnVset_Click);
        btnUset.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnUset.BackColor = Color.LightCyan;
        btnUset.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnUset.Location = new Point(121, 73);
        btnUset.MinimumSize = new Size(37, 0);
        btnUset.Name = "btnUset";
        btnUset.Size = new Size(70, 23);
        btnUset.TabIndex = 129;
        btnUset.Text = "Set";
        btnUset.UseVisualStyleBackColor = false;
        btnUset.Click += new EventHandler(btnUset_Click);
        btnYset.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnYset.BackColor = Color.LightCyan;
        btnYset.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnYset.Location = new Point(120, 73);
        btnYset.MinimumSize = new Size(70, 23);
        btnYset.Name = "btnYset";
        btnYset.Size = new Size(70, 23);
        btnYset.TabIndex = 127;
        btnYset.Text = "Set";
        btnYset.UseVisualStyleBackColor = false;
        btnYset.Click += new EventHandler(btnYset_Click);
        numUpDown_V_Y.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        numUpDown_V_Y.Location = new Point(36, 75);
        numUpDown_V_Y.Maximum = new decimal(new int[4] { 511, 0, 0, 0 });
        numUpDown_V_Y.MinimumSize = new Size(60, 0);
        numUpDown_V_Y.Name = "numUpDown_V_Y";
        numUpDown_V_Y.Size = new Size(60, 22);
        numUpDown_V_Y.TabIndex = 124;
        numUpDown_V_Y.ValueChanged += new EventHandler(numUpDown_Y_ValueChanged);
        numUpDown_V_X.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        numUpDown_V_X.Location = new Point(36, 47);
        numUpDown_V_X.Maximum = new decimal(new int[4] { 127, 0, 0, 0 });
        numUpDown_V_X.MinimumSize = new Size(60, 0);
        numUpDown_V_X.Name = "numUpDown_V_X";
        numUpDown_V_X.Size = new Size(60, 22);
        numUpDown_V_X.TabIndex = 123;
        numUpDown_V_X.ValueChanged += new EventHandler(numUpDown_V_X_ValueChanged);
        numUpDown_U_Y.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        numUpDown_U_Y.Location = new Point(39, 75);
        numUpDown_U_Y.Maximum = new decimal(new int[4] { 511, 0, 0, 0 });
        numUpDown_U_Y.MinimumSize = new Size(60, 0);
        numUpDown_U_Y.Name = "numUpDown_U_Y";
        numUpDown_U_Y.Size = new Size(60, 22);
        numUpDown_U_Y.TabIndex = 122;
        numUpDown_U_Y.ValueChanged += new EventHandler(numUpDown_U_Y_ValueChanged);
        numUpDown_U_X.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        numUpDown_U_X.Location = new Point(39, 47);
        numUpDown_U_X.Maximum = new decimal(new int[4] { 127, 0, 0, 0 });
        numUpDown_U_X.MinimumSize = new Size(60, 0);
        numUpDown_U_X.Name = "numUpDown_U_X";
        numUpDown_U_X.Size = new Size(60, 22);
        numUpDown_U_X.TabIndex = 121;
        numUpDown_U_X.ValueChanged += new EventHandler(numUpDown_U_X_ValueChanged);
        numUpDown_Y_Y.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        numUpDown_Y_Y.Location = new Point(33, 73);
        numUpDown_Y_Y.Maximum = new decimal(new int[4] { 1023, 0, 0, 0 });
        numUpDown_Y_Y.MinimumSize = new Size(60, 0);
        numUpDown_Y_Y.Name = "numUpDown_Y_Y";
        numUpDown_Y_Y.Size = new Size(60, 22);
        numUpDown_Y_Y.TabIndex = 120;
        numUpDown_Y_Y.ValueChanged += new EventHandler(numUpDown_Y_Y_ValueChanged);
        numUpDown_Y_X.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        numUpDown_Y_X.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        numUpDown_Y_X.Location = new Point(33, 45);
        numUpDown_Y_X.Maximum = new decimal(new int[4] { 255, 0, 0, 0 });
        numUpDown_Y_X.MinimumSize = new Size(30, 0);
        numUpDown_Y_X.Name = "numUpDown_Y_X";
        numUpDown_Y_X.Size = new Size(60, 22);
        numUpDown_Y_X.TabIndex = 119;
        numUpDown_Y_X.ValueChanged += new EventHandler(numUpDown_Y_X_ValueChanged);
        labelColorY.AutoSize = true;
        labelColorY.BackColor = Color.Gray;
        labelColorY.Location = new Point(171, 24);
        labelColorY.Name = "labelColorY";
        labelColorY.Size = new Size(19, 14);
        labelColorY.TabIndex = 118;
        labelColorY.Text = "   ";
        label13.AutoSize = true;
        label13.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label13.Location = new Point(13, 78);
        label13.Name = "label13";
        label13.Size = new Size(19, 14);
        label13.TabIndex = 117;
        label13.Text = "Y:";
        label12.AutoSize = true;
        label12.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label12.Location = new Point(13, 52);
        label12.Name = "label12";
        label12.Size = new Size(18, 14);
        label12.TabIndex = 116;
        label12.Text = "X:";
        label11.AutoSize = true;
        label11.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label11.Location = new Point(14, 77);
        label11.Name = "label11";
        label11.Size = new Size(19, 14);
        label11.TabIndex = 115;
        label11.Text = "Y:";
        label10.AutoSize = true;
        label10.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label10.Location = new Point(15, 52);
        label10.Name = "label10";
        label10.Size = new Size(18, 14);
        label10.TabIndex = 114;
        label10.Text = "X:";
        label9.AutoSize = true;
        label9.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label9.Location = new Point(10, 73);
        label9.Name = "label9";
        label9.Size = new Size(19, 14);
        label9.TabIndex = 113;
        label9.Text = "Y:";
        label8.AutoSize = true;
        label8.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label8.Location = new Point(11, 49);
        label8.Name = "label8";
        label8.Size = new Size(18, 14);
        label8.TabIndex = 112;
        label8.Text = "X:";
        labelColorV.AutoSize = true;
        labelColorV.BackColor = Color.FromArgb(255, 128, 255);
        labelColorV.Location = new Point(174, 25);
        labelColorV.Name = "labelColorV";
        labelColorV.Size = new Size(19, 14);
        labelColorV.TabIndex = 111;
        labelColorV.Text = "   ";
        labelColorU.AutoSize = true;
        labelColorU.BackColor = Color.Blue;
        labelColorU.Location = new Point(172, 25);
        labelColorU.Name = "labelColorU";
        labelColorU.Size = new Size(19, 14);
        labelColorU.TabIndex = 110;
        labelColorU.Text = "   ";
        label5.AutoSize = true;
        label5.BackColor = Color.Red;
        label5.Font = new Font("Times New Roman", 10f);
        label5.Location = new Point(221, 74);
        label5.Name = "label5";
        label5.Size = new Size(0, 14);
        label5.TabIndex = 109;
        checkBox_U.AutoSize = true;
        checkBox_U.BackColor = SystemColors.Window;
        checkBox_U.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        checkBox_U.Location = new Point(19, 23);
        checkBox_U.Name = "checkBox_U";
        checkBox_U.Size = new Size(78, 18);
        checkBox_U.TabIndex = 108;
        checkBox_U.Text = "Gamma U";
        checkBox_U.UseVisualStyleBackColor = false;
        checkBox_U.CheckedChanged += new EventHandler(checkBox_U_CheckedChanged);
        checkBox_Y.AutoSize = true;
        checkBox_Y.BackColor = SystemColors.Window;
        checkBox_Y.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        checkBox_Y.Location = new Point(13, 21);
        checkBox_Y.Name = "checkBox_Y";
        checkBox_Y.Size = new Size(78, 18);
        checkBox_Y.TabIndex = 107;
        checkBox_Y.Text = "Gamma Y";
        checkBox_Y.UseVisualStyleBackColor = false;
        checkBox_Y.CheckedChanged += new EventHandler(checkBox_Y_CheckedChanged);
        sectionlabel.AutoSize = true;
        sectionlabel.BackColor = SystemColors.Window;
        sectionlabel.Font = new Font("Cambria", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        sectionlabel.Location = new Point(84, 95);
        sectionlabel.Name = "sectionlabel";
        sectionlabel.Size = new Size(61, 14);
        sectionlabel.TabIndex = 139;
        sectionlabel.Text = "16 Section";
        sectiontrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        sectiontrackBar.BackColor = SystemColors.Window;
        sectiontrackBar.LargeChange = 1;
        sectiontrackBar.Location = new Point(18, 39);
        sectiontrackBar.Maximum = 8;
        sectiontrackBar.Name = "sectiontrackBar";
        sectiontrackBar.Size = new Size(192, 45);
        sectiontrackBar.TabIndex = 138;
        sectiontrackBar.TickStyle = TickStyle.None;
        sectiontrackBar.Value = 4;
        sectiontrackBar.Scroll += new EventHandler(sectiontrackBar_Scroll);
        btnResetCurveY.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnResetCurveY.BackColor = Color.LightCyan;
        btnResetCurveY.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnResetCurveY.Location = new Point(69, 102);
        btnResetCurveY.MinimumSize = new Size(70, 23);
        btnResetCurveY.Name = "btnResetCurveY";
        btnResetCurveY.Size = new Size(70, 23);
        btnResetCurveY.TabIndex = 147;
        btnResetCurveY.Text = "Reset";
        btnResetCurveY.UseVisualStyleBackColor = false;
        btnResetCurveY.Click += new EventHandler(btnResetCurveY_Click);
        btnResetCurveU.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnResetCurveU.BackColor = Color.LightCyan;
        btnResetCurveU.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnResetCurveU.Location = new Point(72, 102);
        btnResetCurveU.MinimumSize = new Size(70, 23);
        btnResetCurveU.Name = "btnResetCurveU";
        btnResetCurveU.Size = new Size(70, 23);
        btnResetCurveU.TabIndex = 148;
        btnResetCurveU.Text = "Reset";
        btnResetCurveU.UseVisualStyleBackColor = false;
        btnResetCurveU.Click += new EventHandler(btnResetCurveU_Click);
        btnResetCurveV.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnResetCurveV.BackColor = Color.LightCyan;
        btnResetCurveV.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnResetCurveV.Location = new Point(75, 102);
        btnResetCurveV.MinimumSize = new Size(70, 23);
        btnResetCurveV.Name = "btnResetCurveV";
        btnResetCurveV.Size = new Size(70, 23);
        btnResetCurveV.TabIndex = 149;
        btnResetCurveV.Text = "Reset";
        btnResetCurveV.UseVisualStyleBackColor = false;
        btnResetCurveV.Click += new EventHandler(btnResetCurveV_Click);
        lbIsCtrlPtY.AutoSize = true;
        lbIsCtrlPtY.Location = new Point(121, 49);
        lbIsCtrlPtY.Name = "lbIsCtrlPtY";
        lbIsCtrlPtY.Size = new Size(69, 14);
        lbIsCtrlPtY.TabIndex = 150;
        lbIsCtrlPtY.Text = "Ctrl Pt: Yes";
        groupCurveY.Controls.Add(numUpDown_Y_X);
        groupCurveY.Controls.Add(lbIsCtrlPtY);
        groupCurveY.Controls.Add(label8);
        groupCurveY.Controls.Add(btnResetCurveY);
        groupCurveY.Controls.Add(numUpDown_Y_Y);
        groupCurveY.Controls.Add(label9);
        groupCurveY.Controls.Add(checkBox_Y);
        groupCurveY.Controls.Add(labelColorY);
        groupCurveY.Controls.Add(btnYset);
        groupCurveY.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupCurveY.Location = new Point(12, 12);
        groupCurveY.Name = "groupCurveY";
        groupCurveY.Size = new Size(209, 131);
        groupCurveY.TabIndex = 151;
        groupCurveY.TabStop = false;
        groupCurveY.Text = "Y";
        groupCurveU.Controls.Add(checkBox_U);
        groupCurveU.Controls.Add(lbIsCtrlPtU);
        groupCurveU.Controls.Add(labelColorU);
        groupCurveU.Controls.Add(btnResetCurveU);
        groupCurveU.Controls.Add(numUpDown_U_X);
        groupCurveU.Controls.Add(numUpDown_U_Y);
        groupCurveU.Controls.Add(label10);
        groupCurveU.Controls.Add(label11);
        groupCurveU.Controls.Add(btnUset);
        groupCurveU.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupCurveU.Location = new Point(227, 12);
        groupCurveU.Name = "groupCurveU";
        groupCurveU.Size = new Size(209, 131);
        groupCurveU.TabIndex = 152;
        groupCurveU.TabStop = false;
        groupCurveU.Text = "U";
        lbIsCtrlPtU.AutoSize = true;
        lbIsCtrlPtU.Location = new Point(122, 49);
        lbIsCtrlPtU.Name = "lbIsCtrlPtU";
        lbIsCtrlPtU.Size = new Size(69, 14);
        lbIsCtrlPtU.TabIndex = 150;
        lbIsCtrlPtU.Text = "Ctrl Pt: Yes";
        groupCurveV.Controls.Add(checkBox_V);
        groupCurveV.Controls.Add(lbIsCtrlPtV);
        groupCurveV.Controls.Add(numUpDown_V_X);
        groupCurveV.Controls.Add(numUpDown_V_Y);
        groupCurveV.Controls.Add(btnResetCurveV);
        groupCurveV.Controls.Add(label12);
        groupCurveV.Controls.Add(label13);
        groupCurveV.Controls.Add(btnVset);
        groupCurveV.Controls.Add(labelColorV);
        groupCurveV.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupCurveV.Location = new Point(442, 12);
        groupCurveV.Name = "groupCurveV";
        groupCurveV.Size = new Size(209, 131);
        groupCurveV.TabIndex = 153;
        groupCurveV.TabStop = false;
        groupCurveV.Text = "V";
        lbIsCtrlPtV.AutoSize = true;
        lbIsCtrlPtV.Location = new Point(124, 49);
        lbIsCtrlPtV.Name = "lbIsCtrlPtV";
        lbIsCtrlPtV.Size = new Size(69, 14);
        lbIsCtrlPtV.TabIndex = 150;
        lbIsCtrlPtV.Text = "Ctrl Pt: Yes";
        groupFileMenu.Controls.Add(btnLoad);
        groupFileMenu.Controls.Add(btnSave);
        groupFileMenu.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupFileMenu.Location = new Point(662, 12);
        groupFileMenu.Name = "groupFileMenu";
        groupFileMenu.Size = new Size(187, 131);
        groupFileMenu.TabIndex = 154;
        groupFileMenu.TabStop = false;
        groupFileMenu.Text = "File";
        labelOpType.AutoSize = true;
        labelOpType.Location = new Point(95, 40);
        labelOpType.Name = "labelOpType";
        labelOpType.Size = new Size(59, 14);
        labelOpType.TabIndex = 138;
        labelOpType.Text = "OpType :";
        comboBoxOpType.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxOpType.FormattingEnabled = true;
        comboBoxOpType.Items.AddRange(new object[2] { "Auto", "Manual" });
        comboBoxOpType.Location = new Point(160, 37);
        comboBoxOpType.Name = "comboBoxOpType";
        comboBoxOpType.Size = new Size(63, 22);
        comboBoxOpType.TabIndex = 137;
        comboBoxOpType.SelectedIndexChanged += new EventHandler(comboBoxOpType_SelectedIndexChanged);
        checkBoxEnable.AutoSize = true;
        checkBoxEnable.Location = new Point(20, 39);
        checkBoxEnable.Name = "checkBoxEnable";
        checkBoxEnable.Size = new Size(62, 18);
        checkBoxEnable.TabIndex = 136;
        checkBoxEnable.Text = "Enable";
        checkBoxEnable.UseVisualStyleBackColor = true;
        checkBoxEnable.CheckedChanged += new EventHandler(checkBoxEnable_CheckedChanged);
        labelIndex.AutoSize = true;
        labelIndex.Location = new Point(17, 91);
        labelIndex.Name = "labelIndex";
        labelIndex.Size = new Size(84, 14);
        labelIndex.TabIndex = 135;
        labelIndex.Text = "Select Index :";
        comboBoxIndex.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBoxIndex.FormattingEnabled = true;
        comboBoxIndex.Items.AddRange(new object[16]
        {
            "Gamma index 0", "Gamma index 1", "Gamma index 2", "Gamma index 3", "Gamma index 4", "Gamma index 5", "Gamma index 6", "Gamma index 7", "Gamma index 8", "Gamma index 9",
            "Gamma index 10", "Gamma index 11", "Gamma index 12", "Gamma index 13", "Gamma index 14", "Gamma index 15"
        });
        comboBoxIndex.Location = new Point(102, 88);
        comboBoxIndex.Name = "comboBoxIndex";
        comboBoxIndex.Size = new Size(121, 22);
        comboBoxIndex.SelectedIndex = 0;
        comboBoxIndex.TabIndex = 134;
        comboBoxIndex.SelectedIndexChanged += new EventHandler(comboBoxIndex_SelectedIndexChanged);
        groupBoxApi2.Controls.Add(labelOpType);
        groupBoxApi2.Controls.Add(comboBoxIndex);
        groupBoxApi2.Controls.Add(comboBoxOpType);
        groupBoxApi2.Controls.Add(labelIndex);
        groupBoxApi2.Controls.Add(checkBoxEnable);
        groupBoxApi2.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupBoxApi2.Location = new Point(279, 592);
        groupBoxApi2.Name = "groupBoxApi2";
        groupBoxApi2.Size = new Size(232, 141);
        groupBoxApi2.TabIndex = 155;
        groupBoxApi2.TabStop = false;
        groupBoxApi2.Text = "Option";
        groupBoxControl.Controls.Add(sectiontrackBar);
        groupBoxControl.Controls.Add(sectionlabel);
        groupBoxControl.Font = new Font("Times New Roman", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        groupBoxControl.Location = new Point(591, 593);
        groupBoxControl.Name = "groupBoxControl";
        groupBoxControl.Size = new Size(232, 140);
        groupBoxControl.TabIndex = 156;
        groupBoxControl.TabStop = false;
        groupBoxControl.Text = "Control";
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.Window;
        Controls.Add(chartY);
        Controls.Add(chartUV);
        Controls.Add(groupBoxControl);
        Controls.Add(groupBoxApi2);
        Controls.Add(groupFileMenu);
        Controls.Add(groupCurveV);
        Controls.Add(groupCurveU);
        Controls.Add(groupCurveY);
        Controls.Add(labelPosition);
        Controls.Add(label5);
        ForeColor = SystemColors.ControlText;
        Name = "YUVGamma";
        Size = new Size(1094, 758);
        Load += new EventHandler(RGBGamma_Load);
        ((ISupportInitialize)chartY).EndInit();
        ((ISupportInitialize)chartUV).EndInit();
        ((ISupportInitialize)numUpDown_V_Y).EndInit();
        ((ISupportInitialize)numUpDown_V_X).EndInit();
        ((ISupportInitialize)numUpDown_U_Y).EndInit();
        ((ISupportInitialize)numUpDown_U_X).EndInit();
        ((ISupportInitialize)numUpDown_Y_Y).EndInit();
        ((ISupportInitialize)numUpDown_Y_X).EndInit();
        ((ISupportInitialize)sectiontrackBar).EndInit();
        groupCurveY.ResumeLayout(false);
        groupCurveY.PerformLayout();
        groupCurveU.ResumeLayout(false);
        groupCurveU.PerformLayout();
        groupCurveV.ResumeLayout(false);
        groupCurveV.PerformLayout();
        groupFileMenu.ResumeLayout(false);
        groupBoxApi2.ResumeLayout(false);
        groupBoxApi2.PerformLayout();
        groupBoxControl.ResumeLayout(false);
        groupBoxControl.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
