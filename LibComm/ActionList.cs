using System.Collections.Generic;
using System.Linq;

namespace SSCamIQTool.LibComm;

public class ActionList
{
    private List<ActionStep> actionList = new();

    private int index = -1;

    public void RemoveByPageIndex(int i)
    {
        int num = 0;
        for (int j = 0; j <= index; j++)
        {
            if (actionList[j].pageIndex == i)
            {
                num++;
            }
        }
        index -= num;
        actionList = actionList.Where((s) => s.OutsidePage(i)).ToList();
    }

    public void Add(ActionStep step)
    {
        if (actionList.Count > 0)
        {
            DeleteTail(index);
            if (index == -1 || !actionList[index].IsEqual(step))
            {
                actionList.Add(step);
                index++;
            }
        }
        else
        {
            actionList.Add(step);
            index++;
        }
    }

    public ActionStep StepBack()
    {
        if (index > 0)
        {
            ActionStep step = actionList[index];
            index--;
            int num = actionList.FindLastIndex(index, (s) => s.itemTag.Equals(step.itemTag));
            return num < 0 ? new ActionStep(step, isReset: true) : actionList[num];
        }
        if (index == 0)
        {
            ActionStep result = new(actionList[index], isReset: true);
            index--;
            return result;
        }
        return null;
    }

    public ActionStep StepNext()
    {
        if (index < actionList.Count - 1)
        {
            index++;
            return actionList[index];
        }
        return null;
    }

    public void DeleteTail(int index)
    {
        if (index < actionList.Count - 1)
        {
            actionList.RemoveRange(index + 1, actionList.Count - (index + 1));
        }
    }

    public void Clear()
    {
        actionList.Clear();
        index = -1;
    }

    public bool IsNextStepOver()
    {
        return index == actionList.Count - 1;
    }

    public bool IsBackStepOver()
    {
        return index == -1;
    }
}
