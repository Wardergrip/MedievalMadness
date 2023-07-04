using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdAnimation : MonoBehaviour
{
    [Header("Animation offset")]
    [SerializeField] private float _maxOffset = 1.0f;

    // Start
    // -----
    void Start()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (var animator in animators)
        {
            animator.SetFloat("Offset", Random.Range(0.0f, _maxOffset));
        }
    }
}
