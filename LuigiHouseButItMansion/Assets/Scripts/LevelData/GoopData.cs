
using UnityEngine;

[CreateAssetMenu(fileName = "GoopData", menuName = "ScriptableObjects/RoomData/Goop")]
public class GoopData : ScriptableObject
{
    public Color goopColor;
    public float cutoffThreshold = 0.1f;
}