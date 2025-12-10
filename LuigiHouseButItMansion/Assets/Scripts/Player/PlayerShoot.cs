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
    [Header("Projectile data")]
    [SerializeField]
    private float projectileSpeed;
    [SerializeField]
    private float shotStrength;
    [SerializeField]
    private Vector3 shootPosition;
    [Header("Collision")]
    [SerializeField]
    private Mesh collisionMesh;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private Vector3 scale;
    [Header("Projectile (visual)")]
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private Material material;
    [SerializeField]
    private Vector3 visualRotation;
    [SerializeField]
    private Vector3 visualScale;

    private TrajectoryGetter trajectoryGetter;
    private Vector3 offset = Vector3.up * 1; // TODO bind this to where the mouse is / looking direction
    private ObjectPool<WaterProjectile> waterProjectilePool = new();

    private void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").Enable();
    }

    private void OnValidate()
    {
        if (collisionMesh != null && mesh != null && scale != Vector3.zero &&
            layerMask != -1) return;
        canShoot = false;
        enabled = false;
    }

    private void Start()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        shootAction.started += _ => { isTryingToShoot = true;};
        shootAction.canceled += _ => { isTryingToShoot = false;};
        
        chamberTimer = new Timer(chamberTime);
        chamberTimer.running = false;
        chamberTimer.onEnd += () => canShoot = true;

        trajectoryGetter = new TrajectoryGetter(collisionMesh, gameObject, scale, layerMask);
    }

    private void Update()
    {
        if (isTryingToShoot)
            TryShoot();
        chamberTimer.Update(Time.deltaTime);
    }

    private void TryShoot()
    {
        if (!canShoot)
            return;
        
        Debug.Log("Shooting!");
        trajectoryGetter.GetTrajectory(GetShootPosition(), transform.forward + offset, shotStrength, 
            (TrajectoryGetter.SplineCollision collisionData, Spline spline) => {
                canShoot = false;
                chamberTimer.Reset();
                Debug.Log($"Hit something! {collisionData.collidedWith}");

                WaterProjectile currentProjectile;
                var foundInactive = waterProjectilePool.GetInactiveObject(out var waterProjectile);
                bool shouldInit = false;
                if (foundInactive)
                    currentProjectile = (WaterProjectile)waterProjectile;
                else
                {
                    currentProjectile = new GameObject("WaterProjectile", typeof(Spline)).AddComponent<WaterProjectile>();
                    currentProjectile.OnFinished = (WaterProjectile doneProjectile) =>
                    {
                        waterProjectilePool.ReturnToInActivePool(doneProjectile);
                    };

                    currentProjectile.spline = currentProjectile.GetComponent<Spline>();
                    currentProjectile.material = material;
                    currentProjectile.mesh = mesh;
                    shouldInit = true;
                }
                
                currentProjectile.transform.position = spline.transform.position;
                currentProjectile.scale = visualScale;
                currentProjectile.rotation = visualRotation;
                
                currentProjectile.usedSpeed = projectileSpeed;
                
                // set spline data
                var projectileSpline = currentProjectile.spline;
                projectileSpline.nodes = new List<SplineNode>(spline.nodes.Count);
                foreach (var node in spline.nodes)
                {
                    projectileSpline.AddNode(new SplineNode(node.Position,node.Direction));
                }

                currentProjectile.RefreshCurves();
                
                if (shouldInit)
                    currentProjectile.Init();
                currentProjectile.ShouldRun = true;
            });
    }

    private Vector3 GetShootPosition()
    {
        return (transform.rotation * shootPosition);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetShootPosition() + transform.position, 0.1f);
    }
}