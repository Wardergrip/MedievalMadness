using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyOutOfBoundsChecker : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("The sphere collider should be turned off and be a trigger to make sure it does not cause problems. It is used to visualise the range. The radius of the sphere collider will be used to check with if players are too far  from this object.")]
    [SerializeField] private SphereCollider _rangeIndicator;

    [Header("Vars")]
    [SerializeField] private float _checkDelay = 5.0f;

    private Coroutine _checkCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        _checkCoroutine = StartCoroutine(Check_Coroutine());
    }

    private IEnumerator Check_Coroutine()
    {
        List<PlayerController> players = null;
        while (true)
        {
            yield return new WaitForSeconds(_checkDelay);
            players = GameSystem.Instance.PlayerManager.Players;
            foreach (PlayerController player in players) 
            {
                if (player == null) continue;
                if (player.PlayerPawn == null) continue;
                if (player.PlayerPawn.SmashHealth == null) continue;
                if ((player.PlayerPawn.GetPlayerPos() - transform.position).sqrMagnitude >= (_rangeIndicator.radius * _rangeIndicator.radius))
                {
                    player.PlayerPawn.SmashHealth.Kill(false);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (_checkCoroutine != null) StopCoroutine(_checkCoroutine);
    }
}
