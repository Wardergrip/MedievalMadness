using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;

public class PlayerPawn : MonoBehaviour
{
    [Header("Player limbs")]
    [SerializeField] private GameObject _ragdollHips;
    [SerializeField] private GameObject _ragdollLeftArm;
    [SerializeField] private GameObject _ragdollRightArm;

    [Header("Objects")]
    [SerializeField] private GameObject _weaponHoldingObject;

    public event Action<PlayerController> AddedWaterSlowEvent;
    public event Action<PlayerController> RemoveWaterSlowEvent;

    public event Action<PlayerController> InFireEvent;
    public void InFire()
    { 
        InFireEvent?.Invoke(PlayerController);
        _ragdollParticles.OnFire();
    }

    // Controller
    // ----------
    public PlayerController PlayerController { get; set; }

    private short _playerID;
    public short PlayerID
    {
        set { _playerID = value; }
        get { return _playerID; }
    }

    private int _gamepadID;
    public int GamepadID
    {
        get { return _gamepadID; }
        set { _gamepadID = value;}
    }

    private bool IsDead { get; set; }

    // Input
    // --------
    private Vector2 _charachterMovementInput;
    private Vector2 _armMovementInput;
    //private bool _isGrabbing = false;
    private bool _canMove = true;

    // Input events
    // -------------
    public void OnCharachterMove(InputAction.CallbackContext ctx) => _charachterMovementInput = ctx.ReadValue<Vector2>();
    public void OnArmMove(InputAction.CallbackContext ctx) => _armMovementInput = ctx.ReadValue<Vector2>();
    public void OnPickUp(InputAction.CallbackContext ctx)
    {
        if (_equippedWeapon == null && LastPickUp)
        {
            // Spawn equipped weapon
            _equippedWeaponPrefab = LastPickUp.PickUpWeaponPrefab();
            _equippedWeapon = Instantiate(_equippedWeaponPrefab, _weaponHoldingObject.transform);

            // Get weapon script
            _equippedWeaponScript = _equippedWeapon.GetComponent<Weapon>();
            _equippedWeaponScript.PlayerParent = this;
            
            // Set swinging variables
            _ragdollSwinging.SetSwingArmWeight(_equippedWeaponScript.Weight);
            _ragdollSwinging.RotateWithSpin = _equippedWeaponScript.RotateWithSpin;
            _ragdollSwinging.RotateWithSwing = _equippedWeaponScript.RotateWithSwing;

            // Set knockback resistance
            if (_equippedWeaponScript.IsShield)
            {
                _smashHealth.KnockbackResistance = 0.5f;
            }

            // Ignore weaponCollider
            _ragdollHandler.IgnoreRagdollColliders(_equippedWeaponScript.GetComponent<BoxCollider>());

            // Reach for object
            _ragdollIKControl.ShowcaseWeapon(1.0f);
        }
    }
    public void OnDrop(InputAction.CallbackContext ctx)
    {
        DropWeapon();  
    }

    public void DropWeapon()
    {
        if (_equippedWeapon)
        {
            Debug.Assert(_equippedWeaponPrefab, $"Tried to drop a weapon without having a weapon prefab!");
            PickUp.SpawnPickUp(GetPlayerPos() - new Vector3(0,0.3f,0), HelperFuncts.GetRandomYOrientation(), _equippedWeaponPrefab);

            Destroy(_equippedWeapon);
            RemoveWeapon();
        }
    }
    public void OnWeaponAbility()
    {
        if (_equippedWeapon)
        {
            _equippedWeaponScript.WeaponAbility();
        }
    }
    //public void OnGrabHold()
    //{
    //    _isGrabbing = true;
    //    _ragdollSwinging.IsSpinning = false;
    //}
    //public void OnGrabRelease()
    //{
    //    _isGrabbing = false;
    //}
    public void OnLeftPunch()
    {
        if (_canMove == false) return;

        // Check conditions
        bool inPassiveState = _ragdollHandler.PassiveRagdollActivated;
        bool isDead = _smashHealth.IsDead;
        bool hasWeapon = _equippedWeapon != null;

        if ((inPassiveState || isDead) == false && hasWeapon == false)
        {
            // Punch
            _punchHand.LeftPunch();
        }
    }
    public void OnRightPunch()
    {
        if (_canMove == false) return;

        // Check conditions
        bool inPassiveState = _ragdollHandler.PassiveRagdollActivated;
        bool isDead = _smashHealth.IsDead;
        bool hasWeapon = _equippedWeapon != null;

        if ((inPassiveState || isDead) == false && hasWeapon == false)
        {
            // Punch
            _punchHand.RightPunch();
        }
    }

