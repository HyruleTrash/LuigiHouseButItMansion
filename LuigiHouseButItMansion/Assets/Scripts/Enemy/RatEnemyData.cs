using UnityEngine;

[CreateAssetMenu(fileName = "RatEnemyData", menuName = "ScriptableObjects/Enemies/RatEnemy")]
public class RatEnemyData : ScriptableObject
{
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public EnemyHealthData healthData = new();
    public float minPlayerHitDistance = 1;
    [HideInInspector]
    public ObjectPool<GameObject> basicEnemyPool = new();
}