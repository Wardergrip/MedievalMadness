using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteSpin : MonoBehaviour
{
    [SerializeField] private float _speed = 1f;
    private Vector3 _calcEulers;

    private void Update()
    {
        _calcEulers = transform.rotation.eulerAngles;
        _calcEulers.y += _speed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(_calcEulers);
    }
}
