
using System;
using LucasCustomClasses;
using SplineMesh;
using UnityEngine;

public class EnemyShoot : ShootHandler
{
    [Header("Enemy specific")]
    [SerializeField]
    private PlayerData playerRef;
    public float minimalSpottingDistance;
    [SerializeField]
    private LayerMask visionLayerMask;
    private Vector3? usedShootPosition = null;
    [SerializeField]
    private float shootDragSpeed = 0.1f;

    private Timer stopShootingTimer;
    private bool shouldShoot = false;

    protected override void Start()
    {
        base.Start();
        playerRef ??= SceneData.instance.GetRegisteredObject<PlayerData>();
        usedShootPosition = GetShootPosition();
    }

    protected override void Update()
    {
        playerRef ??= SceneData.instance.GetRegisteredObject<PlayerData>();
        if (playerRef == null){
            enabled = false;
            return;
        }
        base.Update();
        
        if (Vector3.Distance(transform.position, playerRef.playerRigidbody.transform.position) >
            minimalSpottingDistance)
        {
            usedShootPosition = null;
            if (stopShootingTimer is not { running: true })
                stopShootingTimer = new Timer(2, () => shouldShoot = false);
            stopShootingTimer.Update(Time.deltaTime);
        }
        else
        {
            shouldShoot = true;
            stopShootingTimer.running = false;
        }
        
        if (!shouldShoot)
            return;
        var lookAtPoint = new Vector3(playerRef.playerRigidbody.transform.position.x, transform.position.y, playerRef.playerRigidbody.transform.position.z);
        transform.LookAt(lookAtPoint);
        
        if (Physics.Raycast(GetShootPosition(),
                (playerRef.playerRigidbody.transform.position - GetShootPosition()).normalized,
                out RaycastHit hit,
                Mathf.Infinity,
                visionLayerMask,
                QueryTriggerInteraction.Ignore)
            && hit.transform.gameObject == playerRef.playerRigidbody.gameObject
            )
        {
            TryShoot();
        }
    }

    protected override void SpawnProjectileInstance(LiquidTrajectoryGetter.SplineCollision collisionData, Spline spline)
    {
        canShoot = false;
        chamberTimer.Reset();
        var instance = projectileData.SpawnInstance(collisionData, spline);
        if (instance is BeerProjectileInstance a)
            a.playerRef = playerRef;
    }

    protected override Vector3 GetShootPosition()
    {
        usedShootPosition ??= base.GetShootPosition();
        usedShootPosition = Vector3.Lerp(usedShootPosition.Value, base.GetShootPosition(), shootDragSpeed * Time.deltaTime);
        return usedShootPosition.Value;
    }

    protected override Vector3 GetShotDirection()
    {
        var shotPos = GetShootPosition();
        return (shotPos - new Vector3(transform.position.x, shotPos.y, transform.position.z)).normalized;
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     if (usedShootPosition != null) Gizmos.DrawLine(usedShootPosition.Value, usedShootPosition.Value + GetShotDirection() * minimalSpottingDistance);
    // }
}