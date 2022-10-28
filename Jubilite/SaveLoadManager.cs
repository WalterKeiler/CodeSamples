using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SaveLoadManager : MonoBehaviour
{
    public static PlayerData _playerData;
    public AudioMixer master;
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
