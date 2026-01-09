
using System;
using UnityEngine;
using UnityEngine.Events;

public class MoveTo : MonoBehaviour
{
    [SerializeField]
    private Vector3 destination;
    [SerializeField]
    private float speed = 1;
    private Vector3? originalPosition = null;
    public UnityEvent onReachedDestination;
    [SerializeField] private float sizea = 1;

    private Vector3 foundOriginalPosition;
    private void Update()
    {
        if (originalPosition == null)
            return;
        foundOriginalPosition = originalPosition.Value;
        
        transform.position = Vector3.Lerp(transform.position, destination, speed * Time.deltaTime);
        if (!(Vector3.Distance(transform.position, destination) < 0.1f)) return;
        originalPosition = null;
        onReachedDestination?.Invoke();
        enabled = false;
    }

    public void Initialize(Vector3 dest, float spd)
    {
        destination = dest;
        speed = spd;
        originalPosition = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(foundOriginalPosition, sizea);
        Gizmos.DrawSphere(destination, sizea);
    }
}