using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    public static PopUp Create(Vector3 position, string text)
    {
        var popUp = Instantiate(Resources.Load("DefaultPopUp") as GameObject, position, Quaternion.identity);
        var popUpScript = popUp.GetComponent<PopUp>();
        popUpScript.Setup(text);

        return popUpScript;
    }

    //

    private TextMeshPro _textMesh;
    private Color _textColor;

    [Header("Fade")]
    [SerializeField] private float _disappearTimer = 1.0f;
    [SerializeField] private float _disappearSpeed = 3.0f;
    [Header("Move")]
    [SerializeField] private float _moveYSpeed = 20.0f;
    [Header("Size")]
    [SerializeField] private bool _downScale = true;
    [SerializeField] private float _scaleTimer = 1.0f;
    [SerializeField] private float _scaleSpeed = 1.0f;
    private float _scaleLerp = 1.0f;
    private float _initialFontSize;


    private void Awake()
    {
        _textMesh = GetComponent<TextMeshPro>();
        _initialFontSize = _textMesh.fontSize;
    }
    public void Setup(string text)
    {
        _textMesh.SetText(text);
        _textColor = _textMesh.color;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void Update()
    {
        // Move
        transform.position += new Vector3(0, _moveYSpeed) * Time.deltaTime;

        // Timers
        _disappearTimer -= Time.deltaTime;
        _scaleTimer -= Time.deltaTime;

        // Fade
        if (_disappearTimer <= 0)
        {
            _textColor.a -= _disappearSpeed * Time.deltaTime;
            _textMesh.color = _textColor;
            if (_textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }

        // Scale
        if (_scaleTimer <= 0)
        {
            _scaleLerp += _scaleSpeed * Time.deltaTime * (_downScale ? -1f : 1f);
            _textMesh.fontSize = Mathf.Lerp(0, _initialFontSize, _scaleLerp);
        }
    }
}
