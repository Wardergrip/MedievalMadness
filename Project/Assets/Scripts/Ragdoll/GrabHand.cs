using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GrabHand : MonoBehaviour
{
    [Tooltip("The amount of force it takes before the grabbed object breaks away")]
    [SerializeField] private float _breakForce = 10000.0f;

    // Objects
    private GameObject _grabbedObject;
    private PlayerPawn _playerParent;
    public PlayerPawn PlayerParent
    {
        set { _playerParent = value; }
    }

    // Rigidbodies
    private Rigidbody _armRigidbody;
    public Rigidbody ArmRigidbody
    {
        set { _armRigidbody = value; }
    }

    // Input
    private bool _isGrabbing = false;
    public bool IsGrabbing
    {
        set
        { 
            _isGrabbing = value;
            if (_isGrabbing == false)
            {
                RemoveGrabbedObject();
            }
        }
    }

    // Other
    private FixedJoint _grabbedJoint;

    // Collision
    // ---------
    private void OnTriggerEnter(Collider other)
    {
        // If is not grabbing, return
        if (_isGrabbing == false) return;

        // If already holding object, return
        if (_grabbedObject != null) return;

        // Get the smashHealth component
        SmashHealth otherHealth = other.gameObject.GetComponentInParent<SmashHealth>();
        if (otherHealth == null)
        {
            otherHealth = other.gameObject.GetComponent<SmashHealth>();
            if (otherHealth == null)
            {
                //Debug.Log("Other did not have smashHealth");
                return;
            }
        }

        // Check if not hitting yourself
        PlayerPawn otherPawn = otherHealth.gameObject.GetComponent<PlayerPawn>();
        if (otherPawn == _playerParent)
        {
            //Debug.Log("Hand was hitting parent");
            return;
        }

        // Store gameObject and add fixedJoint
        _grabbedObject = otherPawn.gameObject;
        //otherPawn.GotGrabbed = true;

        _grabbedJoint = _grabbedObject.AddComponent<FixedJoint>();
        _grabbedJoint.breakForce = _breakForce;
        _grabbedJoint.connectedBody = _armRigidbody;
    }
    private void OnTriggerExit(Collider other)
    {
        
    }

    private void RemoveGrabbedObject()
    {
        if (_grabbedObject != null)
        {
            Destroy(_grabbedJoint);

            //_grabbedObject.gameObject.GetComponentInParent<PlayerPawn>().GotGrabbed = false;
            _grabbedObject = null;
        }
    }
}
