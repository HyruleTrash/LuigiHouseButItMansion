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

    private Spline spline;
    private TrajectoryGetter trajectoryGetter;
    private Vector3 offset = Vector3.up * 2;

    private void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").Enable();
    }

    private void OnValidate()
    {
        if (spline == null)
            spline = gameObject.GetComponent<Spline>();
        if (spline == null)
            spline = gameObject.AddComponent<Spline>();
        if (enabled)
            CalculateTrajectory();
    }

    private void Start()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        shootAction.started += _ => { isTryingToShoot = true;};
        shootAction.canceled += _ => { isTryingToShoot = false;};
        
        chamberTimer = new Timer(chamberTime);
        chamberTimer.running = false;
        chamberTimer.onEnd += () => canShoot = true;

        trajectoryGetter = new TrajectoryGetter(mesh, scale);
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
        trajectoryGetter.GetTrajectory(transform.position + shootPosition, Quaternion.identity, transform.forward + offset, shotStrength, 
        () => {
            // temp = data;
            Debug.Log("Pain");
        });
        
        canShoot = false;
        chamberTimer.Reset();
    }

    private void CalculateTrajectory()
    {
        spline.nodes = new List<SplineNode>(2);
        
        var nextNode = shootPosition + (transform.forward + offset) * shotStrength;
        spline.AddNode(new SplineNode(shootPosition, nextNode));

        var distanceBetweenNodes = Vector3.Distance(shootPosition, nextNode);
        nextNode += Vector3.down * distanceBetweenNodes;
        spline.AddNode(new SplineNode( nextNode, nextNode));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + shootPosition, 0.1f);
        //
        // Gizmos.DrawSphere(temp.startPosition, 0.1f);
        // Gizmos.DrawSphere(temp.highestPosition, 0.1f);
        // Gizmos.DrawSphere(temp.endPosition, 0.1f);
    }
}