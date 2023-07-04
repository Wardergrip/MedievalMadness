using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curtain : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How far the curtain extends when closing")]
    [SerializeField] private float _maxCurtainScale = 1f;
    [SerializeField] private float _curtainCloseSpeed = 1f;

    // Curtain
    private float _startScale;
    public Action<bool> CurtainChanged;

    // Start
    // -----
    void Start()
    {
        // Curtain
        _startScale = transform.localScale.z;
    }

    public void CloseCurtain()
    {
        StartCoroutine(CloseCurtaiCor());
    }
    private IEnumerator CloseCurtaiCor()
    {
        float curtainSpeed;
        Vector3 curtainScale = transform.localScale;

        // While curtain not reached scale
        while (curtainScale.z < _maxCurtainScale)
        {
            // Calculate scale
            curtainSpeed = _curtainCloseSpeed * Time.deltaTime;
            curtainScale.z += curtainSpeed;

            // Transform curtain
            transform.localScale = curtainScale;
            yield return null;
        }

        // Send event
        CurtainChanged?.Invoke(true);

        // Open curtain
        StartCoroutine(OpenCurtain());
    }
    private IEnumerator OpenCurtain()
    {
        float curtainSpeed;
        Vector3 curtainScale = transform.localScale;

        // While curtain not reached scale
        while (_startScale < curtainScale.z)
        {
            // Calculate scale
            curtainSpeed = _curtainCloseSpeed * Time.deltaTime;
            curtainScale.z -= curtainSpeed;

            // Transform curtain
            transform.localScale = curtainScale;
            yield return null;
        }

        // Send event
        CurtainChanged?.Invoke(false);
    }
}
