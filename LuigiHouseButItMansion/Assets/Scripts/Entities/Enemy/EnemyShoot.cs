
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

    protected override void Start()
    {
        base.Start();
        playerRef ??= SceneData.instance.GetRegisteredObject<PlayerData>();
    }

    protected override void Update()
    {
        playerRef ??= SceneData.instance.GetRegisteredObject<PlayerData>();
        if (playerRef == null){
            enabled = false;
            return;
        }
        base.Update();
        if (Vector3.Distance(transform.position, playerRef.playerRigidbody.transform.position) > minimalSpottingDistance)
            return;
        if (Physics.Raycast(GetShootPosition(),
                GetShotDirection(),
                out RaycastHit hit,
                Mathf.Infinity,
                visionLayerMask,
                QueryTriggerInteraction.Ignore)
            && hit.transform.gameObject == playerRef.playerRigidbody.gameObject
            )
        {
            TryShoot();
        }
        
        var lookAtPoint = new Vector3(playerRef.playerRigidbody.transform.position.x, transform.position.y, playerRef.playerRigidbody.transform.position.z);
        transform.LookAt(lookAtPoint);
    }

    protected override void SpawnProjectileInstance(LiquidTrajectoryGetter.SplineCollision collisionData, Spline spline)
    {
        canShoot = false;
        chamberTimer.Reset();
        var instance = projectileData.SpawnInstance(collisionData, spline);
        if (instance is BeerProjectileInstance a)
            a.playerRef = playerRef;
    }

    protected override Vector3 GetShotDirection()
    {
        return (playerRef.playerRigidbody.transform.position - GetShootPosition()).normalized;
    }
}