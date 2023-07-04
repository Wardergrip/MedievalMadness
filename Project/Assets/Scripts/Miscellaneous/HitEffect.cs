using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private float _stayTime = 5.0f;

    static public void Spawn(PlayerPawn player, bool wasSpike)
    {
        // Could've been inside the death effect, but I didn't like having a "death" effect script on a "hit" effect prefab

        if (wasSpike) Instantiate(Resources.Load("Spike_HitEffect_Variant") as GameObject, player.GetPlayerPos(), player.GetPlayerTransform().rotation);
        else          Instantiate(Resources.Load("Weapon_HitEffect_Variant") as GameObject, player.GetPlayerPos(), player.GetPlayerTransform().rotation);
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
