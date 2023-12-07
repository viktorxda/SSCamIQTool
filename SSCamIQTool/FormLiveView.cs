using SSCamIQTool.LibComm;
using SSCamIQTool.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SSCamIQTool.SSCamIQTool;

public class FormLiveView : Form
{
    public class Gdi32
    {
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC([In] IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC([In] IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    }

    public struct RECT
    {
        public int left;

        public int top;

        public int right;

        public int bottom;
    }

    public static class User32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
    }

    public delegate void ReleaseForm();

    public static uint PW_CLIENTONLY = 1u;

    public static uint CF_BITMAP = 2u;

    private ReleaseForm m_funcReleaseForm;

    public Process m_ffplayLiveView = new Process();

    public Process m_ffmpegLiveView = new Process();

    public Process m_ffmpegCapturePictureView = new Process();

    public Process m_ffmpegVideoCaptureContiPictureView = new Process();

    private int m_nTimeOutNum = 50;

    private bool m_bLiveViewStart;

    private bool m_bVedioCaptureStart;

    private bool m_bVedioImageContiStart;

    private bool m_bContinuousCaptureStart;

    private int m_nLiveViewWidth;

    private int m_nLiveViewHeight;

    private FormWindowState m_wsPreWindState;

    private int captureNumber;

    private int delaySec;

    private int number = 100;

    private Image zoomImage;

    private Graphics graphics0;

    private int x;

    private int y;

    private int newWidth;

    private int newHeight;

    private Bitmap m_captureImage;

    private bool mousedown;

    private int oriX;

    private int oriY;

    private int moveX;

    private int moveY;

    private int nextX;

    private int nextY;

    private string VideoPath;

    private string liveViewIPAddress = "";

    private ConnectSetting connSetting;

    private IContainer components = null;

    private Panel panelLiveView;

    private CheckBox chkFitToScreen;

    private ToolStrip toolStrip1;

    private ToolStripLabel labelArg;

    private ToolStripTextBox txbLiveViewExtraArg;

    private ToolStripButton btnStartLiveView;

    private ToolStripButton btnCaptureImage;

    private ToolStripButton btnVideo;

    private ToolStripButton btnContinuousCapture;

    private ToolStripLabel labelNumberOfCapture;

    private ToolStripTextBox textBoxNumberOfCapture;

    private ToolStripLabel labelDelaySec;

    private ToolStripTextBox textBoxDelaySec;

    private ToolStripButton LoadImagetoolStripButton;

    private ToolStripButton saveImgtoolStripButton;

    private PictureBox captureImagepictureBox;

    private ToolStripButton VideoImagetoolStripButton;

    public FormLiveView()
    {
        InitializeComponent();
        Application.EnableVisualStyles();
        DoubleBuffered = true;
    }

    private void FormLiveView_FormClosed(object sender, FormClosedEventArgs e)
    {
        m_funcReleaseForm();
    }

    public void SetReleaseFunc(ReleaseForm pfuncRelease)
    {
        m_funcReleaseForm = pfuncRelease;
    }

    public void setConnSetting(ConnectSetting setting)
    {
        connSetting = setting;
    }

