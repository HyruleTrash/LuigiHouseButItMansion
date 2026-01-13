
using System;
using LucasCustomClasses;
using UnityEngine;

public class IceEffectTimer : TimerComp
{
    public float maxTime;
    private PlayerData playerRef;
    private string defaultMovementStateName = "Default";
    private string effectMovementStateName = "Ice";

    public void StartEffect(PlayerData givenRef)
    {
        playerRef ??= givenRef;
        playerRef.playerMovement.SetSpeedData(effectMovementStateName);
        timer = new Timer(maxTime, () =>
        {
            playerRef.playerMovement.SetSpeedData(defaultMovementStateName);
        });
        timer.running = true;
    }
}