
using System;
using JetBrains.Annotations;
using UnityEngine;

public class IsLocationNear : MonoBehaviour
{
    public Func<Vector3> getLocation;
    public float minDistance;
    private bool near = false;
    
    public bool onNear = false;
    public Action OnNear { get; set; }
    public bool onNoLongerNear = false;
    public Action OnNoLongerNear { get; set; }
    public Action DuringNear { get; set; }

    private void Update()
    {
        if (getLocation == null)
            return;

        var distance = Vector3.Distance(getLocation.Invoke(), transform.position);
        if (distance <= minDistance)
        {
            near = true;
            if (onNear)
                OnNear.Invoke();
        }
        else
        {
            if (onNoLongerNear && near)
            {
                OnNoLongerNear.Invoke();
            }
            near = false;
        }

        if (near)
            DuringNear.Invoke();
    }

    private void OnEnable()
    {
        near = false;
    }
}