    // Player info
    // -----------

    public Vector3 GetPlayerPos()
    {
        return _ragdollHips.transform.position;
    }
    public void SetPlayerPos(Vector3 position)
    {
        transform.position = position;
        _ragdollHips.transform.position = position;

        // Can't move when setting position in gameScene (this function gets called whenever the scene gets changed, you might want to remove this when used in other situations)
        if (SceneManager.GetActiveScene().name == _gameSceneName) _canMove = false;
    }

    public Transform GetPlayerTransform()
    {
        return _ragdollHips.transform;
    }

    // Player components
    // -----------------

    private RagdollMovement _ragdollMovement;
    private RagdollSwinging _ragdollSwinging;
    private IKControl _ragdollIKControl;
    private RagdollHandler _ragdollHandler;
    private RagdollAnimation _ragdollAnimation;
    private SmashHealth _smashHealth;
    //private GrabHand _grabHand;
    private PunchHand _punchHand;
    private RagdollParticles _ragdollParticles;

    public SmashHealth SmashHealth { get { return _smashHealth; } }

    // Weapon
    // ------
    public PickUp LastPickUp { get; set; }
    private GameObject _equippedWeaponPrefab;
    private GameObject _equippedWeapon;
    private Weapon _equippedWeaponScript;

    public Weapon EquippedWeaponScript
    {
        get { return _equippedWeaponScript; }
    }

    public void RemoveWeapon()
    {
        PlayerController.CanPickUp = true;

        _equippedWeapon = null;
        _equippedWeaponPrefab = null;
        _equippedWeaponScript = null;

        _smashHealth.KnockbackResistance = 0.0f;

        _ragdollSwinging.SetSwingArmWeight(-1.0f);
        _ragdollSwinging.RotateWithSpin = true;
        _ragdollSwinging.RotateWithSwing = true;
    }

    // Color
    // -----
    public ColorManager.PlayerColors PawnColor { get; set; }

    public void SetBodyMaterial(Material bodyMaterial)
    {
        _ragdollHandler.SetBodyMaterial(bodyMaterial);
    }
    public void SetFeatherMaterial(Material featherMaterial)
    {
        _ragdollHandler.SetFeatherMaterial(featherMaterial);
    }
    public void SetTransparantBodyMaterial(Material bodyMaterial)
    {
        _ragdollHandler.SetTransparantBodyMaterial(bodyMaterial);
    }
    public void SetTransparantFeatherMaterial(Material featherMaterial)
    {
        _ragdollHandler.SetTransparantFeatherMaterial(featherMaterial);
    }

    // Ragdoll
    // ------
    public Rigidbody HipsRigidbody { get; private set; }

    // Other
    // -----
    //private bool _gotGrabbed;
    //public bool GotGrabbed
    //{
    //    set
    //    {
    //        _gotGrabbed = value;
    //        if (_gotGrabbed)
    //        {
    //            _ragdollHandler.SetMass(0.5f);
    //            _ragdollHandler.SetHipForce(false, Vector3.zero);
    //        }
    //        else
    //        {
    //            _ragdollHandler.SetMass();
    //            _ragdollHandler.SetHipForce(true, Vector3.zero);
    //        }
    //    }
    //}

    private string _gameSceneName = "GameScene";

    // Start
    // -----
    void Start()
    {
        // Components
        InitializeComponents();
        
        // Events
        SceneLoader.Instance.SceneLoadedEvent += SceneLoadedEvent;
        GameSystem.Instance.UIManager.AnnouncementPanel.AnnouncementShown += AnnouncementShown;

        SpawnEffect.Spawn(PlayerController);
    }

    private void OnDestroy()
    {
        // Remove events
        GameSystem gameSystem = GameSystem.Instance;
        if (gameSystem)
        {
            UIManager uiManager = gameSystem.UIManager;
            if (uiManager) uiManager.AnnouncementPanel.AnnouncementShown -= AnnouncementShown;

            PlayerManager playerManager = gameSystem.PlayerManager;
            if (playerManager) playerManager.RemoveFromCamera(transform);
        }

        SceneLoader sceneLoader = SceneLoader.Instance;
        if(sceneLoader) sceneLoader.SceneLoadedEvent -= SceneLoadedEvent;

        // Stop rumble
        if (_gamepadID != -1)
        {
            Gamepad currentGamepad = Gamepad.all[_gamepadID];
            currentGamepad.SetMotorSpeeds(0.0f, 0.0f);
        }
    }

