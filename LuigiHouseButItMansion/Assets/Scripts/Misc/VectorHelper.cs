using UnityEngine;

public static class VectorHelper
{
    public static Vector2 GetXZ(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 XZToVector3(Vector2 v, float y)
    {
        return new Vector3(v.x, y, v.y);
    }
}