
using System;
using System.Collections.Generic;
using LucasCustomClasses;
using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    public RoomEntrance otherRoomEntrance;
    private RoomObjectData parentRoom;
    public Collider entranceTrigger;
    private Timer disableTimer;
    [SerializeField]
    private Vector3 spawnPosition;
    
    private void Awake()
    {
        parentRoom = transform.parent.GetComponent<RoomObjectData>();
        if (parentRoom == null)
        {
            enabled = false;
            return;
        }

        if (parentRoom.entrances == null)
            parentRoom.entrances = new List<RoomEntrance>();
        parentRoom.entrances.Add(this);

        if (otherRoomEntrance == null)
            enabled = false;

        if (entranceTrigger != null) return;
        entranceTrigger = GetComponent<Collider>();
        entranceTrigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var dataRef = other.GetComponent<PlayerDataReference>();
            if (dataRef == null) return;
            
            otherRoomEntrance.DisableTriggerTimed();
            otherRoomEntrance.SpawnPlayer(dataRef.playerData);
        }
    }

    private void SpawnPlayer(PlayerData playerData)
    {
        playerData.SetCurrentRoom(parentRoom);
        playerData.SetPlayerPosition(otherRoomEntrance.spawnPosition + transform.position);
    }

    private void Update()
    {
        if (disableTimer is { running: true })
            disableTimer.Update(Time.deltaTime);
    }

    private void DisableTriggerTimed()
    {
        entranceTrigger.enabled = false;
        disableTimer = new Timer(2, () => {entranceTrigger.enabled = true;});
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPosition + transform.position, 0.2f);
    }
}