
using UnityEngine;

[CreateAssetMenu(fileName = "BasicEnemyData", menuName = "ScriptableObjects/Enemies/BasicEnemy")]
public class BasicEnemyData : ScriptableObjectSingleton<BasicEnemyData>
{
    public GameObject enemyPrefab;
    public EnemyHealthData healthData;
    [HideInInspector]
    public ObjectPool<GameObject> basicEnemyPool = new ();
}

public class BasicEnemy : IEnemy
{
    private GameObject instance;
        
    public void Spawn(EnemySpawnManager spawner, Vector3 position)
    {
        EnemyHealthData dataExampleHealth = BasicEnemyData.Instance.healthData;
        EnemyHealth healthComponent;
        
        if (BasicEnemyData.Instance.basicEnemyPool.GetInactiveObject(out var foundEnemy))
        {
            instance = (GameObject)foundEnemy;
            instance.SetActive(true);
            healthComponent = instance.GetComponent<EnemyHealth>();
        }
        else
        {
            instance = Object.Instantiate(BasicEnemyData.Instance.enemyPrefab, position, Quaternion.identity);
            healthComponent = instance.AddComponent<EnemyHealth>();
            
            healthComponent.OnDeath.AddListener((gameObject) =>
            {
                instance.SetActive(false);
                BasicEnemyData.Instance.basicEnemyPool.ReturnToInActivePool(gameObject);
            });
        }

        healthComponent.maxHealth = dataExampleHealth.maxHealth;
        healthComponent.invincibilityFrames = dataExampleHealth.invincibilityFrames;
        
        healthComponent.Revive();
        spawner.Add(instance);
    }
}