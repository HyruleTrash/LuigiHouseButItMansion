
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public class RatEnemy : BaseEnemy
{
    private PlayerData playerRef;
    private RatEnemyData dataInstance;
    
    private Health healthComp;
    private NavMeshAgent agentComp;
    private NavAgentGoToTarget goToPlayerComp;
    private IsLocationNear isPlayerNearComp;

    public override void Spawn(EnemySpawnManager spawner, Vector3 position)
    {
        PrepareSpawn(spawner, position, out RatEnemyData data);
        dataInstance = data;

        healthComp.maxHealth = dataInstance.healthData.maxHealth;
        healthComp.invincibilityFrames = dataInstance.healthData.invincibilityFrames;
        
        healthComp.Revive();
        spawner.Add(Instance);
    }

    protected override void FirstInstance(Vector3 position, object enemyData)
    {
        dataInstance = (RatEnemyData)enemyData;
        playerRef = SceneData.instance.GetRegisteredObject<PlayerData>();
        Instance = Object.Instantiate(dataInstance.enemyPrefab, position, Quaternion.identity);
            
        isPlayerNearComp = Instance.AddComponent<IsLocationNear>();
        isPlayerNearComp.minDistance = dataInstance.minPlayerNearDistance;
        isPlayerNearComp.enabled = false;
        isPlayerNearComp.onNoLongerNear = true;
        isPlayerNearComp.OnNoLongerNear = OnPlayerNoLongerNear;
        isPlayerNearComp.DuringNear = HurtPlayer;
            
        healthComp = Instance.AddComponent<Health>();
        healthComp.OnHit.AddListener(_ =>goToPlayerComp.enabled = true);
        healthComp.OnDeath.AddListener(OnDeath);
            
        goToPlayerComp = Instance.AddComponent<NavAgentGoToTarget>();
        goToPlayerComp.minDistance = dataInstance.minPlayerHitDistance;
        goToPlayerComp.getTargetPosition = () => playerRef.playerRigidbody.gameObject.transform.position;
        goToPlayerComp.playerReached += OnPlayerReached;
            
        agentComp = Instance.GetComponent<NavMeshAgent>();
    }

    protected override void ReUseInstance(object foundEnemy, Vector3 position, object enemyData)
    {
        base.ReUseInstance(foundEnemy, position, enemyData);
        dataInstance = (RatEnemyData)enemyData;
        Instance.transform.position = position;
        
        healthComp = Instance.GetComponent<Health>();
        goToPlayerComp = Instance.GetComponent<NavAgentGoToTarget>();
        agentComp = Instance.GetComponent<NavMeshAgent>();
            
        healthComp.OnDeath.AddListener(OnDeath);
            
        agentComp.enabled = true;
        
        List<Material> tempMatExample = new (dataInstance.enemyPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterials);
        Instance.GetComponentInChildren<MeshRenderer>().SetMaterials(tempMatExample);
    }

    private void HurtPlayer()
    {
        playerRef.TriggerHitFlash();
        playerRef.GetComponent<Health>().Hit(this, dataInstance.damageAmount);
    }

    private void OnPlayerReached()
    {
        HurtPlayer();
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
        ParentSpawner.Remove(instance);
        ParentSpawner.CheckLiveEnemyState();
        agentComp.enabled = false;
        healthComp.OnDeath.RemoveListener(OnDeath);
    }
}