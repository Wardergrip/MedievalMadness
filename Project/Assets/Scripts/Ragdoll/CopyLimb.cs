using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyLimb : MonoBehaviour
{
    [Tooltip("Limb that current limb will have to copy")]
    [SerializeField] private Transform _targetLimb;

    private ConfigurableJoint _configurableJoint;
    private Quaternion _targetStartRotation;

    void Awake()
    {
        // Get configurableJoint
        _configurableJoint = GetComponent<ConfigurableJoint>();
        Debug.Assert(_configurableJoint != null);

        // Get targetLimb startRotation
        Debug.Assert(_targetLimb != null);
        _targetStartRotation = _targetLimb.transform.localRotation;
    }


    void FixedUpdate()
    {
        CopyLimbRotation();
    }

    private void CopyLimbRotation()
    {
        // Calculate the rotation expressed by the joint's axis and secondary axis
        Vector3 right = _configurableJoint.axis;
        Vector3 forward = Vector3.Cross(_configurableJoint.axis, _configurableJoint.secondaryAxis).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

        // Transform into world space
        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

        // Counter-rotate and apply the new local rotation.
        // Joint space is the inverse of world space, so we need to invert our value
        resultRotation *= Quaternion.Inverse(_targetLimb.localRotation) * _targetStartRotation;

        // Transform back into joint space
        resultRotation *= worldToJointSpace;

        // Set target rotation to our newly calculated rotation
        _configurableJoint.targetRotation = resultRotation;
    }
}
