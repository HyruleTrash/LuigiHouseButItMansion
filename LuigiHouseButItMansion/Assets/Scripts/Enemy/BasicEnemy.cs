
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public class BasicEnemy : IEnemy
{
    private GameObject instance;
    private EnemyHealth healthComponent;
    private NavMeshAgent agentComponent;
    private readonly BasicEnemyData dataInstance = AssetBundle.GetAsset<BasicEnemyData>();
    public EnemySpawnManager parentSpawner;
        
    public void Spawn(EnemySpawnManager spawner, Vector3 position)
    {
        parentSpawner = spawner;
        
        if (dataInstance.basicEnemyPool.GetInactiveObject(out var foundEnemy))
        {
            instance = (GameObject)foundEnemy;
            instance.SetActive(true);
            healthComponent = instance.GetComponent<EnemyHealth>();
            agentComponent = instance.GetComponent<NavMeshAgent>();
            
            healthComponent.OnDeath.AddListener(OnDeath);
            
            instance.transform.position = position;
            agentComponent.enabled = true;
            try
            {
                List<Material> tempMatExample = new (dataInstance.enemyPrefab.GetComponent<MeshRenderer>().sharedMaterials);
                instance.GetComponent<MeshRenderer>().SetMaterials(tempMatExample);
            }
            catch (Exception e)
            {
                Debug.Log($"EXCEPTIONTYPE_A {e}");
            }
        }
        else
        {
            try
            {
                instance = Object.Instantiate(dataInstance.enemyPrefab, position, Quaternion.identity);
                healthComponent = instance.AddComponent<EnemyHealth>();
                agentComponent = instance.GetComponent<NavMeshAgent>();
                
                healthComponent.OnDeath.AddListener(OnDeath);
            }
            catch (Exception e)
            {
                Debug.Log($"EXCEPTIONTYPE_B {e}");
                throw;
            }
        }

        healthComponent.maxHealth = dataInstance.healthData.maxHealth;
        healthComponent.invincibilityFrames = dataInstance.healthData.invincibilityFrames;
        
        healthComponent.Revive();
        spawner.Add(instance);
    }

    public void OnDeath(GameObject instance)
    {
        instance.SetActive(false);
        dataInstance.basicEnemyPool.ReturnToInActivePool(instance);
        parentSpawner.Remove(instance);
        parentSpawner.CheckLiveEnemyState();
        agentComponent.enabled = false;
        healthComponent.OnDeath.RemoveListener(OnDeath);
    }
}