    // Update
    // ------
    void Update()
    {
        SyncVariables();
    }

    private void InitializeComponents()
    {
        // Get components
        // --------------
        
        // Hips
        Debug.Assert(_ragdollHips != null, "Pawn needs the hips for movement to work");

        HipsRigidbody = _ragdollHips.GetComponent<Rigidbody>();
        ConfigurableJoint hipsJoint = _ragdollHips.GetComponent<ConfigurableJoint>();
        ConstantForce hipsConstantForce = _ragdollHips.GetComponent<ConstantForce>();

        // Arms
        Debug.Assert(_ragdollRightArm != null, "Pawn needs the swingArm for swinging to work");
        Debug.Assert(_ragdollLeftArm != null, "Pawn needs the grabArm for grabbing to work");

        Rigidbody leftArmRigidbody = _ragdollLeftArm.GetComponent<Rigidbody>();
        Rigidbody rightArmRigidbody = _ragdollRightArm.GetComponent<Rigidbody>();

        // Other
        Debug.Assert(_weaponHoldingObject != null, "Pawn needs weaponHoldingObject to be able to spawn weapons");

        // Create ragdollMovement
        // ----------------------

        _ragdollMovement = gameObject.AddComponent<RagdollMovement>();
        
        _ragdollMovement.HipsRigidbody = HipsRigidbody;
        _ragdollMovement.HipsJoint = hipsJoint;

        // Create ragdollSwinging
        // ----------------------

        _ragdollSwinging = gameObject.AddComponent<RagdollSwinging>();

        _ragdollSwinging.HipsRigidbody = HipsRigidbody;
        _ragdollSwinging.HipsJoint = hipsJoint;

        _ragdollSwinging.RagdollSwingArmRigidbody = rightArmRigidbody;
        _ragdollSwinging.WeaponHoldingObject = _weaponHoldingObject;

        // Get ragdollIKControl
        // --------------------

        _ragdollIKControl = gameObject.GetComponentInChildren<IKControl>();
        
        _ragdollIKControl.HipsTransform = _ragdollHips.transform;

        // Get ragdollHandler
        // ------------------

        _ragdollHandler = gameObject.GetComponent<RagdollHandler>();
        Debug.Assert(_ragdollHandler != null, "Error: no ragdollHandler found on the playerPawn");

        _ragdollHandler.ParentPawn = this;
        _ragdollHandler.HipsConstantForce = hipsConstantForce;
        _ragdollHandler.PawnColor = PawnColor;

        // Get ragdollAnimation
        // --------------------
        _ragdollAnimation = gameObject.GetComponentInChildren<RagdollAnimation>();
        Debug.Assert(_ragdollAnimation != null, "Error: no ragdollAnimation found on the playerPawn");

        _ragdollAnimation.ParentPawn = this;

        // Get smashHealth
        // ---------------
        _smashHealth = gameObject.GetComponent<SmashHealth>();
        Debug.Assert(_smashHealth != null, "Error: no smashHealth found on the playerPawn");

        _smashHealth.Ragdollhandler = _ragdollHandler;

        //// Get grabHand
        //// ------------
        //_grabHand = gameObject.GetComponentInChildren<GrabHand>();
        //Debug.Assert(_grabHand != null, "Error: no grabHand found on the playerPawn");

        //_grabHand.PlayerParent = this;
        //_grabHand.ArmRigidbody = grabArmRigidbody;

        // Get punchHand
        // ------------
        _punchHand = gameObject.GetComponent<PunchHand>();
        Debug.Assert(_punchHand != null, "Error: no punchHand found on playerPawn");

        _punchHand.SmashHealth = _smashHealth;
        _punchHand.LeftArmRigidbody= leftArmRigidbody;
        _punchHand.RightArmRigidbody= rightArmRigidbody;
        _punchHand.HipsRigidbody = HipsRigidbody;

        // Get radollParticles
        // -------------------
        _ragdollParticles = gameObject.GetComponent<RagdollParticles>();
        Debug.Assert(_ragdollParticles != null, "Error, no ragdollParticles found on playerPawn");
    }

