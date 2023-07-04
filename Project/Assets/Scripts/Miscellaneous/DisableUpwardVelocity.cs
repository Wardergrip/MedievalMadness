using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableUpwardVelocity : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private Rigidbody _rigidbody;

    // Start
    void Start()
    {
        Debug.Assert(_rigidbody != null, "Error: no rigidbody found on DisableUpwardVelocity script");
    }

    private void FixedUpdate()
    {
        // If going upward
        if (_rigidbody.velocity.y > 0)
        {
            // Disable Y velocity
            Vector3 newVelocity = _rigidbody.velocity;
            newVelocity.y = 0;

            _rigidbody.velocity = newVelocity;
        }
    }
}
