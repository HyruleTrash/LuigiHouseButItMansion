
using System;
using LucasCustomClasses;
using SplineMesh;
using UnityEngine;

public abstract class ShootHandler : MonoBehaviour
{
    [SerializeField]
    protected ProjectileData projectileData;
    [SerializeField]
    protected Vector3 shootPosition;
    [Header("Timers")]
    [SerializeField]
    protected float chamberTime;
    protected Timer chamberTimer;
    protected bool canShoot = true;
    [Header("Collision")]
    [SerializeField]
    private Mesh collisionMesh;
    [SerializeField]
    protected LayerMask layerMask;
    protected LiquidTrajectoryGetter liquidTrajectoryGetter;
    
    private void OnValidate()
    {
        if (collisionMesh != null && projectileData != null && projectileData.Validate() &&
            layerMask != -1) return;
        canShoot = false;
        enabled = false;
    }
    
    protected virtual void Start()
    {
        chamberTimer = new Timer(chamberTime);
        chamberTimer.running = false;
        chamberTimer.onEnd += () => canShoot = true;
        
        liquidTrajectoryGetter = new LiquidTrajectoryGetter(collisionMesh, gameObject, projectileData.scale, layerMask);
    }

    protected virtual void Update()
    {
        chamberTimer.Update(Time.deltaTime);
    }

    protected Vector3 GetShootPosition()
    {
        return transform.rotation * shootPosition + transform.position;
    }
    
    protected void TryShoot()
    {
        if (!canShoot)
            return;
        
        liquidTrajectoryGetter.GetTrajectory(GetShootPosition() - transform.position, GetShotDirection(), projectileData.shotStrength, SpawnProjectileInstance);
    }

    protected abstract Vector3 GetShotDirection();

    protected virtual void SpawnProjectileInstance(LiquidTrajectoryGetter.SplineCollision collisionData, Spline spline)
    {
        canShoot = false;
        chamberTimer.Reset();
        projectileData.SpawnInstance(collisionData, spline);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetShootPosition(), 0.1f);
    }
}