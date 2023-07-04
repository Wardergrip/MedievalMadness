using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _logMessages = false;

    [Header("Weapon objects")]
    [SerializeField] private GameObject _mesh;
    [SerializeField] private Material _shinyOutlineMaterial;
    [SerializeField] private GameObject _trails;

    [Header("Weapon values")]
    [SerializeField] private bool _isShield = false;
    [SerializeField] private float _damage = 1.0f;
    [SerializeField] private float _knockBack = 1.0f;
    [SerializeField] private int _durability = 10;
    [SerializeField] private float _weight = 1.0f;
    [Tooltip("How hard the controller will rumble when hitting someone")]
    [SerializeField] private float _weaponRumbleStrength = 0.2f;

    [Header("Weapon powerup")]
    [SerializeField] private bool _enablePowerUp = false;
    [SerializeField] private float _powerUpSpeed = 1.0f;

    [Header("Weapon ability")]
    [Tooltip("Currently this changes the force off the shield bash, put it can be a more general variable later if wanted")]
    [SerializeField] private float _abilityPower = 500.0f;
    [SerializeField] private float _abilityCooldown = 1.0f;
    [Tooltip("How long the effects of the ability stay after the first use")]
    [SerializeField] private float _abilityEffectTimer = 0.5f;

    [Header("Weapon constraints")]
    [SerializeField] private float _velocityTreshold = 5.0f;
    [SerializeField] private float _damageCooldown = 0.0f;

    [Header("Weapon rotations")]
    [SerializeField] private bool _rotateWithSwing = true;
    [SerializeField] private bool _rotateWithSpin = true;

    [Header("Events")]
    public UnityEvent OnBreakEvent;

    // Weapon variables
    // ----------------
    public float Damage
    {
        get { return _damage; }
    }

    private BoxCollider _collider;
    private Rigidbody _rigidbody;

    public bool IsShield
    {
        get { return _isShield; }
    }

    public int Durability
    {
        get { return _durability; }
        set
        {
            _durability = value;
            if (_durability <= 0) DestroyWeapon();
        }
    }

    public float Weight
    {
        get { return _weight; }
    }

    private float _attackCooldown = 0.0f;

    private bool _isOnAbilityCooldown { get; set; }
    private bool _isOnEffectCooldown { get; set; }

    public void WeaponActivate(bool activateWeapon)
    {
        _weaponActivated = activateWeapon;
        _activateHasBeenCalled = true;
    }
    private bool _weaponActivated = true;

    private string everythingLayerMask = "Everything";

    // Player variables
    // ----------------
    private PlayerController _playerController;
    public PlayerController PlayerController
    {
        get { return _playerController; }
        set { _playerController = value; }
    }

    private PlayerPawn _playerParent;
    public PlayerPawn PlayerParent
    {
        set { _playerParent = value; }
    }

    private bool _isArmInput;
    public bool IsArmInput
    {
        set 
        { 
            _isArmInput = value; 
            if (_isArmInput ) _trails.SetActive( true );
            else              _trails.SetActive( false );
        }
    }

   public Vector2 ArmInput { private get; set; }

    // PowerUp
    // -------
    private bool _isSpinning = false;
    public bool IsSpinning
    {
        set { _isSpinning = value; }
    }

    private float _currentPower;

    // Other
    // -----
    private float _lastDamageValue;
    public float LastDamageValue
    {
        get { return _lastDamageValue; }
    }

    public bool RotateWithSwing
    {
        get { return _rotateWithSwing; }
    }
    public bool RotateWithSpin
    {
        get { return _rotateWithSpin; }
    }

    public Action<Weapon, PlayerPawn> OnWeaponHit;

    private bool _activateHasBeenCalled = false;

    // Materials
    private MeshRenderer _meshRenderer;
    private Material _meshMaterial;
    private Color _originalOutlineColor;
    private Color _originalColor;
    private string _colorPropertyName = "_base_color";
    private string _outlineColorPropertyName = "_Color";

    // Start
    // -----
    void Start()
    {
        // Get components
        Debug.Assert(_mesh != null, "Weapon needs a weaponMesh to work");
        Debug.Assert(_shinyOutlineMaterial != null, "No shinyOutlineMaterial inserted");

        _collider = gameObject.GetComponent<BoxCollider>();
        Debug.Assert(_collider != null, "Weapon needs a collider to work");

        _rigidbody = gameObject.GetComponent<Rigidbody>();
        Debug.Assert(_rigidbody != null, "Weapon needs a rigidbody to work");

        // Set rigidbody weight
        _rigidbody.mass = _weight;

        // Get/Set material
        _meshRenderer = _mesh.GetComponent<MeshRenderer>();
        if (_meshRenderer != null)
        {
            _meshMaterial = _meshRenderer.materials[0];
            _originalOutlineColor = _meshRenderer.materials[1].GetColor(_outlineColorPropertyName);
        }
        else Debug.Log("No meshRenderer found");
        
        if (_meshMaterial != null) _originalColor = _meshMaterial.GetColor(_colorPropertyName);
        else Debug.Log("No meshMaterial found");

        // Trails
        Debug.Assert(_trails != null, "No trails given");
        _trails.SetActive(false);

        // Late activate call
        if(_activateHasBeenCalled) LateActivateCall();
    }
    private void LateActivateCall()
    {
        _activateHasBeenCalled = false;

        if (_weaponActivated)
        {
            // Change outlineMaterials to normal
            _meshRenderer.materials[1].SetColor(_outlineColorPropertyName, _originalOutlineColor);
        }
        else
        {
            // Change outlineMaterials to shiny
            _meshRenderer.materials[1].SetColor(_outlineColorPropertyName, _shinyOutlineMaterial.GetColor(_outlineColorPropertyName));
            
            // Change Rigidbody Layermask
            _rigidbody.excludeLayers = LayerMask.NameToLayer(everythingLayerMask);
        }
    }

    // Update
    // ------
    void Update()
    {
        // Deplete cooldown
        if (_attackCooldown > 0.0f) _attackCooldown -= Time.deltaTime;

        // PowerUp
        if (_enablePowerUp) PowerUp();
    }

    private void PowerUp()
    {
        float newStep = _powerUpSpeed * Time.deltaTime;

        // If is spinning and weaponActivated
        if (_isSpinning && _weaponActivated)
        {
            // Power up
            _currentPower += newStep;
            _currentPower = Mathf.Clamp(_currentPower, 0, 100.0f);
        }
        // Else
        else
        {
            // Lose power
            _currentPower -= newStep * 2.0f;
            _currentPower = Mathf.Clamp(_currentPower, 0, 100.0f);
        }

        // Change weapon color according to power, change from originalColor to white
        Color white = Color.white;
        Color newColor = _meshMaterial.GetColor(_colorPropertyName);

        float powerPercentage = _currentPower / 100.0f;

        newColor.r = Mathf.Lerp(_originalColor.r, white.r, powerPercentage);
        newColor.g = Mathf.Lerp(_originalColor.g, white.g, powerPercentage);
        newColor.b = Mathf.Lerp(_originalColor.b, white.b, powerPercentage);

        // Set new material
        _meshMaterial.SetColor(_colorPropertyName, newColor);
    }

    // Weapon destroy
    // --------------
    private void DestroyWeapon()
    {
        OnBreakEvent?.Invoke();
        // Spawn particles
        BreakEffect.Spawn(transform);

        // Remove and destroy weapon
        _playerParent.RemoveWeapon();
        Destroy(gameObject);
    }

    // On player collision
    // -------------------
    public bool CanDealDamage()
    {
        // Check if conditions met
        // -----------------------

        // If weapon is not activated, return
        if (_weaponActivated == false)
        {
            if (_logMessages)
                Debug.Log("Weapon not activated");
            return false;
        }

        // If is not actively being swung, return
        if (_isArmInput == false)
        {

            if (_logMessages)
                Debug.Log("No active swinging");
            return false;
        }

        // If weapon is still on cooldown, return
        if (_attackCooldown > 0.0f)
        {
            if (_logMessages)
                Debug.Log("Weapon still on cooldown");
            return false;
        }

        // If didn't velocity treshold, return
        if (Mathf.Abs(_rigidbody.velocity.sqrMagnitude) < _velocityTreshold)
        {
            if (_logMessages)
                Debug.Log("Velocity treshold not reached");
            return false;
        }

        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CanDealDamage() == false) return;

        // Check other collision
        // ---------------------

        // Get the smashHealth component
        SmashHealth otherHealth = collision.gameObject.GetComponentInParent<SmashHealth>();
        if (otherHealth == null)
        {
            otherHealth = collision.gameObject.GetComponent<SmashHealth>();
            if (otherHealth == null)
            {
                if (_logMessages)
                    Debug.Log("Other did not have smashHealth");
                return;
            }
        }

        // Check if not hitting yourself
        PlayerPawn otherPawn = otherHealth.gameObject.GetComponent<PlayerPawn>();
        if (otherPawn == _playerParent)
        {
            if (_logMessages)
                Debug.Log("Weapon was hitting parent");
            return;
        }


        // Calculate damage
        // ----------------

        // Calculate damageValue, damage buffs depend on velocity and currentPower
        _lastDamageValue = _damage * (1 + _rigidbody.velocity.sqrMagnitude + (_currentPower / 100.0f));

        // Use playerPawn as source when having parent, else use itself
        Vector3 sourcePos = transform.position;
        if (_playerParent != null) sourcePos = _playerParent.GetPlayerPos();
        sourcePos.y = otherHealth.gameObject.transform.position.y;

        // Apply damage
        otherHealth.DamageAndPush(_lastDamageValue, _knockBack, sourcePos, true, _playerParent.PlayerID);

        // Push extra far when using shield ability
        if (_isShield && _isOnEffectCooldown)
        {
            otherHealth.Push(_abilityPower, sourcePos);
        }

        // Log damage
        if (_logMessages)
            Debug.Log(gameObject.name + " will deal damage to " + otherHealth.gameObject.name);

        // Send events
        HitEffect.Spawn(otherPawn, false);
        OnWeaponHit?.Invoke(this, otherPawn);

        // Power Up
        // --------

        // Reset powerUp
        _currentPower = 0;

        // Controller Rumble
        // -----------------
        float rumbleStrength = _weaponRumbleStrength + Mathf.Clamp01(_lastDamageValue / SettingsManager.Instance.RumbleSettings.highDamageTreshold) * _weaponRumbleStrength;
        if (SettingsManager.Instance.RumbleSettings.rumbleOnHit)
        {
            if (0 <= _playerParent.GamepadID)
            {
                _playerParent.SetControllerRumble(rumbleStrength, SettingsManager.Instance.RumbleSettings.rumbleDuration);
            }
        }

        // Durability
        // ----------

        // Reset cooldown
        _attackCooldown = _damageCooldown;

        // Lower durability
        --_durability;
        if (_durability == 0)
        {
            DestroyWeapon();
        }
    }

    // Weapon Ability
    // --------------
    public void WeaponAbility()
    {
        // Check conditions
        if (_isArmInput == false) return;
        if (_isOnAbilityCooldown) return;

        // This could be a switch for each weapon
        if (_isShield)
        {
            // Calculate force
            Vector3 forceDirection = new Vector3(ArmInput.x, 0, ArmInput.y);
            forceDirection *= _abilityPower;

            _playerParent.HipsRigidbody.AddForce(forceDirection, ForceMode.Impulse);

            // Empower shield, currently hard-coded
            _damage = 10.0f;
            _collider.size *= 2.0f;
        }

        // Cooldown
        _isOnEffectCooldown = true;
        StartCoroutine(ActivateEffectCooldown());

        _isOnAbilityCooldown = true;
        StartCoroutine(ActivateWeaponCooldown());
    }
    private IEnumerator ActivateEffectCooldown()
    {
        yield return new WaitForSeconds(_abilityEffectTimer);
        _isOnEffectCooldown = false;

        // Currently hard-coded variable resets
        if (_isShield)
        {
            _damage = 0.0f;
            _collider.size /= 2.0f;
        }
    }
    private IEnumerator ActivateWeaponCooldown()
    {
        yield return new WaitForSeconds(_abilityCooldown);
        _isOnAbilityCooldown = false;
    }
}
