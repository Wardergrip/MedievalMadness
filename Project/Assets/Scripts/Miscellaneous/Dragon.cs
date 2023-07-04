using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Dragon : MonoBehaviour
{
    [Header("Dragon spawn settings")]
    [SerializeField] private GameObject _dragonMovingObject;
    [SerializeField] private GameObject _dragonVisuals;
    [SerializeField] private GameObject _fireBreathObject;

    [Header("Events")]
    public UnityEvent OnFireBreathEvent;

    [Header("View variables (do not change these variables)")]
    [Tooltip("Current time before the dragon spawn")]
    [SerializeField] private float _currentDragonTimer = 0.0f;
    [Tooltip("How many times the dragon has flown over the map")]
    [SerializeField]private int _nrTimesFlown = 0;
    [Tooltip("How frequent the dragon will spit out fire")] 
    [SerializeField] private float _fireSpitFrequency;

    // Event start
    // -----------
    private bool _activateDragon = false;
    public bool ActivateDragon
    {
        get { return _activateDragon; }
        set { _activateDragon = value; }
    }

    private bool _eventStarted = false;
    public bool EventStarted
    {
        get { return _eventStarted; }
    }

    // Dragon spawn
    // ------------
    private Vector3 _dragonStartPos;
    private Vector3 _dragonEndPos;

    // Dragon flight
    // -------------
    private bool _isInWarningFlight = true;
    private bool _scaleSizeWithSteps = false;
    private bool _scaleFligthFrequencyWithSteps = false;
    private float _deadPlayerTimerMultiplier = 0.66f;

    // Start
    // -----
    void Start()
    {
        Debug.Assert(_dragonMovingObject != null, "Dragon needs a dragonGameobject for it to work");
        Debug.Assert(_fireBreathObject != null, "Dragon needs a fireBreathObject to be able to breath fire");
        _dragonVisuals.SetActive(false);
        _fireBreathObject.SetActive(false);

        _fireSpitFrequency = SettingsManager.Instance.DragonSettings.MinSpewingFrequency;
        _scaleSizeWithSteps = SettingsManager.Instance.DragonSettings.ScaleSizeWithSteps;


        SetNewEventTimer();
        StartCoroutine(ActivateDragonCoroutine());
    }
    private IEnumerator ActivateDragonCoroutine()
    {
        yield return new WaitForSeconds(SettingsManager.Instance.DragonSettings.StartUpTimer);
        _activateDragon = true;
    }

    // Update
    // ------
    void Update()
    {
        CheckGameState();
        DepleteEventTimer();
        MoveDragon();
    }

    private void CheckGameState()
    {
        if (_scaleSizeWithSteps) return;
        if (_scaleFligthFrequencyWithSteps) return;

        // If more then 2 players
        var players = GameSystem.Instance.PlayerManager.GetPlayers();
        if (2 < players.Count)
        {
            // Check if someone died
            bool someoneHasDied = false;
            foreach (var player in players)
            {
                if (player.Lives.CanRespawn == false)
                {
                    someoneHasDied = true;
                    break;
                }
            }

            // If so
            if (someoneHasDied)
            {
                // Scale with steps, to end round quicker
                _scaleSizeWithSteps = true;
                _scaleFligthFrequencyWithSteps = true;
            }
        }
    }
    private void DepleteEventTimer()
    {
        if (_activateDragon == false) return;
        if (_eventStarted) return;

        // Countdown, start event when below 0
        _currentDragonTimer -= Time.deltaTime;
        if (_currentDragonTimer <= 0)
        {
            _eventStarted = true;
            SpawnDragon();
        }
    }
    
    private void MoveDragon()
    {
        if (_eventStarted == false) return;

        // Move
        Vector3 movementDirection = (_dragonEndPos - _dragonStartPos).normalized;
        if (_isInWarningFlight)
        {
            _dragonMovingObject.transform.position += movementDirection * SettingsManager.Instance.DragonSettings.DragonWarningMovementSpeed * Time.deltaTime;
        }
        else
        {
            _dragonMovingObject.transform.position += movementDirection * SettingsManager.Instance.DragonSettings.DragonHazardMovementSpeed * Time.deltaTime * -1;
        }

        // See if reached target
        float acceptanceDistance = 1.0f;
        if (_isInWarningFlight)
        {
            float dragonDistanceToPos = Vector3.Distance(_dragonMovingObject.transform.position, _dragonEndPos);
            if (dragonDistanceToPos <= acceptanceDistance)
            {
                // Change direction
                _isInWarningFlight = false;
                _dragonMovingObject.transform.LookAt(_dragonStartPos);

                // Start firing
                _fireBreathObject.SetActive(true);
                OnFireBreathEvent?.Invoke();
                InvokeRepeating("SpewFire", 0f, _fireSpitFrequency);
            }
        }
        else
        {
            float dragonDistanceToPos = Vector3.Distance(_dragonMovingObject.transform.position, _dragonStartPos);
            if (dragonDistanceToPos <= acceptanceDistance)
            {
                // End event
                _eventStarted = false;
                _dragonVisuals.SetActive(false);
                SetNewEventTimer();

                // Stop fire
                _fireBreathObject.SetActive(false);
                CancelInvoke("SpewFire");

                // Adjust settings for next flight
                ++_nrTimesFlown;
                float ratio = Mathf.Clamp01(_nrTimesFlown / (float)SettingsManager.Instance.DragonSettings.StepsToReachMax);
                _fireSpitFrequency = Mathf.Lerp(SettingsManager.Instance.DragonSettings.MinSpewingFrequency, SettingsManager.Instance.DragonSettings.MaxSpewingFrequency, ratio);
            }
        }
    }

    private void SpewFire()
    {
        if (_eventStarted == false) return;
        if (_isInWarningFlight) return;

        // Spawn
        Vector3 spawnPos = _dragonMovingObject.transform.position;
        spawnPos += Vector3.down * 2.0f;
        spawnPos += _dragonMovingObject.transform.forward * 5.0f;

        GameObject spawnedObject = Instantiate(SettingsManager.Instance.DragonSettings.FireBomb, spawnPos, Quaternion.identity);
        FireBomb objectScript = spawnedObject.GetComponent<FireBomb>();

        // Optionally hide visuals
        if (SettingsManager.Instance.DragonSettings.ThrowFireBombs == false)
        {
            objectScript.HideVisuals();
        }

        // Optionally scale with steps
        if (_scaleSizeWithSteps)
        {
            float ratio = Mathf.Clamp01(_nrTimesFlown / (float) SettingsManager.Instance.DragonSettings.StepsToReachMax);
            float newScale = Mathf.Lerp(1, 2, ratio);

            Vector3 newScaleVector = Vector3.zero;
            newScaleVector.x = newScale;
            newScaleVector.y = newScale;
            newScaleVector.z = newScale;

            spawnedObject.transform.localScale = newScaleVector;
        }
    }

    private void SetNewEventTimer()
    {
        float minTimer = SettingsManager.Instance.DragonSettings.MinEventTimer;
        float maxTimer = SettingsManager.Instance.DragonSettings.MaxEventTimer;
       
        if (_scaleFligthFrequencyWithSteps)
        {
            minTimer *= _deadPlayerTimerMultiplier;
            maxTimer *= _deadPlayerTimerMultiplier;
            float ratio = Mathf.Clamp01(_nrTimesFlown / (float)SettingsManager.Instance.DragonSettings.StepsToReachMax);
            maxTimer = Mathf.Lerp(maxTimer, minTimer, ratio);
        }

        _currentDragonTimer = Random.Range(minTimer, maxTimer);
    }
    private void SpawnDragon()
    {
        // Show dragon
        // -----------
        _dragonVisuals.SetActive(true);

        // Random startPos
        // ---------------

        // There's probably a better way to do this ^\(0.0)/^
        Vector2 randomAxis;
        randomAxis.x = Random.Range(-1.0f, 1.0f);
        randomAxis.y = Random.Range(-1.0f, 1.0f);

        randomAxis.Normalize();

        Vector3 newDragonPosition = _dragonMovingObject.transform.position;
        newDragonPosition.x = randomAxis.x * SettingsManager.Instance.DragonSettings.DragonSpawnRadius;
        newDragonPosition.z = randomAxis.y * SettingsManager.Instance.DragonSettings.DragonSpawnRadius;

        _dragonStartPos = newDragonPosition;
        _dragonMovingObject.transform.position = newDragonPosition;

        // Parallel Endpos
        // ---------------
        _dragonEndPos = newDragonPosition;
        _dragonEndPos.x *= -1;
        _dragonEndPos.z *= -1;

        // Set up flight
        // -------------
        _dragonMovingObject.transform.LookAt(_dragonEndPos);
        _isInWarningFlight = true;

        // ScreenShake
        // -----------
        GameSystem.Instance.ScreenShake.StartShake(SettingsManager.Instance.DragonSettings.DragonScreenShakeDuration, SettingsManager.Instance.DragonSettings.DragonScreenShakeStrengthMultiplier);
    }
}
