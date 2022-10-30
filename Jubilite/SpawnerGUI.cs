using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Jubilite

// This is the unity component of the custom editor
// it does the backend work of saving the enmies and adding them to the spawner object

// All code written by Walter Keiler 2022

[RequireComponent(typeof(EnemySpawner))]
[RequireComponent(typeof(HeatMap))]
[ExecuteInEditMode]
public class SpawnerGUI : MonoBehaviour
{
    // I only want this script to be running in editor
#if UNITY_EDITOR
    public int beatNum = 1;
    public GameObject baseEnemyPrefab;
    public HeatMap heatMap;
    [HideInInspector]
    public List<Enemy> _enemies = new List<Enemy>();
    [SerializeReference] private Enemy _currentEnemy;
    [Header("Enemy Options")]
    [SerializeField] private int repeatFrequency = 0;
    [SerializeField] private int stopRepeat = 0;

    // Create a new enemy clearing the current enemy
    public void NewEnemy()
    {
        _currentEnemy = new Enemy();
    }
    
    // Move to the next beat and clear all of the data from the current enemy and beat
    public void NextBeat()
    {
        beatNum++;
        _enemies = new List<Enemy>();
        repeatFrequency = 0;
        stopRepeat = 0;
    }
    
    // Move to the next previous beat and clear all of the data from the current enemy and beat
    public void PreviousBeat()
    {
        beatNum--;
        _enemies = new List<Enemy>();
        repeatFrequency = 0;
        stopRepeat = 0;
    }

    // Create a enemy based on the data entered into the editor and add it into the list of enemies on this beat
    public void CreateEnemy(EnemiesToSpawn.SpawnPoints spawnPoint, List<EnemyMove> move)
    {
        _currentEnemy.spawnBeat = beatNum;
        _currentEnemy.repeatFrequency = repeatFrequency;
        _currentEnemy.stopRepeatingBeat = stopRepeat;
        _currentEnemy.spawnPoint = spawnPoint;
        
        _currentEnemy.enemyBehavior = ScriptableObject.CreateInstance<EnemyBehavior>();
        
        _currentEnemy._enemyMoves = new List<EnemyMove>();

#line hidden
        _currentEnemy.enemyBehavior.movementBehavior = _currentEnemy._enemyMoves;

        for (int i = 0; i < move.Count; i++)
        {
            _currentEnemy.enemyBehavior.movementBehavior.Add(move[i]);
        }

        _enemies.Add(_currentEnemy);
    }

    // Insert a new enemy into an already existing beat
    public void InsertEnemy(EnemiesToSpawn.SpawnPoints spawnPoint, List<EnemyMove> move, int i)
    {
        _currentEnemy = new Enemy();
        _currentEnemy.spawnBeat = beatNum;
        _currentEnemy.repeatFrequency = repeatFrequency;
        _currentEnemy.stopRepeatingBeat = stopRepeat;
        _currentEnemy.spawnPoint = spawnPoint;
        
        _currentEnemy.enemyBehavior = ScriptableObject.CreateInstance<EnemyBehavior>();
        _currentEnemy._enemyMoves = new List<EnemyMove>();
#line hidden
        _currentEnemy.enemyBehavior.movementBehavior = _currentEnemy._enemyMoves;
        
        foreach (var var in move)
        {
            _currentEnemy.enemyBehavior.movementBehavior.Add(var);
        }
        
        _enemies[i] = _currentEnemy;
    }
    
