using UnityEngine;
using UnityEngine.Events;

public class StartEvent : MonoBehaviour
{
    public UnityEvent StartUnityEvent;
    void Start()
    {
        StartUnityEvent?.Invoke();
    }
}
