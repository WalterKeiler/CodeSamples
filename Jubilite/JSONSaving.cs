using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;

public class JSONSaving : MonoBehaviour
{
    public static PlayerData _playerData;

    public static string path = "";
    public static string persitentPath = "";

    private static JSONSaving _jsonSaving;

    private void Awake()
    {
        if (_jsonSaving)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
            _jsonSaving = this;
        }
    }

    void Start()
    {
        SetPaths();
#if UNITY_EDITOR
        if (!File.Exists(path))
        {
            CreatePlayerData();
            SaveData(_playerData);
        }
#else
        if (!File.Exists(persitentPath))
        {
            CreatePlayerData();
            SaveData(_playerData);
        }
#endif
        
        else
        {
            LoadData();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            //SaveLoadManager.SaveData(SceneManager.GetActiveScene().buildIndex, "Test");
            //SaveData(_playerData);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log(SaveLoadManager.LoadData().ToString());
            //LoadData();
        }
    }

    public static void CreatePlayerData()
    {
        List<Level> newLevel = new List<Level>();
        List<int> scores = new List<int>(1){0};
        newLevel.Add(new Level(-1,scores,0));
        _playerData = new PlayerData(new PlayerSettings(),"Dev", newLevel, true, 0);
    }

    public static void SetPaths()
    {
        //Debug.Log("Set Path");
#if UNITY_EDITOR
        path = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveData.json";
#else
        persitentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveData.json";
#endif
    }

    public static void SaveData(PlayerData _newData)
    {
#if UNITY_EDITOR
        string savePath = path;
#else
        string savePath = persitentPath;
#endif
        Debug.Log("Saving Data at " + savePath);

        string json = JsonUtility.ToJson(_newData);
        
        Debug.Log(json);

        using StreamWriter writer = new StreamWriter(savePath);
        writer.Write(json);
    }

    public static void LoadData()
    {
#if UNITY_EDITOR
        Debug.Log(path);
        if (!File.Exists(path))
        {
            Debug.Log("Save");
            SaveData(_playerData);
        }
        
        using StreamReader reader = new StreamReader(path);

        string json = reader.ReadToEnd();

        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        _playerData = data;
        Debug.Log((data.ToString()));
#else
        Debug.Log(persitentPath);
        if (!File.Exists(persitentPath))
        {
            Debug.Log("Save");
            SaveData(_playerData);
        }
        
        using StreamReader reader = new StreamReader(persitentPath);

        string json = reader.ReadToEnd();

        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        _playerData = data;
        Debug.Log((data.ToString()));
#endif
    }

    public static void DeleteData()
    {
        Debug.Log("Delete Save Data");
#if UNITY_EDITOR
        File.Delete(path);
        CreatePlayerData();
        SaveData(_playerData);
#else
        File.Delete(persitentPath);
        CreatePlayerData();
        SaveData(_playerData);
#endif
    }
}
