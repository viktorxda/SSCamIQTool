using System;
using System.Collections.Generic;

namespace SSCamIQTool.SSCamIQTool;

internal class GenGlobalTone
{
    public long[] m_genParam = new long[7];

    public long[] m_genSft = new long[31];

    public List<long> m_genX = new List<long>();

    public List<long> m_genPoint = new List<long>();

    private List<double> m_genFcureLeft = new List<double>();

    private List<double> m_genFcureRight = new List<double>();

    private List<double> m_genBlending = new List<double>();

    private List<double> m_genFcureFinal = new List<double>();

    public string[] m_genParamTitle = new string[7] { "fdelta_left\n(-65536~65536)", "fdelta_right\n(-65536~65536)", "adelta\n(0~8196)", "amax\n(-1024~1024)", "amin\n(-1024~1024)", "aslope\n(0~8196)", "a_coe3\n(0~8196)" };

    public long[] m_genParamMin = new long[7] { -65536L, -65536L, 0L, -1024L, -1024L, 0L, 0L };

    public long[] m_genParamMax = new long[7] { 65536L, 65536L, 8196L, 1024L, 1024L, 8196L, 8196L };

    private long y_base = 4096L;

    public void CalcX()
    {
        m_genX.Clear();
        m_genX.Add(0L);
        for (int i = 1; i < 32; i++)
        {
            long item = m_genX[i - 1] + (long)Math.Pow(2.0, m_genSft[i - 1]);
            m_genX.Add(item);
        }
    }

    public void CalcFcureLeft()
    {
        m_genFcureLeft.Clear();
        double num = m_genParam[0];
        double num2 = y_base;
        double num3 = 0.0;
        double num4 = 0.0;
        for (int i = 0; i < 32; i++)
        {
            num3 = m_genX[i];
            num4 = 0.0;
            num4 = num != 0.0 ? !(num > 0.0) ? num2 * Math.Log10(1.0 - (Math.Log10(1.0 - num3 / 65536.0 - num / 65536.0) - Math.Log10((0.0 - num) / 65536.0)) / (Math.Log10(1.0 - num / 65536.0) - Math.Log10((0.0 - num) / 65536.0))) : num2 * (Math.Log10(num3 / 65536.0 + num / 65536.0) - Math.Log10(num / 65536.0)) / (Math.Log10(1.0 + num / 65536.0) - Math.Log10(num / 65536.0)) : num2 * num3 / 65536.0;
            m_genFcureLeft.Add(num4);
        }
    }

    public void CalcFcureRight()
    {
        m_genFcureRight.Clear();
        double num = m_genParam[1];
        double num2 = y_base;
        double num3 = 0.0;
        double num4 = 0.0;
        for (int i = 0; i < 32; i++)
        {
            num3 = m_genX[i];
            num4 = 0.0;
            num4 = num != 0.0 ? !(num > 0.0) ? num2 * Math.Log10(1.0 - (Math.Log10(1.0 - num3 / 65536.0 - num / 65536.0) - Math.Log10((0.0 - num) / 65536.0)) / (Math.Log10(1.0 - num / 65536.0) - Math.Log10((0.0 - num) / 65536.0))) : num2 * (Math.Log10(num3 / 65536.0 + num / 65536.0) - Math.Log10(num / 65536.0)) / (Math.Log10(1.0 + num / 65536.0) - Math.Log10(num / 65536.0)) : num2 * num3 / 65536.0;
            m_genFcureRight.Add(num4);
        }
    }

    public void CalcBlending()
    {
        m_genBlending.Clear();
        double num = y_base;
        double num2 = 0.0;
        double num3 = m_genParam[2];
        double num4 = m_genParam[3];
        double num5 = m_genParam[4];
        double num6 = m_genParam[5];
        double num7 = m_genParam[6];
        Math.Tanh(0.5);
        for (int i = 0; i < 32; i++)
        {
            num2 = m_genX[i];
            double num8 = Math.Tanh(num6 / 1024.0 * (Math.Log(num2 / 65536.0 * 1024.0 / num3 + 1E-15, Math.E) + 1.0) / (Math.Log(1024.0 / num3, Math.E) + 1.0) - num7 / 1024.0);
            double item = num * ((num4 + num5) / 2048.0 - (num4 - num5) / 2048.0 * num8);
            m_genBlending.Add(item);
        }
    }

    public void CalcFcureFinal()
    {
        m_genFcureFinal.Clear();
        m_genPoint.Clear();
        long num = y_base;
        long num2 = 0L;
        for (int i = 0; i < 32; i++)
        {
            double num3 = m_genFcureLeft[i];
            double num4 = m_genFcureRight[i];
            double num5 = m_genBlending[i];
            double num6 = num4 + num5 / num * (num3 - num4);
            m_genFcureFinal.Add(num6);
            if (i == 31)
            {
                double num7 = m_genFcureFinal[30];
                double num8 = m_genX[30];
                num6 = (4095.0 - num7) * 16384.0 / (65535.0 - num8);
                num2 = (long)Math.Floor(num6) + m_genPoint[30];
            }
            else
            {
                num2 = (long)Math.Floor(num6);
            }
            m_genPoint.Add(num2);
        }
    }

    public void update()
    {
        CalcX();
        CalcFcureLeft();
        CalcFcureRight();
        CalcBlending();
        CalcFcureFinal();
    }

    public void SetData(ref long[] param, ref long[] sft)
    {
        m_genSft = sft;
        m_genParam = param;
    }

    public void GetFinal(ref List<long> fcureFinal)
    {
        fcureFinal = m_genPoint;
    }
}
