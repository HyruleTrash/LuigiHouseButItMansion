using System;
using System.Collections.Generic;
using LucasCustomClasses;
using SplineMesh;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : ShootHandler
{
    [Header("Input")]
    [SerializeField]
    private InputActionAsset inputActionAsset;
    private InputAction aimAction;
    private InputAction shootAction;
    private bool isTryingToShoot;
    private bool wasTryingToShoot;
    private Vector3? lastLookDirection;

    private void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").Enable();
    }

    protected override void Start()
    {
        base.Start();
        shootAction = inputActionAsset.FindActionMap("Player").FindAction("Attack");
    }

    protected override void Update()
    {
        base.Update();
        isTryingToShoot = Mathf.Approximately(shootAction.ReadValue<float>(), 1f);
        if (isTryingToShoot)
        {
            TryShoot();
            wasTryingToShoot = true;
        }
        else if (wasTryingToShoot) // mouseUp
            lastLookDirection = null;
    }

    private Vector3 CalculateShootDirectionMouse()
    {
        MouseRayGetter.instance.GetRelevantHitBasedOnLastDirection(ref lastLookDirection, GetShootPosition(), layerMask, out _);
        return lastLookDirection ?? transform.forward;
    }

    protected override Vector3 GetShotDirection()
    {
        return CalculateShootDirectionMouse();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (!Application.isPlaying)
            return;
        Gizmos.DrawLine(GetShootPosition(), GetShootPosition() + CalculateShootDirectionMouse() * projectileData.shotStrength);
    }
}