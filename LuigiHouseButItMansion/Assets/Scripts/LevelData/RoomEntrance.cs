
using System;
using System.Collections.Generic;
using LucasCustomClasses;
using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    public RoomEntrance otherRoomEntrance;
    [SerializeField]
    private RoomObjectData parentRoom;
    public Collider entranceTrigger;
    private Timer disableTimer;
    [SerializeField]
    private Vector3 spawnPosition;
    [Header("Visuals")]
    [SerializeField]
    private List<GameObject> tempLockObjects = new();
    private bool locked;
#if UNITY_EDITOR
    public MeshFilter doorRenderObject;
#endif
    public LevelGenerator levelGeneratorRef;
    
    public void Init(RoomObjectData roomObjectData)
    {
        parentRoom = roomObjectData;
        parentRoom.AddEntrance(this);
    }

    private void OnDestroy() => parentRoom.RemoveEntrance(this);

    private void Awake()
    {
        if (otherRoomEntrance == null)
            enabled = false;

        if (entranceTrigger != null) return;
        entranceTrigger = GetComponent<Collider>();
        entranceTrigger.isTrigger = true;
    }

    private void Start() => UnLock();

    private void OnTriggerEnter(Collider other)
    {
        if (locked)
            return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var dataRef = other.GetComponent<ComponentReference>();
            if (dataRef == null) return;

            if (otherRoomEntrance == null)
                otherRoomEntrance = levelGeneratorRef.GetConnectedRoom(this);
            
            if (otherRoomEntrance == null)
                return;
            otherRoomEntrance.DisableTriggerTimed();
            otherRoomEntrance.SpawnPlayer(dataRef.GetReference<PlayerData>());
        }
    }

    private void SpawnPlayer(PlayerData playerData)
    {
        RoomObjectData.SetCurrentRoom(parentRoom);
        playerData.SetPlayerPosition(GetSpawnPosition());
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

    public Vector3 GetSpawnPosition()
    {
        return transform.rotation * spawnPosition + transform.position;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GetSpawnPosition(), 0.2f);
    }

    private void SetTempLockObjects(bool state)
    {
        foreach (var tempLockObject in tempLockObjects)
        {
            tempLockObject.SetActive(state);
        }
    }

    public void Lock()
    {
        SetTempLockObjects(true);
        locked = true;
    }

    public void UnLock()
    {
        SetTempLockObjects(false);
        locked = false;
    }
}