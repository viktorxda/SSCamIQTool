using SSCamIQTool.LibComm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSCamIQTool.SSCamIQTool;

public class FormHSV : Form
{
    private int YMin;

    private int YMax;

    private int XMin = 0;

    private int XMax = 23;

    private int nSelectIdx;

    private bool bSelectPoint;

    private GuiItem item;

    private GuiGroup group;

    private IQComm comm;

    private Series seriesLine;

    private Series seriesPoint;

    private List<Point> featurePoint = new List<Point>();

    private bool m_bAutoWrite = true;

    private IContainer components = null;

    private Chart chartHSV;

    private ComboBox comboBoxIndex;

    private Label labelText;

    private Label labelX;

    private NumericUpDown numericUpDownX;

    private Label labelY;

    private NumericUpDown numericUpDownY;

    private Button butWrite;

    public FormHSV(GuiGroup group, GuiItem item, IQComm comm, bool bAW)
    {
        InitializeComponent();
        seriesLine = chartHSV.Series["SeriesLine"];
        seriesPoint = chartHSV.Series["SeriesPoint"];
        if (item.Text.EndsWith("HueAngleShiftLut") || item.Text.EndsWith("Hue") || item.Text.Contains("HueLut"))
        {
            if (File.Exists(Application.StartupPath + "\\Icon\\hue.png"))
            {
                chartHSV.ChartAreas[0].BackImage = Application.StartupPath + "\\Icon\\hue.png";
            }
            else
            {
                MessageBox.Show("\\Icon\\hue.png not found");
            }
        }
        else if (File.Exists(Application.StartupPath + "\\Icon\\sat.png"))
        {
            chartHSV.ChartAreas[0].BackImage = Application.StartupPath + "\\Icon\\sat.png";
        }
        else
        {
            MessageBox.Show("\\Icon\\sat.png not found");
        }
        InitChart(item);
        Text = item.Text;
        this.group = group;
        this.comm = comm;
        m_bAutoWrite = bAW;
    }

    private void InitChart(GuiItem item)
    {
        this.item = item;
        YMin = (int)item.MinValue;
        YMax = (int)item.MaxValue;
        labelText.Text = item.Text;
        if (item.XSize > 0)
        {
            XMax = item.XSize - 1;
        }
        if (item.YSize == 1)
        {
            comboBoxIndex.Visible = false;
        }
        chartHSV.ChartAreas[0].AxisX.Minimum = XMin;
        chartHSV.ChartAreas[0].AxisX.Maximum = XMax;
        chartHSV.ChartAreas[0].AxisX.Interval = 2.0;
        chartHSV.ChartAreas[0].AxisY.Minimum = YMin;
        chartHSV.ChartAreas[0].AxisY.Maximum = YMax;
        chartHSV.ChartAreas[0].AxisY.Interval = (YMax - YMin) / 8;
        chartHSV.Legends[0].Enabled = false;
        numericUpDownX.Minimum = XMin;
        numericUpDownX.Maximum = XMax;
        numericUpDownY.Minimum = YMin;
        numericUpDownY.Maximum = YMax;
        UpdateSeriesPoint();
    }

