
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public class RatEnemy : IEnemy
{
    private GameObject instance;
    private PlayerData playerRef;
    
    private Health healthComp;
    private NavMeshAgent agentComp;
    private NavAgentGoToTarget goToPlayerComp;
    private IsLocationNear isPlayerNearComp;
    
    private readonly RatEnemyData dataInstance = AssetBundle.GetAsset<RatEnemyData>();
    public EnemySpawnManager parentSpawner;
        
    public void Spawn(EnemySpawnManager spawner, Vector3 position)
    {
        parentSpawner = spawner;
        
        if (dataInstance.basicEnemyPool.GetInactiveObject(out var foundEnemy))
        {
            instance = (GameObject)foundEnemy;
            instance.SetActive(true);
            healthComp = instance.GetComponent<Health>();
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
            playerRef = SceneData.instance.GetRegisteredObject<PlayerData>();
            instance = Object.Instantiate(dataInstance.enemyPrefab, position, Quaternion.identity);
            
            isPlayerNearComp = instance.AddComponent<IsLocationNear>();
            isPlayerNearComp.minDistance = dataInstance.minPlayerNearDistance;
            isPlayerNearComp.enabled = false;
            isPlayerNearComp.onNoLongerNear = true;
            isPlayerNearComp.OnNoLongerNear = OnPlayerNoLongerNear;
            isPlayerNearComp.DuringNear = HurtPlater;
            
            healthComp = instance.AddComponent<Health>();
            healthComp.OnHit.AddListener(_ =>goToPlayerComp.enabled = true);
            
            goToPlayerComp = instance.AddComponent<NavAgentGoToTarget>();
            goToPlayerComp.minDistance = dataInstance.minPlayerHitDistance;
            goToPlayerComp.getTargetPosition = () => playerRef.playerRigidbody.gameObject.transform.position;
            goToPlayerComp.playerReached += OnPlayerReached;
            
            agentComp = instance.GetComponent<NavMeshAgent>();
            
            healthComp.OnDeath.AddListener(OnDeath);
        }

        healthComp.maxHealth = dataInstance.healthData.maxHealth;
        healthComp.invincibilityFrames = dataInstance.healthData.invincibilityFrames;
        
        healthComp.Revive();
        spawner.Add(instance);
    }

    private void HurtPlater()
    {
        playerRef.TriggerHitFlash();
        playerRef.GetComponent<Health>().Hit(this, 1);
    }

    private void OnPlayerReached()
    {
        HurtPlater();
        isPlayerNearComp.getLocation = () => playerRef.playerRigidbody.gameObject.transform.position;
        isPlayerNearComp.enabled = true;
        goToPlayerComp.enabled = false;
    }

    private void OnPlayerNoLongerNear()
    {
        goToPlayerComp.enabled = true;
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