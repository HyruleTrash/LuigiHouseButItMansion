using System;
using System.Collections.Generic;
using SplineMesh;
using Unity.VisualScripting;
using UnityEngine;

public class WaterProjectileInstance : LiquidProjectileInstance
{
    public ParticleSystem splashParticle;
    
    public void CollisionLogic(LiquidTrajectoryGetter.SplineCollision collisionData, GameObject waterSplashPrefab, Action<LiquidProjectileInstance, GameObject, LiquidTrajectoryGetter.SplineCollision> onCollision)
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

        onCollision?.Invoke(this, collisionData.collidedWith.gameObject, collisionData);
    }
}