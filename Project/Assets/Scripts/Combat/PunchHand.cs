using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchHand : MonoBehaviour
{
    [Header("Damage settings")]
    [SerializeField] private float _attackCooldown = 0.5f;
    [SerializeField] private float _damage = 1;
    [SerializeField] private float _knockBack = 1;

    [Header("Force settings")]
    [Tooltip("How hard the player will throw out their punch")]
    [SerializeField] private float _handThrowForce = 100.0f;
    [Tooltip("How heavy the arm will be when punching")]
    [SerializeField] private float _handMass = 10.0f;

    [Header("Other settings")]
    [Tooltip("How long it will take before certain effects like the armMass will return to normal")]
    [SerializeField] private float _resetTimer = 0.3f;
    [SerializeField] private float _rumbleStrength = 0.3f;

    // Player pawn
    public SmashHealth SmashHealth { private get; set; }
    public Rigidbody HipsRigidbody { private get; set; }

    private Rigidbody _leftArmRigidbody;
    public Rigidbody LeftArmRigidbody
    {
        set
        {
            // Arm
            _leftArmRigidbody = value;
            _leftArmMass = value.mass;

            // Hand
            _leftHand = value.gameObject.GetComponentInChildren<HandDamager>();
            Debug.Assert(_leftHand != null, "Error: no HandDamager found on leftArm");
            SetHandSettings(_leftHand);
        }
    }
    private Rigidbody _rightArmRigidbody;
    public Rigidbody RightArmRigidbody
    {
        set
        {
            // Arm
            _rightArmRigidbody = value;
            _rightArmMass = value.mass;

            // Hand
            _rightHand = value.gameObject.GetComponentInChildren<HandDamager>();
            Debug.Assert(_rightHand != null, "Error: no HandDamager found on rightArm");
            SetHandSettings(_rightHand);
        }
    }

    private float _leftArmMass;
    private float _rightArmMass;

    // Damage
    private bool _canAttack = true;

    // Hands
    private HandDamager _leftHand;
    private HandDamager _rightHand;

    // Public functions
    // ----------------
    public void LeftPunch()
    {
        if (_canAttack)
        {
            _canAttack = false;

            // Make arm heavier
            HeavierArm(_leftArmRigidbody, _handMass);

            // Throw punch
            _leftHand.CanAttack = true;
            ThrowPunch(_leftArmRigidbody);

            // Start coroutines
            StartCoroutine(EffectReset(_leftArmRigidbody, _leftHand, _leftArmMass, _resetTimer));
            StartCoroutine(AttackCooldown(_attackCooldown));
        }
    }
    public void RightPunch()
    {
        if (_canAttack)
        {
            _canAttack = false;

            // Make arm heavier
            HeavierArm(_rightArmRigidbody, _handMass);

            // Throw punch
            _rightHand.CanAttack = true;
            ThrowPunch(_rightArmRigidbody);

            // Start coroutines
            StartCoroutine(EffectReset(_rightArmRigidbody, _rightHand, _rightArmMass, _resetTimer));
            StartCoroutine(AttackCooldown(_attackCooldown));
        }
    }

    // Private functions
    // -----------------
    private void SetHandSettings(HandDamager hand)
    {
        hand.Smashealth = SmashHealth;

        hand.Damage= _damage;
        hand.Knockback = _knockBack;

        hand.CanAttack = false;

        hand.RumbleStrength = _rumbleStrength;
    }

    private void HeavierArm(Rigidbody arm, float mass)
    {
        arm.mass = mass;
    }
    private void ThrowPunch(Rigidbody arm)
    {
        Vector3 force = HipsRigidbody.transform.forward * _handThrowForce;
        arm.AddForce(force, ForceMode.Impulse);
    }

    private IEnumerator EffectReset(Rigidbody arm, HandDamager hand, float originalMass, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        hand.CanAttack = false;
        arm.mass = originalMass;
    }
    private IEnumerator AttackCooldown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _canAttack = true;
    }
}
