using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollSwinging : MonoBehaviour
{
    // Player variables
    // ----------------
    private ConfigurableJoint _hipsJoint;
    public ConfigurableJoint HipsJoint
    {
        set { _hipsJoint = value; }
    }

    public Rigidbody HipsRigidbody { private get; set; }

    private GameObject _weaponHoldingObject;
    public GameObject WeaponHoldingObject
    {
        set
        {
            _weaponHoldingObject = value;
            _weaponHoldingStartRotation = _weaponHoldingObject.transform.localRotation;
        }
    }

    Quaternion _weaponHoldingStartRotation;

    private Rigidbody _ragdollSwingArmRigidbody;
    public Rigidbody RagdollSwingArmRigidbody
    {
        set
        {
            _ragdollSwingArmRigidbody = value;
            _swingArmstartWeight = _ragdollSwingArmRigidbody.mass;
        }
    }

    private float _swingArmstartWeight = 1.0f;

    // Put on -1 to reset to normal
    public void SetSwingArmWeight(float weight = -1.0f)
    {
        if (weight == -1.0f)
        {
            _ragdollSwingArmRigidbody.mass = _swingArmstartWeight;
        }
        else
        {
            _ragdollSwingArmRigidbody.mass = weight;
        }
    }

    // Input
    // -----
    private Vector2 _armInput;
    public Vector2 ArmInput
    {
        set { _armInput = value; }
    }

    private bool _isGrabbing;
    public bool IsGrabbing
    {
        set { _isGrabbing = value; }
    }

    private Vector2 _lastArmInput;

    private bool _isArmInput;
    public bool IsArmInput
    {
        set { _isArmInput = value; }
    }

    // Spinning
    // --------
    private bool _isSpinning;
    public bool IsSpinning
    {
        get { return _isSpinning; }
        set
        { 
            _isSpinning = value;
            _totalSpinRotation = 0;
        }
    }

    private float _totalSpinRotation;

    // Rotations
    // ---------
    public bool RotateWithSwing { get; set; }
    public bool RotateWithSpin { get; set; }

    // Other
    // -----


    // Updates
    // -------
    void Update()
    {
        HandleSpinning();
        HandleWeaponHoldingRotation();
    }

    private void FixedUpdate()
    {
        HandleSwinging();
    }

    private void HandleSpinning()
    {
        // If armInput and isn't grabbing
        if (_isArmInput && _isGrabbing == false)
        {
            // Calculate angle between lastInput and current one, then store
            float angle = Vector2.SignedAngle(_lastArmInput, _armInput);
            _totalSpinRotation += angle;

            // If full rotation done
            float fullRotation = 360.0f;
            if (Mathf.Abs(_totalSpinRotation) >= fullRotation)
            {
                // Is spinning
                _isSpinning = true;
            }
        }
        // When no armInput
        else
        {
            // Reset variables
            _isSpinning = false;
            _totalSpinRotation = 0;
        }

        // Store armInput
        _lastArmInput = _armInput;
    }
    private void HandleWeaponHoldingRotation()
    {
        // Rotation variables
        float singleStep = SettingsManager.Instance.PlayerSettings.weaponObjectRotationSpeed * Time.deltaTime;

        Quaternion desiredRotation;
        Quaternion rotation;

        // Rotate when armInput
        if (_isArmInput && RotateWithSwing)
        {
            // Start angles
            Vector3 startAngles = _weaponHoldingStartRotation.eulerAngles;

            // Hold lower when spinning
            if (_isSpinning && RotateWithSpin)
            {
                desiredRotation = Quaternion.Euler(startAngles.x + 90.0f, startAngles.y, startAngles.z);
            }
            else
            {
                desiredRotation = Quaternion.Euler(startAngles.x + 30.0f, startAngles.y, startAngles.z + 45.0f);
            }

            rotation = Quaternion.RotateTowards(_weaponHoldingObject.transform.localRotation, desiredRotation, singleStep);
            _weaponHoldingObject.transform.localRotation = rotation;
        }
        // Rotate back to original rotation
        else
        {
            singleStep *= 2.0f;
            desiredRotation = _weaponHoldingStartRotation;
            rotation = Quaternion.RotateTowards(_weaponHoldingObject.transform.localRotation, desiredRotation, singleStep);

            _weaponHoldingObject.transform.localRotation = rotation;
        }
    }

    private void HandleSwinging()
    {
        // If no armInput, return
        if (_isArmInput == false) return;

        // Rotating Movement
        // -----------------

        // Calculate desired rotation
        Vector3 normalizedDirection = new Vector3(_armInput.x, 0, _armInput.y).normalized;
        float desiredAngle = Mathf.Atan2(normalizedDirection.z, normalizedDirection.x) * Mathf.Rad2Deg;

        float singleStep = SettingsManager.Instance.PlayerSettings.rotationSpeed * Time.deltaTime;
        Quaternion desiredRotation = Quaternion.Euler(0.0f, desiredAngle - 90.0f, 0.0f);
        Quaternion lerpedRotation = Quaternion.Lerp(_hipsJoint.targetRotation, desiredRotation, singleStep);

        // Set new rotation
        _hipsJoint.targetRotation = lerpedRotation;
    }
}
