using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RumbleSettings", menuName = "GameSettings/ControllerRumble")]
public class RumbleSettings : ScriptableObject
{
    public bool rumbleOnDamage = false;
    public bool rumbleOnHit = true;
    public float rumbleDuration = 0.2f;
    [Tooltip("This will be considered as getting \"High\" damage, changing variables such as Rumble accordingly")]
    public float highDamageTreshold = 35.0f;
}
