using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static SpawnList;

[CreateAssetMenu(fileName = "SpawnList",menuName = "SpawnList")]
public class SpawnList : ScriptableObject
{
    [System.Serializable]
    public class SpawnableObject
    {
        public GameObject Prefab;
        public float Weight;
    }
    public SpawnableObject[] spawnableObjects;

    public GameObject NextSpawnableObject()
    {
        float totalWeight = spawnableObjects.Sum(obj => obj.Weight);

        float randomWeight = UnityEngine.Random.Range(0f, totalWeight);

        foreach (SpawnableObject obj in spawnableObjects)
        {
            randomWeight -= obj.Weight;
            if (randomWeight <= 0)
            {
                return obj.Prefab;
            }
        }

        throw new UnityException("Something went terribly wrong. TotalWeight might've been 0");
    }
}
