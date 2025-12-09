
using System;
using System.Collections.Generic;
using SplineMesh;
using UnityEngine;

public class TrajectoryGetter
{
    private Spline spline;
    private GameObject gameObject;
    private Mesh mesh;
    private Vector3 scale;

    public TrajectoryGetter(Mesh mesh, Vector3 scale)
    {
        this.mesh = mesh;
        this.scale = scale;

        gameObject = new GameObject($"{typeof(TrajectoryGetter)}_{mesh.name}", typeof(Spline));
        spline = gameObject.GetComponent<Spline>();
    }

    public void GetTrajectory(Vector3 position, Quaternion rotation, Vector3 direction, float strength, Action onEnd)
    {
        spline.nodes = new List<SplineNode>(2);
        
        var nextNode = position + (direction * strength);
        spline.AddNode(new SplineNode(position, nextNode));

        var distanceBetweenNodes = Vector3.Distance(position, nextNode);
        nextNode += Vector3.down * distanceBetweenNodes;
        spline.AddNode(new SplineNode( nextNode, nextNode));
        
        onEnd?.Invoke();
    }
}