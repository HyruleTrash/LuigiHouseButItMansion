
using System;
using SplineMesh;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "BeerProjectileData", menuName = "ScriptableObjects/Projectile/BeerProjectile")]
public class BeerProjectileData : ProjectileData
{
    [Space(5)]
    public GameObject splashPrefab;
    private ObjectPool<BeerProjectileInstance> projectilePool = new();
    [SerializeField]
    private float maxEffectTime;
    private float? actualBrushSize;
    public float brushSize = 1.5f;

    public override LiquidProjectileInstance SpawnInstance(LiquidTrajectoryGetter.SplineCollision collisionData, Spline spline, Action<LiquidProjectileInstance, GameObject, LiquidTrajectoryGetter.SplineCollision> onCollision)
    {
        BeerProjectileInstance currentInstance = null;
        var foundInactive = projectilePool.GetInactiveObject(out var foundInstance);
        bool shouldInit = false;
        if (foundInactive)
            currentInstance = (BeerProjectileInstance)foundInstance;
        else
        {
            LiquidProjectileInstance.CreateNew(projectilePool,
                mesh,
                material,
                (cI, _) =>
                {
                    if (cI != null)
                        ((BeerProjectileInstance)cI).splashParticle.transform.rotation = splashPrefab.transform.rotation;
                },
                out currentInstance,
                out shouldInit);
            
            var splashParticle = Instantiate(splashPrefab, currentInstance.projectileHandler.transform, false);
            currentInstance.splashParticle = splashParticle.GetComponent<ParticleSystem>();
        }
        actualBrushSize = null;
        
        onCollision += CheckIfCollidedWasPlayer;
        currentInstance.SetData(spline, visualRotation, visualScale, projectileSpeed);
        currentInstance.SetSplineData(spline);
        currentInstance.CollisionLogic(collisionData, splashPrefab, onCollision);
        
        if (shouldInit)
            currentInstance.projectileHandler.Init();
        currentInstance.projectileHandler.ShouldRun = true;
        return currentInstance;
    }
    
    private float GetBrushSize(Vector3Int res, Bounds roomBounds)
    {
        var metersPerTexelX = roomBounds.size.x / (res.x - 1);
        actualBrushSize ??= brushSize / metersPerTexelX;
        return actualBrushSize.Value;
    }
    
    private void CheckIfCollidedWasPlayer(LiquidProjectileInstance instance, GameObject collidedWithGameObject, LiquidTrajectoryGetter.SplineCollision collisionData)
    {
        RoomObjectData.CurrentRoom.goopManager.ApplyGoopAt(collisionData.contactPoint, GetBrushSize);
        if (collidedWithGameObject.layer != LayerMask.NameToLayer("Player")) return;
        
        if (instance is not BeerProjectileInstance shotInstance || shotInstance.playerRef == null)
            return;
        
        var hit = shotInstance.playerRef.healthComp;
        if (hit == null)
            return;
        
        hit.Hit(this, damage);
        shotInstance.playerRef.TriggerHitFlash();

        var effectTimer = shotInstance.playerRef.GetComponent<IceEffectTimer>();
        if (effectTimer is null)
        {
            effectTimer = shotInstance.playerRef.AddComponent<IceEffectTimer>();
            effectTimer.maxTime = maxEffectTime;
        }
        effectTimer.StartEffect(shotInstance.playerRef);
    }
}