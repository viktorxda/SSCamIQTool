using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SSCamIQTool.CamTool;

public class FormSetRawFrameCount : Form
{
    public delegate void SeFrameCountHandler(uint frame_count, uint loop_count);

    private readonly SeFrameCountHandler SetFrameCount;

    private readonly IContainer components = null;

    private NumericUpDown RawFrameCountNumericUpDown;

    private Label RawFrameCountLabel;

    private Button SaveRawFrameCountButton;

    private Label label1;

    private NumericUpDown RawFrameLoopCountNumericUpDown;

    public FormSetRawFrameCount(uint frame_count, uint loop_count, SeFrameCountHandler setRawCount)
    {
        InitializeComponent();
        SetFrameCount = setRawCount;
        if (loop_count == 0)
        {
            loop_count = 1u;
        }
        RawFrameCountNumericUpDown.Value = frame_count;
        RawFrameLoopCountNumericUpDown.Value = loop_count;
    }

    private void SaveRawFrameCountButton_Click(object sender, EventArgs e)
    {
        uint frame_count = uint.Parse(RawFrameCountNumericUpDown.Value.ToString());
        uint loop_count = uint.Parse(RawFrameLoopCountNumericUpDown.Value.ToString());
        SetFrameCount(frame_count, loop_count);
        Close();
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
        RawFrameCountNumericUpDown = new NumericUpDown();
        RawFrameCountLabel = new Label();
        SaveRawFrameCountButton = new Button();
        label1 = new Label();
        RawFrameLoopCountNumericUpDown = new NumericUpDown();
        ((ISupportInitialize)RawFrameCountNumericUpDown).BeginInit();
        ((ISupportInitialize)RawFrameLoopCountNumericUpDown).BeginInit();
        SuspendLayout();
        RawFrameCountNumericUpDown.Location = new System.Drawing.Point(112, 35);
        RawFrameCountNumericUpDown.Maximum = new decimal(new int[4] { 1000, 0, 0, 0 });
        RawFrameCountNumericUpDown.Name = "RawFrameCountNumericUpDown";
        RawFrameCountNumericUpDown.Size = new System.Drawing.Size(64, 21);
        RawFrameCountNumericUpDown.TabIndex = 0;
        RawFrameCountNumericUpDown.TextAlign = HorizontalAlignment.Center;
        RawFrameCountLabel.AutoSize = true;
        RawFrameCountLabel.Font = new System.Drawing.Font("Cambria", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        RawFrameCountLabel.Location = new System.Drawing.Point(12, 35);
        RawFrameCountLabel.Name = "RawFrameCountLabel";
        RawFrameCountLabel.Size = new System.Drawing.Size(84, 16);
        RawFrameCountLabel.TabIndex = 1;
        RawFrameCountLabel.Text = "FrameCount";
        SaveRawFrameCountButton.Font = new System.Drawing.Font("Cambria", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        SaveRawFrameCountButton.Location = new System.Drawing.Point(204, 45);
        SaveRawFrameCountButton.Name = "SaveRawFrameCountButton";
        SaveRawFrameCountButton.Size = new System.Drawing.Size(58, 23);
        SaveRawFrameCountButton.TabIndex = 2;
        SaveRawFrameCountButton.Text = "Set";
        SaveRawFrameCountButton.UseVisualStyleBackColor = true;
        SaveRawFrameCountButton.Click += new EventHandler(SaveRawFrameCountButton_Click);
        label1.AutoSize = true;
        label1.Font = new System.Drawing.Font("Cambria", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        label1.Location = new System.Drawing.Point(15, 65);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(77, 16);
        label1.TabIndex = 3;
        label1.Text = "Loop Count";
        RawFrameLoopCountNumericUpDown.Location = new System.Drawing.Point(112, 65);
        RawFrameLoopCountNumericUpDown.Maximum = new decimal(new int[4] { 1000, 0, 0, 0 });
        RawFrameLoopCountNumericUpDown.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
        RawFrameLoopCountNumericUpDown.Name = "RawFrameLoopCountNumericUpDown";
        RawFrameLoopCountNumericUpDown.Size = new System.Drawing.Size(64, 21);
        RawFrameLoopCountNumericUpDown.TabIndex = 4;
        RawFrameLoopCountNumericUpDown.TextAlign = HorizontalAlignment.Center;
        RawFrameLoopCountNumericUpDown.Value = new decimal(new int[4] { 1, 0, 0, 0 });
        AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(284, 109);
        Controls.Add(RawFrameLoopCountNumericUpDown);
        Controls.Add(label1);
        Controls.Add(SaveRawFrameCountButton);
        Controls.Add(RawFrameCountLabel);
        Controls.Add(RawFrameCountNumericUpDown);
        Name = "FormSetRawFrameCount";
        Text = "SetRawFrameCount";
        ((ISupportInitialize)RawFrameCountNumericUpDown).EndInit();
        ((ISupportInitialize)RawFrameLoopCountNumericUpDown).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
