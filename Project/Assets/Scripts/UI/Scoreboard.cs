using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] private GameObject _background;
    private RectTransform _backgroundRectTransform;
    private Vector3 _originalBackgroundScale;
    [SerializeField] private List<GameObject> _scoreBoardEntryObjs;
    private List<ScoreboardEntry> _scoreBoardEntries = new List<ScoreboardEntry>();
    [SerializeField] private float _scaleDuration = 1.0f;

    private void Start()
    {
        _background.SetActive(false);
        foreach (var entry in _scoreBoardEntryObjs)
        {
            var scoreBoardEntry = entry.GetComponent<ScoreboardEntry>();
            Debug.Assert(scoreBoardEntry, "One of the objects in the scoreboard entry list does not contain the scoreboard entry script.");
            _scoreBoardEntries.Add(scoreBoardEntry);
        }
        _backgroundRectTransform = _background.GetComponent<RectTransform>();
        Debug.Assert(_backgroundRectTransform, $"Background is not a UI element because no RectTransform is found");
        _originalBackgroundScale = _backgroundRectTransform.localScale;
        StartCoroutine(LateStart_Coroutine(0.1f));
    }

    private IEnumerator LateStart_Coroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //
        foreach (var entry in _scoreBoardEntryObjs) 
        {
            entry.SetActive(false);
        }
    }

    public void AssignPlayer(short playerId)
    {
        _scoreBoardEntries.Find(x => !x.IsPlayerAssigned).AssignPlayer(playerId);
    }

    public void UnassignPlayer(short playerId) 
    {
        int idx = _scoreBoardEntries.FindIndex(x => x.AssignedPlayerId == playerId);
        if (idx > -1)
        {
            _scoreBoardEntries[idx].AssignPlayer(null);
        }
    }

    private void SetScoreboardVisibility(bool visible)
    {
        _background.SetActive(visible);
        foreach (var entry in _scoreBoardEntries)
        {
            if (entry.IsPlayerAssigned)
            {
                entry.gameObject.SetActive(visible);
                if (visible)
                {
                    entry.FirstUpdate();
                }
            }
        }
    }

    public void SetScoreboardVisibility(float time)
    {
        StartCoroutine(ShowScoreboard_Coroutine(time));
    }

    private IEnumerator ShowScoreboard_Coroutine(float waitTime)
    {
        SetScoreboardVisibility(true);
        _backgroundRectTransform.localScale = Vector3.zero;

        float elapsedTime = 0f;

        while (elapsedTime < _scaleDuration)
        {
            float t = EaseInOut(elapsedTime / _scaleDuration);

            _backgroundRectTransform.localScale = Vector3.Lerp(Vector3.zero, _originalBackgroundScale, t);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _backgroundRectTransform.localScale = _originalBackgroundScale;
        yield return new WaitForSeconds(waitTime);

        elapsedTime = 0f;
        while (elapsedTime < _scaleDuration)
        {
            float t = EaseInOut(elapsedTime / _scaleDuration);

            _backgroundRectTransform.localScale = Vector3.Lerp(_originalBackgroundScale, Vector3.zero, t);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        SetScoreboardVisibility(false);
        _backgroundRectTransform.localScale = _originalBackgroundScale;
    }

    private float EaseInOut(float t)
    {
        return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
    }

    public bool IsScoreboardVisible { get { return _background.activeInHierarchy; } }
}
