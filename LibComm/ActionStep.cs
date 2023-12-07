namespace SSCamIQTool.LibComm;

public class ActionStep
{
    public readonly int pageIndex;

    public readonly bool isReset;

    public readonly string itemTag;

    public readonly int dataIndex;

    public readonly long[] dataValue;

    public ActionStep(int pageIndex, bool isReset, string itemTag, int dataIndex, long[] dataValue)
    {
        this.pageIndex = pageIndex;
        this.isReset = isReset;
        this.itemTag = itemTag;
        this.dataIndex = dataIndex;
        this.dataValue = dataValue;
    }

    public ActionStep(ActionStep step, bool isReset)
    {
        pageIndex = step.pageIndex;
        this.isReset = isReset;
        itemTag = step.itemTag;
        dataIndex = step.dataIndex;
        dataValue = step.dataValue;
    }

    public bool IsEqual(ActionStep step)
    {
        if (step.pageIndex == pageIndex && step.isReset == isReset && step.itemTag == itemTag && step.dataIndex == dataIndex && IsDataEquals(step))
        {
            return true;
        }
        return false;
    }

    private bool IsDataEquals(ActionStep step)
    {
        if (dataValue.Length == step.dataValue.Length)
        {
            for (int i = 0; i < dataValue.Length; i++)
            {
                if (dataValue[i] != step.dataValue[i])
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public bool OutsidePage(int pageIndex)
    {
        return this.pageIndex != pageIndex;
    }
}
