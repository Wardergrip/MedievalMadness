using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    [Tooltip("If no player is assigned to this panel, this panel will hide itself")]
    [SerializeField] private bool _hideUnused = true;
    [Header("Object references")]
    [SerializeField] private GameObject _panelObj;
    private Image _panelImage;
    [SerializeField] private GameObject _iconObj;
    private Image _iconImage;
    private HelmetShake _helmetShake;
    [SerializeField] private GameObject _fireForeground;
    [SerializeField] private GameObject _fireBackground;
    private float _fireWaitTime = 0.5f;
    private float _fireTimer;
    private Coroutine _fireCoroutine = null;
    public HelmetShake HelmetShake { get { return _helmetShake; } }
    [SerializeField] private GameObject _healthObj;
    private TextMeshProUGUI _healthText;
    [SerializeField] private GameObject _LifeObj;
    private Image _lifeImage;
    private List<GameObject> _lifeObjList = new List<GameObject>();
    [SerializeField] private GameObject _scoreTextObj;
    private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _scoreNumberObj;
    private TextMeshProUGUI _scoreNumberText;

    [Header("Target")]
    [SerializeField] private int _targetWidth = 1920;

    [Header("Font settings")]
    [SerializeField] private int _bigFontSize = 120;
    [SerializeField] private int _smallFontSize = 90;
    [Tooltip("The health color gets changed relatively to this number")]
    [SerializeField] private int _maxPercentage = 500;

    private Color _originalIconColor;

    [Header("Watch values")]
    [SerializeField] private short _assignedPlayerId = short.MinValue;

    // Start is called before the first frame update
    void Start()
    {
        // Assert correct references
        Debug.Assert(_panelObj, $"Panel object not referenced on {gameObject.name}");
        _panelImage = _panelObj.GetComponent<Image>();
        Debug.Assert(_panelImage, $"No image on panel {_panelImage.gameObject.name} found. Error on {gameObject.name}");

        Debug.Assert(_iconObj, $"Icon object not referenced on {gameObject.name}");
        _iconImage = _iconObj.GetComponent<Image>();
        Debug.Assert(_iconImage, $"No image on panel {_iconImage.gameObject.name} found. Error on {gameObject.name}");
        _originalIconColor = _iconImage.color;
        _helmetShake = _iconObj.GetComponent<HelmetShake>();
        Debug.Assert(_helmetShake, $"No helmetshake script found on {_iconImage.gameObject.name}. Error on {gameObject.name}");

        Debug.Assert(_healthObj, $"Health object not referenced on {gameObject.name}");
        _healthText = _healthObj.GetComponent<TextMeshProUGUI>();
        Debug.Assert(_healthText, $"No TMProUGUI on panel {_healthObj.gameObject.name} found. Error on {gameObject.name}");

        Debug.Assert(_LifeObj, $"Life object not referenced on {gameObject.name}");
        _lifeImage = _LifeObj.GetComponent<Image>();
        Debug.Assert(_lifeImage, $"No image on {_LifeObj.name}");

        Debug.Assert(_scoreTextObj, $"ScoreText object not referenced on {gameObject.name}");
        _scoreText = _scoreTextObj.GetComponent<TextMeshProUGUI>();
        Debug.Assert(_scoreText, $"No TMProUGUI not referenced on {_scoreTextObj.gameObject.name}");
            
        Debug.Assert(_scoreNumberObj, $"ScoreText object not referenced on {gameObject.name}");
        _scoreNumberText = _scoreNumberObj.GetComponent<TextMeshProUGUI>();
        Debug.Assert(_scoreNumberText, $"No TMProUGUI not referenced on {_scoreNumberObj.gameObject.name}");

        Debug.Assert(_fireForeground, $"Fireforeground is not referenced on {gameObject.name}");
        Debug.Assert(_fireBackground, $"Firebackground is not referenced on {gameObject.name}");
        SetFireVisibility(false);

        StartCoroutine(SubscribeManager_Coroutine());

        _panelObj.SetActive(!_hideUnused);
    }

    private IEnumerator SubscribeManager_Coroutine()
    {
        var gameSys = GameSystem.Instance;
        while (gameSys.ColorManager == null)
        {
            yield return null;
        }
        gameSys.ColorManager.ColorChangeEvent += UpdateColor;
    }

    public void AssignPlayer(short? playerId)
    {
        if (playerId == null)
        {
            if (_assignedPlayerId != short.MinValue)
            {
                // Unsubscribe
            }
            _assignedPlayerId = short.MinValue;
            _panelObj.SetActive(false);
            return;
        }
        Debug.Assert(playerId.Value < 4 && playerId.Value > -1, "Player ID is in an unexpected range");
        _assignedPlayerId = playerId.Value;
    }

    public bool IsPlayerAssigned { get { return _assignedPlayerId != short.MinValue;} }
    public short AssignedPlayerId { get { return _assignedPlayerId; } }

    private void UpdateColor(PlayerPawn playerPawn, ColorManager.PlayerColors colorID)
    {
        if (playerPawn.PlayerController.PlayerID != _assignedPlayerId) return;

        _iconImage.sprite = GameSystem.Instance.UIManager.GetIcon(colorID);
        _lifeImage.sprite = GameSystem.Instance.UIManager.GetHeart(colorID);
        _panelImage.color = GameSystem.Instance.UIManager.GetPanelColor(colorID);

        foreach (var life in _lifeObjList)
        {
            life.GetComponent<Image>().sprite = _lifeImage.sprite;
        }
    }

    public void Show() => _panelObj.SetActive(true);

    public void Hide() => _panelObj.SetActive(false);

    public void SetGreyOut(bool state)
    {
        _panelImage.color = state ? Color.grey : GameSystem.Instance.UIManager.GetPanelColor(GameSystem.Instance.PlayerManager.GetPlayer(AssignedPlayerId).PawnColor);
        _iconImage.color = state ? Color.grey : _originalIconColor;
    }

    public void SetScoreText(string scoreText)
    {
        _scoreText.text = scoreText;
    }

    public void SetScoreNumberText(string scoreNumberText) 
    {
        _scoreNumberText.text = scoreNumberText;
    }

    public void SetHealthText(string text, bool addPercent = true)
    {
        // Set text
        // --------
        _healthText.text = text + (addPercent ? "%" : "");

        // Change color accordingly
        // ------------------------
        if (text != "dead")
        {
            Color darkRed = new Color
            {
                r = 0.5f,
                g = 0.0f,
                b = 0.0f,
            };

            // Get relativePercentage
            int currentPercentage = int.Parse(text);
            if (currentPercentage > _maxPercentage) currentPercentage = _maxPercentage;
            float relativePercentage = (float)currentPercentage / _maxPercentage;

            // Lerp between colors
            Color newColor = Color.white;
            newColor.r = Mathf.Lerp(1.0f, darkRed.r, relativePercentage);
            newColor.g = Mathf.Lerp(1.0f, darkRed.g, relativePercentage);
            newColor.b = Mathf.Lerp(1.0f, darkRed.b, relativePercentage);

            // Set color
            _healthText.color = newColor;
        }
    }

    public void SetHealthTextSizeBig()
    {
        _healthText.fontSize = _bigFontSize;
    }

    public void SetHealthTextSizeSmall() 
    {
        _healthText.fontSize = _smallFontSize;
    }

    public void SetHealthTextFontSize(int size)
    {
        _healthText.fontSize = size;
    }

    public void SetAmountOfLives(int amountOfLives) 
    {
        if (amountOfLives == _lifeObjList.Count) return;

        foreach (var lifeObj in _lifeObjList) 
        { 
            Destroy(lifeObj); 
        }
        _lifeObjList.Clear();
        if (amountOfLives <= 0) 
        { 
            return; 
        }


        // What happens is that objects get moved to adjusts to the resolution
        // This means that the space between a heart looks different between resolutions because
        // we kind of "hard code" the space between. This space between is not scaled by the resolution
        // So we do it manually by calculating the ratio between the target and current width we are
        // trying to achieve
        float targetWidth = (float)_targetWidth;
        float currentWidth = Screen.width;
        float resAdjust = currentWidth / targetWidth;

        _LifeObj.SetActive(true);
        RectTransform lifeRect = _LifeObj.GetComponent<RectTransform>();
        Debug.Assert(lifeRect, "Liferect is null. Are you sure that life obj is an UI object?");
        float objWidth = lifeRect.rect.width * resAdjust;
        
        for (int i = 0; i < amountOfLives; ++i) 
        {
            GameObject obj = Instantiate(_LifeObj, _panelObj.transform);
            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.position = lifeRect.position + new Vector3((-amountOfLives / 2 * objWidth) + i * objWidth + (amountOfLives % 2 == 0 ? objWidth / 2.0f : 0), 0, 0);
            _lifeObjList.Add(obj);
        }
        _LifeObj.SetActive(false);
    }

    public void SetFireVisibility(bool value) 
    {
        _fireForeground.SetActive(value);
        _fireBackground.SetActive(value);
        if (value)
        {
            _fireTimer = _fireWaitTime;
            if (_fireCoroutine == null)
            {
                _fireCoroutine = StartCoroutine(HideFireVisibility_Coroutine());
            }
        }
    }

    private IEnumerator HideFireVisibility_Coroutine()
    {
        while (_fireTimer > 0)
        {
            _fireTimer -= Time.deltaTime;
            yield return null;
        }
        SetFireVisibility(false);
        _fireCoroutine = null;
    }

    private void OnDestroy()
    {
        GameSystem gameSystem = GameSystem.Instance;
        if (gameSystem)
        {
            ColorManager colorManager = gameSystem.ColorManager;
            if (colorManager) colorManager.ColorChangeEvent -= UpdateColor;
        }
        if (_fireCoroutine != null) StopCoroutine(_fireCoroutine);
    }
}
