using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeathEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _stayTime = 5.0f;
    [Tooltip("How long the aberration will stay for. MAKE SURE THAT THIS IS SMALLER THEN THE STAYTIME")]
    [SerializeField] private float _chromaticAberrationTime = 3.0f;
    [SerializeField] private float _chromaticAberrationStrength = 10.0f;

    // Post processing
    private Volume _postProcessVolume;
    private ChromaticAberration _chromaticAberration;

    private bool _goBack = false;
    private bool _finished = false;

    static public void Spawn(PlayerController player)
    {
        // Blood
        Instantiate(Resources.Load("DeathEffects") as GameObject, player.PlayerPawn.GetPlayerPos(),player.PlayerPawn.GetPlayerTransform().rotation);

        // Death ray
        GameObject deathRay = Instantiate(Resources.Load("DeathRay_Variant") as GameObject, player.PlayerPawn.GetPlayerPos(), Quaternion.identity);
        deathRay.transform.localScale = new Vector3( 10, 10, 10 );
    }

    private void Start()
    {
        // Get volume
        Camera camera = Camera.main;
        _postProcessVolume = camera.GetComponentInChildren<Volume>();
        if (_postProcessVolume)
        {
            // Get aberration
            _postProcessVolume.profile.TryGet(out _chromaticAberration);
            Debug.Assert(_chromaticAberration != null, "Error: no chromatic abberation found on postProcessingVolume");
        }

        // Start self destruct
        StartCoroutine(SelfDestruct_Coroutine());
    }
    private void Update()
    {
        if (_postProcessVolume == null) return;
        if (_chromaticAberration == null) return;
        if (_finished) return;

        // Calculate chromatic aberration
        float changeSpeed = (_chromaticAberrationStrength * 2) / _chromaticAberrationTime;
        if (_goBack) changeSpeed *= -1;

        float currentValue = _chromaticAberration.intensity.value;
        float newValue = currentValue + changeSpeed * Time.deltaTime;

        // Clamp value
        if (_goBack && newValue <= 0)
        {
            newValue = 0;
            _finished = true;
        }
        else if (_chromaticAberrationStrength <= newValue)
        {
            newValue = _chromaticAberrationStrength;
            _goBack = true;
        }

        // Set new value
        _chromaticAberration.intensity.Override(newValue);
    }

    private IEnumerator SelfDestruct_Coroutine()
    {
        yield return new WaitForSeconds(_stayTime);

        // To be sure, set aberration back to 0
        if (_chromaticAberration != null)
        {
            _chromaticAberration.intensity.Override(0);
        }

        // Destroy
        Destroy(gameObject);
    }
}
