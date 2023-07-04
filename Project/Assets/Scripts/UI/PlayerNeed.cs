using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNeed : MonoBehaviour
{
    [SerializeField] private GameObject _playerNecessity;
    private TextMeshPro _TextComponent;
    [SerializeField] private Color _insufficientColor = Color.red;
    [SerializeField] private Color _sufficientColor = Color.green;

    private int _currentNrPlayers = 0;
    public int CurrentNrPlayers
    {
        set { _currentNrPlayers = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert( _playerNecessity != null, "PlayerNeed needs _playerNecessity to show nr needed player" );
        _TextComponent = _playerNecessity.GetComponent<TextMeshPro>();

        GameSystem.Instance.PlayerManager.AddPlayerEvent += OnPlayerAdd;
        GameSystem.Instance.PlayerManager.RemovePlayerEvent += OnPlayerRemove;

        UpdateText();
    }

    private void OnPlayerAdd(PlayerController controller) { UpdateText(); }
    private void OnPlayerRemove(PlayerController controller) { UpdateText(); }

    public void UpdateText()
    {
        _TextComponent.SetText(_currentNrPlayers + "/" + GameSystem.Instance.PlayerManager.GetNrPlayers());
        UpdateColor();
    }
    private void UpdateColor()
    {
        if (_currentNrPlayers >= GameSystem.Instance.PlayerManager.GetNrPlayers() / 2 && _currentNrPlayers != 0)
        {
            _TextComponent.color = _sufficientColor;
        }
        else
        {
            _TextComponent.color = _insufficientColor;
        }
    }
}
