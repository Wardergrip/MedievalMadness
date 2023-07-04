using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static SpawnList;

[CreateAssetMenu(fileName = "WeaponExclusiveGameRound",menuName = "GameRound/WeaponExclusive")]
public class WeaponExclusiveGameRound : GameRound
{
    [Header("Weapons")]
    [Tooltip("A random weapon will be selected from this list, according to the weights. That weapon will spawn everywhere.")]
    [SerializeField] private SpawnList _weapons;

    public override void Init()
    {
        GameObject weapon = _weapons.NextSpawnableObject();
        SpawnList newSpawnList = CreateInstance("SpawnList") as SpawnList;
        newSpawnList.spawnableObjects = new SpawnableObject[1];
        newSpawnList.spawnableObjects[0] = new SpawnableObject()
        {
            Prefab = weapon,
            Weight = 1
        };
        PlatformSpawnList = newSpawnList;
        StairsSpawnList = newSpawnList;
        FieldSpawnList = newSpawnList;

        RoundName = $"{weapon.name} only!";
    }
}
