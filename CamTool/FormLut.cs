using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSCamIQTool.CamTool;

public class FormLut : Form
{
    private int m_curIdx = -1;

    private bool m_chartSelectY;

    private readonly IQComm m_Comm;

    private readonly GuiItem m_Item;

    private readonly GuiGroup m_Group;

    private readonly bool m_bAutoWrite = true;

    private readonly List<Point> FeaturePoint = new();

    private int YMin;

    private int YMax = 63;

    private readonly int XMin = 0;

    private int XMax = 15;

    private readonly IContainer components = null;

    private Chart LutChart;

    private Label TitleLabel;

    private ComboBox IndexComboBox;

    private Label label1;

    private NumericUpDown XNumericUpDown;

    private Label label2;

    private NumericUpDown YNumericUpDown;

    private Button WriteButton;

    public FormLut(GuiGroup group, GuiItem item, IQComm comm, bool bAutoWrite)
    {
        InitializeComponent();
        m_Comm = comm;
        m_Item = item;
        m_Group = group;
        m_bAutoWrite = bAutoWrite;
        InitChart();
        InitParameters();
        UpdateChart();
    }

    private void InitParameters()
    {
        TitleLabel.Text = m_Item.Text;
        IndexComboBox.SelectedIndex = 0;
        Text = m_Group.Name;
    }

    private void InitChart()
    {
        XMax = 15;
        YMin = (int)m_Item.MinValue;
        YMax = (int)m_Item.MaxValue;
        XNumericUpDown.Minimum = XMin;
        XNumericUpDown.Maximum = XMax;
        YNumericUpDown.Minimum = YMin;
        YNumericUpDown.Maximum = YMax;
        if (m_Item.YSize == 1)
        {
            IndexComboBox.Visible = false;
        }
        if (File.Exists(Application.StartupPath + "/Icon/lut.png"))
        {
            LutChart.ChartAreas[0].BackImage = Application.StartupPath + "/Icon/lut.png";
            LutChart.ChartAreas[0].BackImageWrapMode = ChartImageWrapMode.Scaled;
        }
        LutChart.ChartAreas[0].AxisY.Maximum = YMax;
    }

    private void UpdateGuiItem()
    {
        int selectedIndex = IndexComboBox.SelectedIndex;
        if (m_curIdx == -1)
        {
            m_curIdx = Convert.ToInt32(XNumericUpDown.Value);
        }
        m_Item.DataValue[(selectedIndex * m_Item.XSize) + m_curIdx] = (long)LutChart.Series["SeriesLut"].Points[m_curIdx].YValues[0];
        _ = m_Group.WriteGroup(m_Comm);
    }

    private void WriteButton_Click(object sender, EventArgs e)
    {
        if (m_Group.Action == API_ACTION.RW)
        {
            UpdateGuiItem();
        }
    }

