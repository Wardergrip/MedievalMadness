using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayInMiddle : MonoBehaviour
{
    [SerializeField] private List<Transform> _objects = new List<Transform>();
    private Vector3 _position;

    private void FixedUpdate()
    {
        _position = Vector3.zero;
        foreach (var obj in _objects)
        {
            _position += obj.transform.position;
        }

        // This is to prevent dividing by 0 or 1 which is either impossible or useless.
        if (_objects.Count > 1)
        {
            _position /= _objects.Count;
        }

        transform.position = _position;
    }

    public void AddObject(Transform obj)
    {
        if (obj == null) return;
        _objects.Add(obj);
    }

    public void RemoveObject(Transform obj) 
    {
        if (obj == null) return;
        _objects.Remove(obj);
    }
}
