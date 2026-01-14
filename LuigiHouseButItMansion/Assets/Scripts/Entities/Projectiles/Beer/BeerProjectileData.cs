
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

    public override LiquidProjectileInstance SpawnInstance(LiquidTrajectoryGetter.SplineCollision collisionData, Spline spline, Action<LiquidProjectileInstance, GameObject> onCollision)
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
        
        onCollision += CheckIfCollidedWasPlayer;
        currentInstance.SetData(spline, visualRotation, visualScale, projectileSpeed);
        currentInstance.SetSplineData(spline);
        currentInstance.CollisionLogic(collisionData, splashPrefab, onCollision);
        
        if (shouldInit)
            currentInstance.projectileHandler.Init();
        currentInstance.projectileHandler.ShouldRun = true;
        return currentInstance;
    }
    
    private void CheckIfCollidedWasPlayer(LiquidProjectileInstance instance, GameObject collidedWithGameObject)
    {
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