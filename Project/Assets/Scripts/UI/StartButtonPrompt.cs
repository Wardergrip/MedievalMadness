using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButtonPrompt : MonoBehaviour
{
    private void Start()
    {
        if (GameSystem.Instance.PlayerManager.Players.Count > 0)
        {
            Destroy(gameObject);
        }
        GameSystem.Instance.PlayerManager.AddPlayerEvent += AddPlayerEvent;
    }

    private void AddPlayerEvent(PlayerController controller)
    {
        GameSystem.Instance.PlayerManager.AddPlayerEvent -= AddPlayerEvent;
        Destroy(gameObject);
    }
}
