using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PickUpAble : MonoBehaviour
{
    [SerializeField] private BoxCollider _pickUpTriggerZone;
    public BoxCollider GetBox 
    { 
        get 
        { 
            if (_pickUpTriggerZone == null)
            {
                _pickUpTriggerZone = GetComponent<BoxCollider>();
            }
            return _pickUpTriggerZone; 
        } 
    }

    private void Awake()
    {
        GetBox.enabled = false;
    }
}
