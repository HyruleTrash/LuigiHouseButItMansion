
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SplineMesh;
using Unity.Mathematics;
using UnityEngine;

public class LiquidTrajectoryGetter
{
    private GameObject parent;
    
    private Spline spline;
    private GameObject splineObject;
    private GameObject collisionObj;
    private SphereCollider sphereCollider;
    
    private LayerMask layerMask;
    
    public struct SplineCollision
    {
        public bool collided;
        public Vector3 contactPoint;
        public Vector3 direction;
        public float distance;
        public Vector3 highestPoint;
        public Collider collidedWith;
    }

    public LiquidTrajectoryGetter(float radius, GameObject parent, LayerMask layerMask)
    {
        this.layerMask = layerMask;
        this.parent = parent;

        splineObject = new GameObject($"{typeof(LiquidTrajectoryGetter)}", typeof(Spline));
        collisionObj = new GameObject($"{typeof(LiquidTrajectoryGetter)}_collisionChecker", typeof(SphereCollider));
        collisionObj.transform.SetParent(splineObject.transform);
        
        splineObject.layer = LayerMask.NameToLayer("Projectile");
        collisionObj.layer = splineObject.layer;
        
        spline = splineObject.GetComponent<Spline>();
        
        sphereCollider = collisionObj.GetComponent<SphereCollider>();
        sphereCollider.radius = radius;
        sphereCollider.isTrigger = true;
    }

    /// <summary>
    /// Returns collision data and the calculated spline trajectory, this spline's data needs to be copied not used directly
    /// </summary>
    /// <param name="onEnd">function that's called to handle the found data</param>
    public void GetTrajectory(Vector3 shotStartPosition, Vector3 direction, float strength, Action<SplineCollision, Spline> onEnd)
    {
        splineObject.transform.position = parent.transform.position;
        
        spline.nodes = new List<SplineNode>(2);
        spline.RefreshCurves();
        
        var nextNode = shotStartPosition + direction * strength;
        spline.AddNode(new SplineNode(shotStartPosition, nextNode));

        var distanceBetweenNodes = Vector3.Distance(shotStartPosition, nextNode);
        nextNode += Vector3.down * distanceBetweenNodes;
        spline.AddNode(new SplineNode( nextNode, nextNode));

        var output = CheckCollisionAllongSpline();
        if (output.collided)
        {
            // Collided
            var offsetDirection = (-output.direction + direction) / 2;
            spline.nodes[1].Position = (output.contactPoint - splineObject.transform.position) + offsetDirection * output.distance;
            offsetDirection = (-output.direction + direction) / 2 * 0.1f;
            spline.nodes[1].Direction = spline.nodes[1].Position + offsetDirection;

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
        spline.RefreshCurves();

        onEnd?.Invoke(output, spline);
    }

    private SplineCollision CheckCollisionAllongSpline()
    {
        var stepSize = Mathf.Max(
            sphereCollider.radius,
            0.01f
        );
        sphereCollider.enabled = true;

        Vector3 highestPoint = GetHighestPointInSpline(stepSize);

        for (float i = 0; i < spline.Length; i += stepSize)
        {
            var curve = spline.GetSampleAtDistance(i);
            var checkOriginPos = curve.location + splineObject.transform.position;

            Collider[] colliders = Physics.OverlapSphere(
                checkOriginPos,
                sphereCollider.radius,
                layerMask
            );
            if (colliders.Length <= 0) continue;
            
            foreach (var collision in colliders)
            {
                if (collision.gameObject == parent) continue; 
                if ((layerMask & (1 << collision.gameObject.layer)) == 0) continue; 
                // Inside bounds and layer

                collisionObj.transform.position = checkOriginPos;
                    
                var rot = splineObject.transform.rotation;
                var posOther = collision.transform.position;
                var rotOther = collision.transform.rotation;

                if (!Physics.ComputePenetration(
                        sphereCollider, checkOriginPos, rot,
                        collision, posOther, rotOther,
                        out var direction, out var distance)) continue;
                // collision penetrated
                
                collisionObj.transform.localPosition = Vector3.zero;
                sphereCollider.enabled = false;
                return new SplineCollision {collided = true, contactPoint = checkOriginPos, distance = distance, direction = direction, highestPoint = highestPoint, collidedWith = collision};
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