using System;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentGoToTarget : MonoBehaviour
{
    public float minDistance = 1;
    private NavMeshAgent navMeshAgent;
    public Action playerReached;
    public Func<Vector3> getTargetPosition;
    public Action fixedUpdate;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (getTargetPosition == null || navMeshAgent == null)
            enabled = false;
    }

    private void Update()
    {
        if (getTargetPosition == null || !navMeshAgent){
            enabled = false;
            return;
        }
        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return;

        var targetPos = getTargetPosition.Invoke();
        if (Vector2.Distance(VectorHelper.GetXZ(targetPos), VectorHelper.GetXZ(transform.position)) > navMeshAgent.radius + minDistance)
            navMeshAgent.SetDestination(targetPos);
        else
        {
            navMeshAgent.SetDestination(transform.position);
            playerReached?.Invoke();
        }
    }

    private void FixedUpdate()
    {
        fixedUpdate?.Invoke();
    }
}
