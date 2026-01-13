using System;
using UnityEngine;

public class MoveToAndBack : MonoBehaviour
{
    private bool to = false;
    [SerializeField]
    private Vector3 toward;
    [SerializeField]
    private Vector3 backward;
    [SerializeField]
    private float speed = 1;
    private Vector3? originalPosition = null;

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
        if (originalPosition == null)
            return;
        var foundOriginalPosition = originalPosition.Value;

        Vector3 destination;
        bool result;
        
        if (to)
        {
            destination = foundOriginalPosition + toward;
            result = false;
        }
        else
        {
            destination = foundOriginalPosition + backward;
            result = true;
        }
        
        transform.position = Vector3.Lerp(transform.position, destination, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, destination) < 0.1f)
            to = result;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        if (originalPosition == null)
            originalPosition = transform.position;
        Gizmos.DrawSphere(toward + originalPosition.Value, 0.1f);
        Gizmos.DrawSphere(backward + originalPosition.Value, 0.1f);
    }
}
