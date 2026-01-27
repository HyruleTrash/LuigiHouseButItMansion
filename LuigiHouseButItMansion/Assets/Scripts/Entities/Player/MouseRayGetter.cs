using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseRayGetter : SingletonBehaviour<MouseRayGetter>
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private RenderTexture renderTexture;
    private RaycastHit[] hitsBuffer = new RaycastHit[20];
    public float minMouseActivity = 2f;
    
    public Ray GetMouseRay()
    {
        float CalculateMousePos(float mPos, float sSize, int rTex) => 1f / sSize * mPos * rTex;
        
        var mousePos = Mouse.current.position.ReadValue();
        var trueMousePos = new Vector2(CalculateMousePos(mousePos.x, Screen.width, renderTexture.width),
            CalculateMousePos(mousePos.y, Screen.height, renderTexture.height));
        
        return cam.ScreenPointToRay(trueMousePos);
    }

    public RaycastHit? GetRelevantHitBasedOnLastDirection(ref Vector3? lastLookDirection, Vector3 origin, LayerMask layerMask, out bool sizeZero, float threshold = 0.5f)
    {
        sizeZero = false;
        var mouseRay = GetMouseRay();
        
        var size = Physics.RaycastNonAlloc(mouseRay, hitsBuffer, Mathf.Infinity, layerMask);
        if (size == 0)
        {
            sizeZero = true;
            return null;
        }
        
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float movementAmount = mouseDelta.magnitude;
        
        Array.Sort(hitsBuffer, 0, size,
            Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));
        
        foreach (var hit in hitsBuffer)
        {
            var newLookDir = (hit.point - origin).normalized;
            if (lastLookDirection == null || movementAmount > minMouseActivity)
            {
                lastLookDirection = newLookDir;
                return hit;
            }
            
            var dot = Vector3.Dot(lastLookDirection.Value, newLookDir);
            if (!(dot > threshold)) continue;
            lastLookDirection = newLookDir;
            return hit;
        }
        
        return null;
    }
}