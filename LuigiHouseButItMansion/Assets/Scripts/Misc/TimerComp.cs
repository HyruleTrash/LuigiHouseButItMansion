
using System;
using LucasCustomClasses;
using UnityEngine;

public class TimerComp :MonoBehaviour
{
    public Timer timer;

    private void Update()
    {
        if (timer != null && timer.running)
            timer.Update(Time.deltaTime);
    }
}