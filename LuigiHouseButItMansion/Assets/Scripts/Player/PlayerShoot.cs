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
    [SerializeField]
    private float damage;
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
    public ObjectPool<WaterProjectileInstance> waterProjectilePool = new();

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
        }
        else
            shootDirection = transform.forward;
    }

    private void TryShoot()
    {
        if (!canShoot)
            return;
        
        CalculateShootDirectionMouse();
        trajectoryGetter.GetTrajectory(GetShootPosition(), shootDirection, shotStrength, SpawnProjectileInstance);
    }

    private void SpawnProjectileInstance(TrajectoryGetter.SplineCollision collisionData, Spline spline)
    {
        canShoot = false;
        chamberTimer.Reset();

        WaterProjectileInstance currentInstance;
        var foundInactive = waterProjectilePool.GetInactiveObject(out var foundInstance);
        bool shouldInit = false;
        if (foundInactive)
            currentInstance = (WaterProjectileInstance)foundInstance;
        else
        {
            WaterProjectileInstance.CreateNew(this, mesh, material, waterSplashPrefab, out currentInstance, out shouldInit);
        }
        
        currentInstance.SetData(spline, visualRotation, visualScale, projectileSpeed);
        currentInstance.SetSplineData(spline);
        currentInstance.CollisionLogic(collisionData, waterSplashPrefab, CheckIfCollidedWithWasDamagable);
        
        if (shouldInit)
            currentInstance.projectile.Init();
        currentInstance.projectile.ShouldRun = true;
    }

    private void CheckIfCollidedWithWasDamagable(GameObject collidedWithGameObject)
    {
        if (collidedWithGameObject.layer != LayerMask.NameToLayer("Damagable")) return;
        IDamagable[] damagables;
        damagables = collidedWithGameObject.GetComponents<IDamagable>();
        if (damagables.Length == 0)
            return;
        
        IDamagable hit = null;
        foreach (var d in damagables)
        {
            d.Hit(this, damage);
            hit = d;
        }
        if (hit == null)
            return;
        
        var rendererComponent = collidedWithGameObject.GetComponentInChildren<MeshRenderer>();
        if (hit.HitFlashKey == -1 || EntityHitFlash.instance.GetRegisteredEntity(hit.HitFlashKey) == null)
            hit.HitFlashKey = EntityHitFlash.instance.RegisterEntity(rendererComponent);
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