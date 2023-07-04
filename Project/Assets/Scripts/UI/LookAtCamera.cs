using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [Tooltip("If no object is assigned, it will look for an object that has a camera.")]
    [SerializeField] private GameObject _cameraObject;
    private Camera _camera;
    [SerializeField] private bool _lockZRotation = false;
    [SerializeField] private bool _lockXRotation = true;
    [SerializeField] private bool _invertText = false;

    private Vector3 _lookPos = new Vector3();
    private bool _isInitialized = false;

    // Start is called before the first frame update
    void Start()
    {
        if (_cameraObject == null)
        {
            _camera = FindAnyObjectByType<Camera>();
            _cameraObject = _camera.gameObject;
        }
        else
        {
            _camera = _cameraObject.GetComponent<Camera>();
        }

        Debug.Assert(_camera, "No camera found!");
        if (_camera) _isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isInitialized == false) return;
        if (_cameraObject == null) return;

        _lookPos.y = _cameraObject.transform.position.y;
        _lookPos.x = _cameraObject.transform.position.x;
        _lookPos.z = _cameraObject.transform.position.z;
        if (_lockZRotation)
        {
            _lookPos.z = transform.position.z;
        }
        if (_lockXRotation)
        {
            _lookPos.x = transform.position.x;
        }

        transform.LookAt(_lookPos);
        if (_invertText) transform.Rotate(Vector3.up, 180.0f);
    }
}
