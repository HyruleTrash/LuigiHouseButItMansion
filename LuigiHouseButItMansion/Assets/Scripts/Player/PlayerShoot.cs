using System;
using System.Collections.Generic;
using LucasCustomClasses;
using SplineMesh;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private LayerMask mouseLayerMask;
    private InputAction aimAction;
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
    private float usedShotStrength;
    [SerializeField]
    private Vector3 shootPosition;
    private Vector3 shootDirection;
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
    [SerializeField]
    private GameObject waterSplashPrefab;

    private TrajectoryGetter trajectoryGetter;
    private ObjectPool<WaterProjectileInstance> waterProjectilePool = new();

    public class WaterProjectileInstance
    {
        public WaterProjectile projectile;
        public ParticleSystem splashParticle;
    }

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

    private void CalculateShootDirectionMouse()
    {
        if (Physics.Raycast(MouseRayGetter.instance.GetMouseRay(), out var hit, Mathf.Infinity, layerMask))
        {
            var shootPos = GetShootPosition() + transform.position;
            shootDirection = (hit.point - shootPos).normalized;
            usedShotStrength = shotStrength * Vector3.Distance(hit.point, shootPos);
        }
        else
        {
            shootDirection = transform.forward;
            usedShotStrength = shotStrength;
        }
    }

    private void TryShoot()
    {
        if (!canShoot)
            return;
        
        CalculateShootDirectionMouse();
        
        trajectoryGetter.GetTrajectory(GetShootPosition(), shootDirection, shotStrength, 
            (TrajectoryGetter.SplineCollision collisionData, Spline spline) => {
                canShoot = false;
                chamberTimer.Reset();
                // Debug.Log($"Hit something! {collisionData.collidedWith}");

                WaterProjectileInstance currentInstance;
                var foundInactive = waterProjectilePool.GetInactiveObject(out var foundInstance);
                bool shouldInit = false;
                if (foundInactive)
                    currentInstance = (WaterProjectileInstance)foundInstance;
                else
                {
                    currentInstance = new()
                    {
                        projectile = new GameObject("WaterProjectile", typeof(Spline)).AddComponent<WaterProjectile>()
                    };
                    currentInstance.projectile.OnFinished = (WaterProjectile doneProjectile) =>
                    {
                        currentInstance.splashParticle.transform.rotation = waterSplashPrefab.transform.rotation;
                        waterProjectilePool.ReturnToInActivePool(currentInstance);
                    };

                    currentInstance.projectile.spline = currentInstance.projectile.GetComponent<Spline>();
                    currentInstance.projectile.material = material;
                    currentInstance.projectile.mesh = mesh;
                    
                    var splashParticle = Instantiate(waterSplashPrefab, currentInstance.projectile.transform, false);
                    
                    currentInstance.splashParticle = splashParticle.GetComponent<ParticleSystem>();
                    currentInstance.projectile.OnEndHit = () =>
                    {
                        currentInstance.splashParticle.Play();
                    };
                    
                    shouldInit = true;
                }
                
                // splash effect data
                if (collisionData.collided)
                {
                    var offsetDirection = waterSplashPrefab.transform.rotation * (-collisionData.direction * collisionData.distance);
                    currentInstance.splashParticle.transform.position = collisionData.contactPoint + offsetDirection;

                    currentInstance.splashParticle.transform.rotation = Quaternion.LookRotation(collisionData.direction, waterSplashPrefab.transform.up);
                }
                
                // projectile Data
                currentInstance.projectile.transform.position = spline.transform.position;
                currentInstance.projectile.scale = visualScale;
                currentInstance.projectile.rotation = visualRotation;
                
                currentInstance.projectile.usedSpeed = projectileSpeed;
                
                // set spline data
                var projectileSpline = currentInstance.projectile.spline;
                projectileSpline.nodes = new List<SplineNode>(spline.nodes.Count);
                foreach (var node in spline.nodes)
                {
                    projectileSpline.AddNode(new SplineNode(node.Position,node.Direction));
                }

                currentInstance.projectile.RefreshCurves();
                
                if (shouldInit)
                    currentInstance.projectile.Init();
                currentInstance.projectile.ShouldRun = true;
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
        Gizmos.DrawLine(GetShootPosition() + transform.position, GetShootPosition() + transform.position + shootDirection * shotStrength);
        
    }
}