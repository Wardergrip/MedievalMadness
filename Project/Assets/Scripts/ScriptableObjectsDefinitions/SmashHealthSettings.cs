using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SmashHealthSettings",menuName ="GameSettings/SmashHealth")]
public class SmashHealthSettings : ScriptableObject
{
    public float damageMultiplier = 1;
    public float forceMultiplier = 1;
    public bool enableDamagePopUps = true;
}