    private void FFplayStart()
    {
        string text = Application.StartupPath + Settings.Default.LiveViewExe;
        if (File.Exists(text))
        {
            m_ffplayLiveView.StartInfo.FileName = text;
            m_ffplayLiveView.StartInfo.Arguments = txbLiveViewExtraArg.Text + " " + liveViewIPAddress;
            m_ffplayLiveView.StartInfo.CreateNoWindow = true;
            m_ffplayLiveView.StartInfo.RedirectStandardOutput = true;
            m_ffplayLiveView.StartInfo.UseShellExecute = false;
            m_ffplayLiveView.EnableRaisingEvents = true;
            m_ffplayLiveView.OutputDataReceived += delegate
            {
            };
            m_ffplayLiveView.ErrorDataReceived += delegate
            {
            };
            m_ffplayLiveView.Exited += delegate
            {
            };
            m_ffplayLiveView.Start();
            Thread.Sleep(500);
            if (User32.SetParent(m_ffplayLiveView.MainWindowHandle, panelLiveView.Handle) == IntPtr.Zero)
            {
                for (int i = 0; i < m_nTimeOutNum; i++)
                {
                    Thread.Sleep(500);
                    if (User32.SetParent(m_ffplayLiveView.MainWindowHandle, panelLiveView.Handle) != IntPtr.Zero)
                    {
                        break;
                    }
                }
            }
            m_bLiveViewStart = true;
            RECT rect = default;
            User32.GetWindowRect(m_ffplayLiveView.MainWindowHandle, ref rect);
            m_nLiveViewWidth = rect.right - rect.left;
            m_nLiveViewHeight = rect.bottom - rect.top;
            SetLiveViewWindow();
        }
        else
        {
            MessageBox.Show("There is no ffplay.exe file in <Bin> folder.", "Live View", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
    }

    private void FFplayStop()
    {
        try
        {
            m_ffplayLiveView.Kill();
            m_bLiveViewStart = false;
        }
        catch
        {
        }
    }

    public static Bitmap PrintInactiveWindow(IntPtr hwnd)
    {
        RECT rect = default;
        User32.GetWindowRect(hwnd, ref rect);
        IntPtr dC = User32.GetDC(hwnd);
        IntPtr hDC = Gdi32.CreateCompatibleDC(dC);
        IntPtr intPtr = Gdi32.CreateCompatibleBitmap(dC, rect.right - rect.left, rect.bottom - rect.top);
        Gdi32.SelectObject(hDC, intPtr);
        User32.PrintWindow(hwnd, hDC, PW_CLIENTONLY);
        User32.OpenClipboard(IntPtr.Zero);
        User32.EmptyClipboard();
        User32.SetClipboardData(CF_BITMAP, intPtr);
        User32.CloseClipboard();
        Bitmap result = Image.FromHbitmap(intPtr);
        Gdi32.DeleteDC(hDC);
        Gdi32.DeleteObject(intPtr);
        User32.ReleaseDC(IntPtr.Zero, dC);
        return result;
    }

    public static Bitmap PrintWindow(IntPtr hwnd)
    {
        RECT lpRect = default;
        User32.GetClientRect(hwnd, out lpRect);
        int num = lpRect.right - lpRect.left;
        int num2 = lpRect.bottom - lpRect.top;
        Bitmap bitmap = new Bitmap(num, num2, PixelFormat.Format32bppArgb);
        Graphics graphics = Graphics.FromImage(bitmap);
        IntPtr hdc = graphics.GetHdc();
        User32.PrintWindow(hwnd, hdc, PW_CLIENTONLY);
        graphics.ReleaseHdc(hdc);
        graphics.Dispose();
        return bitmap;
    }

    private void SetOriginalScreen()
    {
        if (m_bLiveViewStart && m_ffplayLiveView.MainWindowHandle != IntPtr.Zero)
        {
            int num = Size.Width - ClientSize.Width;
            int num2 = Size.Height - ClientSize.Height;
            int num3 = (Size.Width - ClientSize.Width + 1) / 2;
            int num4 = num3;
            int num5 = Size.Height - ClientSize.Height - num3;
            panelLiveView.Width = m_nLiveViewWidth - num;
            panelLiveView.Height = m_nLiveViewHeight - num2;
            User32.MoveWindow(m_ffplayLiveView.MainWindowHandle, -num4, -num5, m_nLiveViewWidth, m_nLiveViewHeight, bRepaint: true);
        }
    }

    private void FitToScreen()
    {
        if (m_bLiveViewStart && m_ffplayLiveView.MainWindowHandle != IntPtr.Zero)
        {
            int num = Size.Width - ClientSize.Width;
            int num2 = Size.Height - ClientSize.Height;
            int num3 = (Size.Width - ClientSize.Width + 1) / 2;
            int num4 = num3;
            int num5 = Size.Height - ClientSize.Height - num3;
            int num6 = ClientSize.Width - panelLiveView.Location.X * 2;
            int num7 = ClientSize.Height - panelLiveView.Location.Y - num3;
            if (VerticalScroll.Visible)
            {
                num6 += SystemInformation.VerticalScrollBarWidth;
            }
            if (HorizontalScroll.Visible)
            {
                num7 += SystemInformation.HorizontalScrollBarHeight;
            }
            int nWidth = num6 + num;
            int nHeight = num7 + num2;
            panelLiveView.Width = num6;
            panelLiveView.Height = num7;
            User32.MoveWindow(m_ffplayLiveView.MainWindowHandle, -num4, -num5, nWidth, nHeight, bRepaint: true);
            HorizontalScroll.Maximum = 100;
            VerticalScroll.Maximum = 100;
            AutoScroll = false;
            HorizontalScroll.Visible = false;
            VerticalScroll.Visible = false;
            AutoScroll = true;
        }
    }

    private void SetLiveViewWindow()
    {
        if (chkFitToScreen.Checked)
        {
            FitToScreen();
        }
        else
        {
            SetOriginalScreen();
        }
    }

    private void chkFitToScreen_CheckedChanged(object sender, EventArgs e)
    {
        SetLiveViewWindow();
    }

    private void FormLiveView_SizeChanged(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Maximized)
        {
            if (chkFitToScreen.Checked)
            {
                FitToScreen();
            }
            m_wsPreWindState = FormWindowState.Maximized;
        }
        if (WindowState == FormWindowState.Normal && m_wsPreWindState == FormWindowState.Maximized && chkFitToScreen.Checked)
        {
            FitToScreen();
            m_wsPreWindState = FormWindowState.Normal;
        }
    }

    private void FormLiveView_ResizeEnd(object sender, EventArgs e)
    {
        if (chkFitToScreen.Checked)
        {
            FitToScreen();
        }
    }

    private void FormLiveView_ResizeBegin(object sender, EventArgs e)
    {
        if (chkFitToScreen.Checked)
        {
            AutoScroll = false;
        }
    }

    private void VideoCaptureStart()
    {
        string text = Application.StartupPath + Settings.Default.SaveFolder;
        if (!Directory.Exists(text))
        {
            Directory.CreateDirectory(text);
        }
        string text2 = Application.StartupPath + "\\bin\\ffmpeg.exe";
        if (File.Exists(text2))
        {
            new ProcessStartInfo();
            DateTime now = DateTime.Now;
            m_ffmpegLiveView.StartInfo.FileName = text2;
            string text3 = text + "\\capture_" + now.Month.ToString("D2") + now.Day.ToString("D2") + now.Hour.ToString("D2") + now.Minute.ToString("D2") + now.Second.ToString("D2") + "video.avi";
            m_ffmpegLiveView.StartInfo.Arguments = "-i " + liveViewIPAddress + " " + text3;
            m_ffmpegLiveView.StartInfo.CreateNoWindow = false;
            m_ffmpegLiveView.StartInfo.RedirectStandardOutput = false;
            m_ffmpegLiveView.StartInfo.UseShellExecute = false;
            m_ffmpegLiveView.EnableRaisingEvents = true;
            m_ffmpegLiveView.OutputDataReceived += delegate
            {
            };
            m_ffmpegLiveView.ErrorDataReceived += delegate
            {
            };
            m_ffmpegLiveView.Exited += delegate
            {
            };
            m_ffmpegLiveView.Start();
            m_bVedioCaptureStart = true;
        }
        else
        {
            MessageBox.Show("There is no ffmpeg.exe file in <Bin> folder.", "Live View", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
    }

    private void textBoxNumberOfCapture_TextChanged(object sender, EventArgs e)
    {
    }

    private void textBoxDelaySec_TextChanged(object sender, EventArgs e)
    {
    }

    private void btnStartLiveView_Click(object sender, EventArgs e)
    {
        if (connSetting.IsEmpty())
        {
            MessageBox.Show("Please check connection", "Live View", MessageBoxButtons.OK);
            return;
        }
        captureImagepictureBox.Visible = false;
        liveViewIPAddress = "rtsp://" + connSetting.HostName + "/video0";
        if (m_bLiveViewStart)
        {
            FFplayStop();
            btnStartLiveView.Image = GuiParser.GetImageByName("play.png");
            btnStartLiveView.Text = "Start";
        }
        else
        {
            FFplayStart();
            btnStartLiveView.Image = GuiParser.GetImageByName("stop.png");
            btnStartLiveView.Text = "Stop";
        }
    }

    private void btnCaptureImage_Click(object sender, EventArgs e)
    {
        captureImagepictureBox.Visible = false;
        captureNumber = int.Parse(textBoxNumberOfCapture.Text);
        delaySec = (int)(Convert.ToSingle(textBoxDelaySec.Text) * 1000f);
        if (m_bLiveViewStart && m_ffplayLiveView.MainWindowHandle != IntPtr.Zero)
        {
            User32.SetParent(m_ffplayLiveView.MainWindowHandle, IntPtr.Zero);
            User32.MoveWindow(m_ffplayLiveView.MainWindowHandle, 0, 0, m_nLiveViewWidth, m_nLiveViewHeight, bRepaint: true);
            string text = Application.StartupPath + Settings.Default.SaveFolder;
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            for (int i = 1; i < captureNumber + 1; i++)
            {
                DateTime now = DateTime.Now;
                string filename = text + "\\capture_" + now.Month.ToString("D2") + now.Day.ToString("D2") + now.Hour.ToString("D2") + now.Minute.ToString("D2") + now.Second.ToString("D2") + "_" + i + ".png";
                Thread.Sleep(delaySec);
                PrintWindow(m_ffplayLiveView.MainWindowHandle).Save(filename, ImageFormat.Png);
            }
            User32.SetParent(m_ffplayLiveView.MainWindowHandle, panelLiveView.Handle);
            SetLiveViewWindow();
        }
    }

    private void VideoCaptureStop()
    {
        try
        {
            m_ffmpegLiveView.Kill();
            m_bVedioCaptureStart = false;
        }
        catch
        {
        }
    }

    private void btnVideo_Click(object sender, EventArgs e)
    {
        if (connSetting.IsEmpty())
        {
            MessageBox.Show("Please check connection", "Live View", MessageBoxButtons.OK);
            return;
        }
        captureImagepictureBox.Visible = false;
        liveViewIPAddress = "rtsp://" + connSetting.HostName + "/video0";
        if (m_bVedioCaptureStart)
        {
            VideoCaptureStop();
            btnVideo.Image = GuiParser.GetImageByName("videoCamera.png");
            btnVideo.Text = "video save start";
        }
        else
        {
            VideoCaptureStart();
            btnVideo.Image = GuiParser.GetImageByName("videoStop.png");
            btnVideo.Text = "video save stop";
        }
    }

    private void VideoImageContiCaptureStop()
    {
        try
        {
            m_ffmpegVideoCaptureContiPictureView.Kill();
            m_bVedioCaptureStart = false;
        }
        catch
        {
        }
    }

    private void VideoImageContiCaptureStart()
    {
        string text = string.Concat(Application.StartupPath + Settings.Default.SaveFolder, "\\VideoCaptureImageContinuous");
        if (!Directory.Exists(text))
        {
            Directory.CreateDirectory(text);
        }
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.ShowDialog();
        openFileDialog.Filter = "Media Files|*.mpg;*.avi;*.wma;*.mov;*.wav;*.mp2;*.mp3|All Files|*.*";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            VideoPath = openFileDialog.FileName;
        }
        string text2 = Application.StartupPath + "\\bin\\ffmpeg.exe";
        if (File.Exists(text2))
        {
            new ProcessStartInfo();
            DateTime now = DateTime.Now;
            m_ffmpegVideoCaptureContiPictureView.StartInfo.FileName = text2;
            string text3 = text + "\\captureVidImgContinuous_" + now.Month.ToString("D2") + now.Day.ToString("D2") + now.Hour.ToString("D2") + now.Minute.ToString("D2") + now.Second.ToString("D2") + "-%4d.png";
            m_ffmpegVideoCaptureContiPictureView.StartInfo.Arguments = "-i " + VideoPath + " " + text3;
            m_ffmpegVideoCaptureContiPictureView.StartInfo.CreateNoWindow = false;
            m_ffmpegVideoCaptureContiPictureView.StartInfo.RedirectStandardOutput = false;
            m_ffmpegVideoCaptureContiPictureView.StartInfo.UseShellExecute = false;
            m_ffmpegVideoCaptureContiPictureView.EnableRaisingEvents = true;
            m_ffmpegVideoCaptureContiPictureView.Start();
            m_bVedioImageContiStart = true;
        }
        else
        {
            MessageBox.Show("There is no ffmpeg.exe file in <Bin> folder.", "Live View", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
    }

    private void VideoImagetoolStripButton_Click(object sender, EventArgs e)
    {
        captureImagepictureBox.Visible = false;
        if (m_bVedioImageContiStart)
        {
            VideoImageContiCaptureStop();
            VideoImagetoolStripButton.Text = "video Capture Image Continuous Save Start";
        }
        else
        {
            VideoImageContiCaptureStart();
            VideoImagetoolStripButton.Text = "video Capture Image Continuous Save Stop";
        }
    }

    private void ContinuousCaptureImg()
    {
        string text = Application.StartupPath + Settings.Default.SaveFolder;
        string path = text + "\\captureContinuous";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string text2 = Application.StartupPath + "\\bin\\ffmpeg.exe";
        if (File.Exists(text2))
        {
            new ProcessStartInfo();
            DateTime now = DateTime.Now;
            m_ffmpegCapturePictureView.StartInfo.FileName = text2;
            string text3 = text + "\\captureContinuous\\captureContinuous_" + now.Month.ToString("D2") + now.Day.ToString("D2") + now.Hour.ToString("D2") + now.Minute.ToString("D2") + now.Second.ToString("D2") + "-%4d.png";
            m_ffmpegCapturePictureView.StartInfo.Arguments = "-i " + liveViewIPAddress + " " + text3;
            m_ffmpegCapturePictureView.StartInfo.CreateNoWindow = false;
            m_ffmpegCapturePictureView.StartInfo.RedirectStandardOutput = false;
            m_ffmpegCapturePictureView.StartInfo.UseShellExecute = false;
            m_ffmpegCapturePictureView.EnableRaisingEvents = true;
            m_ffmpegCapturePictureView.Start();
            m_bContinuousCaptureStart = true;
        }
        else
        {
            MessageBox.Show("There is no ffmpeg.exe file in <Bin> folder.", "Live View", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
    }

    private void ContinuousCaptureImgStop()
    {
        try
        {
            m_ffmpegCapturePictureView.Kill();
            m_bContinuousCaptureStart = false;
        }
        catch
        {
        }
    }

    private void btnContinuousCapture_Click(object sender, EventArgs e)
    {
        if (connSetting.IsEmpty())
        {
            MessageBox.Show("Please check connection", "Live View", MessageBoxButtons.OK);
            return;
        }
        captureImagepictureBox.Visible = false;
        liveViewIPAddress = "rtsp://" + connSetting.HostName + "/video0";
        if (m_bContinuousCaptureStart)
        {
            ContinuousCaptureImgStop();
            btnContinuousCapture.Image = GuiParser.GetImageByName("continuousCapture.png");
            btnContinuousCapture.Text = "continuous capture frame image start";
        }
        else
        {
            ContinuousCaptureImg();
            btnContinuousCapture.Image = GuiParser.GetImageByName("continuousStop.png");
            btnContinuousCapture.Text = "Stop";
        }
    }

    private void LoadImagetoolStripButton_Click(object sender, EventArgs e)
    {
        captureImagepictureBox.Visible = true;
        captureImagepictureBox.Image = null;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Title = "Open the img file you wish";
        openFileDialog.InitialDirectory = Application.StartupPath;
        openFileDialog.Filter = "Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Png Image (.png)|*.png|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            m_captureImage = new Bitmap(openFileDialog.FileName);
            captureImagepictureBox.Image = m_captureImage;
            Bitmap image = (Bitmap)captureImagepictureBox.Image;
            captureImagepictureBox.Image = image;
        }
    }

    private void saveImgtoolStripButton_Click(object sender, EventArgs e)
    {
    }

    private void captureImagepictureBox_MouseEnter(object sender, EventArgs e)
    {
        captureImagepictureBox.Focus();
        captureImagepictureBox.MouseWheel += captureImagepictureBox_MouseWheel;
    }

    private void captureImagepictureBox_MouseWheel(object sender, MouseEventArgs e)
    {
        try
        {
            captureImagepictureBox.Image = null;
            int num = 0;
            if (e.Delta == -120)
            {
                num = GetZoomStep();
                number -= num;
                if (number < 5)
                {
                    number = 5;
                }
            }
            else
            {
                num = GetZoomStep();
                number += num;
            }
            zoomImage = new Bitmap(m_captureImage.Width, m_captureImage.Height);
            graphics0 = Graphics.FromImage(zoomImage);
            newWidth = m_captureImage.Width * number / 100;
            newHeight = m_captureImage.Height * number / 100;
            graphics0.DrawImage(m_captureImage, x, y, newWidth, newHeight);
            graphics0.Dispose();
            captureImagepictureBox.BackgroundImage = zoomImage;
        }
        catch
        {
            Console.WriteLine("Mouse Wheel Error!!");
        }
    }

    private int GetZoomStep()
    {
        number = number / 5 * 5;
        if (number <= 50)
        {
            return 5;
        }
        if (number <= 100)
        {
            return 10;
        }
        if (number <= 200)
        {
            return 25;
        }
        return 50;
    }

    private void captureImagepictureBox_MouseDown(object sender, MouseEventArgs e)
    {
        mousedown = true;
        oriX = e.X;
        oriY = e.Y;
    }

    private void captureImagepictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (mousedown)
        {
            try
            {
                moveX = e.X;
                moveY = e.Y;
                nextX = moveX - oriX;
                nextY = moveY - oriY;
                zoomImage = new Bitmap(m_captureImage.Width, m_captureImage.Height);
                graphics0 = Graphics.FromImage(zoomImage);
                newWidth = m_captureImage.Width * number / 100;
                newHeight = m_captureImage.Height * number / 100;
                Thread.Sleep(150);
                graphics0.DrawImage(m_captureImage, x + nextX, y + nextY, newWidth, newHeight);
                graphics0.Dispose();
                captureImagepictureBox.BackgroundImage = zoomImage;
                _ = x;
                _ = nextX;
                _ = y;
                _ = nextY;
            }
            catch
            {
                Console.WriteLine("Mouse Move Error!!");
            }
        }
    }

    private void captureImagepictureBox_MouseUp(object sender, MouseEventArgs e)
    {
        try
        {
            x += nextX;
            y += nextY;
            mousedown = false;
            zoomImage = new Bitmap(m_captureImage.Width, m_captureImage.Height);
            graphics0 = Graphics.FromImage(zoomImage);
            newWidth = m_captureImage.Width * number / 100;
            newHeight = m_captureImage.Height * number / 100;
            graphics0.DrawImage(m_captureImage, x, y, newWidth, newHeight);
            graphics0.Dispose();
            captureImagepictureBox.BackgroundImage = zoomImage;
        }
        catch
        {
            Console.WriteLine("Mouse Up Error!!");
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
        ComponentResourceManager resources = new ComponentResourceManager(typeof(FormLiveView));
        panelLiveView = new Panel();
        captureImagepictureBox = new PictureBox();
        chkFitToScreen = new CheckBox();
        toolStrip1 = new ToolStrip();
        labelArg = new ToolStripLabel();
        txbLiveViewExtraArg = new ToolStripTextBox();
        btnStartLiveView = new ToolStripButton();
        btnCaptureImage = new ToolStripButton();
        btnVideo = new ToolStripButton();
        VideoImagetoolStripButton = new ToolStripButton();
        btnContinuousCapture = new ToolStripButton();
        labelNumberOfCapture = new ToolStripLabel();
        textBoxNumberOfCapture = new ToolStripTextBox();
        labelDelaySec = new ToolStripLabel();
        textBoxDelaySec = new ToolStripTextBox();
        LoadImagetoolStripButton = new ToolStripButton();
        saveImgtoolStripButton = new ToolStripButton();
        panelLiveView.SuspendLayout();
        ((ISupportInitialize)captureImagepictureBox).BeginInit();
        toolStrip1.SuspendLayout();
        SuspendLayout();
        panelLiveView.BackColor = Color.LightCyan;
        panelLiveView.Controls.Add(captureImagepictureBox);
        panelLiveView.Location = new Point(9, 50);
        panelLiveView.Name = "panelLiveView";
        panelLiveView.Size = new Size(897, 469);
        panelLiveView.TabIndex = 3;
        captureImagepictureBox.BackColor = Color.LightYellow;
        captureImagepictureBox.Cursor = Cursors.Hand;
        captureImagepictureBox.Location = new Point(0, 0);
        captureImagepictureBox.Name = "captureImagepictureBox";
        captureImagepictureBox.Size = new Size(897, 469);
        captureImagepictureBox.TabIndex = 0;
        captureImagepictureBox.TabStop = false;
        captureImagepictureBox.Visible = false;
        captureImagepictureBox.MouseDown += new MouseEventHandler(captureImagepictureBox_MouseDown);
        captureImagepictureBox.MouseEnter += new EventHandler(captureImagepictureBox_MouseEnter);
        captureImagepictureBox.MouseMove += new MouseEventHandler(captureImagepictureBox_MouseMove);
        captureImagepictureBox.MouseUp += new MouseEventHandler(captureImagepictureBox_MouseUp);
        chkFitToScreen.AutoSize = true;
        chkFitToScreen.BackColor = Color.WhiteSmoke;
        chkFitToScreen.Font = new Font("Times New Roman", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        chkFitToScreen.ForeColor = Color.Blue;
        chkFitToScreen.Location = new Point(786, 16);
        chkFitToScreen.Name = "chkFitToScreen";
        chkFitToScreen.Size = new Size(99, 19);
        chkFitToScreen.TabIndex = 5;
        chkFitToScreen.Text = "Fit To Screen";
        chkFitToScreen.UseVisualStyleBackColor = false;
        chkFitToScreen.CheckedChanged += new EventHandler(chkFitToScreen_CheckedChanged);
        toolStrip1.AutoSize = false;
        toolStrip1.BackColor = Color.WhiteSmoke;
        toolStrip1.Dock = DockStyle.None;
        toolStrip1.Items.AddRange(new ToolStripItem[13]
        {
            labelArg, txbLiveViewExtraArg, btnStartLiveView, btnCaptureImage, btnVideo, VideoImagetoolStripButton, btnContinuousCapture, labelNumberOfCapture, textBoxNumberOfCapture, labelDelaySec,
            textBoxDelaySec, LoadImagetoolStripButton, saveImgtoolStripButton
        });
        toolStrip1.Location = new Point(9, 9);
        toolStrip1.Name = "toolStrip1";
        toolStrip1.Size = new Size(903, 35);
        toolStrip1.TabIndex = 12;
        toolStrip1.Text = "toolStrip1";
        labelArg.Font = new Font("Times New Roman", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelArg.ForeColor = Color.FromArgb(128, 64, 0);
        labelArg.Name = "labelArg";
        labelArg.Size = new Size(21, 32);
        labelArg.Text = "Arg:";
        txbLiveViewExtraArg.Name = "txbLiveViewExtraArg";
        txbLiveViewExtraArg.Size = new Size(160, 35);
        txbLiveViewExtraArg.Text = "";
        btnStartLiveView.AutoSize = false;
        btnStartLiveView.DisplayStyle = ToolStripItemDisplayStyle.Image;
        btnStartLiveView.Image = GuiParser.GetImageByName("btnStartLiveView.png");
        btnStartLiveView.ImageTransparentColor = Color.Magenta;
        btnStartLiveView.Name = "btnStartLiveView";
        btnStartLiveView.Size = new Size(36, 36);
        btnStartLiveView.Text = "start";
        btnStartLiveView.Click += new EventHandler(btnStartLiveView_Click);
        btnCaptureImage.AutoSize = false;
        btnCaptureImage.DisplayStyle = ToolStripItemDisplayStyle.Image;
        btnCaptureImage.Image = GuiParser.GetImageByName("btnCaptureImage.png");
        btnCaptureImage.ImageTransparentColor = Color.Magenta;
        btnCaptureImage.Name = "btnCaptureImage";
        btnCaptureImage.Size = new Size(36, 36);
        btnCaptureImage.Text = "capture";
        btnCaptureImage.Click += new EventHandler(btnCaptureImage_Click);
        btnVideo.AutoSize = false;
        btnVideo.DisplayStyle = ToolStripItemDisplayStyle.Image;
        btnVideo.Image = GuiParser.GetImageByName("btnVideo.png");
        btnVideo.ImageTransparentColor = Color.Magenta;
        btnVideo.Name = "btnVideo";
        btnVideo.Size = new Size(36, 36);
        btnVideo.Text = "video save start";
        btnVideo.Click += new EventHandler(btnVideo_Click);
        VideoImagetoolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        VideoImagetoolStripButton.Image = GuiParser.GetImageByName("videoImagetool.png");
        VideoImagetoolStripButton.ImageTransparentColor = Color.Magenta;
        VideoImagetoolStripButton.Name = "VideoImagetoolStripButton";
        VideoImagetoolStripButton.Size = new Size(23, 32);
        VideoImagetoolStripButton.Text = "Choose Video and capture continuous image";
        VideoImagetoolStripButton.Click += new EventHandler(VideoImagetoolStripButton_Click);
        btnContinuousCapture.AutoSize = false;
        btnContinuousCapture.DisplayStyle = ToolStripItemDisplayStyle.Image;
        btnContinuousCapture.Image = GuiParser.GetImageByName("btnContinuousCapture.png");
        btnContinuousCapture.ImageTransparentColor = Color.Magenta;
        btnContinuousCapture.Name = "btnContinuousCapture";
        btnContinuousCapture.Size = new Size(36, 36);
        btnContinuousCapture.Text = "continuous capture frame image";
        btnContinuousCapture.Click += new EventHandler(btnContinuousCapture_Click);
        labelNumberOfCapture.Font = new Font("Times New Roman", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelNumberOfCapture.ForeColor = Color.FromArgb(128, 64, 0);
        labelNumberOfCapture.Name = "labelNumberOfCapture";
        labelNumberOfCapture.Size = new Size(82, 32);
        labelNumberOfCapture.Text = "capture num:";
        textBoxNumberOfCapture.BackColor = Color.White;
        textBoxNumberOfCapture.BorderStyle = BorderStyle.FixedSingle;
        textBoxNumberOfCapture.Name = "textBoxNumberOfCapture";
        textBoxNumberOfCapture.Size = new Size(35, 35);
        textBoxNumberOfCapture.Text = "1";
        textBoxNumberOfCapture.TextChanged += new EventHandler(textBoxNumberOfCapture_TextChanged);
        labelDelaySec.Font = new Font("Times New Roman", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelDelaySec.ForeColor = Color.FromArgb(128, 64, 0);
        labelDelaySec.Name = "labelDelaySec";
        labelDelaySec.Size = new Size(65, 32);
        labelDelaySec.Text = "delay Sec:";
        textBoxDelaySec.BackColor = Color.White;
        textBoxDelaySec.BorderStyle = BorderStyle.FixedSingle;
        textBoxDelaySec.Name = "textBoxDelaySec";
        textBoxDelaySec.Size = new Size(35, 35);
        textBoxDelaySec.Text = "0";
        textBoxDelaySec.TextChanged += new EventHandler(textBoxDelaySec_TextChanged);
        LoadImagetoolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        LoadImagetoolStripButton.Image = GuiParser.GetImageByName("LoadImgtool.png");
        LoadImagetoolStripButton.ImageTransparentColor = Color.Magenta;
        LoadImagetoolStripButton.Name = "LoadImagetoolStripButton";
        LoadImagetoolStripButton.Size = new Size(23, 32);
        LoadImagetoolStripButton.Text = "Load image";
        LoadImagetoolStripButton.Click += new EventHandler(LoadImagetoolStripButton_Click);
        saveImgtoolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        saveImgtoolStripButton.Image = GuiParser.GetImageByName("saveImgtool.png");
        saveImgtoolStripButton.ImageTransparentColor = Color.Magenta;
        saveImgtoolStripButton.Name = "saveImgtoolStripButton";
        saveImgtoolStripButton.Size = new Size(23, 32);
        saveImgtoolStripButton.Text = "toolStripButton2";
        saveImgtoolStripButton.ToolTipText = "save image";
        saveImgtoolStripButton.Visible = false;
        saveImgtoolStripButton.Click += new EventHandler(saveImgtoolStripButton_Click);
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Azure;
        ClientSize = new Size(924, 542);
        Controls.Add(chkFitToScreen);
        Controls.Add(toolStrip1);
        Controls.Add(panelLiveView);
        Name = "FormLiveView";
        Text = "FormLiveView";
        FormClosed += new FormClosedEventHandler(FormLiveView_FormClosed);
        panelLiveView.ResumeLayout(false);
        ((ISupportInitialize)captureImagepictureBox).EndInit();
        toolStrip1.ResumeLayout(false);
        toolStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
