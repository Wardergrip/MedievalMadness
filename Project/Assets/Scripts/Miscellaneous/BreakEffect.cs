using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakEffect : MonoBehaviour
{
    [SerializeField] private float _stayTime = 5.0f;

    static public void Spawn(Transform transform)
    {
        Instantiate(Resources.Load("WeaponBreakEffect_Variant") as GameObject, transform.position, transform.rotation);
    }

    private void Start()
    {
        StartCoroutine(SelfDestruct_Coroutine());
    }
    private IEnumerator SelfDestruct_Coroutine()
    {
        yield return new WaitForSeconds(_stayTime);
        Destroy(gameObject);
    }
}
