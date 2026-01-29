using System;
using System.Collections.Generic;
using UnityEngine;

public class RunStartManager : MonoBehaviour
{
    [SerializeField]
    private List<MonoBehaviour> onRunStarts;

    private void OnValidate()
    {
        var isValid = true;
        for (var i = 0; i < onRunStarts.Count; i++)
        {
            var component = onRunStarts[i];
            if (component == null) isValid = false;
            if (component != null && component is not IOnRunStart)
            {
                onRunStarts[i] = null;
            }
        }

        if (isValid) isValid = onRunStarts.Count != 0;

        enabled = isValid;
    }

    private void Start()
    {
        SceneData.instance.RegistereObject<ScoreCounter>(new ScoreCounter(), true);
        foreach (var comp in onRunStarts) ((IOnRunStart)comp).OnRunStart();
    }
}
