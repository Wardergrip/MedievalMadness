using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HazardSettings", menuName = "GameSettings/Hazard")]
public class HazardSettings : ScriptableObject
{
    public float sqrdVelocityTreshold = 0.0f;
    public float sqrdBumpVelocityTreshold = 50.0f;
    public float damageAmount = 1.0f;
    public float knockBackAmount = 100.0f;
    public float damageFrequency = 0.5f;
    public float cameraShakeTime = 1.0f;
}
