using System.Collections.Generic;
using UnityEngine;

public class DrunkEnemy : BaseEnemy
{
    private PlayerData playerRef;
    private DrunkEnemyData dataInstance;
    
    private Health healthComp;
    
    public override void Spawn(EnemySpawnManager spawner, Vector3 spawnPosition)
    {
        PrepareSpawn(spawner, spawnPosition, out DrunkEnemyData data);
        dataInstance = data;
        
        healthComp.maxHealth = dataInstance.healthData.maxHealth;
        healthComp.invincibilityFrames = dataInstance.healthData.invincibilityFrames;
        
        healthComp.Revive();
        spawner.Add(Instance);
    }

    protected override void FirstInstance(Vector3 position, object enemyData)
    {
        dataInstance = (DrunkEnemyData)enemyData;
        playerRef = SceneData.instance.GetRegisteredObject<PlayerData>();
        Instance = Object.Instantiate(dataInstance.enemyPrefab, position, Quaternion.identity);
        
        
        healthComp = Instance.AddComponent<Health>();
        healthComp.OnDeath.AddListener(OnDeath);
    }

    protected override void ReUseInstance(object foundEnemy, Vector3 position, object enemyData)
    {
        base.ReUseInstance(foundEnemy, position, enemyData);
        dataInstance = (DrunkEnemyData)enemyData;
        Instance.transform.position = position;
        
        healthComp = Instance.GetComponent<Health>();
        
        healthComp.OnDeath.AddListener(OnDeath);
        
        List<Material> tempMatExample = new (dataInstance.enemyPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterials);
        Instance.GetComponentInChildren<MeshRenderer>().SetMaterials(tempMatExample);
    }
    
    public void OnDeath(GameObject instance)
    {
        instance.SetActive(false);
        dataInstance.basicEnemyPool.ReturnToInActivePool(instance);
        ParentSpawner.Remove(instance);
        ParentSpawner.CheckLiveEnemyState();
        
        healthComp.OnDeath.RemoveListener(OnDeath);
    }
}