using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FireHazard : MonoBehaviour
{
    [Header("Burning")]
    [SerializeField] private float _burnTime = 20.0f;
    [SerializeField] private float _currentBurnTime = 0;

    [Header("Particles")]
    [SerializeField] private GameObject _fireParticleEffect;
    [SerializeField] private int _numberOfParticles = 20;

    private BoxCollider _boxCollider;

    // Start
    // -----
    void Start()
    {
        // Get variables
        Debug.Assert(_fireParticleEffect != null, "FireHazard needs fireParticle for it to work");
        _boxCollider = GetComponent<BoxCollider>();

        // Spawn particles
        Vector3 spawnLocation = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;
        for (int idx = 0; idx < _numberOfParticles; ++idx)
        {
            spawnLocation = _boxCollider.GetRandomPositionInBox();
            spawnRotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);

            Instantiate(_fireParticleEffect, spawnLocation, spawnRotation, transform);
        }

        // Call late start
        float lateStartWaitTime = 0.1f;
        StartCoroutine(LateStart(lateStartWaitTime));
    }
    private IEnumerator LateStart(float waitTime)
    {
        // After wait time, start function
        yield return new WaitForSeconds(waitTime);

        Hazard hazardComponent = GetComponent<Hazard>();
        Debug.Assert(hazardComponent != null, "No hazardComponent found on the fireObject");

        hazardComponent.SpawnHitEffect = false;
        hazardComponent.IsFire = true;
    }

    // Update
    // ------
    void Update()
    {
        _currentBurnTime += Time.deltaTime;
        if (_currentBurnTime >= _burnTime)
        {
            Destroy(gameObject);
        }
    }
}
