using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    [SerializeField] private float _timer = 1.0f;
    public float Timer { get { return _timer; } set {  _timer = value; } }
    void Start()
    {
        StartCoroutine(Destroy_Coroutine());
    }

    IEnumerator Destroy_Coroutine()
    {
        yield return new WaitForSeconds(_timer);
        Destroy(gameObject);
    }
}
