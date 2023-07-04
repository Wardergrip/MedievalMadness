using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RagdollMovement : MonoBehaviour
{
    // Player variables
    // ----------------
    [Header("Watch Values")]
    [SerializeField] private float _currentMovementSpeed = -1f;

    // Hips
    private Rigidbody _hipsRigidbody;
    public Rigidbody HipsRigidbody
    {
        set { _hipsRigidbody = value; }
    }

    private ConfigurableJoint _hipsJoint;
    public ConfigurableJoint HipsJoint
    {
        set { _hipsJoint = value; }
    }

    // Input
    // -----
    private Vector2 _movementInput;
    public Vector2 MovementInput
    {
        set { _movementInput = value; }
    }

    private Vector2 _lastMovementInput;

    private bool _isMovementInput;
    public bool IsMovementInput
    {
        set { _isMovementInput = value; }
    }

    private bool _isArmInput;
    public bool IsArmInput
    {
        set { _isArmInput = value; }
    }

    // Other
    // -----
    private bool _passiveRagdollActivated;
    public bool PassiveRagdollActivated
    {
        set { _passiveRagdollActivated = value; }
    }

    private void Start()
    {
        _currentMovementSpeed = SettingsManager.Instance.PlayerSettings.baseMovementSpeed;
    }

    private void FixedUpdate()
    {
        // If passive ragdoll enabled, return
        if (_passiveRagdollActivated) return;

        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        // Calculate movement
        Vector3 inputMovement = _movementInput.x * Vector3.right + _movementInput.y * Vector3.forward;
        Vector3 finalMovement = inputMovement.normalized * _currentMovementSpeed * Time.deltaTime;

        // Set velocity to rigidbody
        if(_hipsRigidbody.isKinematic == false) _hipsRigidbody.velocity = finalMovement;

        // If there was movement, store it
        if(_isMovementInput) _lastMovementInput = _movementInput;
    }
    private void HandleRotation()
    {
        // Rotation is handled by the swinging if armInput
        if (_isArmInput) return;

        // Calculate desired rotation
        Vector3 normalizedDirection = new Vector3(_lastMovementInput.x, 0, _lastMovementInput.y).normalized;
        float desiredAngle = Mathf.Atan2(normalizedDirection.z, normalizedDirection.x) * Mathf.Rad2Deg;
        
        float singleStep = SettingsManager.Instance.PlayerSettings.rotationSpeed * Time.deltaTime;
        Quaternion desiredRotation = Quaternion.Euler(0.0f, desiredAngle - 90.0f, 0.0f);
        Quaternion lerpedRotation = Quaternion.Lerp(_hipsJoint.targetRotation, desiredRotation, singleStep);

        // Set new rotation
        _hipsJoint.targetRotation = lerpedRotation;
    }

    public void AddSlow()
    {
        _currentMovementSpeed = SettingsManager.Instance.PlayerSettings.baseMovementSpeed * SettingsManager.Instance.PlayerSettings.waterSlowMultiplier;
    }
    public void RemoveSlow()
    {
        _currentMovementSpeed = SettingsManager.Instance.PlayerSettings.baseMovementSpeed;
    }
}