    private void SyncVariables()
    {
        // Don't update when isDead
        if (_smashHealth.IsDead) return;

        // Don't process input when in passive ragdoll
        Vector2 movementInput = _charachterMovementInput;
        Vector2 armInput = _armMovementInput;
        //bool isGrabbing = _isGrabbing;
        bool passiveRagdollActivated = _ragdollHandler.PassiveRagdollActivated;

        if (passiveRagdollActivated)
        {
            movementInput = Vector2.zero;
            armInput = Vector2.zero;
            //isGrabbing = false;
        }

        // Don't move when told not to
        if (_canMove == false) movementInput = Vector2.zero;

        // Check if is input
        bool isMovementInput = movementInput.x != 0 || movementInput.y != 0;
        bool isArmInput = armInput.x != 0 || armInput.y != 0;

        // Smash Health
        // ------------
        _smashHealth.IsArmInput = isArmInput;

        // Ragdoll movement
        // ----------------
        _ragdollMovement.MovementInput = movementInput;
        _ragdollMovement.IsMovementInput = isMovementInput;
        _ragdollMovement.IsArmInput = isArmInput;
        _ragdollMovement.PassiveRagdollActivated = passiveRagdollActivated;

        // Ragdoll swinging
        // ----------------
        _ragdollSwinging.ArmInput = _armMovementInput;
        _ragdollSwinging.IsArmInput = isArmInput;

        // RagdollIKControl
        // ----------------
        _ragdollIKControl.IsArmInput = isArmInput;
        //_ragdollIKControl.IsGrabbing = isGrabbing;
        _ragdollIKControl.IsSpinning = _ragdollSwinging.IsSpinning;

        if (_equippedWeapon != null) _ragdollIKControl.HasWeapon = true;
        else                         _ragdollIKControl.HasWeapon = false;

        // GrabHand
        // --------
        //_grabHand.IsGrabbing = isGrabbing;

        // Ragdoll Handler
        // ---------------
        //_ragdollHandler.IsGrabbing = isGrabbing;
        _ragdollHandler.IsMovementInput = isMovementInput;
        _ragdollHandler.Health = _smashHealth.Health;

        // RagdollAnimation
        // ----------------
        _ragdollAnimation.IsMovement = isMovementInput;

        // Ragdoll Particles
        // -----------------
        _ragdollParticles.IsMovementInput = isMovementInput;
        _ragdollParticles.PassiveRagdollActivated = passiveRagdollActivated;

        // Weapon
        // ------
        if (_equippedWeaponScript)
        {
            _equippedWeaponScript.IsArmInput = isArmInput;
            _equippedWeaponScript.ArmInput = armInput;
            _equippedWeaponScript.IsSpinning = _ragdollSwinging.IsSpinning;
        }
    }

    public void AddSlow()
    {
        _ragdollMovement.AddSlow();
        AddedWaterSlowEvent?.Invoke(PlayerController);
    }
    public void RemoveSlow()
    {
        _ragdollMovement.RemoveSlow();
        RemoveWaterSlowEvent?.Invoke(PlayerController);
    }

    public void OnDeath(bool wasSpike)
    {
        // Notify components
        _ragdollAnimation.OnDeath();
        _ragdollHandler.SetDead(wasSpike);
        PlayerController.OnDeath();

        // Set dead and destroy
        IsDead = true;
        StartCoroutine(DestroySelf(0.2f));
    }
    private IEnumerator DestroySelf(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    private void SceneLoadedEvent(string sceneName)
    {
        // If in gameScene
        if (sceneName == _gameSceneName)
        {
            // Can't move
            _canMove = false;
            Debug.Log("Can't move");
        }

        // Destroy if dead
        if (IsDead) Destroy(this.gameObject);
    }
    private void AnnouncementShown(GameObject announcementPanel)
    {
        _canMove = true;
        Debug.Log("Can move");
    }

    public void SetControllerRumble(float strength, float duration)
    {
        Gamepad currentGamepad = Gamepad.all[_gamepadID];
        currentGamepad.SetMotorSpeeds(strength, strength);
        StartCoroutine(ResetRumble(currentGamepad, duration));
    }
    private IEnumerator ResetRumble(Gamepad gamepad, float duration)
    {
        yield return new WaitForSeconds(duration);
        gamepad.SetMotorSpeeds(0.0f, 0.0f);
    }
}
