using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHandler : MonoBehaviour
{
    [Header("Renders")]
    [SerializeField] private SkinnedMeshRenderer _ragdollSkin;
    [SerializeField] private SkinnedMeshRenderer _ragdollFeather;
    [SerializeField] private SkinnedMeshRenderer _ragdollTransparantSkin;
    [SerializeField] private SkinnedMeshRenderer _ragdollTransparantFeather;

    [Header("OnHit variables")]
    [Tooltip("How long it takes before the ragdoll gets back up again")]
    [SerializeField] private float _maxHitTime = 1.0f;
    [SerializeField] private float _minFlickerTime = 0.05f;
    [SerializeField] private float _maxFlickerTime = 0.15f;
    [Tooltip("This will be considered as having \"High\" health, changing variables such as Flicker accordingly")]
    [SerializeField] private float _highHealthTreshold = 500.0f;
    [Tooltip("When enabled, flicker color changes from white to red, to dark-red according to the gained health")]
    [SerializeField] private bool _dynamicColors = false;

    [Header("VerticalMovement")]
    [Tooltip("How low the ragdoll can be before it starts to gain more hipsForce")]
    [SerializeField] private float _minHeightTreshold = 0.375f;
    [Tooltip("How high the ragdoll can be before it loses it's hipsForce")]
    [SerializeField] private float _maxHeightTreshold = 0.5f;
    [SerializeField] private bool _ragdollWhenFalling = false;
    [SerializeField] private LayerMask _layersToIgnore;

    // Input
    public bool IsMovementInput { private get; set; }

    // Health
    private float _health;
    public float Health
    {
        set { _health = value; }
    }

    // Ragdoll limbs
    private ConstantForce _hipsConstantForce;
    public ConstantForce HipsConstantForce
    {
        set
        {
            _hipsConstantForce = value;
            _startConstantForce = _hipsConstantForce.force;
        }
    }
    private Vector3 _startConstantForce;

    private ConfigurableJoint[] _limbJoints;
    private JointDrive[] _limbJointDrives;
    private float[] _limbMass;

    // Ragdoll rigidbodies and colliders
    private Rigidbody[] _limbRigidbodies;
    private Collider[] _limbColliders;

    // Flicker
    Material _ragdollBodyMaterial;
    Material _ragdollTransparantBodyMaterial;
    Color _ragdollStartColor;
    private string _colorPropertyName = "_base_color";

    // Passive ragdoll
    private bool _passiveRagdollActivated = false;
    public bool PassiveRagdollActivated
    {
        get { return _passiveRagdollActivated; }
    }

    private bool _isHipsControlled;

    // Color
    private ColorManager.PlayerColors _pawnColor;
    public ColorManager.PlayerColors PawnColor
    {
        set { _pawnColor = value; }
    }

    // Other
    private PlayerPawn _parentPawn;
    public PlayerPawn ParentPawn
    {
        set { _parentPawn = value; }
    }

    //private bool _isGrabbing;
    //public bool IsGrabbing
    //{
    //    set 
    //    {
    //        bool initValue = _isGrabbing;
    //        _isGrabbing = value;
    //        if (_isGrabbing && _isGrabbing != initValue)
    //        {
    //            SetAngularDrive(2.0f, true);
    //        }
    //        else if (_isGrabbing != initValue)
    //        {
    //            SetAngularDrive();
    //        }
    //    }
    //}

    // Start
    // -----
    void Start()
    {
        // Get ragdoll joints
        // ------------------
        _limbJoints = gameObject.GetComponentsInChildren<ConfigurableJoint>();
        _limbJointDrives = new JointDrive[_limbJoints.Length];

        // Store all the jointDrives
        int index = 0;
        foreach (ConfigurableJoint joint in _limbJoints)
        {
            _limbJointDrives[index] = joint.angularXDrive;
            ++index;
        }

        // Get ragdoll rigidbodies and colliders
        // -------------------------------------
        _limbRigidbodies = gameObject.GetComponentsInChildren<Rigidbody>();
        _limbColliders = gameObject.GetComponentsInChildren<Collider>();

        // Get limb mass
        _limbMass = new float[_limbRigidbodies.Length];
        index = 0;
        foreach (Rigidbody rigidbody in _limbRigidbodies)
        {
            _limbMass[index] = rigidbody.mass;
            ++index;            
        }

        // Check playerBody
        // ----------------
        Debug.Assert(_ragdollSkin != null, "Error: no ragdollSkin put on ragdollHandler");
        Debug.Assert(_ragdollFeather != null, "Error: no ragdollFeather put on ragdollHandler");
        Debug.Assert(_ragdollTransparantSkin != null, "Error: no ragdollTransparantSkin put on ragdollHandler");
        Debug.Assert(_ragdollTransparantFeather != null, "Error: no ragdollTransparantFeather put on ragdollHandler");

        // Set ragdoll material
        // --------------------
        GameSystem.Instance.ColorManager.SetPawnColor(_parentPawn, _pawnColor);
    }

    // Update
    // ------
    void Update()
    {
        HipForceHandler();
    }

    private void HipForceHandler()
    {
        bool passiveRagdollActivated = false;

        // Hit raycast
        RaycastHit raycast;
        if (Physics.Raycast(_parentPawn.GetPlayerPos(), Vector3.down, out raycast, 1000.0f, ~_layersToIgnore))
        {
            // If distance to ground is more then start height
            if (raycast.distance > _maxHeightTreshold)
            {
                // Disable hipsForce
                _hipsConstantForce.force = Vector3.zero;
                if (_ragdollWhenFalling)
                {
                    PassiveRagdoll(true, true);
                    passiveRagdollActivated = true;
                }
            }
            // If distance too close to ground
            else if(raycast.distance < _minHeightTreshold)
            {
                // Beef up hipsForce
                _hipsConstantForce.force = _startConstantForce * 2.0f;
            }
            else
            {
                // Enable hipsForce
                _hipsConstantForce.force = _startConstantForce;
            }
        }
        else
        {
            // Enable hipsForce
            _hipsConstantForce.force = _startConstantForce;
        }

        // If ragdoll was not changed this frame and the passiveRagdoll is currently being controlled by this function
        if (passiveRagdollActivated == false && _passiveRagdollActivated && _isHipsControlled)
        {
            // Reset Passive ragdoll
            PassiveRagdoll(false);
        }
    }

    // OnHit
    // -----
    public void OnHit(bool enableRagdoll)
    {
        // Flicker
        EnableFlicker();
        StartCoroutine(Reset_BodyColor());

        // Ragdoll
        if (enableRagdoll)
        {
            _hipsConstantForce.force = Vector3.zero;
            PassiveRagdoll(true);
            StartCoroutine(Reset_Coroutine());
        }
    }

    // Flicker
    // -------
    private void EnableFlicker()
    {
        // Change color
        Color damageColor = Color.white;

        if (_dynamicColors)
        {
            float newAttributes = 1 - Mathf.InverseLerp(0.0f, _highHealthTreshold, _health);
            float newRed = Mathf.Clamp(1 - Mathf.InverseLerp(0.0f, 2 * _highHealthTreshold, _health), 0.1f, 1.0f);

            damageColor.r = newRed;
            damageColor.g = newAttributes;
            damageColor.b = newAttributes;
        }

        // Set new color
        _ragdollBodyMaterial.SetColor(_colorPropertyName, damageColor);
        _ragdollTransparantBodyMaterial.SetColor(_colorPropertyName, damageColor);
    }
    
    private IEnumerator Reset_BodyColor()
    {
        yield return new WaitForSeconds(Mathf.Lerp(_minFlickerTime, _maxFlickerTime, Mathf.Clamp01(_health/_highHealthTreshold)));

        // Reset bodyColor
        _ragdollBodyMaterial.SetColor(_colorPropertyName, _ragdollStartColor);
        _ragdollTransparantBodyMaterial.SetColor(_colorPropertyName, _ragdollStartColor);
    }
    private IEnumerator Reset_Coroutine()
    {
        yield return new WaitForSeconds(_maxHitTime);

        // Reset joints
        PassiveRagdoll(false);

        // Re-Enable constantForce
        _hipsConstantForce.force = _startConstantForce;
    }

    // Color
    // -----

    public void SetBodyMaterial(Material bodyMaterial)
    {
        _ragdollSkin.material = new Material(bodyMaterial);
        _ragdollBodyMaterial = _ragdollSkin.material;
        _ragdollStartColor = _ragdollBodyMaterial.GetColor(_colorPropertyName);
    }
    public void SetTransparantBodyMaterial(Material bodyMaterial)
    {
        _ragdollTransparantSkin.material = new Material(bodyMaterial);
        _ragdollTransparantBodyMaterial = _ragdollTransparantSkin.material;
    }
    public void SetFeatherMaterial(Material featherMaterial)
    {
        _ragdollFeather.material = new Material(featherMaterial);
    }
    public void SetTransparantFeatherMaterial(Material featherMaterial)
    {
        _ragdollTransparantFeather.material = new Material(featherMaterial);
    }

    // Passive ragdoll
    // ---------------
    public void PassiveRagdoll(bool activate, bool isHipsControlled = false)
    {
        // If activate passive ragdoll
        if (activate)
        {
            // Set Constant Force to 0
            SetHipForce(false, Vector3.zero);

            // Set all Angular Drives to 0
            SetAngularDrive(0.0f);

            // Enable passive ragdoll
            _passiveRagdollActivated = true;
        }
        // If deactivate passive ragdoll
        else
        {
            // Set Constant Force to original
            SetHipForce(true, Vector3.zero);

            // Set all Angular Drives to original
            SetAngularDrive();

            // Disable passive ragdoll
            _passiveRagdollActivated = false;
        }

        // Set isHipsControlled
        _isHipsControlled = isHipsControlled;
    }

    // Mass
    // ----
    public void SetMass(float mass = -1.0f)
    {
        int index = 0;
        foreach (Rigidbody rigidbody in _limbRigidbodies)
        {
            if (mass == -1.0f) rigidbody.mass = _limbMass[index];
            else               rigidbody.mass = mass;
            
            ++index;
        }
    }
    public void SetHipForce(bool reset, Vector3 newForce)
    {
        if (reset)  _hipsConstantForce.force = _startConstantForce;
        else        _hipsConstantForce.force = newForce;

    }

    // Dead
    // ----
    public void SetDead(bool wasSpike)
    {
        // Passive ragdoll
        PassiveRagdoll(true);

        if (wasSpike)
        {
            // Stop motion and disable rigidbodies
            foreach (Rigidbody rigidbody in _limbRigidbodies)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.detectCollisions = false;
                rigidbody.isKinematic = true;
            }

            // Stop hips ConstantForce
            _hipsConstantForce.force = Vector3.zero;
        }
    }


    // Ignore collisions
    // -----------------
    public void IgnoreRagdollColliders(Collider colliderToIgnore, bool ignore = true)
    {
        foreach (Collider ragdollCollider in _limbColliders)
        {
            Physics.IgnoreCollision(colliderToIgnore, ragdollCollider, ignore);
        }
    }

    // Angular forces
    private void SetAngularDrive(float amount = -1.0f, bool multiply = false)
    {
        // Check if should set to orinal or not
        JointDrive newAngularDrive = _limbJointDrives[0];
        if (amount != -1.0f)
        {
            if (multiply) newAngularDrive.positionSpring *= amount;
            else          newAngularDrive.positionSpring = amount;
        }

        // Set all Angular Drives to original
        int index = 0;
        foreach (ConfigurableJoint joint in _limbJoints)
        {
            if (amount == -1.0f)
            {
                joint.angularXDrive = _limbJointDrives[index];
                joint.angularYZDrive = _limbJointDrives[index];
            }
            else
            {
                joint.angularXDrive = newAngularDrive;
                joint.angularYZDrive = newAngularDrive;
            }
          
            ++index;
        }
    }
}