
using UnityEngine;

public abstract class BaseEnemy
{
    protected EnemySpawnManager ParentSpawner { get; private set; }
    protected GameObject Instance { get; set; }

    protected void PrepareSpawn<T>(EnemySpawnManager spawner, Vector3 position, out T enemyData) where T : BaseEnemyData
    {
        ParentSpawner = spawner;
        enemyData = AssetBundle.GetAsset<T>();
        
        if (enemyData.basicEnemyPool.GetInactiveObject(out var foundEnemy))
            ReUseInstance(foundEnemy, position, enemyData);
        else
            FirstInstance(position, enemyData);
    }

    public abstract void Spawn(EnemySpawnManager spawner, Vector3 position);

    protected abstract void FirstInstance(Vector3 position, object enemyData);

    protected virtual void ReUseInstance(object foundEnemy, Vector3 position, object enemyData)
    {
        Instance = (GameObject)foundEnemy;
        Instance.SetActive(true);
    }
}

public class BaseEnemyData : ScriptableObject
{
    [HideInInspector]
    public ObjectPool<GameObject> basicEnemyPool = new();

    public bool usesSpawnPoint;
}