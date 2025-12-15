
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class BasicEnemy : IEnemy
{
    private GameObject instance;
        
    public void Spawn(EnemySpawnManager spawner, Vector3 position)
    {
        BasicEnemyData dataInstance = AssetBundle.GetAsset<BasicEnemyData>();
        EnemyHealth healthComponent;
        
        
        if (dataInstance == null)
            Debug.Log("ABTICH");
        if (dataInstance.enemyPrefab == null)
            Debug.Log("MISSINGPREFAB");
        if (dataInstance.basicEnemyPool == null)
            Debug.Log("MISSING_POOL");
        
        if (dataInstance.basicEnemyPool.GetInactiveObject(out var foundEnemy))
        {
            instance = (GameObject)foundEnemy;
            instance.SetActive(true);
            healthComponent = instance.GetComponent<EnemyHealth>();
            try
            {
                instance.GetComponent<MeshRenderer>().SetMaterials(new List<Material>(dataInstance.enemyPrefab.GetComponent<MeshRenderer>().materials));
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
            
                healthComponent.OnDeath.AddListener((gameObject) =>
                {
                    instance.SetActive(false);
                    dataInstance.basicEnemyPool.ReturnToInActivePool(gameObject);
                    spawner.Remove(instance);
                    spawner.CheckLiveEnemyState();
                });
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
}