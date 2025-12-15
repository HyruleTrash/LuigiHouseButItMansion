using UnityEngine;

[CreateAssetMenu(fileName = "BasicEnemyData", menuName = "ScriptableObjects/Enemies/BasicEnemy")]
public class BasicEnemyData : ScriptableObject
{
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public EnemyHealthData healthData = new();
    [HideInInspector]
    public ObjectPool<GameObject> basicEnemyPool = new();
}