    // Create a beat that has all of the enemies for it and then save both the enemies and bet object to the project files
    public void CreateBeat()
    {
        heatMap = GetComponent<HeatMap>();
//#line hidden
        var beat = ScriptableObject.CreateInstance<SpawnInfoScriptableObject>();

        SerializedObject so = new SerializedObject(beat);
        so.ApplyModifiedProperties();
        
        // Check to see if the object already exists
        string folderPath = "Assets/ScriptableObjects/Beats/" + SceneManager.GetActiveScene().name;
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string path = folderPath + "/" + SceneManager.GetActiveScene().name + "-Beat_" + beatNum + ".asset";
        if (!File.Exists(path))
        {
            Undo.RecordObject(beat,path);
            PrefabUtility.RecordPrefabInstancePropertyModifications(beat);
            AssetDatabase.CreateAsset(beat,path);
            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Object>(path));
        }
        else
        {
            // If it already exists create another object with a copy indicator
            int copies = 1;
            for (int i = 0; i < copies; i++)
            {
                path = folderPath + "/" + SceneManager.GetActiveScene().name + "-Beat_" + beatNum  + "("+copies+")" + ".asset";
                if (!File.Exists(path))
                {
                    Undo.RecordObject(beat,path);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(beat);
                    AssetDatabase.CreateAsset(beat,path);
                    EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Object>(path));
                }
                else
                {
                    copies++;
                }
            }
        }

        List<EnemiesToSpawn> enemiesToSpawn = new List<EnemiesToSpawn>();
        // Take all the enemies and create objects for all of them through the same object seen above
        int enemyNum = 0;
        foreach (var enemy in _enemies)
        {
            enemyNum++;
            EnemiesToSpawn enemyData = new EnemiesToSpawn();
            enemyData._spawnPoint = enemy.spawnPoint;
            enemyData._enemyBehavior = enemy.enemyBehavior;
            enemyData._objectToSpawn = baseEnemyPrefab;

            string enemyFolderPath = "Assets/ScriptableObjects/EnemyBehaviors/" + SceneManager.GetActiveScene().name +
                                     "/" + "_Beat" + beatNum;
            
            if (!Directory.Exists(enemyFolderPath))
            {
                Directory.CreateDirectory(enemyFolderPath);
            }
            
            string enemyPath = enemyFolderPath + "/" + SceneManager.GetActiveScene().name + "-Beat_" + beatNum + "_Enemy_" + enemyNum + ".asset";
            
            if (!File.Exists(enemyPath))
            {
                Undo.RecordObject(enemy.enemyBehavior,enemyPath);
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.enemyBehavior);
                AssetDatabase.CreateAsset(enemy.enemyBehavior,enemyPath);
                EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Object>(enemyPath));
            }
            else
            {
                int copies = 1;
                for (int i = 0; i < copies; i++)
                {
                    enemyPath = enemyFolderPath + "/" + SceneManager.GetActiveScene().name + "-Beat_" + beatNum + "_Enemy_" + enemyNum + "("+copies+")" + ".asset";
                    if (!File.Exists(enemyPath))
                    {
                        Undo.RecordObject(enemy.enemyBehavior,enemyPath);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.enemyBehavior);
                        AssetDatabase.CreateAsset(enemy.enemyBehavior,enemyPath);
                        EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Object>(enemyPath));
                    }
                    else
                    {
                        copies++;
                    }
                }
            }
            
            enemiesToSpawn.Add(enemyData);
        }

        beat._enemiesToSpawn = enemiesToSpawn;
        beat._beatToSpawnOn = beatNum;
        beat._repeatFrequency = repeatFrequency;
        beat._stopRepeatingBeat = stopRepeat;

        _enemies = new List<Enemy>();
        
        GetComponent<EnemySpawner>()._enemiesToSpawn.Add(beat);
        repeatFrequency = 0;
        stopRepeat = 0;
    }

    // Reste everything
    public void Reset()
    {
        beatNum = 1;
        _enemies = new List<Enemy>();
        _currentEnemy = new Enemy();
    }
#endif
}

// Holds all the enemy information
[Serializable]
public class Enemy
{
    public int spawnBeat;
    public int repeatFrequency;
    public int stopRepeatingBeat;

    public EnemiesToSpawn.SpawnPoints spawnPoint;
    public EnemyBehavior enemyBehavior;
    public List<EnemyMove> _enemyMoves;

}