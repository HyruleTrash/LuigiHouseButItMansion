using UnityEngine;

[CreateAssetMenu(fileName = "RatEnemyData", menuName = "ScriptableObjects/Enemies/RatEnemy")]
public class RatEnemyData : ScriptableObject
{
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public EnemyHealthData healthData = new();
    public float minPlayerHitDistance = 1;
    [Tooltip("Used for if the player walks out of attack range")]
    public float minPlayerNearDistance = 2;
    [HideInInspector]
    public ObjectPool<GameObject> basicEnemyPool = new();
}