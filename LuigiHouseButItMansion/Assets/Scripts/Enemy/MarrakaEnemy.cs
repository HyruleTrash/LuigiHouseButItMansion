using System;
using System.Collections.Generic;
using LucasCustomClasses;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public class MarrakaEnemy : BaseEnemy
{
    private PlayerData playerRef;
    private MarrakaEnemyData dataInstance;
    
    private Health healthComp;
    private NavMeshAgent agentComp;
    private NavAgentGoToTarget goToPlayerComp;
    private IsLocationNear isPlayerNearComp;
    private TimerComp hideAndSeekTimerComp;
    private InteractableObject hauntedInteractable;
    
    public override void Spawn(EnemySpawnManager spawner, Vector3 position)
    {
        PrepareSpawn(spawner, position, out MarrakaEnemyData data);
        dataInstance = data;
        
        healthComp.OnHit.AddListener(OnHit);
        healthComp.OnDeath.AddListener(OnDeath);
        hideAndSeekTimerComp.timer = new Timer(dataInstance.hideAndSeekTime, OnHideAndSeekEnd)
        {
            running = false
        };
        
        spawner.Add(Instance);
        
        TriggerHideMechanic(spawner);
    }

    protected override void FirstInstance(Vector3 position, object enemyData)
    {
        dataInstance = (MarrakaEnemyData)enemyData;
        playerRef = SceneData.instance.GetRegisteredObject<PlayerData>();
        Instance = Object.Instantiate(dataInstance.enemyPrefab, position, Quaternion.identity);
            
        isPlayerNearComp = Instance.AddComponent<IsLocationNear>();
        isPlayerNearComp.minDistance = dataInstance.minPlayerNearDistance;
        isPlayerNearComp.enabled = false;
        isPlayerNearComp.onNoLongerNear = true;
        isPlayerNearComp.OnNoLongerNear = OnPlayerNoLongerNear;
        isPlayerNearComp.DuringNear = HurtPlayer;
        
        healthComp = Instance.AddComponent<Health>();
            
        goToPlayerComp = Instance.AddComponent<NavAgentGoToTarget>();
        goToPlayerComp.minDistance = dataInstance.minPlayerHitDistance;
        goToPlayerComp.getTargetPosition = () => playerRef.playerRigidbody.gameObject.transform.position;
        goToPlayerComp.playerReached += OnPlayerReached;
        goToPlayerComp.enabled = false;
            
        agentComp = Instance.GetComponent<NavMeshAgent>();
        agentComp.enabled = false;

        hideAndSeekTimerComp = Instance.AddComponent<TimerComp>();
    }

    protected override void ReUseInstance(object foundEnemy, Vector3 position, object enemyData)
    {
        dataInstance = (MarrakaEnemyData)enemyData;
        base.ReUseInstance(foundEnemy, position, enemyData);
        Instance.transform.position = position; 
        
        healthComp = Instance.GetComponent<Health>();
        
        goToPlayerComp = Instance.GetComponent<NavAgentGoToTarget>();
        goToPlayerComp.enabled = false;
        
        agentComp = Instance.GetComponent<NavMeshAgent>();
        agentComp.enabled = false;
        
        hideAndSeekTimerComp = Instance.GetComponent<TimerComp>();
        
        List<Material> tempMatExample = new (dataInstance.enemyPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterials);
        Instance.GetComponentInChildren<MeshRenderer>().SetMaterials(tempMatExample);
    }

    private void TriggerHideMechanic(EnemySpawnManager spawner)
    {
        var interactableObjectsManager = spawner.parentRoom.interactableObjectsManager;
        if (interactableObjectsManager == null)
        {
            OnDeath(Instance);
            return;
        }

        hauntedInteractable = interactableObjectsManager.GetRandom();
        if (hauntedInteractable == null)
        {
            OnDeath(Instance);
            return;
        }
        
        hauntedInteractable.OnInteract.AddListener(() =>
        {
            hideAndSeekTimerComp.timer.running = false;
            SetChaseState(dataInstance.weakState);
        });
        
        hideAndSeekTimerComp.timer.running = true;
    }
    
    private void OnHideAndSeekEnd()
    {
        Debug.Log("time out");
        SetChaseState(dataInstance.strongState);
    }
    
    private void SetChaseState(MarrakaEnemyData.ChaseState chaseState)
    {
        healthComp.maxHealth = chaseState.healthData.maxHealth;
        healthComp.invincibilityFrames = chaseState.healthData.invincibilityFrames;
        agentComp.speed = chaseState.speed;
        
        // TODO Trigger spawn animation
        Instance.transform.position = hauntedInteractable.GetSpawnPoint();
        agentComp.enabled = true;
        goToPlayerComp.enabled = true;
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
    
    private void OnHit(GameObject _)
    {
        goToPlayerComp.enabled = true;
    }
    
    private void OnDeath(GameObject instance)
    {
        instance.SetActive(false);
        dataInstance.basicEnemyPool.ReturnToInActivePool(instance);
        ParentSpawner.Remove(instance);
        ParentSpawner.CheckLiveEnemyState();
        agentComp.enabled = false;
        healthComp.OnDeath.RemoveListener(OnDeath);
        healthComp.OnHit.RemoveListener(OnHit);
    }
}
