using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image _loadingScreenBar;
    [SerializeField] private TextMeshProUGUI _tipText;
    [SerializeField] private bool _newLineAfterPreface;
    [SerializeField] private string _prefaceText;
    [SerializeField] private List<string> _tipTexts;

    public Image LoadingScreenBar {  get { return _loadingScreenBar; } }

    private void Start()
    {
        Debug.Assert(_loadingScreenBar, "No loading screen bar assigned");
        if (_tipText)
        {
            _tipText.text = string.IsNullOrEmpty(_prefaceText) ? "" : _prefaceText;
            if (_newLineAfterPreface) _tipText.text += "\n";
            _tipText.text += _tipTexts[UnityEngine.Random.Range(0,_tipTexts.Count - 1)];
        }
        else if (_tipTexts.Count > 0) 
        {
            Debug.LogWarning("No tipText assigned but tiptexts are available.");
        }
    }
}
