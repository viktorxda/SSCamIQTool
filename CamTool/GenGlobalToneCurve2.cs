using System;
using System.Collections.Generic;

namespace SSCamIQTool.CamTool;

internal class GenGlobalToneCurve2
{
    public long[] m_genParam = new long[9];

    private readonly long[] m_genIndex = new long[33];

    private readonly double[] m_genIndexNor = new double[33];

    public List<long> m_genX = new();

    private readonly List<double> m_genCammaLeft = new();

    private readonly List<double> m_genCammaRight = new();

    private readonly List<double> m_genBlending = new();

    private readonly List<double> m_genCammaFinal = new();

    public List<long> m_genPoint = new();

    public string[] m_genParamTitle = new string[9] { "a_left\n(0~65535)", "r_left\n(0~65535)", "a_right\n(0~65535)", "r_right\n(0~65535)", "adelta\n(0~8196)", "amax\n(-1024~1024)", "amin\n(-1024~1024)", "aslope\n(0~8196)", "a_coe3\n(0~8196)" };

    public long[] m_genParamMin = new long[9] { 0L, 0L, 0L, 0L, 0L, -1024L, -1024L, 0L, 0L };

    public long[] m_genParamMax = new long[9] { 65536L, 65536L, 65536L, 65536L, 8196L, 1024L, 1024L, 8196L, 8196L };

    private readonly long y_base = 4096L;

    private readonly double[] degamma = new double[4];

    public void CalcIndex()
    {
        m_genX.Clear();
        for (int i = 0; i < 33; i++)
        {
            m_genIndex[i] = i;
            m_genIndexNor[i] = i / 32.0;
            m_genX.Add(i);
        }
        double num = m_genParam[0];
        double num2 = m_genParam[1];
        double num3 = m_genParam[2];
        double num4 = m_genParam[3];
        _ = m_genParam[4];
        _ = m_genParam[5];
        _ = m_genParam[6];
        _ = m_genParam[7];
        _ = m_genParam[8];
        degamma[0] = num / 1024.0 / ((num2 / 1024.0) - 1.0);
        degamma[1] = Math.Pow(1.0 + (num / 1024.0), num2 / 1024.0) * Math.Pow((num2 / 1024.0) - 1.0, (num2 / 1024.0) - 1.0) / (Math.Pow(num / 1024.0, (num2 / 1024.0) - 1.0) * Math.Pow(num2 / 1024.0, num2 / 1024.0));
        degamma[2] = num3 / 1024.0 / ((num4 / 1024.0) - 1.0);
        degamma[3] = Math.Pow(1.0 + (num3 / 1024.0), num4 / 1024.0) * Math.Pow((num4 / 1024.0) - 1.0, (num4 / 1024.0) - 1.0) / (Math.Pow(num3 / 1024.0, (num4 / 1024.0) - 1.0) * Math.Pow(num4 / 1024.0, num4 / 1024.0));
    }

    public void CalcGammaLeft()
    {
        double num2 = degamma[0];
        double num3 = degamma[1];
        double num4 = y_base;
        double num5 = m_genParam[0];
        double num6 = m_genParam[1];
        m_genCammaLeft.Clear();
        for (int i = 0; i < 33; i++)
        {
            double num = m_genIndexNor[i];
            double num7 = !(num < num2) ? num4 * Math.Pow((num + (num5 / 1024.0)) / (1.0 + (num5 / 1024.0)), num6 / 1024.0) : num4 * (num / num3);
            m_genCammaLeft.Add(num7);
        }
    }

    public void CalcGammaRight()
    {
        double num2 = degamma[2];
        double num3 = y_base;
        double num4 = degamma[3];
        double num5 = m_genParam[2];
        double num6 = m_genParam[3];
        m_genCammaRight.Clear();
        for (int i = 0; i < 33; i++)
        {
            double num = m_genIndexNor[i];
            double item = !(num < num2) ? num3 * Math.Pow((num + (num5 / 1024.0)) / (1.0 + (num5 / 1024.0)), num6 / 1024.0) : num3 * (num / num4);
            m_genCammaRight.Add(item);
        }
    }

    public void CalcBlending()
    {
        double num = y_base;
        double num2 = m_genParam[4];
        double num3 = m_genParam[5];
        double num4 = m_genParam[6];
        double num5 = m_genParam[7];
        double num6 = m_genParam[8];
        m_genBlending.Clear();
        for (int i = 0; i < 33; i++)
        {
            double num7 = m_genIndex[i];
            double num8 = Math.Tanh((num5 / 1024.0 * (Math.Log((num7 / 32.0 * 1024.0 / num2) + 1E-15, Math.E) + 1.0) / (Math.Log(1024.0 / num2) + 1.0)) - (num6 / 1024.0));
            double num9 = num * (((num3 + num4) / 2048.0) - ((num3 - num4) / 2048.0 * num8));
            m_genBlending.Add(num9);
        }
    }

    public void CalcFinal()
    {
        double num3 = y_base;
        m_genCammaFinal.Clear();
        m_genPoint.Clear();
        for (int i = 0; i < 33; i++)
        {
            double num4 = m_genCammaLeft[i];
            double num = m_genCammaRight[i];
            double num2 = m_genBlending[i];
            double num5 = num + (num2 / num3 * (num4 - num));
            m_genCammaFinal.Add(num5);
            if (i == 32)
            {
                num5 = Math.Min(num5, 4095.0);
            }
            long num6 = (long)Math.Floor(num5);
            m_genPoint.Add(num6);
        }
    }

    public void GetData(ref List<long> final)
    {
        final = m_genPoint;
    }

    public void SetParam(ref long[] param)
    {
        m_genParam = param;
    }

    public void update()
    {
        CalcIndex();
        CalcGammaLeft();
        CalcGammaRight();
        CalcBlending();
        CalcFinal();
    }
}
