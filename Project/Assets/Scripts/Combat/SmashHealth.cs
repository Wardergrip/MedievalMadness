using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SmashHealth : MonoBehaviour
{
    [Header("Player variables")]
    [Tooltip("This is the rigidbody the forces will be applied to")]
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField, Range(0,1)] private float _knockbackResistance = 0;
    [SerializeField] private float _spawnInvincibilityTime = 1.0f;

    public float KnockbackResistance 
    { 
        get { return _knockbackResistance;  } 
        set
        {
            _knockbackResistance = value;
            Mathf.Clamp01(_knockbackResistance);
        }
    }
    [SerializeField] private bool _isAttachedToPlayer = true;

    public event Action<PlayerController> DamageEvent;
    public event Action<PlayerController> PushEvent;

    public UnityEvent<PlayerController> DamageUnityEvent;
    public UnityEvent<PlayerController> PushUnityEvent;
    public UnityEvent<PlayerController> OnDeathEvent;

    // Player
    // ------
    public Rigidbody RigidBody
    {
        get { return _rigidBody; }
    }

    public PlayerPawn PlayerPawn { get; private set; }
    private RagdollHandler _ragdollHandler;
    public RagdollHandler Ragdollhandler
    {
        set { _ragdollHandler = value; }
    }

    private bool _isArmInput;
    public bool IsArmInput
    {
        set { _isArmInput = value; }
    }

    private bool _isDead;
    public bool IsDead
    {
        get { return _isDead; }
    }

    // Damage Over Time related
    // ------------------------
    private bool _isDOTActive = false;
    public bool IsDOTActive { get { return _isDOTActive; } }
    private float _DOTDamage;
    private float _DOTTotalTime;
    private float _DOTTickTime;

    // Health
    // ------
    [Header("Watch Values")]
    [SerializeField] private float _health = 0;
    public float Health { get { return _health; } set { _health = value; } }

    // Other
    // -----
    private short _playerIdLastDamagerDealer = short.MinValue;
    private bool _isInvincible = true;

    // Start
    // -----
    void Start()
    {
        // Get rigidbody
        if (_rigidBody == null)
        {
            _rigidBody = GetComponent<Rigidbody>();
            Debug.Assert(_rigidBody, $"No rigidbody assigned or found on {gameObject.name}");
        }

        // Get playerPawn
        if (_isAttachedToPlayer)
        {
            PlayerPawn = GetComponent<PlayerPawn>();
            Debug.Assert(PlayerPawn, $"No playerpawn found on {gameObject.name}");
        }

        // Invincibility
        StartCoroutine(RemoveInvincibility(_spawnInvincibilityTime));
    }
    private IEnumerator RemoveInvincibility(float seconds)
    {
        yield return new WaitForSeconds( seconds );
        _isInvincible = false;
    }

    // Damage
    // ------
    public void DamageAndPush(float damage, float knockBack, Vector3 source, bool enableRagdoll = true, short playerIdDamageDealer = short.MinValue)
    {
        // Return on invincibility
        if (_isInvincible) return;

        // Damage
        Damage(damage, source);

        // Pushing
        Push(knockBack, source);

        // Flicker
        _ragdollHandler?.OnHit(enableRagdoll);

        // Controller Rumble
        float rumbleStrength = Mathf.Clamp01(damage / SettingsManager.Instance.RumbleSettings.highDamageTreshold);
        Rumble(rumbleStrength);

        // Score
        if (playerIdDamageDealer == short.MinValue || playerIdDamageDealer == PlayerPawn.PlayerID)
        {
            // if the source is not a player, or themselves
            return;
        }
        GameSystem.Instance.ScoreManager.AddScoreHit(playerIdDamageDealer);
        _playerIdLastDamagerDealer = playerIdDamageDealer;
    }
    public void Damage(float damage, Vector3? source = null)
    {
        // Return on invincibility
        if (_isInvincible) return;

        // Health
        _health += (damage * SettingsManager.Instance.SmashHealthSettings.damageMultiplier);
        DamageEvent?.Invoke(PlayerPawn.PlayerController);
        DamageUnityEvent?.Invoke(PlayerPawn.PlayerController);
        if (_isAttachedToPlayer == false) Debug.Log("Damage registered on shield");
        
        // Pop up
        if (SettingsManager.Instance.SmashHealthSettings.enableDamagePopUps && _isAttachedToPlayer && damage > float.Epsilon)
        {
            PopUp.Create(PlayerPawn.GetPlayerPos(), ((int)damage * SettingsManager.Instance.SmashHealthSettings.damageMultiplier).ToString());
        }
    }
    public void Push(float knockBack, Vector3 forceSource)
    {
        // Return on invincibility
        if (_isInvincible) return;

        // Calculate direction
        Vector3 sourceToPlayer = Vector3.ProjectOnPlane((_isAttachedToPlayer ? PlayerPawn.GetPlayerPos() - forceSource : transform.position - forceSource).normalized, Vector3.up);
        Vector3 dir = sourceToPlayer * (1 + _health) * knockBack * SettingsManager.Instance.SmashHealthSettings.forceMultiplier;
        dir *= (1 - Mathf.Clamp01(_knockbackResistance));
        
        // Add force
        _rigidBody.AddForce(dir);
        
        // Event
        PushEvent?.Invoke(PlayerPawn.PlayerController);
        PushUnityEvent?.Invoke(PlayerPawn.PlayerController);
    }
    public void Rumble(float strength)
    {
        if ((SettingsManager.Instance.RumbleSettings.rumbleOnDamage && _isAttachedToPlayer))
        {
            if (0 <= PlayerPawn.GamepadID)
            {
                Debug.Log(strength);
                PlayerPawn.SetControllerRumble(strength, SettingsManager.Instance.RumbleSettings.rumbleDuration);
            }
        }
    }

    /// <summary>
    /// Increases health over time. Only one can be active
    /// </summary>
    /// <param name="damage">Amount to add each damage tick</param>
    /// <param name="totalTime">Amount of time DOT will take</param>
    /// <param name="tickTime">Amount of time between each time damage is done</param>
    public void DamageOverTime(float damage, float totalTime,float tickTime)
    {
        // Return on invincibility
        if (_isInvincible) return;

        if (_isDOTActive) return;

        _isDOTActive = true;
        _DOTDamage = damage;
        _DOTTotalTime = totalTime;
        _DOTTickTime = tickTime;
        StartCoroutine(DOT_Coroutine());
    }

    private IEnumerator DOT_Coroutine()
    {
        if (_isDOTActive == false) yield return null;

        while (_DOTTotalTime > 0 && _DOTTotalTime < _DOTTickTime)
        {
            Damage(_DOTDamage);
            _DOTTotalTime -= _DOTTickTime;
            yield return new WaitForSeconds(_DOTTickTime);
        }

        _isDOTActive = false;
        yield return null;
    }

    public void Kill(bool wasSpike = true)
    {
        // If already dead, return
        if (_isDead) return;

        // If last player with health, return
        bool isLast = true;
        foreach (var player in GameSystem.Instance.PlayerManager.GetPlayers())
        {
            if (player.PlayerID != PlayerPawn.PlayerID)
            {
                if (0 < player.Lives.Amount)
                {
                    isLast = false;
                    break;
                }
            }
        }
        if (isLast) return;

        _isDead = true;
        OnDeathEvent?.Invoke(PlayerPawn.PlayerController);
        PlayerPawn?.OnDeath(wasSpike);

        // Drop weapon
        PlayerPawn?.DropWeapon();

        // Score
        if (_playerIdLastDamagerDealer == short.MinValue) 
        {
            return;
        }
        GameSystem.Instance.ScoreManager.AddScoreElimination(_playerIdLastDamagerDealer);
        if (_isAttachedToPlayer)
        {
            PopUp.Create(PlayerPawn.GetPlayerPos(), GameSystem.Instance.ScoreManager.ScoreForElimination.ToString());
        }
    }
    public void ResetHealth()
    {
        _health = 0;
        DamageEvent?.Invoke(PlayerPawn.PlayerController);
        DamageUnityEvent?.Invoke(PlayerPawn.PlayerController);
    }
}
