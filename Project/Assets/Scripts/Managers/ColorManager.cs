using System;
using System.Collections;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    [Header("PlayerColors")]
    [Header("Red")]
    [SerializeField] private Material _firstPlayerBody;
    [SerializeField] private Material _firstPlayerFeather;
    [SerializeField] private Material _firstPlayerTransparantBody;
    [SerializeField] private Material _firstPlayerTransparantFeather;

    [Header("Blue")]
    [SerializeField] private Material _secondPlayerBody;
    [SerializeField] private Material _secondPlayerFeather;
    [SerializeField] private Material _secondPlayerTransparantBody;
    [SerializeField] private Material _secondPlayerTransparantFeather;

    [Header("Green")]
    [SerializeField] private Material _thirdPlayerBody;
    [SerializeField] private Material _thirdPlayerFeather;
    [SerializeField] private Material _thirdPlayerTransparantBody;
    [SerializeField] private Material _thirdPlayerTransparantFeather;

    [Header("Yellow")]
    [SerializeField] private Material _fourthPlayerBody;
    [SerializeField] private Material _fourthPlayerFeather;
    [SerializeField] private Material _fourthPlayerTransparantBody;
    [SerializeField] private Material _fourthPlayerTransparantFeather;

    // Enum
    public enum PlayerColors
    {
        Red,
        Purple,
        Green,
        Yellow,
        NR_COLORS
    }

    // Colors
    private bool[] _availableColors;
    public event Action<PlayerPawn, PlayerColors> ColorChangeEvent;

    // Start
    // -----
    void Start()
    {
        StartCoroutine(SubscribeManagers_Coroutine());

        // Set available colors
        _availableColors = new bool[(int) PlayerColors.NR_COLORS];

        _availableColors[(int) PlayerColors.Red] = true;
        _availableColors[(int) PlayerColors.Purple] = true;
        _availableColors[(int) PlayerColors.Green] = true;
        _availableColors[(int) PlayerColors.Yellow] = true;
    }

    private IEnumerator SubscribeManagers_Coroutine()
    {
        var gameSys = GameSystem.Instance;
        while (gameSys.PlayerManager == null)
        {
            yield return null;
        }
        gameSys.PlayerManager.AddPlayerEvent += AddPlayerEvent;
        gameSys.PlayerManager.RemovePlayerEvent += RemovePlayerEvent;
    }

    // Change Pawn Color
    // -----------------
    public void SetPawnColor(PlayerPawn playerPawn, PlayerColors colorID)
    {
        switch (colorID)
        {
            case PlayerColors.Red:
                playerPawn.SetBodyMaterial(_firstPlayerBody);
                playerPawn.SetFeatherMaterial(_firstPlayerFeather);
                playerPawn.SetTransparantBodyMaterial(_firstPlayerTransparantBody);
                playerPawn.SetTransparantFeatherMaterial(_firstPlayerTransparantFeather);
                break;

            case PlayerColors.Purple:
                playerPawn.SetBodyMaterial(_secondPlayerBody);
                playerPawn.SetFeatherMaterial(_secondPlayerFeather);
                playerPawn.SetTransparantBodyMaterial(_secondPlayerTransparantBody);
                playerPawn.SetTransparantFeatherMaterial(_secondPlayerTransparantFeather);
                break;

            case PlayerColors.Green:
                playerPawn.SetBodyMaterial(_thirdPlayerBody);
                playerPawn.SetFeatherMaterial(_thirdPlayerFeather);
                playerPawn.SetTransparantBodyMaterial(_thirdPlayerTransparantBody);
                playerPawn.SetTransparantFeatherMaterial(_thirdPlayerTransparantFeather);
                break;

            case PlayerColors.Yellow:
                playerPawn.SetBodyMaterial(_fourthPlayerBody);
                playerPawn.SetFeatherMaterial(_fourthPlayerFeather);
                playerPawn.SetTransparantBodyMaterial(_fourthPlayerTransparantBody);
                playerPawn.SetTransparantFeatherMaterial(_fourthPlayerTransparantFeather);
                break;
        }

        ColorChangeEvent?.Invoke(playerPawn, colorID);
    }
    public void SetAvailablePawnColor(PlayerPawn playerPawn, PlayerColors colordID)
    {
        int colordIdx = (int) colordID;

        // Loop through colors
        for (int idx = 0; idx < (int) PlayerColors.NR_COLORS; ++idx)
        {
            if (_availableColors[colordIdx])
            {
                // Set material
                SetPawnColor(playerPawn, (PlayerColors) colordIdx);

                // Change available colors
                _availableColors[colordIdx] = false;
                _availableColors[(int) playerPawn.PawnColor] = true;

                playerPawn.PawnColor = (PlayerColors) colordIdx;
                playerPawn.PlayerController.PawnColor = (PlayerColors) colordIdx;

                return;
            }
            else
            {
                ++colordIdx;
                if ((int) PlayerColors.NR_COLORS <= colordIdx) colordIdx = 0;
            }
        }
    }

    // Player event
    // ------------
    private void AddPlayerEvent(PlayerController obj)
    {
        int colorID = obj.PlayerID;

        // Loop through colors
        for (int idx = 0; idx < (int) PlayerColors.NR_COLORS; ++idx)
        {
            // If available, use that color
            if (_availableColors[colorID])
            {
                // Change available colors
                _availableColors[colorID] = false;
                obj.PawnColor = (PlayerColors) colorID;

                return;
            }
            // Else, go to next color
            else
            {
                ++colorID;
                if ((int) PlayerColors.NR_COLORS <= colorID) colorID = 0;
            }
        }
    }
    private void RemovePlayerEvent(PlayerController obj)
    {
        _availableColors[(int) obj.PawnColor] = true;
    }
}
