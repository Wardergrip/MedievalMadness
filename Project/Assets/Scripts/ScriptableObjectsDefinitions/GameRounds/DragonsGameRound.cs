using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameRound", menuName = "GameRound/Dragon")]
public class DragonsGameRound : GameRound
{
    [Header("Dragon settings")]
    [SerializeField] private int _minimumAmount = 3;
    [SerializeField] private int _maximumAmount = 10;

    private int _amount;

    public override void Init()
    {
        _amount = Random.Range(_minimumAmount, _maximumAmount);

        var dragon = FindAnyObjectByType<Dragon>();

        for (int i = 0; i < _amount; ++i) 
        {
            Instantiate(dragon.gameObject);
        }
    }
}
