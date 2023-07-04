using UnityEngine;

public class SpeedCapper : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [Tooltip("If the value is under 0, the max is not changed")]
    [SerializeField] private float _maxLinearVelocity;
    [Tooltip("If the value is under 0, the max is not changed")]
    [SerializeField] private float _maxAngularVelocity;
    private void Start()
    {
        if (_maxLinearVelocity >= 0)
        {
            _rigidbody.maxLinearVelocity = _maxLinearVelocity;
        }
        if (_maxAngularVelocity >= 0)
        {
            _rigidbody.maxAngularVelocity = _maxAngularVelocity;
        }
    }
}