    private void chartHSV_MouseDown(object sender, MouseEventArgs e)
    {
        int num = (YMax - YMin) / 16;
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartHSV, e.X, e.Y);
        if (axisValuesFromMouse == null)
        {
            return;
        }
        Point point = new Point((int)Math.Round(axisValuesFromMouse.Item1), (int)Math.Round(axisValuesFromMouse.Item2));
        for (int i = 0; i < seriesPoint.Points.Count; i++)
        {
            if (seriesPoint.Points[i].XValue == point.X && seriesPoint.Points[i].YValues[0] >= point.Y - num && seriesPoint.Points[i].YValues[0] <= point.Y + num)
            {
                nSelectIdx = i;
                chartHSV.Cursor = Cursors.NoMoveVert;
                bSelectPoint = true;
                numericUpDownX.Value = (decimal)seriesPoint.Points[i].XValue;
                numericUpDownY.Value = (decimal)seriesPoint.Points[i].YValues[0];
                break;
            }
        }
    }

    private void chartHSV_MouseMove(object sender, MouseEventArgs e)
    {
        Tuple<double, double> axisValuesFromMouse = GetAxisValuesFromMouse(ref chartHSV, e.X, e.Y);
        if (axisValuesFromMouse == null || !bSelectPoint || axisValuesFromMouse == null)
        {
            return;
        }
        if (axisValuesFromMouse.Item2 < YMin)
        {
            featurePoint[nSelectIdx] = new Point(featurePoint[nSelectIdx].X, YMin);
        }
        else if (axisValuesFromMouse.Item2 > YMax)
        {
            featurePoint[nSelectIdx] = new Point(featurePoint[nSelectIdx].X, YMax);
        }
        else
        {
            featurePoint[nSelectIdx] = new Point(featurePoint[nSelectIdx].X, (int)Math.Round(axisValuesFromMouse.Item2));
        }
        seriesPoint.Points.Clear();
        foreach (Point item in featurePoint)
        {
            seriesPoint.Points.AddXY(item.X, item.Y);
        }
        numericUpDownY.Value = featurePoint[nSelectIdx].Y;
        if (group.Action == API_ACTION.RW)
        {
            _ = m_bAutoWrite;
        }
    }

    private void chartHSV_MouseUp(object sender, MouseEventArgs e)
    {
        if (bSelectPoint && group.Action == API_ACTION.RW && m_bAutoWrite)
        {
            UpdateGuiItem();
        }
        bSelectPoint = false;
        chartHSV.Cursor = Cursors.Cross;
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

    private void UpdateGuiItem()
    {
        int selectedIndex = comboBoxIndex.SelectedIndex;
        item.DataValue[selectedIndex * item.XSize + nSelectIdx] = (long)seriesPoint.Points[nSelectIdx].YValues[0];
        group.WriteGroup(comm);
    }

    private void UpdateSeriesPoint()
    {
        int selectedIndex = comboBoxIndex.SelectedIndex;
        featurePoint.Clear();
        seriesPoint.Points.Clear();
        for (int i = 0; i < item.XSize; i++)
        {
            seriesPoint.Points.AddXY(i, item.DataValue[selectedIndex * item.XSize + i]);
            Point point = new Point(i, (int)item.DataValue[selectedIndex * item.XSize + i]);
            featurePoint.Add(point);
        }
        numericUpDownX.Value = 0m;
        numericUpDownY.Value = featurePoint[0].Y;
    }

    private void comboBoxIndex_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateSeriesPoint();
    }

    private void numericUpDownX_ValueChanged(object sender, EventArgs e)
    {
        nSelectIdx = (int)numericUpDownX.Value;
        numericUpDownY.Value = featurePoint[nSelectIdx].Y;
    }

    private void numericUpDownY_ValueChanged(object sender, EventArgs e)
    {
        if (seriesPoint.Points[nSelectIdx].YValues[0] == (double)numericUpDownY.Value)
        {
            return;
        }
        featurePoint[nSelectIdx] = new Point(nSelectIdx, (int)numericUpDownY.Value);
        seriesPoint.Points.Clear();
        foreach (Point item in featurePoint)
        {
            seriesPoint.Points.AddXY(item.X, item.Y);
        }
        if (group.Action == API_ACTION.RW && m_bAutoWrite)
        {
            UpdateGuiItem();
        }
    }

    private void butWrite_Click(object sender, EventArgs e)
    {
        if (group.Action == API_ACTION.RW)
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
        ChartArea chartArea = new ChartArea();
        Legend legend = new Legend();
        Series series = new Series();
        Series series2 = new Series();
        chartHSV = new Chart();
        comboBoxIndex = new ComboBox();
        labelText = new Label();
        labelX = new Label();
        numericUpDownX = new NumericUpDown();
        labelY = new Label();
        numericUpDownY = new NumericUpDown();
        butWrite = new Button();
        ((ISupportInitialize)chartHSV).BeginInit();
        ((ISupportInitialize)numericUpDownX).BeginInit();
        ((ISupportInitialize)numericUpDownY).BeginInit();
        SuspendLayout();
        chartArea.AxisX.LineDashStyle = ChartDashStyle.Dot;
        chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
        chartArea.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
        chartArea.AxisX2.LineDashStyle = ChartDashStyle.Dot;
        chartArea.AxisX2.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
        chartArea.AxisX2.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
        chartArea.AxisY.LineDashStyle = ChartDashStyle.Dot;
        chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
        chartArea.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
        chartArea.AxisY2.LineDashStyle = ChartDashStyle.Dot;
        chartArea.AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
        chartArea.AxisY2.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
        chartArea.BackImageAlignment = ChartImageAlignmentStyle.Center;
        chartArea.BackImageWrapMode = ChartImageWrapMode.Scaled;
        chartArea.Name = "ChartArea1";
        chartHSV.ChartAreas.Add(chartArea);
        chartHSV.Cursor = Cursors.Cross;
        legend.Name = "Legend1";
        chartHSV.Legends.Add(legend);
        chartHSV.Location = new Point(25, 68);
        chartHSV.Name = "chartHSV";
        series.ChartArea = "ChartArea1";
        series.ChartType = SeriesChartType.Line;
        series.Color = Color.Gray;
        series.Legend = "Legend1";
        series.Name = "SeriesLine";
        series2.BorderColor = Color.Gray;
        series2.ChartArea = "ChartArea1";
        series2.ChartType = SeriesChartType.Point;
        series2.Color = Color.FromArgb(64, 64, 64);
        series2.Legend = "Legend1";
        series2.Name = "SeriesPoint";
        series2.YValuesPerPoint = 2;
        chartHSV.Series.Add(series);
        chartHSV.Series.Add(series2);
        chartHSV.Size = new Size(642, 200);
        chartHSV.TabIndex = 140;
        chartHSV.Text = "chartHSV";
        chartHSV.MouseDown += new MouseEventHandler(chartHSV_MouseDown);
        chartHSV.MouseMove += new MouseEventHandler(chartHSV_MouseMove);
        chartHSV.MouseUp += new MouseEventHandler(chartHSV_MouseUp);
        comboBoxIndex.FormattingEnabled = true;
        comboBoxIndex.Items.AddRange(new object[16]
        {
            "Index 0", "Index 1", "Index 2", "Index 3", "Index 4", "Index 5", "Index 6", "Index 7", "Index 8", "Index 9",
            "Index 10", "Index 11", "Index 12", "Index 13", "Index 14", "Index 15"
        });
        comboBoxIndex.SelectedIndex = 0;
        comboBoxIndex.Location = new Point(582, 30);
        comboBoxIndex.Name = "comboBoxIndex";
        comboBoxIndex.Size = new Size(85, 25);
        comboBoxIndex.TabIndex = 141;
        comboBoxIndex.SelectedIndexChanged += new EventHandler(comboBoxIndex_SelectedIndexChanged);
        labelText.AutoSize = true;
        labelText.Location = new Point(26, 30);
        labelText.Name = "labelText";
        labelText.Size = new Size(32, 17);
        labelText.TabIndex = 144;
        labelText.Text = "Text";
        labelX.AutoSize = true;
        labelX.Location = new Point(26, 289);
        labelX.Name = "labelX";
        labelX.Size = new Size(19, 17);
        labelX.TabIndex = 145;
        labelX.Text = "X:";
        numericUpDownX.Location = new Point(49, 287);
        numericUpDownX.Name = "numericUpDownX";
        numericUpDownX.Size = new Size(54, 23);
        numericUpDownX.TabIndex = 146;
        numericUpDownX.ValueChanged += new EventHandler(numericUpDownX_ValueChanged);
        labelY.AutoSize = true;
        labelY.Location = new Point(158, 289);
        labelY.Name = "labelY";
        labelY.Size = new Size(36, 17);
        labelY.TabIndex = 147;
        labelY.Text = "Shift:";
        numericUpDownY.Location = new Point(205, 287);
        numericUpDownY.Name = "numericUpDownY";
        numericUpDownY.Size = new Size(54, 23);
        numericUpDownY.TabIndex = 148;
        numericUpDownY.ValueChanged += new EventHandler(numericUpDownY_ValueChanged);
        butWrite.Location = new Point(315, 286);
        butWrite.Name = "butWrite";
        butWrite.Size = new Size(75, 23);
        butWrite.TabIndex = 149;
        butWrite.Text = "Write";
        butWrite.UseVisualStyleBackColor = true;
        butWrite.Click += new EventHandler(butWrite_Click);
        AutoScaleDimensions = new SizeF(7f, 17f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(685, 362);
        Controls.Add(butWrite);
        Controls.Add(numericUpDownY);
        Controls.Add(labelY);
        Controls.Add(numericUpDownX);
        Controls.Add(labelX);
        Controls.Add(labelText);
        Controls.Add(comboBoxIndex);
        Controls.Add(chartHSV);
        Font = new Font("Times New Roman", 9f);
        Name = "FormHSV";
        Text = "HSV";
        ((ISupportInitialize)chartHSV).EndInit();
        ((ISupportInitialize)numericUpDownX).EndInit();
        ((ISupportInitialize)numericUpDownY).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
