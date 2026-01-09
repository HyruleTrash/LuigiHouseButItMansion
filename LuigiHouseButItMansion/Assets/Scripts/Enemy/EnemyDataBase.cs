
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class EnemyDataBase
{
    public static EnemyDataBase Instance
    {
        get
        {
            instance ??= new EnemyDataBase();
            return instance;
        }
    }
    [CanBeNull] private static EnemyDataBase instance = null;

    public Dictionary<Type, BaseEnemyData> data = new()
    {
        {typeof(RatEnemy), AssetBundle.GetAsset<RatEnemyData>()},
        {typeof(MarrakaEnemy), AssetBundle.GetAsset<MarrakaEnemyData>()},
        {typeof(DrunkEnemy), AssetBundle.GetAsset<DrunkEnemyData>()}
    };

    public BaseEnemyData GetData(Type type)
    {
        foreach (var pair in data)
        {
            if (pair.Key == type)
                return pair.Value;
        }
        Debug.Log($"Could not find data for type {type}");
        return null;
    }
}