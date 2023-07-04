using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<short, uint> _playerScores = new Dictionary<short, uint>();

    [Header("Settings")]
    [SerializeField] private uint _scoreForHit = 5;
    [SerializeField] private uint _scoreForElimination = 75;
    [SerializeField] private uint _scoreForSoleSurvivor = 250;
    public uint ScoreForHit { get { return _scoreForHit; } }
    public uint ScoreForElimination { get { return _scoreForElimination; } }
    public uint ScoreForSoleSurvivor { get { return _scoreForSoleSurvivor; } }

    [Header("Debug")]
    [Tooltip("Will turn the Watch values header into a visualisation of all scores registered.")]
    [SerializeField] private bool _visualiseScoreInInspector = false;

    class IdScorePair { public short Id{get;set;} public uint Score { get; set; } }
    [Header("Watch values")]
    [SerializeField] private List<IdScorePair> _scorePairList;

    public event Action<short> ScoreUpdatedEvent;

    // Adding score
    public void AddScoreHit(short playerId)
    {
        AddScore(playerId, _scoreForHit);
    }

    public void AddScoreElimination(short playerId) 
    {
        AddScore(playerId, _scoreForElimination);
    }

    public void AddScoreSoleSurvivor(short playerId) 
    {
        AddScore(playerId, _scoreForSoleSurvivor);
    }

    private void AddScore(short playerId, uint score)
    {
        if (!_playerScores.ContainsKey(playerId))
        {
            _playerScores.Add(playerId, 0);
        }
        uint currentScore = _playerScores[playerId];
        _playerScores[playerId] = currentScore + score;
        ScoreUpdatedEvent?.Invoke(playerId);
    }

    //
    public void ResetScore()
    {
        List<short> playerIds = new List<short>();
        foreach (var playerId in _playerScores.Keys) 
        {
            playerIds.Add(playerId);
        }
        _playerScores.Clear();
        foreach (var playerId in playerIds) 
        {
            ScoreUpdatedEvent?.Invoke(playerId);
        }
    }

    /// <summary>
    /// Returns 0 if the playerId is not registered in the internal dictionary.
    /// </summary>
    /// <param name="playerId">Id of the player</param>
    /// <returns>Score</returns>
    public uint GetScore(short playerId)
    {
        if (_playerScores.ContainsKey(playerId) == false)
        {
            return 0;
        }
        return _playerScores[playerId];
    }

    public short? GetWinningPlayerId()
    {
        short? winningPlayerId = null;
        uint winningPlayerScore = 0;
        foreach (var playerScore in _playerScores)
        {
            if (playerScore.Value > winningPlayerScore)
            {
                winningPlayerScore = playerScore.Value;
                winningPlayerId = playerScore.Key;
            }
        }
        return winningPlayerId;
    }

    // Debugging 
#if (UNITY_EDITOR)
    // https://support.unity.com/hc/en-us/articles/208456906-Excluding-Scripts-and-Assets-from-builds
    // This #if makes that this update call only gets used when testing in editor and not when builds are made.
    private void Update()
    {
        if (_visualiseScoreInInspector)
        {
            if (_scorePairList == null)
            {
                _scorePairList = new List<IdScorePair>();
            }
            _scorePairList.Clear();
            foreach (var playerScore in _playerScores)
            {
                var pair = new IdScorePair();
                pair.Id = playerScore.Key;
                pair.Score = playerScore.Value;
                _scorePairList.Add(pair);
            }
        }
    }
#endif
}
