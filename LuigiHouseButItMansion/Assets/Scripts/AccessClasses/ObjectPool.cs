
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T>
{
    private readonly List<T> activeObjects = new();
    private readonly Stack<T> inActiveObjects = new();

    public bool GetInactiveObject(out object obj)
    {
        obj = null;
        if (inActiveObjects.Count <= 0)
            return false;
        obj = inActiveObjects.Pop();
        activeObjects.Add((T)obj);
        return true;
    }

    public void ReturnToInActivePool(T obj)
    {
        activeObjects.Remove(obj);
        inActiveObjects.Push(obj);
    }
}