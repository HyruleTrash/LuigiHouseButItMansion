
using UnityEngine;

public class BasicEnemy : IEnemy
{
    private GameObject instance;
        
    public void Spawn(EnemySpawnManager spawner, Vector3 position)
    {
        instance = Object.Instantiate(BasicEnemyData.Instance.enemyPrefab, position, Quaternion.identity);
        spawner.Add(instance);
    }
}