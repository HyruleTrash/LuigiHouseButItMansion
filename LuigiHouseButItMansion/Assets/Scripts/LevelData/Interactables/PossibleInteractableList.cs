
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PossibleInteractableList", menuName = "ScriptableObjects/RoomData/PossibleInteractableList")]
public class PossibleInteractableList : ScriptableObject
{
    public List<GameObject> prefabs;
}