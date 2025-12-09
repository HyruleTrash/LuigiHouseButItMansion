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

        trajectoryGetter = new TrajectoryGetter(mesh, gameObject);
    }

    private void Update()
    {
        if (isTryingToShoot)
            TryShoot();
        chamberTimer.Update(Time.deltaTime);
    }

    // private TrajectoryGetter.TrajectoryData temp;
    private void TryShoot()
    {
        if (!canShoot)
            return;
        
        Debug.Log("Shooting!");
        trajectoryGetter.GetTrajectory(transform.position + shootPosition, transform.forward + offset, shotStrength, 
        () => {
            // temp = data;
            Debug.Log("Pain");
        });
        
        canShoot = false;
        chamberTimer.Reset();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + shootPosition, 0.1f);

        if (trajectoryGetter == null || trajectoryGetter.temp == null)
            return;
        foreach (var pos in trajectoryGetter.temp)
        {
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }
}