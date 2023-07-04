using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpawnEffect : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private VisualEffect _effect;

    [Header("Settings")]
    [SerializeField] private float _stayTime = 5.0f;

    static private PlayerController _player;

    static public void Spawn(PlayerController player)
    {
        // SpawnEffect
        _player = player;
        Instantiate(Resources.Load("SpawnEffect_Variant") as GameObject, player.PlayerPawn.GetPlayerPos(), player.PlayerPawn.GetPlayerTransform().rotation);
    }

    // Start
    // -----
    private void Start()
    {
        // Change color according to player
        string colorPropertyName = "Color";
        Vector4 color = Color.white;

        switch (_player.PawnColor)
        {
            case ColorManager.PlayerColors.Red:
                color = new Vector4(148 / 255f, 35 / 255f, 44 / 255f, 1);
                break;

            case ColorManager.PlayerColors.Purple:
                color = new Vector4(37 / 255f, 12 / 255f, 99 / 255f, 1);
                break;

            case ColorManager.PlayerColors.Green:
                color = new Vector4(12 / 255f, 99 / 255f, 27 / 255f, 1);
                break;

            case ColorManager.PlayerColors.Yellow:
                color = new Vector4(166 / 255f, 189 / 255f, 23 / 255f, 1);
                break;
        }

        _effect.SetVector4(colorPropertyName, color);

        // Self destruct
        StartCoroutine(SelfDestruct_Coroutine());
    }
    private IEnumerator SelfDestruct_Coroutine()
    {
        yield return new WaitForSeconds(_stayTime);
        Destroy(gameObject);
    }
}
