using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnAreaSettings", menuName = "GameSettings/SpawnArea")]
public class SpawnAreaSettings : ScriptableObject
{
    public uint totalMaxWeaponsOnGround = 25;
}
