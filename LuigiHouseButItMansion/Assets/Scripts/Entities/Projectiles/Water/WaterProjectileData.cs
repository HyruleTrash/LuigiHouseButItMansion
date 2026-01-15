
using System;
using SplineMesh;
using UnityEngine;

[CreateAssetMenu(fileName = "WaterProjectileData", menuName = "ScriptableObjects/Projectile/WaterProjectile")]
public class WaterProjectileData : ProjectileData
{
    [Space(5)]
    public GameObject waterSplashPrefab;
    private ObjectPool<WaterProjectileInstance> waterProjectilePool = new();
    
    public override LiquidProjectileInstance SpawnInstance(LiquidTrajectoryGetter.SplineCollision collisionData, Spline spline, Action<LiquidProjectileInstance, GameObject, LiquidTrajectoryGetter.SplineCollision> onCollision)
    {
        WaterProjectileInstance currentInstance = null;
        var foundInactive = waterProjectilePool.GetInactiveObject(out var foundInstance);
        bool shouldInit = false;
        if (foundInactive)
            currentInstance = (WaterProjectileInstance)foundInstance;
        else
        {
            LiquidProjectileInstance.CreateNew(waterProjectilePool,
                mesh,
                material,
                (cI, _) =>
                {
                    if (cI != null)
                        ((WaterProjectileInstance)cI).splashParticle.transform.rotation = waterSplashPrefab.transform.rotation;
                },
                out currentInstance,
                out shouldInit);
            
            var splashParticle = Instantiate(waterSplashPrefab, currentInstance.projectileHandler.transform, false);
            currentInstance.splashParticle = splashParticle.GetComponent<ParticleSystem>();
        }
        
        onCollision += CheckIfCollidedWithWasDamagable;
        currentInstance.SetData(spline, visualRotation, visualScale, projectileSpeed);
        currentInstance.SetSplineData(spline);
        currentInstance.CollisionLogic(collisionData, waterSplashPrefab, onCollision);
        
        if (shouldInit)
            currentInstance.projectileHandler.Init();
        currentInstance.projectileHandler.ShouldRun = true;
        return currentInstance;
    }
    
    
    private void CheckIfCollidedWithWasDamagable(LiquidProjectileInstance _, GameObject collidedWithGameObject, LiquidTrajectoryGetter.SplineCollision collisionData)
    {
        SceneData.instance.GetRegisteredObject<RoomObjectData>().goopManager.RemoveGoopAt(collisionData.contactPoint, collisionData.direction.normalized);
        
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
}