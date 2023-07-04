using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DragonSettings", menuName = "GameSettings/Dragon")]
public class DragonSettings : ScriptableObject
{
    [Header("Dragon spawn settings")]
    [Tooltip("What the dragon will be firing when flying in it's active state")]
    public GameObject FireBomb;
    [Tooltip("How far the dragon can spawn from the middle point")]
    public float DragonSpawnRadius = 1.0f;
    public float DragonScreenShakeDuration = 2.0f;
    public float DragonScreenShakeStrengthMultiplier = 2.0f;

    [Header("Dragon movement settings")]
    public float DragonWarningMovementSpeed = 100.0f;
    public float DragonHazardMovementSpeed = 75.0f;

    [Header("Dragon fire settings")]
    [Tooltip("Dragon will go from minFrequency to maxFrequenct in x amount of steps")]
    public int StepsToReachMax = 10;
    public float MinSpewingFrequency = 0.5f;
    public float MaxSpewingFrequency = 0.25f;
    [Tooltip("These spheres will get used behind the scenes eitherway, but disabling this will disable only the visual part of the bomb")]
    public bool ThrowFireBombs = true;
    [Tooltip("When enabled, every ball of fire and the follow-up fireHazard will get bigger with every step, clamping to twice the original size")]
    public bool ScaleSizeWithSteps = false;
    public bool ScaleFlightFrequencyWithSteps = false;

    [Header("Dragon timer settings")]
    [Tooltip("How long it takes before the dragon start spawning")]
    public float StartUpTimer = 60.0f;
    [Tooltip("This is the minimum amount it takes before the dragon can spawn")]
    public float MinEventTimer = 30.0f;
    [Tooltip("This is the maximum amount it takes before the dragon can spawn")]
    public float MaxEventTimer = 60.0f;
}
