using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "GameSettings/Player")]
public class PlayerSettings : ScriptableObject
{
    public int startLives = 3;
    public float spawnTimer = 1.0f;
    public float baseMovementSpeed = 1.0f;
    public float waterSlowMultiplier = 0.6f;
    public float rotationSpeed = 1.0f;
    public float weaponObjectRotationSpeed = 1.0f;
}
