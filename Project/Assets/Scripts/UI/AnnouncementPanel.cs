using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnnouncementPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _panelImage;
    [SerializeField] private TextMeshProUGUI _announcementTitle;
    [SerializeField] private TextMeshProUGUI _announcementDescription;

    public TextMeshProUGUI Title { get { return _announcementTitle; } }
    public TextMeshProUGUI Description { get { return _announcementDescription; } }

    [Header("+ === Settings === +")]
    [Header("Fade")]
    [SerializeField] private float _disappearTimer = 1.0f;
    [SerializeField] private float _disappearSpeed = 3.0f;
    [Header("Move")]
    [SerializeField] private float _moveYSpeed = 20.0f;
    [Header("Size")]
    [SerializeField] private bool _downScale = true;
    [SerializeField] private float _scaleTimer = 1.0f;
    [SerializeField] private float _scaleSpeed = 1.0f;

    private float _disappearElapsedTimer = 1.0f;

    private float _initialPanelAlpha = 1.0f;
    private Color _titleColor;
    private Color _descColor;
    private Color _panelColor;
    private float _scaleLerp = 1.0f;
    private float _initialTitleFontSize;
    private float _initialDescFontSize;

    public event Action<GameObject> AnnouncementShown;
    public bool HasShownAnnouncement { get; private set; }

    private void Awake()
    {
        gameObject.SetActive(false);
        _initialTitleFontSize = _announcementTitle.fontSize;
        _initialDescFontSize = _announcementDescription.fontSize;

        _titleColor = _announcementTitle.color;
        _descColor = _announcementDescription.color;
        _panelColor = _panelImage.color;

        _initialPanelAlpha = _panelColor.a;
        _disappearElapsedTimer = _disappearTimer;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _disappearElapsedTimer = _disappearTimer;
        HasShownAnnouncement = false;

        StartCoroutine(Effects_Coroutine());
    }

    private IEnumerator Effects_Coroutine()
    {
        _titleColor.a = 1.0f;
        _descColor.a = 1.0f;
        _panelColor.a = _initialPanelAlpha;

        _announcementTitle.color = _titleColor;
        _announcementDescription.color = _descColor;
        _panelImage.color = _panelColor;

        while (_titleColor.a > 0)
        {
            yield return null;

            // Move
            transform.position += new Vector3(0, _moveYSpeed) * Time.deltaTime;

            // Timers
            _disappearElapsedTimer -= Time.deltaTime;
            _scaleTimer -= Time.deltaTime;

            // Fade
            if (_disappearElapsedTimer <= 0)
            {
                _titleColor.a -= _disappearSpeed * Time.deltaTime;
                _announcementTitle.color = _titleColor;
                _descColor.a -= _disappearSpeed * Time.deltaTime;
                _announcementDescription.color = _descColor;
                _panelColor.a -= _disappearSpeed * Time.deltaTime;
                _panelImage.color = _panelColor;
            }

            // Scale
            if (_scaleTimer <= 0)
            {
                _scaleLerp += _scaleSpeed * Time.deltaTime * (_downScale ? -1f : 1f);
                _announcementTitle.fontSize = Mathf.Lerp(0, _initialTitleFontSize, _scaleLerp);
                _announcementDescription.fontSize = Mathf.Lerp(0, _initialDescFontSize, _scaleLerp);
            }
        }

        HasShownAnnouncement = true;
        AnnouncementShown?.Invoke(gameObject);
        gameObject.SetActive(false);
    }
}
