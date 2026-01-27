using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DirectionBasedEntranceList : IEnumerable<RoomEntrance>
{
    [Serializable]
    public class ListInstance
    {
        public Vector3 key;
        public List<RoomEntrance> entrances;

        public ListInstance(Vector3 key, List<RoomEntrance> entrances)
        {
            this.key = key;
            this.entrances = entrances;
        }
    }
    [SerializeField]
    private List<ListInstance> entrances = new();

    public void Remove(RoomEntrance roomEntrance)
    {
        for (var i = 0; i < entrances.Count; i++)
        {
            var entranceList = entrances[i];
            if (!entranceList.entrances.Contains(roomEntrance)) continue;
            
            entranceList.entrances.Remove(roomEntrance);
            if (entranceList.entrances.Count == 0)
                entrances.Remove(entranceList);
            break;
        }
    }

    public void AddRange(IEnumerable<RoomEntrance> range)
    {
        foreach (var entrance in range) Add(entrance);
    }
    
    public void Add(RoomEntrance entrance)
    {
        var dir = entrance.transform.forward;
        if (!ContainsKey(dir))
            entrances.Add(new ListInstance(dir, new List<RoomEntrance>()));
        GetFromDir(dir).Add(entrance);
    }

    public List<RoomEntrance> GetFromDir(Vector3 dir)
    {
        var found = entrances.FirstOrDefault(x => x.key == dir);
        return found?.entrances;
    }

    public bool ContainsKey(Vector3 dir) => entrances.Any(entrance => entrance.key == dir);
    
    public IEnumerator<RoomEntrance> GetEnumerator()
    {
        foreach (var listInstance in entrances)
        {
            foreach (var entrance in listInstance.entrances)
            {
                yield return entrance;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public RoomEntrance Find(Func<object, bool> func)
    {
        return entrances.SelectMany(listInstance => listInstance.entrances.Where(entrance => func(entrance))).FirstOrDefault();
    }
}