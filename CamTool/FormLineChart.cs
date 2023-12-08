using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSCamIQTool.CamTool;

public class FormLineChart : Form
{
    private int m_curIdx = -1;

    private bool m_chartSelectY;

    private readonly IQComm m_Comm;

    private readonly GuiItem m_Item;

    private readonly GuiGroup m_Group;

    private readonly bool m_bAutoWrite = true;

    private Chart GridChart = new();

    private readonly List<Point> FeaturePoint = new();

    private int YMin;

    private int YMax = 127;

    private readonly int XMin = 0;

    private int XMax = 15;

    private readonly IContainer components = null;

    private Label XLabel;

    private Label YLabel;

    private ComboBox IndexComboBox;

    private NumericUpDown YNumericUpDown;

    private NumericUpDown XNumericUpDown;

    private Button WriteButton;

    private Label TitleLabel;

    public FormLineChart(GuiGroup group, GuiItem item, IQComm comm, bool bAutoWrite)
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
        XMax = m_Item.XSize - 1;
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
        GridChart.Location = new Point(15, 40);
        GridChart.Size = new Size(640, 300);
        Controls.Add(GridChart);
        ChartArea item = new();
        GridChart.ChartAreas.Add(item);
        GridChart.ChartAreas[0].AxisX.Minimum = 0.0;
        GridChart.ChartAreas[0].AxisX.Maximum = XMax;
        GridChart.ChartAreas[0].AxisX.Interval = 1.0;
        GridChart.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Number;
        GridChart.ChartAreas[0].AxisX.LabelStyle.Format = "D1";
        GridChart.ChartAreas[0].AxisY.Minimum = YMin;
        GridChart.ChartAreas[0].AxisY.Maximum = YMax;
        GridChart.ChartAreas[0].AxisY.IntervalType = DateTimeIntervalType.Number;
        GridChart.ChartAreas[0].AxisY.LabelStyle.Format = "D1";
        _ = new Series("SeriesLine");
        _ = GridChart.Series.Add("SeriesLine");
        GridChart.Series["SeriesLine"].BorderWidth = 2;
        GridChart.Series["SeriesLine"].Color = Color.Blue;
        GridChart.Series["SeriesLine"].ChartType = SeriesChartType.Line;
        GridChart.MouseDown += LineChart_MouseDown;
        GridChart.MouseMove += LineChart_MouseMove;
        GridChart.MouseUp += LineChart_MouseUp;
    }

    private void UpdateChart()
    {
        FeaturePoint.Clear();
        GridChart.Series["SeriesLine"].Points.Clear();
        int selectedIndex = IndexComboBox.SelectedIndex;
        for (int i = 0; i <= XMax; i++)
        {
            _ = GridChart.Series["SeriesLine"].Points.AddXY(i, m_Item.DataValue[(selectedIndex * m_Item.XSize) + i]);
            GridChart.Series["SeriesLine"].Points[i].MarkerStyle = MarkerStyle.Circle;
            GridChart.Series["SeriesLine"].Points[i].MarkerSize = 6;
            GridChart.Series["SeriesLine"].Points[i].MarkerColor = Color.Red;
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
        for (int i = 0; i < GridChart.Series["SeriesLine"].Points.Count; i++)
        {
            if (GridChart.Series["SeriesLine"].Points[i].XValue == ptX)
            {
                nIdx = i;
                break;
            }
        }
    }

    private void LineChart_MouseDown(object sender, MouseEventArgs e)
    {
        int nIdx = -1;
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref GridChart, e.X, e.Y);
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
                XNumericUpDown.Value = (decimal)GridChart.Series["SeriesLine"].Points[m_curIdx].XValue;
                YNumericUpDown.Value = (decimal)GridChart.Series["SeriesLine"].Points[m_curIdx].YValues[0];
            }
            else
            {
                m_curIdx = -1;
                m_chartSelectY = false;
            }
        }
    }

    private void LineChart_MouseMove(object sender, MouseEventArgs e)
    {
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref GridChart, e.X, e.Y);
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
        GridChart.Series["SeriesLine"].Points.Clear();
        foreach (Point item in FeaturePoint)
        {
            _ = GridChart.Series["SeriesLine"].Points.AddXY(item.X, item.Y);
            GridChart.Series["SeriesLine"].Points[num].MarkerStyle = MarkerStyle.Circle;
            GridChart.Series["SeriesLine"].Points[num].MarkerSize = 6;
            GridChart.Series["SeriesLine"].Points[num].MarkerColor = Color.Red;
            num++;
        }
    }

    private void LineChart_MouseUp(object sender, MouseEventArgs e)
    {
        m_chartSelectY = false;
        GridChart.Cursor = Cursors.Cross;
    }

    private void IndexComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (GridChart != null && GridChart.Series["SeriesLine"] != null)
        {
            UpdateChart();
        }
    }

    private void UpdateGuiItem()
    {
        int selectedIndex = IndexComboBox.SelectedIndex;
        if (m_curIdx == -1)
        {
            m_curIdx = Convert.ToInt32(XNumericUpDown.Value);
        }
        m_Item.DataValue[(selectedIndex * m_Item.XSize) + m_curIdx] = (long)GridChart.Series["SeriesLine"].Points[m_curIdx].YValues[0];
        _ = m_Group.WriteGroup(m_Comm);
    }

    private void WriteButton_Click(object sender, EventArgs e)
    {
        if (m_Group.Action == API_ACTION.RW)
        {
            UpdateGuiItem();
        }
    }

    private void XNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        m_curIdx = Convert.ToInt32(XNumericUpDown.Value);
        YNumericUpDown.Value = FeaturePoint[m_curIdx].Y;
    }

    private void YNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        int num = 0;
        if (m_curIdx == -1)
        {
            m_curIdx = Convert.ToInt32(XNumericUpDown.Value);
        }
        if (GridChart.Series["SeriesLine"].Points[m_curIdx].YValues[0] == (double)YNumericUpDown.Value)
        {
            return;
        }
        FeaturePoint[m_curIdx] = new Point(m_curIdx, (int)YNumericUpDown.Value);
        GridChart.Series["SeriesLine"].Points.Clear();
        foreach (Point item in FeaturePoint)
        {
            _ = GridChart.Series["SeriesLine"].Points.AddXY(item.X, item.Y);
            GridChart.Series["SeriesLine"].Points[num].MarkerStyle = MarkerStyle.Circle;
            GridChart.Series["SeriesLine"].Points[num].MarkerSize = 6;
            GridChart.Series["SeriesLine"].Points[num].MarkerColor = Color.Red;
            num++;
        }
        if (m_Group.Action == API_ACTION.RW && m_bAutoWrite)
        {
            UpdateGuiItem();
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
        XLabel = new Label();
        YLabel = new Label();
        IndexComboBox = new ComboBox();
        YNumericUpDown = new NumericUpDown();
        XNumericUpDown = new NumericUpDown();
        WriteButton = new Button();
        TitleLabel = new Label();
        ((ISupportInitialize)YNumericUpDown).BeginInit();
        ((ISupportInitialize)XNumericUpDown).BeginInit();
        SuspendLayout();
        XLabel.AutoSize = true;
        XLabel.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        XLabel.Location = new Point(42, 381);
        XLabel.Name = "XLabel";
        XLabel.Size = new Size(32, 16);
        XLabel.TabIndex = 5;
        XLabel.Text = "X：";
        YLabel.AutoSize = true;
        YLabel.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        YLabel.Location = new Point(183, 381);
        YLabel.Name = "YLabel";
        YLabel.Size = new Size(31, 16);
        YLabel.TabIndex = 7;
        YLabel.Text = "Y：";
        IndexComboBox.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        IndexComboBox.FormattingEnabled = true;
        IndexComboBox.Items.AddRange(new object[16]
        {
            "Index 0", "Index 1", "Index 2", "Index 3", "Index 4", "Index 5", "Index 6", "Index 7", "Index 8", "Index 9",
            "Index 10", "Index 11", "Index 12", "Index 13", "Index 14", "Index 15"
        });
        IndexComboBox.Location = new Point(612, 9);
        IndexComboBox.Name = "IndexComboBox";
        IndexComboBox.Size = new Size(108, 24);
        IndexComboBox.TabIndex = 9;
        IndexComboBox.SelectedIndexChanged += new EventHandler(IndexComboBox_SelectedIndexChanged);
        YNumericUpDown.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        YNumericUpDown.Location = new Point(206, 378);
        YNumericUpDown.Name = "YNumericUpDown";
        YNumericUpDown.Size = new Size(93, 24);
        YNumericUpDown.TabIndex = 10;
        YNumericUpDown.ValueChanged += new EventHandler(YNumericUpDown_ValueChanged);
        XNumericUpDown.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        XNumericUpDown.Location = new Point(68, 379);
        XNumericUpDown.Name = "XNumericUpDown";
        XNumericUpDown.Size = new Size(88, 24);
        XNumericUpDown.TabIndex = 11;
        XNumericUpDown.ValueChanged += new EventHandler(XNumericUpDown_ValueChanged);
        WriteButton.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        WriteButton.Location = new Point(383, 378);
        WriteButton.Name = "WriteButton";
        WriteButton.Size = new Size(75, 23);
        WriteButton.TabIndex = 12;
        WriteButton.Text = "Write";
        WriteButton.UseVisualStyleBackColor = true;
        WriteButton.Click += new EventHandler(WriteButton_Click);
        TitleLabel.AutoSize = true;
        TitleLabel.Font = new Font("Times New Roman", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
        TitleLabel.Location = new Point(33, 13);
        TitleLabel.Name = "TitleLabel";
        TitleLabel.Size = new Size(33, 16);
        TitleLabel.TabIndex = 13;
        TitleLabel.Text = "Title";
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(798, 432);
        Controls.Add(TitleLabel);
        Controls.Add(WriteButton);
        Controls.Add(XNumericUpDown);
        Controls.Add(YNumericUpDown);
        Controls.Add(IndexComboBox);
        Controls.Add(YLabel);
        Controls.Add(XLabel);
        Name = "FormLineChart";
        Text = "FormLineChart";
        ((ISupportInitialize)YNumericUpDown).EndInit();
        ((ISupportInitialize)XNumericUpDown).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
