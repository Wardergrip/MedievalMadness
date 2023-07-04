using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Breakable : MonoBehaviour
{
    [Header("Object Settings")]
    [SerializeField] private float _objectHealth = 50.0f;

    [Header("Situational Settings")]
    [Tooltip("When enabled the player can deal damage to the object on collision, when the object then gets destroyed the player gains some damage")]
    [SerializeField] private bool _canInteractWithPlayer = false;
    [SerializeField] private float _damage = 5.0f;
    [SerializeField] private float _knockback = 2.5f;

    // On collision
    private void OnTriggerEnter(Collider collision)
    {
        // Check type
        // ----------

        // Hand
        HandDamager handScript = collision.gameObject.GetComponent<HandDamager>();
        if (handScript)
        {
            // If can deal damage
            if (handScript.CanAttack)
            {
                // Lose health
                LoseHealth(handScript.Damage);
            }
            // Else
            else
            {
                // Check if has weapon
                Weapon weaponScript = handScript.Smashealth.PlayerPawn.EquippedWeaponScript;
                if (weaponScript)
                {
                    // If is active
                    if(weaponScript.CanDealDamage())
                    {
                        // Lose health
                        LoseHealth(weaponScript.Damage);
                    }
                }
            }
        }

        if (_canInteractWithPlayer)
        {
            // Player
            SmashHealth healthScript = collision.gameObject.GetComponentInParent<SmashHealth>();
            if (healthScript)
            {
                // Lose health
                float playerDamage = 5.0f;
                LoseHealth(playerDamage, healthScript);
            }
        }
    }

    private void LoseHealth(float amount, SmashHealth health = null)
    {
        // Lower health
        _objectHealth -= amount;

        // If destroyed
        if (_objectHealth < 0)
        {
            // Check if was player
            if (health != null)
            {
                // Deal damage to player (in assumption that player has been thrown in breakable object)
                health.DamageAndPush(_damage, _knockback, transform.position, false);
            }

            // Spawn break effect
            BreakEffect.Spawn(transform);

            // Destroy self
            Destroy(gameObject);
        }
    }
}
