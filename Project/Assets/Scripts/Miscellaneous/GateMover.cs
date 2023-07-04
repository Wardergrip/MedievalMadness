using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GateMover : MonoBehaviour
{
    [SerializeField] private GameObject _gate;
    [SerializeField] private float _raisedHeight = 1.8f;
    [SerializeField] private float _totalRaiseTime = 5.9f;
    /*[SerializeField] private float _raiseSpeed = 1f;*/
    [SerializeField] private float _falldownSpeed = 1f;
    private Vector3 _originalPos;
    private Coroutine _activeCoroutine = null;

    //public float TotalRaiseTime
    //{
    //    set
    //    {
    //        _raiseSpeed = _raisedHeight / value;
    //    }
    //}

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(_gate, "Gate not assigned");
        _originalPos = _gate.transform.position;
        Debug.Assert(_raisedHeight > 0f, "Raised height can't be negative");
        //Debug.Assert(_raiseSpeed > 0f, "Raise speed can't be negative");
        Debug.Assert(_totalRaiseTime > 0f, "Total raise time can't be negative");
    }

    public void RaiseGate()
    {
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
        }
        _activeCoroutine = StartCoroutine(RaiseGate_Coroutine());
    }
    public void DropGate()
    {
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
        }
        _activeCoroutine = StartCoroutine(DropGate_Coroutine());
    }

    private IEnumerator RaiseGate_Coroutine()
    {
        Vector3 newPos = _originalPos;
        while (_gate.transform.position.y < _originalPos.y + _raisedHeight)
        {
            newPos.y += /*_raiseSpeed*/(_raisedHeight / _totalRaiseTime) * Time.deltaTime;
            _gate.transform.position = newPos;
            yield return null;
        }
        _gate.transform.position = new Vector3(_originalPos.x, _originalPos.y + _raisedHeight, _originalPos.z);
        _activeCoroutine = null;
    }

    private IEnumerator DropGate_Coroutine()
    {
        Vector3 newPos = _gate.transform.position;
        float totalTime = 0.0f;
        while (_gate.transform.position.y > _originalPos.y)
        {
            totalTime += Time.deltaTime;
            newPos.y -= 0.5f * _falldownSpeed * totalTime * totalTime;
            _gate.transform.position = newPos;
            yield return null;
        }
        _gate.transform.position = _originalPos;
        _activeCoroutine = null;
    }
}
