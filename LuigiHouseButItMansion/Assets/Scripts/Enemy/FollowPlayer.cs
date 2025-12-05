using System;
using UnityEngine;
using UnityEngine.AI;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]
    private float minDistance = 1;
    private GameObject playerBody;
    private NavMeshAgent navMeshAgent;
    public Action playerReached;

    private void Start()
    {
        playerBody = SceneData.instance.GetRegisteredObject<PlayerData>().playerRigidbody.gameObject;
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (playerBody == null || navMeshAgent == null)
            enabled = false;
    }

    private void Update()
    {
        var playerPos = playerBody.transform.position;
        if (Vector2.Distance(VectorHelper.GetXZ(playerPos), VectorHelper.GetXZ(transform.position)) > navMeshAgent.radius + minDistance)
            navMeshAgent.SetDestination(playerPos);
        else
        {
            navMeshAgent.SetDestination(transform.position);
            playerReached?.Invoke();
        }
    }
}
