using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

// Jubilite

// This is the Manager that is used to interface with the JSONSaving script and is called throughout the game

// All code written by Walter Keiler 2022

public class SaveLoadManager : MonoBehaviour
{
    public static PlayerData _playerData;
    public AudioMixer master;
    
    // Make sure all the player settings are loaded in and set
    public void Start()
    {
        _playerData = new PlayerData(new PlayerSettings(),"", new List<Level>(), true, 0);
        LoadData();
        SettingsManager.screenShake = _playerData.settings.screenShake;
        SettingsManager.dPad = _playerData.settings.dPad;
        SettingsManager.musicVol = _playerData.settings.volumeMusic;
        SettingsManager.SFXVol = _playerData.settings.volumeSFX;
        SettingsManager.quality = _playerData.settings.quality;
        master.SetFloat("Music Vol", _playerData.settings.volumeMusic);
        master.SetFloat("SFX Vol", _playerData.settings.volumeSFX);
        QualitySettings.SetQualityLevel(_playerData.settings.quality);
    }
    
    // This is the master save function for saving level and player data
    public static void SaveData(int levelID = -1, int score = -1, int stars = 0, bool newSave = true, string name = "")
    {
        if (JSONSaving.path == null || JSONSaving.persitentPath == null)
        {
            JSONSaving.SetPaths();
        }
#if UNITY_EDITOR
        if (!File.Exists(JSONSaving.path))
        {
            _playerData = new PlayerData(new PlayerSettings(),"", new List<Level>(), true, 0);
        }
        else
        {
            _playerData = LoadData();
        }
#else
        if (!File.Exists(JSONSaving.persitentPath))
        {
            _playerData = new PlayerData(new PlayerSettings(), "", new List<Level>(), true, 0);
        }
        else
        {
            _playerData = LoadData();
        }
#endif
        
        if (name != "")
        {
            _playerData.name = name;
        }

        // Check to see if the player has already completed this level
        if (levelID >= 0)
        {
            Level newLevel = new Level(levelID, new List<int>(), stars);

            foreach (var levels in _playerData.completedLevels)
            {
                if (levels.levelID == levelID)
                {
                    levels.scores.Add(score);

                    if (levels.stars < stars)
                    {
                        _playerData.totalStars += (stars - levels.stars);
                        levels.stars = stars;
                        Debug.Log(_playerData.totalStars);
                    }

                    JSONSaving.SaveData(_playerData);
                    return;
                }
            }
            
            // If the player has not previously completed this level write all of its data in
            if (!_playerData.completedLevels.Contains(newLevel))
            {
                newLevel.scores.Add(score);
                newLevel.stars = stars;
                _playerData.totalStars += newLevel.stars;
                _playerData.completedLevels.Add(newLevel);
                Debug.Log(_playerData.totalStars);
            }
        }

        _playerData.newSave = newSave;
        
        JSONSaving.SaveData(_playerData);
    }

    // This is a save function specific to game settings
    public static void SaveSettings(float volumeMusic = 0, float volumeSFX = 0, bool dPad = false, bool screenShake = true, int quality = 0)
    {
        if (JSONSaving.path == null || JSONSaving.persitentPath == null)
        {
            JSONSaving.SetPaths();
        }
#if UNITY_EDITOR
        if (!File.Exists(JSONSaving.path))
        {
            _playerData = new PlayerData(new PlayerSettings(),"", new List<Level>(), true, 0);
        }
        else
        {
            _playerData = LoadData();
        }
#else
        if (!File.Exists(JSONSaving.persitentPath))
        {
            _playerData = new PlayerData(new PlayerSettings(), "", new List<Level>(), true, 0);
        }
        else
        {
            _playerData = LoadData();
        }
#endif

        _playerData.settings.quality = quality;
        _playerData.settings.volumeMusic = volumeMusic;
        _playerData.settings.volumeSFX = volumeSFX;
        _playerData.settings.dPad = dPad;
        _playerData.settings.screenShake = screenShake;

        JSONSaving.SaveData(_playerData);
    }
    
    public static PlayerData LoadData()
    {
        JSONSaving.LoadData();
        return _playerData = JSONSaving._playerData;
    }

    public static void DeleteSaveData()
    {
        JSONSaving.DeleteData();
    }
}

// This is the player data class that holds all of the important things we need to keep track of about the player
public class PlayerData
{
    public PlayerSettings settings;
    public string name;
    public List<Level> completedLevels;
    public bool newSave;
    public int totalStars;
    
    public PlayerData(PlayerSettings settings, string name, List<Level> completedLevels, bool newSave, int totalStars)
    {
        this.settings = settings;
        this.name = name;
        this.completedLevels = completedLevels;
        this.newSave = newSave;
        this.totalStars = totalStars;
    }

    public override string ToString()
    {
        return $"Player {name} they have completed {completedLevels.Count} levels.";
    }
}

// Holds all the data pertaining to a level
[System.Serializable]
public class Level
{
    public int levelID;
    public List<int> scores;
    public int stars;

    public Level(int levelID, List<int> scores, int stars)
    {
        this.levelID = levelID;
        this.scores = scores;
        this.stars = stars;
    }

    public override string ToString()
    {
        return $"Player has completed level{levelID} {scores.Count} times.";
    }
}

// Holds all of the game settings
[Serializable]
public class PlayerSettings
{
    public float volumeMusic = 0;
    public float volumeSFX = 0;
    public bool dPad = false;
    public bool screenShake = true;
    public int quality = 0;

    public PlayerSettings(float volumeMusic = 0, float volumeSfx = 0, bool dPad = false, bool screenShake = true, int quality = 0)
    {
        this.volumeMusic = volumeMusic;
        this.volumeSFX = volumeSfx;
        this.dPad = dPad;
        this.screenShake = screenShake;
        this.quality = quality;
    }
}