    private void YNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        int num = 0;
        if (m_curIdx == -1)
        {
            m_curIdx = Convert.ToInt32(XNumericUpDown.Value);
        }
        if (LutChart.Series["SeriesLut"].Points[m_curIdx].YValues[0] == (double)YNumericUpDown.Value)
        {
            return;
        }
        FeaturePoint[m_curIdx] = new Point(m_curIdx, (int)YNumericUpDown.Value);
        LutChart.Series["SeriesLut"].Points.Clear();
        foreach (Point item in FeaturePoint)
        {
            _ = LutChart.Series["SeriesLut"].Points.AddXY(item.X, item.Y);
            LutChart.Series["SeriesLut"].Points[num].MarkerStyle = MarkerStyle.Circle;
            LutChart.Series["SeriesLut"].Points[num].MarkerSize = 6;
            LutChart.Series["SeriesLut"].Points[num].MarkerColor = Color.Black;
            num++;
        }
        if (m_Group.Action == API_ACTION.RW && m_bAutoWrite)
        {
            UpdateGuiItem();
        }
    }

    private void XNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        m_curIdx = Convert.ToInt32(XNumericUpDown.Value);
        YNumericUpDown.Value = FeaturePoint[m_curIdx].Y;
    }

    private void IndexComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateChart();
    }

    private void UpdateChart()
    {
        FeaturePoint.Clear();
        LutChart.Series["SeriesLut"].Points.Clear();
        int selectedIndex = IndexComboBox.SelectedIndex;
        for (int i = 0; i <= XMax; i++)
        {
            _ = LutChart.Series["SeriesLut"].Points.AddXY(i, m_Item.DataValue[(selectedIndex * m_Item.XSize) + i]);
            LutChart.Series["SeriesLut"].Points[i].MarkerStyle = MarkerStyle.Circle;
            LutChart.Series["SeriesLut"].Points[i].MarkerSize = 6;
            LutChart.Series["SeriesLut"].Points[i].MarkerColor = Color.Black;
            Point item = new(i, (int)m_Item.DataValue[(selectedIndex * m_Item.XSize) + i]);
            FeaturePoint.Add(item);
        }
        XNumericUpDown.Value = 0m;
        YNumericUpDown.Value = FeaturePoint[0].Y;
    }

    private Tuple<double, double> GetAxisValuesFromMouse(ref Chart chartShow, int x, int y)
    {
        ChartArea chartArea = chartShow.ChartAreas[0];
        double item = 0.0;
        double item2 = 0.0;
        int num = 10;
        int num2 = 10;
        try
        {
            if (num < x && x < chartShow.Size.Width - num)
            {
                item = chartArea.AxisX.PixelPositionToValue(x);
            }
            if (num2 >= y || y >= chartShow.Size.Height - num2)
            {
                return null;
            }
            item2 = chartArea.AxisY.PixelPositionToValue(y);
        }
        catch
        {
            _ = "Position Error!" + Environment.NewLine + "Pos: (" + x + "," + y + "), Size:" + chartShow.Size.Width + "x" + chartShow.Size.Height;
        }
        return new Tuple<double, double>(item, item2);
    }

    private void FindPoint(int ptX, ref int nIdx)
    {
        nIdx = -1;
        for (int i = 0; i < LutChart.Series["SeriesLut"].Points.Count; i++)
        {
            if (LutChart.Series["SeriesLut"].Points[i].XValue == ptX)
            {
                nIdx = i;
                break;
            }
        }
    }

    private void LutChart_MouseDown(object sender, MouseEventArgs e)
    {
        int nIdx = -1;
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref LutChart, e.X, e.Y);
        if (axisValuesFromMouse == null)
        {
            return;
        }
        Point point = new((int)Math.Round(axisValuesFromMouse.Item1), (int)Math.Round(axisValuesFromMouse.Item2));
        if (e.Button == MouseButtons.Left)
        {
            FindPoint(point.X, ref nIdx);
            if (nIdx != -1)
            {
                m_chartSelectY = true;
                m_curIdx = nIdx;
                XNumericUpDown.Value = (decimal)LutChart.Series["SeriesLut"].Points[m_curIdx].XValue;
                YNumericUpDown.Value = (decimal)LutChart.Series["SeriesLut"].Points[m_curIdx].YValues[0];
            }
            else
            {
                m_curIdx = -1;
                m_chartSelectY = false;
            }
        }
    }

    private void LutChart_MouseMove(object sender, MouseEventArgs e)
    {
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref LutChart, e.X, e.Y);
        if (axisValuesFromMouse == null || !m_chartSelectY || axisValuesFromMouse == null)
        {
            return;
        }
        FeaturePoint[m_curIdx] = axisValuesFromMouse.Item2 < YMin
            ? new Point(FeaturePoint[m_curIdx].X, YMin)
            : axisValuesFromMouse.Item2 > YMax
                ? new Point(FeaturePoint[m_curIdx].X, YMax)
                : new Point(FeaturePoint[m_curIdx].X, (int)Math.Round(axisValuesFromMouse.Item2));
        YNumericUpDown.Value = FeaturePoint[m_curIdx].Y;
        int num = 0;
        LutChart.Series["SeriesLut"].Points.Clear();
        foreach (Point item in FeaturePoint)
        {
            _ = LutChart.Series["SeriesLut"].Points.AddXY(item.X, item.Y);
            LutChart.Series["SeriesLut"].Points[num].MarkerStyle = MarkerStyle.Circle;
            LutChart.Series["SeriesLut"].Points[num].MarkerSize = 6;
            LutChart.Series["SeriesLut"].Points[num].MarkerColor = Color.Black;
            num++;
        }
    }

    private void LutChart_MouseUp(object sender, MouseEventArgs e)
    {
        m_chartSelectY = false;
        LutChart.Cursor = Cursors.Cross;
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
        LutChart = new Chart();
        TitleLabel = new Label();
        IndexComboBox = new ComboBox();
        label1 = new Label();
        XNumericUpDown = new NumericUpDown();
        label2 = new Label();
        YNumericUpDown = new NumericUpDown();
        WriteButton = new Button();
        ((ISupportInitialize)LutChart).BeginInit();
        ((ISupportInitialize)XNumericUpDown).BeginInit();
        ((ISupportInitialize)YNumericUpDown).BeginInit();
        SuspendLayout();
        chartArea.AxisX.Interval = 1.0;
        chartArea.AxisX.Maximum = 16.0;
        chartArea.AxisX.Minimum = 0.0;
        chartArea.AxisY.Maximum = 63.0;
        chartArea.AxisY.Minimum = 0.0;
        chartArea.BackImageWrapMode = ChartImageWrapMode.Unscaled;
        chartArea.Name = "ChartArea1";
        LutChart.ChartAreas.Add(chartArea);
        legend.Name = "Legend1";
        LutChart.Legends.Add(legend);
        LutChart.Location = new Point(26, 38);
        LutChart.Name = "LutChart";
        series.ChartArea = "ChartArea1";
        series.ChartType = SeriesChartType.Line;
        series.Color = Color.Black;
        series.IsVisibleInLegend = false;
        series.Legend = "Legend1";
        series.Name = "SeriesLut";
        LutChart.Series.Add(series);
        LutChart.Size = new Size(645, 363);
        LutChart.TabIndex = 0;
        LutChart.Text = "chart1";
        LutChart.MouseDown += new MouseEventHandler(LutChart_MouseDown);
        LutChart.MouseMove += new MouseEventHandler(LutChart_MouseMove);
        LutChart.MouseUp += new MouseEventHandler(LutChart_MouseUp);
        TitleLabel.AutoSize = true;
        TitleLabel.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        TitleLabel.Location = new Point(43, 13);
        TitleLabel.Name = "TitleLabel";
        TitleLabel.Size = new Size(33, 16);
        TitleLabel.TabIndex = 1;
        TitleLabel.Text = "Title";
        IndexComboBox.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        IndexComboBox.FormattingEnabled = true;
        IndexComboBox.Items.AddRange(new object[16]
        {
            "Index 0", "Index 1", "Index 2", "Index 3", "Index 4", "Index 5", "Index 6", "Index 7", "Index 8", "Index 9",
            "Index 10", "Index 11", "Index 12", "Index 13", "Index 14", "Index 15"
        });
        IndexComboBox.Location = new Point(522, 7);
        IndexComboBox.Name = "IndexComboBox";
        IndexComboBox.Size = new Size(70, 24);
        IndexComboBox.TabIndex = 2;
        IndexComboBox.SelectedIndexChanged += new EventHandler(IndexComboBox_SelectedIndexChanged);
        label1.AutoSize = true;
        label1.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label1.Location = new Point(71, 423);
        label1.Name = "label1";
        label1.Size = new Size(32, 16);
        label1.TabIndex = 3;
        label1.Text = "X：";
        XNumericUpDown.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        XNumericUpDown.Location = new Point(100, 419);
        XNumericUpDown.Name = "XNumericUpDown";
        XNumericUpDown.Size = new Size(78, 24);
        XNumericUpDown.TabIndex = 4;
        XNumericUpDown.ValueChanged += new EventHandler(XNumericUpDown_ValueChanged);
        label2.AutoSize = true;
        label2.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        label2.Location = new Point(227, 421);
        label2.Name = "label2";
        label2.Size = new Size(31, 16);
        label2.TabIndex = 5;
        label2.Text = "Y：";
        YNumericUpDown.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        YNumericUpDown.Location = new Point(252, 418);
        YNumericUpDown.Name = "YNumericUpDown";
        YNumericUpDown.Size = new Size(78, 24);
        YNumericUpDown.TabIndex = 6;
        YNumericUpDown.ValueChanged += new EventHandler(YNumericUpDown_ValueChanged);
        WriteButton.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        WriteButton.Location = new Point(406, 418);
        WriteButton.Name = "WriteButton";
        WriteButton.Size = new Size(75, 23);
        WriteButton.TabIndex = 7;
        WriteButton.Text = "Write";
        WriteButton.UseVisualStyleBackColor = true;
        WriteButton.Click += new EventHandler(WriteButton_Click);
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(736, 467);
        Controls.Add(WriteButton);
        Controls.Add(YNumericUpDown);
        Controls.Add(label2);
        Controls.Add(XNumericUpDown);
        Controls.Add(label1);
        Controls.Add(IndexComboBox);
        Controls.Add(TitleLabel);
        Controls.Add(LutChart);
        Name = "FormLut";
        Text = "FormLut";
        ((ISupportInitialize)LutChart).EndInit();
        ((ISupportInitialize)XNumericUpDown).EndInit();
        ((ISupportInitialize)YNumericUpDown).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
