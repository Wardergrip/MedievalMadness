using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// LayerFromLayerMask
// https://answers.unity.com/questions/760533/best-way-to-set-a-gameobject-layer-programmaticall.html
public class LayermaskHelpers
{
    public static int GetHighestLayerFromMask(LayerMask aMask)
    {
        uint val = (uint)aMask.value;
        if (val == 0)
            return -1;
        int layer = 0;
        if (val > 0xFFFF) // XXXX XXXX XXXX XXXX 0000 0000 0000 0000
        {
            layer += 16;
            val >>= 16;
        }
        if (val > 0xFF) // XXXX XXXX 0000 0000
        {
            layer += 8;
            val >>= 8;
        }
        if (val > 0xF) // XXXX 0000
        {
            layer += 4;
            val >>= 4;
        }
        if (val > 0x3) // XX00
        {
            layer += 2;
            val >>= 2;
        }
        if ((val & 0x2) != 0) // X0
            layer += 1;
        return layer;
    }

    public static int GetLowestLayerFromMask(LayerMask aMask)
    {
        uint val = (uint)aMask.value;
        if (val == 0)
            return -1;
        for (int i = 0; i < 32; i++)
        {
            if ((val & (1 << i)) != 0)
                return i;
        }
        return -1;
    }
}
//

[RequireComponent(typeof(BoxCollider))]
public class SpawnArea : MonoBehaviour
{
    private bool _isGettingDestroyed = false;

    [Header("Timing")]
    [SerializeField] private float _initialDelay = 1f;
    [SerializeField] private float _delay = 5f;
    [Tooltip("How much the delay can varry (Maximum)")]
    [SerializeField] private float _delayVarriance = 1f;

    [Header("Amount")]
    [Tooltip("The amount of weapons that spawn initially. This by passes capacity check.")]
    [SerializeField] private uint _initialAmount = 0;
    [Tooltip("The maximum amount of total weapons on the ground is a static variable.")]
    [SerializeField] private uint _maxHere = 10;
    private uint _totalHere = 0;
    private static uint _total = 0;

    [Header("Spawning")]
    [Tooltip("Amount of attempts to spawn an object that doesn't overlap. If this number is high, it may introduce lag.")]
    [SerializeField] private uint _nonOverlapAttempts = 20;
    [SerializeField] private bool _noSpaceIsError = false;
    [SerializeField] private float _minInbetweenDistance = 1;
    [Tooltip("This is the layer that will be checked to make sure objects don't overlap")]
    [SerializeField] private LayerMask _nonOverlapLayer;
    [Tooltip("When true, use the lowest layer in the layermask to assign to spawned objects. If false, use highest layer.")]
    [SerializeField] private bool _assignLowestLayer = true;
    private BoxCollider _spawnBox;
    [SerializeField] private bool _encapsulateInPickUp = true;

    [Header("Objects")]
    [SerializeField] private SpawnList _spawnableObjects;

    public SpawnList SpawnableObjects
    {
        get { return _spawnableObjects; }
        set { _spawnableObjects = value; }
    }

    private bool AtCapacity { get { return !((_total < SettingsManager.Instance.SpawnAreaSettings.totalMaxWeaponsOnGround) && (_totalHere < _maxHere)); } }

    // Start is called before the first frame update
    void Start()
    {
        _spawnBox = GetComponent<BoxCollider>();
        Debug.Assert(_spawnBox.isTrigger, "Box collider is not a trigger");
        foreach (var obj in _spawnableObjects.spawnableObjects)
        {
            Debug.Assert(obj.Weight > 0, $"Weight of one (or more) of the spawnable objects is 0 or lower on {gameObject.name}");
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(_initialDelay);
        while (_isGettingDestroyed == false)
        {
            // Are we at capacity?
            if (!AtCapacity)
            {
                TrySpawnInBox();
            }
            yield return new WaitForSeconds(Mathf.Lerp(_delay - _delayVarriance, _delay + _delayVarriance, UnityEngine.Random.value));
        }
    }

    public void SpawnInitialWeapons()
    {
        for (int i = 0; i < _initialAmount; ++i)
        {
            TrySpawnInBox();
        }
    }

    private void TrySpawnInBox()
    {
        // Are there objects to spawn?
        if (_spawnableObjects.spawnableObjects.Length > 0)
        {
            // GetAvailablePosInBox throws exception if no space is found
            try
            {
                Spawn(_spawnableObjects.NextSpawnableObject(), GetAvailablePosInBox(), HelperFuncts.GetRandomYOrientation());
            }
            catch (ExhaustedSpawnBoxException ex)
            {
                if (_noSpaceIsError)
                {
                    Debug.LogWarning(ex);
                    Debug.Log($"SpawnAttempts might be too low ({_nonOverlapAttempts}), totalMax might be too high ({_maxHere}, {SettingsManager.Instance.SpawnAreaSettings.totalMaxWeaponsOnGround}), size might be too small ({_minInbetweenDistance}) or layer might be incorrect ({_nonOverlapLayer}).");
                }
            }
        }
    }

    private Vector3 GetAvailablePosInBox()
    {
        Collider[] colls = new Collider[1];
        Vector3 pos;

        uint attempts = 0;
        int collAmount = 0;
        do
        {
            pos = _spawnBox.GetRandomPositionInBox();
            if (attempts >= _nonOverlapAttempts)
            {
                throw new ExhaustedSpawnBoxException();
            }

            collAmount = Physics.OverlapBoxNonAlloc(pos, new Vector3(_minInbetweenDistance, _minInbetweenDistance, _minInbetweenDistance), colls, Quaternion.identity, _nonOverlapLayer);
            if (collAmount == 0)
            {
                return pos;
            }
            attempts++;
        } while (collAmount != 0);

        return pos;
    }

    private void Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject spawnedObject = _encapsulateInPickUp ? PickUp.SpawnPickUp(position, rotation, prefab) : Instantiate(prefab, position, rotation);
        spawnedObject.AddComponent<SpawnAreaTracker>().Creator = this;
        if (_assignLowestLayer)
        {
            spawnedObject.layer = LayermaskHelpers.GetLowestLayerFromMask(_nonOverlapLayer);
        }
        else
        {
            spawnedObject.layer = LayermaskHelpers.GetHighestLayerFromMask(_nonOverlapLayer); 
        }
        ++_totalHere;
        ++_total;
    }

    private void OnDestroy()
    {
        _isGettingDestroyed = true;
    }

    // Removal handling
    private void NotifyDestroy()
    {
        --_total;
        --_totalHere;
    }

    private class SpawnAreaTracker : MonoBehaviour
    {
        public SpawnArea Creator { get; set; }

        private void OnDestroy()
        {
            Creator.NotifyDestroy();
        }
    }

    private class ExhaustedSpawnBoxException : Exception
    {
        public ExhaustedSpawnBoxException() { }

        public ExhaustedSpawnBoxException(string message)
            : base(message) { }

        public ExhaustedSpawnBoxException(string message, Exception inner)
            : base(message, inner) { }
    }
}