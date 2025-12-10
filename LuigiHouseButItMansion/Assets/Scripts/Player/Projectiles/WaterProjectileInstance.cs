using System;
using System.Collections.Generic;
using SplineMesh;
using Unity.VisualScripting;
using UnityEngine;

public class WaterProjectileInstance
{
    public WaterProjectile projectile;
    private ParticleSystem splashParticle;

    public static void CreateNew(PlayerShoot parent, Mesh mesh, Material material, GameObject waterSplashPrefab, 
        out WaterProjectileInstance returnedInstance, out bool shouldInit)
    {
        var currentInstance = new WaterProjectileInstance
        {
            projectile = new GameObject("WaterProjectile", typeof(Spline)).AddComponent<WaterProjectile>()
        };
        currentInstance.projectile.OnFinished = (_) =>
        {
            currentInstance.splashParticle.transform.rotation = waterSplashPrefab.transform.rotation;
            parent.waterProjectilePool.ReturnToInActivePool(currentInstance);
        };

        currentInstance.projectile.spline = currentInstance.projectile.GetComponent<Spline>();
        currentInstance.projectile.material = material;
        currentInstance.projectile.mesh = mesh;
                    
        var splashParticle = GameObject.Instantiate(waterSplashPrefab, currentInstance.projectile.transform, false);
                    
        currentInstance.splashParticle = splashParticle.GetComponent<ParticleSystem>();
        
        returnedInstance = currentInstance;
        shouldInit = true;
    }

    public void SetData(Spline spline, Vector3 visualRotation, Vector3 visualScale, float projectileSpeed)
    {
        projectile.transform.position = spline.transform.position;
        projectile.scale = visualScale;
        projectile.rotation = visualRotation;
        
        projectile.usedSpeed = projectileSpeed;
    }

    public void SetSplineData(Spline spline)
    {
        var projectileSpline = projectile.spline;
        projectileSpline.nodes = new List<SplineNode>(spline.nodes.Count);
        foreach (var node in spline.nodes)
        {
            projectileSpline.AddNode(new SplineNode(node.Position,node.Direction));
        }

        projectile.RefreshCurves();
    }
    
    public void CollisionLogic(TrajectoryGetter.SplineCollision collisionData, GameObject waterSplashPrefab, Action<GameObject> onCollision)
    {
        if (!collisionData.collided)
        {
            projectile.OnEndHit = null;
            return;
        }
        
        projectile.OnEndHit = () =>
        {
            splashParticle.Play();
        };
        
        var offsetDirection = waterSplashPrefab.transform.rotation * (-collisionData.direction * collisionData.distance);
        splashParticle.transform.position = collisionData.contactPoint + offsetDirection;

        splashParticle.transform.rotation = Quaternion.LookRotation(collisionData.direction, waterSplashPrefab.transform.up);

        onCollision?.Invoke(collisionData.collidedWith.gameObject);
    }
}