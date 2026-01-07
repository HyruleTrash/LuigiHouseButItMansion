
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public class RatEnemy : IEnemy
{
    private GameObject instance;
    private EnemyHealth healthComp;
    private NavMeshAgent agentComp;
    private NavAgentGoToTarget goToPlayerComp;
    private readonly RatEnemyData dataInstance = AssetBundle.GetAsset<RatEnemyData>();
    public EnemySpawnManager parentSpawner;
        
    public void Spawn(EnemySpawnManager spawner, Vector3 position)
    {
        parentSpawner = spawner;
        
        if (dataInstance.basicEnemyPool.GetInactiveObject(out var foundEnemy))
        {
            instance = (GameObject)foundEnemy;
            instance.SetActive(true);
            healthComp = instance.GetComponent<EnemyHealth>();
            goToPlayerComp = instance.GetComponent<NavAgentGoToTarget>();
            agentComp = instance.GetComponent<NavMeshAgent>();
            
            healthComp.OnDeath.AddListener(OnDeath);
            
            instance.transform.position = position;
            agentComp.enabled = true;
            List<Material> tempMatExample = new (dataInstance.enemyPrefab.GetComponent<MeshRenderer>().sharedMaterials);
            instance.GetComponent<MeshRenderer>().SetMaterials(tempMatExample);
        }
        else
        {
            instance = Object.Instantiate(dataInstance.enemyPrefab, position, Quaternion.identity);
            healthComp = instance.AddComponent<EnemyHealth>();
            healthComp.OnHit.AddListener(_ =>goToPlayerComp.enabled = true);
            
            goToPlayerComp = instance.AddComponent<NavAgentGoToTarget>();
            goToPlayerComp.minDistance = dataInstance.minPlayerHitDistance;
            goToPlayerComp.getTargetPosition = () => SceneData.instance.GetRegisteredObject<PlayerData>().playerRigidbody.gameObject.transform.position;
            goToPlayerComp.playerReached += () =>
            {
                Debug.Log("Reached");
                goToPlayerComp.enabled = false;
            };
            
            agentComp = instance.GetComponent<NavMeshAgent>();
            
            healthComp.OnDeath.AddListener(OnDeath);
        }

        healthComp.maxHealth = dataInstance.healthData.maxHealth;
        healthComp.invincibilityFrames = dataInstance.healthData.invincibilityFrames;
        
        healthComp.Revive();
        spawner.Add(instance);
    }

    public void OnDeath(GameObject instance)
    {
        instance.SetActive(false);
        dataInstance.basicEnemyPool.ReturnToInActivePool(instance);
        parentSpawner.Remove(instance);
        parentSpawner.CheckLiveEnemyState();
        agentComp.enabled = false;
        healthComp.OnDeath.RemoveListener(OnDeath);
    }
}