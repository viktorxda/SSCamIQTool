using System;
using System.Text;

namespace SSCamIQTool.LibComm;

internal class RingBuff
{
    private byte[] ringBuff;

    private int nextWritePosition;

    private int nextReadPosition;

    private int buffSize;

    private int unusedSize;

    public RingBuff(int _size)
    {
        buffSize = unusedSize = _size;
        ringBuff = new byte[buffSize];
        nextReadPosition = nextWritePosition = 0;
    }

    public bool WriteBuff(byte[] _buff)
    {
        int num = _buff.Length;
        if (unusedSize < num)
        {
            return false;
        }
        int num2 = buffSize - nextWritePosition;
        if (num > num2)
        {
            Array.Copy(_buff, 0, ringBuff, nextWritePosition, num2);
            Array.Copy(_buff, num2, ringBuff, 0, num - num2);
            nextWritePosition = num - num2;
        }
        else if (num == num2)
        {
            Array.Copy(_buff, 0, ringBuff, nextWritePosition, num);
            nextWritePosition = 0;
        }
        else
        {
            Array.Copy(_buff, 0, ringBuff, nextWritePosition, num);
            nextWritePosition += num;
        }
        unusedSize -= num;
        return true;
    }

    public bool WriteBuff(byte[] _buff, int len)
    {
        int num = len;
        num = len > _buff.Length ? _buff.Length : num;
        if (unusedSize < num)
        {
            return false;
        }
        int num2 = buffSize - nextWritePosition;
        if (num > num2)
        {
            Array.Copy(_buff, 0, ringBuff, nextWritePosition, num2);
            Array.Copy(_buff, num2, ringBuff, 0, num - num2);
            nextWritePosition = num - num2;
        }
        else if (num == num2)
        {
            Array.Copy(_buff, 0, ringBuff, nextWritePosition, num);
            nextWritePosition = 0;
        }
        else
        {
            Array.Copy(_buff, 0, ringBuff, nextWritePosition, num);
            nextWritePosition += num;
        }
        unusedSize -= num;
        return true;
    }

    public int ReadBuff(byte[] readbuff, int len, bool MovePosition)
    {
        int num = buffSize - unusedSize;
        if (len <= 0 || num <= 0)
        {
            return 0;
        }
        int num2 = num >= len ? len : num;
        int num3 = buffSize - nextReadPosition;
        if (num2 > num3)
        {
            Array.Copy(ringBuff, nextReadPosition, readbuff, 0, num3);
            Array.Copy(ringBuff, 0, readbuff, num3, num2 - num3);
            if (MovePosition)
            {
                nextReadPosition = num2 - num3;
            }
        }
        else if (num2 == num3)
        {
            Array.Copy(ringBuff, nextReadPosition, readbuff, 0, num2);
            if (MovePosition)
            {
                nextReadPosition = 0;
            }
        }
        else
        {
            Array.Copy(ringBuff, nextReadPosition, readbuff, 0, num2);
            if (MovePosition)
            {
                nextReadPosition += num2;
            }
        }
        unusedSize += num2;
        return num2;
    }

    public string GetBuffInfo()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(":writePositon:" + nextWritePosition);
        stringBuilder.Append(".  readPosition:" + nextReadPosition);
        stringBuilder.Append(".  size:");
        stringBuilder.Append(buffSize);
        stringBuilder.Append("   .enable read:" + (buffSize - unusedSize) + ".enable write:" + unusedSize);
        stringBuilder.Append("\r\n");
        stringBuilder.Append("1-10 byte: ");
        for (int i = 0; i < 10; i++)
        {
            stringBuilder.Append(ringBuff[i]);
        }
        return stringBuilder.ToString();
    }
}
