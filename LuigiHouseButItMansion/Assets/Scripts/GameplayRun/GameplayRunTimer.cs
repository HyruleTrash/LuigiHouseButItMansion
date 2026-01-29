using System;
using LucasCustomClasses;
using UnityEngine;
using UnityEngine.Events;

public class GameplayRunTimer : TimerComp, IOnRunStart
{
    [SerializeField]
    private float maxRunTime;
    public UnityEvent onRunEnd;
    private bool wasDisabled = false;
    public Action<string> onPlaying;
    
    public void OnRunStart()
    {
        timer = new Timer(maxRunTime, () =>
        {
            enabled = false;
            onRunEnd.Invoke();
        })
        {
            running = true
        };
        timer.onPlaying = _ => onPlaying?.Invoke(timer.GetFormattedTime(true));
    }

    private void OnDisable() => wasDisabled = true;

    private void OnEnable()
    {
        if (!wasDisabled) return;
        wasDisabled = false;
        OnRunStart();
    }
    
    public void AddToRunTime(float time) => timer.Add(time);
}