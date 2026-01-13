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
            TryShoot();
    }

    private Vector3 CalculateShootDirectionMouse()
    {
        if (!Physics.Raycast(MouseRayGetter.instance.GetMouseRay(), out var hit, Mathf.Infinity, layerMask))
            return transform.forward;
        var shootPos = GetShootPosition();
        return (hit.point - shootPos).normalized;
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