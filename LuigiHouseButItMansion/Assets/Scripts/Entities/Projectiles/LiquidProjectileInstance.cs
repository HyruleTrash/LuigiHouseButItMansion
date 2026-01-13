using System;
using System.Collections.Generic;
using SplineMesh;
using Unity.VisualScripting;
using UnityEngine;

public class LiquidProjectileInstance
{
    public LiquidProjectileHandler projectileHandler;

    public void SetSplineData(Spline spline)
    {
        var projectileSpline = projectileHandler.spline;
        projectileSpline.nodes = new List<SplineNode>(spline.nodes.Count);
        foreach (var node in spline.nodes)
        {
            projectileSpline.AddNode(new SplineNode(node.Position,node.Direction));
        }

        projectileHandler.RefreshCurves();
    }
    
    public static void CreateNew<T>(ObjectPool<T> pool, Mesh mesh, Material material, Action<LiquidProjectileInstance, LiquidProjectileHandler> onFishished,
        out T returnedInstance, out bool shouldInit) where T : LiquidProjectileInstance, new()
    {
        var currentInstance = new T
        {
            projectileHandler = new GameObject("WaterProjectile", typeof(Spline)).AddComponent<LiquidProjectileHandler>()
        };
        onFishished += (_, __) => pool.ReturnToInActivePool(currentInstance);
        currentInstance.projectileHandler.OnFinished = onFishished;

        currentInstance.projectileHandler.spline = currentInstance.projectileHandler.GetComponent<Spline>();
        currentInstance.projectileHandler.material = material;
        currentInstance.projectileHandler.mesh = mesh;
        currentInstance.projectileHandler.currentInstanceReference = currentInstance;
        
        returnedInstance = currentInstance;
        shouldInit = true;
    }
    
    public void SetData(Spline spline, Vector3 visualRotation, Vector3 visualScale, float projectileSpeed)
    {
        projectileHandler.transform.position = spline.transform.position;
        projectileHandler.scale = visualScale;
        projectileHandler.rotation = visualRotation;
        
        projectileHandler.usedSpeed = projectileSpeed;
    }
}