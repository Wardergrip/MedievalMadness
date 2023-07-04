using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKControl : MonoBehaviour
{
    [Tooltip("Object ragdoll will reach to when swinging")]
    [SerializeField] private GameObject _reachObject;

    // Player variables
    // ----------------
    public Transform HipsTransform { private get; set; }

    // Input
    // -----
    private bool _isArmInput;
    public bool IsArmInput
    {
        set 
        { 
            _isArmInput = value;
            if (_isReaching && _isArmInput)
            {
                _isReaching = false;
            }
        }
    }

    //private bool _isGrabbing;
    //public bool IsGrabbing
    //{
    //    set { _isGrabbing = value; }
    //}

    // Other
    // -----
    private Animator _animator;

    private bool _isSpinning;
    public bool IsSpinning
    {
        set { _isSpinning = value; }
    }

    private bool _hasWeapon;
    public bool HasWeapon
    {
        set { _hasWeapon = value; }
    }

    private bool _isReaching = false;

    // Start
    // -----
    void Start()
    {
        // Get animator component
        _animator = GetComponent<Animator>();

        Debug.Assert(_animator != null, "IKControl needs Animator component to be used");
        Debug.Assert(_reachObject != null, "IKControl needs reachObject to be used");
    }

    // Callback for calculating IK
    // ---------------------------
    private void OnAnimatorIK(int layerIndex)
    {
        // If no animator, return
        if (_animator == null) return;

        RightHandIK();
        LeftHandIK();
    }

    public void ShowcaseWeapon(float resetTime)
    {
        // Start coroutine
        _isReaching = true;
        StartCoroutine(ResetHandCor(resetTime));
    }
    private IEnumerator ResetHandCor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _isReaching = false;
    }

    private void RightHandIK()
    {
        Vector3 reachPos = _reachObject.transform.position;
        if (_isReaching)
        {
            reachPos += Vector3.up * 7.5f;
            Debug.DrawLine(reachPos, reachPos, Color.yellow, 1.0f);
        }

        // If is swinging and has weapon
        if (_isArmInput && _hasWeapon || _isReaching)
        {
            // Set the right hand target position and rotation
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, reachPos);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, _reachObject.transform.localRotation);
        }
        // If not swinging
        else
        {
            // Set the position and rotation of the hand back to the original position
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }
    }

    private void LeftHandIK()
    {
        // Set the left hand target position and rotation when spinning with weapon or grabbing
        if ((_isSpinning && _hasWeapon) /*|| _isGrabbing*/)
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, _reachObject.transform.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, _reachObject.transform.localRotation);
        }
        else
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }
}
