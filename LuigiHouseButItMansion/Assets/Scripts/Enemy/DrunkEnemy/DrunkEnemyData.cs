using UnityEngine;

[CreateAssetMenu(fileName = "DrunkEnemyData", menuName = "ScriptableObjects/Enemies/DrunkEnemy")]
public class DrunkEnemyData : BaseEnemyData
{
    public GameObject enemyPrefab;
    public EnemyHealthData healthData = new();
    public int damageAmount = 1;
}