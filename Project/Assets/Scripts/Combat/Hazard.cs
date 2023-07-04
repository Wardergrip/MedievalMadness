using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Hazard : MonoBehaviour
{
    [Header("Individual variables")]
    [SerializeField] private bool _useIndividualVelocityTreshold = false;
    [SerializeField] private float _individualSqrdVelocityTreshold = 1.0f;

    [SerializeField] private bool _useIndividualDamager = false;
    [SerializeField] private float _individualDamageAmount = 1.0f;
    [SerializeField] private float _individualKnockBackAmount = 1.0f;
    [SerializeField] private float _individualDamageFrequency = 0.5f;
    [SerializeField] private bool _makeRagdollPassive;

    [Header("Other")]
    [SerializeField] private bool _isLevelBoundary = false;

    [Header("View variables")]
    [SerializeField] private float _thisDamageAmount;         // Damage for this object
    [SerializeField] private float _thisKnockBackAmount;      // KnockBack for this object
    [SerializeField] private float _thisDamageFrequency;      // DamageFrequency for this object
    [SerializeField] private bool _thisPassiveRagdoll;        // Passive ragdoll for this object


    // Damage and knockBack
    private float _currentDamageTimer;

    private float _thisSqrdVelocityTreshold;    // Treshold for this object

    // Collider
    private BoxCollider _boxCollider;

    // Other
    private bool _canDealDamage = false;
    public bool SpawnHitEffect { get; set; } = true;
    public bool IsFire { get; set; } = false;

    // Start
    // -----
    void Start()
    {
        // Get collider
        _boxCollider = GetComponent<BoxCollider>();
        Debug.Assert(_boxCollider.isTrigger, "Box collider on " + gameObject + " must be trigger");

        // Call late start
        float lateStartWaitTime = 0.1f;
        StartCoroutine(LateStart(lateStartWaitTime));
    }
    private IEnumerator LateStart(float waitTime)
    {
        // After wait time, start function
        yield return new WaitForSeconds(waitTime);

        // Set this velocityTreshold
        if (_useIndividualVelocityTreshold) _thisSqrdVelocityTreshold = _individualSqrdVelocityTreshold;
        else                                _thisSqrdVelocityTreshold = SettingsManager.Instance.HazardSettings.sqrdVelocityTreshold;

        // Set this damageAmount
        if (_useIndividualDamager)
        {
            _thisDamageAmount = _individualDamageAmount;
            _thisKnockBackAmount = _individualKnockBackAmount;
            _thisDamageFrequency = _individualDamageFrequency;
            _thisPassiveRagdoll = _makeRagdollPassive;
        }
        else
        {
            _thisDamageAmount = SettingsManager.Instance.HazardSettings.damageAmount;
            _thisKnockBackAmount = SettingsManager.Instance.HazardSettings.knockBackAmount;
            _thisDamageFrequency = SettingsManager.Instance.HazardSettings.damageFrequency;
            _thisPassiveRagdoll = true;
        }

        // Set can deal damage
        _canDealDamage = true;
    }


    // Update
    // ------
    private void Update()
    {
        // Countdown
        if (0 < _currentDamageTimer) _currentDamageTimer -= Time.deltaTime;
    }

    // On player collision
    // -------------------
    private void OnTriggerStay(Collider other)
    {
        // When cannot deal damage yet, return
        if (_canDealDamage == false) return;

        // When still on cooldown, return
        if (0 < _currentDamageTimer) return;
        _currentDamageTimer = _thisDamageFrequency;

        // When is weapon, return
        Weapon weaponScript = other.GetComponent<Weapon>();
        if (weaponScript != null) return;

        // When not smashHealth, return
        SmashHealth otherHealth = other.GetComponentInParent<SmashHealth>();
        if (otherHealth == null) return;

        // If is dead, return
        if (otherHealth.IsDead) return;

        // Send event
        PlayerPawn playerPawn = otherHealth.PlayerPawn;
        if (IsFire) playerPawn.InFire();

        // When reached velocityTresholds
        if (otherHealth.RigidBody.velocity.sqrMagnitude > _thisSqrdVelocityTreshold)
        {
            // Kill pawn
            otherHealth.Kill(!_isLevelBoundary);

            // Camera shake
            GameSystem.Instance.ScreenShake.StartShake(SettingsManager.Instance.HazardSettings.cameraShakeTime);
        }
        // Else, bounce back
        else
        {
            // Get sourcePos
            Vector3 sourcePos = transform.position;
            sourcePos.y = otherHealth.gameObject.transform.position.y;

            // Calculate knockback amount
            float velocityPercentage = otherHealth.RigidBody.velocity.sqrMagnitude / SettingsManager.Instance.HazardSettings.sqrdBumpVelocityTreshold;
            if (1 < velocityPercentage) velocityPercentage = 1;

            // Deal damage
            otherHealth.DamageAndPush(_thisDamageAmount, _thisKnockBackAmount * velocityPercentage, sourcePos, _thisPassiveRagdoll);

            // Send event
            if (SpawnHitEffect) HitEffect.Spawn(playerPawn, true);
        }
    }
}
