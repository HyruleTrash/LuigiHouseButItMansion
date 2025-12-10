
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SplineMesh;
using Unity.Mathematics;
using UnityEngine;

public class TrajectoryGetter
{
    private GameObject parent;
    
    private Spline spline;
    private GameObject splineObject;
    private GameObject meshObject;
    private MeshCollider meshCollider;
    
    private Mesh mesh;
    private LayerMask layerMask;

    public TrajectoryGetter(Mesh mesh, GameObject parent, Vector3 scale, LayerMask layerMask)
    {
        this.mesh = mesh;
        this.layerMask = layerMask;
        this.parent = parent;

        splineObject = new GameObject($"{typeof(TrajectoryGetter)}_{mesh.name}", typeof(Spline));
        meshObject = new GameObject($"{typeof(TrajectoryGetter)}_{mesh.name}_Mesh", typeof(MeshCollider));
        meshObject.transform.SetParent(splineObject.transform);
        
        splineObject.layer = LayerMask.NameToLayer("Projectile");
        meshObject.layer = splineObject.layer;
        
        spline = splineObject.GetComponent<Spline>();
        
        meshCollider = meshObject.GetComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
        meshCollider.sharedMesh = mesh;
        
        meshObject.transform.localScale = scale;
    }

    /// <summary>
    /// Returns collision data and the calculated spline trajectory, this spline's data needs to be copied not used directly
    /// </summary>
    /// <param name="onEnd">function thats called to handle the found data</param>
    public void GetTrajectory(Vector3 shotStartPosition, Vector3 direction, float strength, Action<SplineCollision, Spline> onEnd)
    {
        splineObject.transform.position = parent.transform.position;
        
        spline.nodes = new List<SplineNode>(2);
        
        var nextNode = shotStartPosition + direction * strength;
        spline.AddNode(new SplineNode(shotStartPosition, nextNode));

        var distanceBetweenNodes = Vector3.Distance(shotStartPosition, nextNode);
        nextNode += Vector3.down * distanceBetweenNodes;
        spline.AddNode(new SplineNode( nextNode, nextNode));

        var output = CheckCollisionAllongSpline(shotStartPosition);
        if (output.collided)
        {
            // Collided
            spline.nodes[1].Position = (output.contactPoint - splineObject.transform.position) + -output.direction * output.distance;
            spline.nodes[1].Direction = spline.nodes[1].Position;

            spline.nodes[0].Direction = new Vector3(spline.nodes[1].Position.x, output.highestPoint.y, spline.nodes[1].Position.z);
            var directionTowardsHighestPoint = (output.highestPoint - spline.nodes[1].Position).normalized;
            spline.nodes[0].Direction -= directionTowardsHighestPoint;
        }
        else
        {
            if (Physics.Raycast(spline.nodes[1].Position + splineObject.transform.position, Vector3.down,
                    out var hit, math.INFINITY, layerMask))
            {
                // trajectory irrelevant going straight down
                var collisionPosition = hit.point - splineObject.transform.position;
                spline.AddNode(new SplineNode(collisionPosition, collisionPosition));
            }
            else
            {
                // shot into the void
                var voidPosition = spline.nodes[1].Position + Vector3.down * 90;
                spline.AddNode(new SplineNode(voidPosition, voidPosition));
            }
        }

        onEnd?.Invoke(output, spline);
    }

    public List<Vector3> temp = new();

    public struct SplineCollision
    {
        public bool collided;
        public Vector3 contactPoint;
        public Vector3 direction;
        public float distance;
        public Vector3 highestPoint;
    }

    private SplineCollision CheckCollisionAllongSpline(Vector3 shotStartPosition)
    {
        var stepSize = mesh.bounds.extents.magnitude / 2;
        var halfExtents = mesh.bounds.extents / 2;
        meshCollider.enabled = true;

        Vector3 highestPoint = GetHighestPointInSpline(stepSize);

        for (float i = 0; i < spline.Length; i += stepSize)
        {
            var curve = spline.GetSampleAtDistance(i);
            var checkOriginPos = curve.location + splineObject.transform.position;
            temp.Add(checkOriginPos);

            Collider[] colliders = Physics.OverlapBox(checkOriginPos, halfExtents);
            if (colliders.Length <= 0) continue;
            
            foreach (var collision in colliders)
            {
                if ((layerMask & (1 << collision.gameObject.layer)) == 0) continue; 
                // Inside bounds and layer

                Debug.Log(collision.gameObject.name);

                meshObject.transform.position = checkOriginPos;
                    
                var rot = splineObject.transform.rotation;
                var posOther = collision.transform.position;
                var rotOther = collision.transform.rotation;

                if (!Physics.ComputePenetration(
                        meshCollider, checkOriginPos, rot,
                        collision, posOther, rotOther,
                        out var direction, out var distance)) continue;
                // collision penetrated
                
                meshObject.transform.localPosition = Vector3.zero;
                meshCollider.enabled = false;
                return new SplineCollision {collided = true, contactPoint = checkOriginPos, distance = distance, direction = direction, highestPoint = highestPoint};
            }
        }

        return new SplineCollision { collided = false };
    }

    private Vector3 GetHighestPointInSpline(float stepSize)
    {
        var highestPoint = Vector3.zero;
        for (float i = 0; i < spline.Length; i += stepSize)
        {
            var curve = spline.GetSampleAtDistance(i);
            var checkOriginPos = curve.location;
            if (checkOriginPos.y > highestPoint.y)
                highestPoint = checkOriginPos;
        }
        return highestPoint;
    }
}