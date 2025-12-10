using System;
using System.Collections.Generic;
using LucasCustomClasses;
using SplineMesh;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActionAsset;
    private InputAction shootAction;
    private bool isTryingToShoot;
    [Header("Timers")]
    [SerializeField]
    private float chamberTime;
    private Timer chamberTimer;
    private bool canShoot = true;
    [Header("Projectiles")]
    [SerializeField]
    private float projectileSpeed;
    [SerializeField]
    private float shotStrength;
    [SerializeField]
    private Vector3 shootPosition;
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private Vector3 scale;

    private TrajectoryGetter trajectoryGetter;
    private Vector3 offset = Vector3.up * 1;

    private void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").Enable();
    }

    private void Start()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        shootAction.started += _ => { isTryingToShoot = true;};
        shootAction.canceled += _ => { isTryingToShoot = false;};
        
        chamberTimer = new Timer(chamberTime);
        chamberTimer.running = false;
        chamberTimer.onEnd += () => canShoot = true;

        trajectoryGetter = new TrajectoryGetter(mesh, gameObject, scale, layerMask);
    }

    private void Update()
    {
        if (isTryingToShoot)
            TryShoot();
        chamberTimer.Update(Time.deltaTime);
    }

    private TrajectoryGetter.SplineCollision temp;
    private void TryShoot()
    {
        if (!canShoot)
            return;
        
        Debug.Log("Shooting!");
        trajectoryGetter.GetTrajectory(GetShootPosition(), transform.forward + offset, shotStrength, 
            (TrajectoryGetter.SplineCollision collisionData, Spline spline) => {
                temp = collisionData;
                Debug.Log("Hit something!");
            });
        canShoot = false;
        chamberTimer.Reset();
    }

    private Vector3 GetShootPosition()
    {
        return (transform.rotation * shootPosition);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetShootPosition() + transform.position, 0.1f);

        if (trajectoryGetter == null || trajectoryGetter.temp == null)
            return;
        Gizmos.color = Color.orange;
        foreach (var pos in trajectoryGetter.temp)
        {
            Gizmos.DrawSphere(pos, 0.1f);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(temp.contactPoint, 0.1f);
        Gizmos.DrawLine(temp.contactPoint, temp.contactPoint + temp.direction * temp.distance);
    }
}