using System;

public class ScoreCounter
{
    public int GoldCount
    {
        get => goldCount;
        set
        {
            if (value != goldCount)
                onGoldCountChange?.Invoke(value);
            goldCount = value;
        }
    }

    public float CleanCount
    {
        get => cleanCount;
        set
        {
            if (value != cleanCount)
                onCleanCountChange?.Invoke(value);
            cleanCount = value;
        }
    }

    public Action<int> onGoldCountChange;
    public Action<float> onCleanCountChange;
    private int goldCount;
    private float cleanCount;
}