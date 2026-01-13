
using System;
using UnityEngine;

public class BeerProjectileInstance : LiquidProjectileInstance
{
    public ParticleSystem splashParticle;
    public PlayerData playerRef;
    public void CollisionLogic(LiquidTrajectoryGetter.SplineCollision collisionData, GameObject waterSplashPrefab, Action<LiquidProjectileInstance, GameObject> onCollision)
    {
        if (!collisionData.collided)
        {
            projectileHandler.OnEndHit = null;
            return;
        }
        
        projectileHandler.OnEndHit = () =>
        {
            splashParticle.Play();
        };
        
        var offsetDirection = waterSplashPrefab.transform.rotation * (-collisionData.direction * collisionData.distance);
        splashParticle.transform.position = collisionData.contactPoint + offsetDirection;

        splashParticle.transform.rotation = Quaternion.LookRotation(collisionData.direction, waterSplashPrefab.transform.up);

        onCollision?.Invoke(this, collisionData.collidedWith.gameObject);
    }
}