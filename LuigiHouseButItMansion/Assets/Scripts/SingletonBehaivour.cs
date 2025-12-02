using UnityEngine;

/// <summary>
/// Simple singleton class that can be inherited from, gets auto instantiated if it doesn't exist yet
/// </summary>
public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject temp = new GameObject(typeof(T).Name);
                _instance = (T)temp.AddComponent(typeof(T));
            }
            return _instance;
        }
    }
    
    protected void Awake()
    {
        if (_instance != null && _instance != GetComponent<T>())
        {
            Destroy(gameObject);
        } else {
            _instance = GetComponent<T>();
            DontDestroyOnLoad(gameObject);
        }
    }
}