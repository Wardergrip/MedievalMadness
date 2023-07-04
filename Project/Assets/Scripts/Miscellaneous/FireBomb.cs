using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class FireBomb : MonoBehaviour
{
    [Header("GameObjects")]
    [SerializeField] private GameObject _fireHazard;
    [SerializeField] private GameObject _visuals;

    [Header("Settings")]
    [Tooltip("How hard to the fireBomb gets thrown downwards at start")]
    [SerializeField] private float _startForce = 100.0f;

    // Start
    // -----
    private void Start()
    {
        Debug.Assert(_fireHazard != null, "FireBomb needs fireHazard to work");
        Debug.Assert(_visuals != null, "FireBomb needs visuals to work");

        // Shoot bomb downwards
        GetComponent<Rigidbody>().AddForce(Vector3.down * _startForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Spawn fireHazard
        GameObject spawnedHazard = Instantiate(_fireHazard, transform.position, Quaternion.identity);
        spawnedHazard.transform.localScale = gameObject.transform.localScale;

        // Destroy self
        Destroy(gameObject);
    }

    public void HideVisuals()
    {
        _visuals.SetActive(false);
    }
}
