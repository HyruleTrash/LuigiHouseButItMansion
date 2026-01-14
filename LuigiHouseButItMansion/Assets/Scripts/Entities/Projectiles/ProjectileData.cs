
using System;
using SplineMesh;
using UnityEngine;

public abstract class ProjectileData : ScriptableObject
{
    [Header("Data")]
    public float projectileSpeed;
    public float shotStrength;
    public float damage;
    [Header("Visual")]
    public Mesh mesh;
    public Vector3 scale;
    [Space(5)]
    public Material material;
    public Vector3 visualRotation;
    public Vector3 visualScale;

    public bool Validate()
    {
        return mesh != null && scale != Vector3.zero;
    }

    public abstract LiquidProjectileInstance SpawnInstance(LiquidTrajectoryGetter.SplineCollision collisionData, Spline spline,
        Action<LiquidProjectileInstance, GameObject, LiquidTrajectoryGetter.SplineCollision> onCollision = null);
}