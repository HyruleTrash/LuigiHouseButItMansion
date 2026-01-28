using System;
using System.Collections.Generic;
using LucasCustomClasses;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public class MarrakaEnemy : BaseEnemy
{
    private static readonly int spawnTriggerAnimId = Animator.StringToHash("Spawn");
    private PlayerData playerRef;
    private MarrakaEnemyData dataInstance;
    
    private Health healthComp;
    private TimerComp stunTimerComp;
    private TimerComp stunInvincibilityTimerComp;
    private bool stunned = false;
    
    private NavMeshAgent agentComp;
    private NavAgentGoToTarget goToPlayerComp;
    private IsLocationNear isPlayerNearComp;

    private Animator animator;
    private MoveTo moveOutOfInteractableAnimComp;
    private TimerComp hideAndSeekTimerComp;
    private InteractableObject hauntedInteractable;
    
    public override void Spawn(EnemySpawnManager spawner, Vector3 spawnPosition)
    {
        PrepareSpawn(spawner, spawnPosition, out MarrakaEnemyData data);
        dataInstance = data;
        
        healthComp.OnHit.AddListener(OnHit);
        healthComp.OnDeath.AddListener(OnDeath);
        hideAndSeekTimerComp.timer = new Timer(dataInstance.hideAndSeekTime, OnHideAndSeekEnd)
        {
            running = false
        };
        Instance.GetComponent<CapsuleCollider>().enabled = false;
        
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
        stunTimerComp = Instance.AddComponent<TimerComp>();
        stunInvincibilityTimerComp = Instance.AddComponent<TimerComp>();

        animator = Instance.GetComponent<Animator>();
        moveOutOfInteractableAnimComp = Instance.AddComponent<MoveTo>();
        moveOutOfInteractableAnimComp.enabled = false;
    }

    protected override void ReUseInstance(object foundEnemy, Vector3 position, object enemyData)
    {
        dataInstance = (MarrakaEnemyData)enemyData;
        base.ReUseInstance(foundEnemy, position, enemyData);
        Instance.transform.position = position; 
        
        stunned = false;
        healthComp = Instance.GetComponent<Health>();
        
        goToPlayerComp = Instance.GetComponent<NavAgentGoToTarget>();
        goToPlayerComp.enabled = false;
        
        agentComp = Instance.GetComponent<NavMeshAgent>();
        agentComp.enabled = false;

        var timers = Instance.GetComponents<TimerComp>();
        hideAndSeekTimerComp = timers[0];
        stunTimerComp = timers[1];
        stunInvincibilityTimerComp = timers[2];
        
        List<Material> tempMatExample = new (dataInstance.enemyPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterials);
        Instance.GetComponentInChildren<MeshRenderer>().SetMaterials(tempMatExample);
        
        animator = Instance.GetComponent<Animator>();
        
        moveOutOfInteractableAnimComp = Instance.GetComponent<MoveTo>();
        moveOutOfInteractableAnimComp.enabled = false;
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
        SetChaseState(dataInstance.strongState);
    }
    
    private void SetChaseState(MarrakaEnemyData.ChaseState chaseState)
    {
        healthComp.maxHealth = chaseState.healthData.maxHealth;
        healthComp.invincibilityFrames = chaseState.healthData.invincibilityFrames;
        agentComp.speed = chaseState.speed;
        
        stunTimerComp.timer = new Timer(chaseState.stunlockTime, OnStunFinished)
        {
            running = false
        };
        stunInvincibilityTimerComp.timer = new Timer(chaseState.stunInvincibilityTime, OnStunInvincibilityFinished)
        {
            running = false
        };
        
        animator.SetTrigger(spawnTriggerAnimId);
        var spawnPoint = hauntedInteractable.GetSpawnPoint();
        Instance.transform.position = spawnPoint;
        
        var dirTowardsPlayer = (playerRef.playerRigidbody.gameObject.transform.position - spawnPoint).normalized;
        if (!NavMesh.SamplePosition(spawnPoint + dirTowardsPlayer, out var hit, 5,
                agentComp.areaMask)) return;
        var destination = hit.position;
        destination.y += agentComp.height / 2 - agentComp.baseOffset / 2;
        
        moveOutOfInteractableAnimComp.Initialize(destination, dataInstance.spawnAnimSpeed);
        moveOutOfInteractableAnimComp.enabled = true;
        moveOutOfInteractableAnimComp.onReachedDestination = new();
        moveOutOfInteractableAnimComp.onReachedDestination.AddListener(() =>
        {
            agentComp.Warp(destination);
            agentComp.enabled = true;
            goToPlayerComp.enabled = true;
            Instance.GetComponent<CapsuleCollider>().enabled = true;
        });
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
        if (stunned)
            return;
        goToPlayerComp.enabled = true;
        agentComp.enabled = false;
        stunTimerComp.timer.Reset();
    }
    
    private void OnStunFinished()
    {
        agentComp.enabled = true;
        stunned = true;
        stunInvincibilityTimerComp.timer.Reset();
    }
    
    private void OnStunInvincibilityFinished()
    {
        stunned = false;
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
