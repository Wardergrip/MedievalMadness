using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameRound", menuName = "GameRound/Normal")]
public class GameRound : ScriptableObject
{
    [Header("General")]
    public string RoundName;
    public string Description;

    [Header("Weapons to spawn")]
    public SpawnList PlatformSpawnList;
    public SpawnList StairsSpawnList;
    public SpawnList FieldSpawnList;

    [Header("Settings")]
    public HazardSettings HazardSettings;
    public PlayerSettings PlayerSettings;
    public SmashHealthSettings SmashHealthSettings;
    public SpawnAreaSettings SpawnAreaSettings;
    public DragonSettings DragonSettings;

    public virtual void Init()
    {
    }
}
