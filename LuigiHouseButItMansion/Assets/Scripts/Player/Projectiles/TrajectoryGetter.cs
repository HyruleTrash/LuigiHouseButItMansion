
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SplineMesh;
using UnityEngine;

public class TrajectoryGetter
{
    private Spline spline;
    private GameObject gameObject;
    private Mesh mesh;
    private LayerMask layerMask;

    public TrajectoryGetter(Mesh mesh, GameObject parent)
    {
        this.mesh = mesh;

        gameObject = new GameObject($"{typeof(TrajectoryGetter)}_{mesh.name}", typeof(Spline));
        spline = gameObject.GetComponent<Spline>();
        
        gameObject.transform.position = parent.transform.position;
        gameObject.transform.rotation = parent.transform.rotation;
    }

    public void GetTrajectory(Vector3 position, Vector3 direction, float strength, Action onEnd)
    {
        spline.nodes = new List<SplineNode>(2);
        
        var nextNode = position + direction * (strength * direction.y);
        spline.AddNode(new SplineNode(position, nextNode));

        var distanceBetweenNodes = Vector3.Distance(position, nextNode);
        nextNode += Vector3.down * distanceBetweenNodes;
        spline.AddNode(new SplineNode( nextNode, nextNode));

        var output = CheckCollisionAllongSpline();
        
        onEnd?.Invoke();
    }

    public List<Vector3> temp = new();

    [CanBeNull]
    private object CheckCollisionAllongSpline()
    {
        float stepSize = mesh.bounds.extents.magnitude;
        var halfExtents = mesh.bounds.extents / 2;

        for (float i = 0; i < spline.Length; i += stepSize)
        {
            var curve = spline.GetSampleAtDistance(i);
            temp.Add(curve.location);

            var hits = Physics.BoxCastAll(curve.location, halfExtents, Vector3.zero);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (( layerMask & (1 << hit.collider.gameObject.layer)) != 0)
                    {
                        // yup
                    }
                    else
                    {
                        // nope
                    }
                }
            }
        }

        return null;
    }
}