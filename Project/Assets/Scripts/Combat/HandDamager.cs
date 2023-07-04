using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class HandDamager : MonoBehaviour
{
    [SerializeField] private GameObject _trails;

    // PlayerPawn
    public SmashHealth Smashealth { get; set; }

    // Damage
    public float Damage { get; set; }
    public float Knockback { private get; set; }

    private bool _canAttack;
    public bool CanAttack
    {
        get { return _canAttack; }
        set 
        { 
            _canAttack = value;
            if (_canAttack) _trails.SetActive(true);
            else            _trails.SetActive(false);
        }
    }

    // Other
    public float RumbleStrength { private get; set; }
    private string _MainMenuName = "MainMenuScene";

    // Start
    // -----
    private void Start()
    {
        Debug.Assert(_trails != null, "Warning: no trails found hands");
        _trails.SetActive(false);
    }

    // On collision
    // ------------
    private void OnTriggerEnter(Collider other)
    {
        // Check if conditions met
        // -----------------------

        // If cannot attack, return
        if (CanAttack == false) return;


        // Check other collision
        // ---------------------

        // Get the smashHealth component
        SmashHealth otherHealth = other.gameObject.GetComponentInParent<SmashHealth>();
        if (otherHealth == null)
        {
            otherHealth = other.gameObject.GetComponent<SmashHealth>();
            if (otherHealth == null) return;
        }

        // Check if not hitting yourself
        if (otherHealth == Smashealth) return;


        // Calculate damage
        // ----------------

        // Use playerPawn as source when having parent, else use itself
        Vector3 sourcePos = transform.position;
        if (Smashealth.PlayerPawn != null) sourcePos = Smashealth.PlayerPawn.GetPlayerPos();
        sourcePos.y = otherHealth.gameObject.transform.position.y;

        // Apply damage
        // Don't give ID when in MainMenu, to prevent from having additional score there
        if(SceneManager.GetActiveScene().name == _MainMenuName)
        {
            otherHealth.DamageAndPush(Damage, Knockback, sourcePos, true);
        }
        else
        {
            otherHealth.DamageAndPush(Damage, Knockback, sourcePos, true, Smashealth.PlayerPawn.PlayerID);
        }


        // Send events
        HitEffect.Spawn(otherHealth.PlayerPawn, false);

        // Set cooldown
        // ------------
        CanAttack = false;

        // Controller Rumble
        // -----------------
        if (SettingsManager.Instance.RumbleSettings.rumbleOnHit)
        {
            if (0 <= Smashealth.PlayerPawn.GamepadID)
            {
                Smashealth.PlayerPawn.SetControllerRumble(RumbleStrength, SettingsManager.Instance.RumbleSettings.rumbleDuration);
            }
        }
    }
}
