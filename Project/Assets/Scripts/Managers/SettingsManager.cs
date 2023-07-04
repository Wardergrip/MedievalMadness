using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private HazardSettings _hazardSettings;
    [SerializeField] private PlayerSettings _playerSettings;
    [SerializeField] private RumbleSettings _rumbleSettings;
    [SerializeField] private SmashHealthSettings _smashHealthSettings;
    [SerializeField] private SpawnAreaSettings _spawnAreaSettings;
    [SerializeField] private DragonSettings _dragonSettings;

    public HazardSettings HazardSettings
    {
        get 
        { 
            if (_hazardSettings == null)
            {
                _hazardSettings = Resources.Load("Settings/Main_HazardSettings") as HazardSettings;
            }
            return _hazardSettings; 
        }
        set { _hazardSettings = value; }
    }
    public PlayerSettings PlayerSettings
    {
        get 
        {
            if (_playerSettings == null)
            {
                _playerSettings = Resources.Load("Settings/Main_PlayerSettings") as PlayerSettings;
            }
            return _playerSettings; 
        }
        set { _playerSettings = value; }
    }
    public RumbleSettings RumbleSettings
    {
        get
        {
            if (_rumbleSettings == null)
            {
                _rumbleSettings = Resources.Load("Settings/Main_RumbleSettings") as RumbleSettings;
            }
            return _rumbleSettings;
        }
        set { _rumbleSettings = value; }
    }
    public SmashHealthSettings SmashHealthSettings
    {
        get
        {
            if (_smashHealthSettings == null)
            {
                _smashHealthSettings = Resources.Load("Settings/Main_SmashHealthSettings") as SmashHealthSettings;
            }
            return _smashHealthSettings;
        }
        set { _smashHealthSettings = value; }
    }
    public SpawnAreaSettings SpawnAreaSettings
    {
        get 
        {
            if (_spawnAreaSettings == null)
            {
                _spawnAreaSettings = Resources.Load("Settings/Main_SpawnAreaSettings") as SpawnAreaSettings;
            }
            return _spawnAreaSettings;
        }
        set { _spawnAreaSettings = value; }
    }

    public DragonSettings DragonSettings
    {
        get
        {
            if (_dragonSettings == null)
            {
                _dragonSettings = Resources.Load("Settings/Main_DragonSettings") as DragonSettings;
            }
            return _dragonSettings;
        }
        set { _dragonSettings = value; }
    }
}
