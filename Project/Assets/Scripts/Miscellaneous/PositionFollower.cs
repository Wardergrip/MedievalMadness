using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionFollower : MonoBehaviour
{
    [SerializeField] private Transform _transformToFollow;

    public Transform TransformToFollow { get { return _transformToFollow; } set { _transformToFollow = value; } }

    private void Update()
    {
        transform.position = _transformToFollow.position;
    }
}
