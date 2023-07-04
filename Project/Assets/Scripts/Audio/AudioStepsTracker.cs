using UnityEngine;
using UnityEngine.Events;

public class AudioStepsTracker : MonoBehaviour
{
    [SerializeField] private float _triggerDistance = 1.0f;

    public UnityEvent DistanceReachedEvent;
    public UnityEvent DistanceReachedWaterEvent;

    [Header("Watch Values")]
    [SerializeField] private float _calculatedVal;
    [SerializeField] private Vector3 _oldPos;

    private int _isInWaterCounter = 0;

    private void Start()
    {
        _oldPos = transform.position;
        var playerPawn = GetComponentInParent<PlayerPawn>();
        Debug.Assert(playerPawn, "Did not find playerpawn script in parent");
        playerPawn.AddedWaterSlowEvent += AddWaterSlowEvent;
        playerPawn.RemoveWaterSlowEvent += RemoveWaterSlowEvent;
    }

    void FixedUpdate()
    {
        _calculatedVal = (_oldPos - transform.position).sqrMagnitude;
        if (_calculatedVal > _triggerDistance)
        {
            _oldPos = transform.position;

            if (_isInWaterCounter > 0)     DistanceReachedWaterEvent?.Invoke();
            else                DistanceReachedEvent?.Invoke();
        }
    }

    private void AddWaterSlowEvent(PlayerController player)
    {
        ++_isInWaterCounter;
    }

    private void RemoveWaterSlowEvent(PlayerController player)
    {
        --_isInWaterCounter;
    }
}
