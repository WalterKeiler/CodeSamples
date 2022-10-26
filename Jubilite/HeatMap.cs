using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using _Scripts.Systems;
using UnityEngine;

public class HeatMap : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    public int beatNum;
    //[SerializeField] private Texture2D output;
    [Range(0,1)]
    [SerializeField] private float addToGradient = .25f;
    [Range(0,.5f)]
    [SerializeField] private float nextPosColorOffset = .15f;
    [Range(1, 3)] 
    [SerializeField] private int numberOfCubes = 3;
    public Gradient colorGradient;
    
    private SpawnInfoScriptableObject[] _enemiesToSpawn;
    private List<int> _beatsToSpawnOn;
    private List<int> _beatsToStopOn;
    private List<int> _beatsToSpawnOn2;
    private List<EnemiesToSpawn.SpawnPoints> _spawnedPoints;
    
    private List<int> _spawnedBeat;
    private List<Vector2> _currentPos;
    private List<Vector2> _currentDir;
    private List<EnemyBehavior> _spawnedBehaviors;

    private List<int> _nextBeat;
    private List<Vector2> _nextPos;
    private List<Vector2> _nextDir;
    private List<EnemyBehavior> _nextBehaviors;
    
    private List<int> _nextBeat2;
    private List<Vector2> _nextPos2;
    private List<Vector2> _nextDir2;
    private List<EnemyBehavior> _nextBehaviors2;
    
    public Color[,] heatMapOutput = new Color[9,5];

    public List<HeatMapPerBeat> heatMaps = new List<HeatMapPerBeat>(10000);
    
    private void Awake()
    {
        CreateHeatMap();
    }

    [ContextMenu("Reset Arrays")]
    public void ResetArrays()
    {

        _spawnedPoints = new List<EnemiesToSpawn.SpawnPoints>();
        _spawnedBeat = new List<int>();
        _currentPos = new List<Vector2>();
        _currentDir = new List<Vector2>();
        _spawnedBehaviors = new List<EnemyBehavior>();

        _nextBeat = new List<int>();
        _nextPos = new List<Vector2>();
        _nextDir = new List<Vector2>();
        _nextBehaviors = new List<EnemyBehavior>();
        
        _nextBeat2 = new List<int>();
        _nextPos2 = new List<Vector2>();
        _nextDir2 = new List<Vector2>();
        _nextBehaviors2 = new List<EnemyBehavior>();
    }
    
    [ContextMenu("Create Heat Map")]
    public void CreateHeatMap()
    {
        //heatMapOutput = new Color[9 * beatNum, 5];

        heatMaps = new List<HeatMapPerBeat>();

        for (int i = 0; i < beatNum; i++)
        {
            heatMaps.Add(new HeatMapPerBeat());
            heatMaps[i].heatMapOutput = new Color[9, 5];
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    heatMaps[i].heatMapOutput[x,y] = Color.black;
                    //heatMapOutput[x,y] = Color.black;
                }
            }
        }
        
        
        _enemiesToSpawn = enemySpawner._enemiesToSpawn.ToArray();
        _beatsToSpawnOn = new List<int>();
        _beatsToSpawnOn2 = new List<int>();
        _beatsToStopOn = new List<int>();
        for (int i = 0; i < _enemiesToSpawn.Length; i++)
        {
            _beatsToSpawnOn.Add(_enemiesToSpawn[i]._beatToSpawnOn);
            _beatsToSpawnOn2.Add(_enemiesToSpawn[i]._beatToSpawnOn);
            _beatsToStopOn.Add(_enemiesToSpawn[i]._stopRepeatingBeat);
        }
        
        ResetArrays();
        
        for (int i = 0; i < beatNum; i++)
        {
            GetEnemies(i);
            UpdatePosition(i);
            if(numberOfCubes > 1)
                UpdateNextPosition(i);
            if(numberOfCubes > 2)
                UpdateNextPosition2(i);
            RemoveObj(i);
            UpdateTexture(i);
        }
        //Debug.Log("Done");
    }

    void GetEnemies(int i)
    {
        for (int j = 0; j < _beatsToSpawnOn.Count; j++)
        {
            //Debug.Log("beat num: " + i + " output " + (i % (_enemiesToSpawn[j]._repeatFrequency + _enemiesToSpawn[j]._beatToSpawnOn)));
            if (_beatsToSpawnOn[j] == i && (i <= _beatsToStopOn[j] || _beatsToStopOn[j] == 0))
            {
                foreach (var spawn in _enemiesToSpawn[j]._enemiesToSpawn)
                {
                    _spawnedPoints.Add(spawn._spawnPoint);
                    
                    _spawnedBeat.Add(i);
                    _currentPos.Add(StartPosition(spawn._spawnPoint));
                    _currentDir.Add(StartDirection(spawn._spawnPoint));
                    _spawnedBehaviors.Add(spawn._enemyBehavior);

                    _nextBeat.Add(i - 1);
                    _nextPos.Add(StartPosition(spawn._spawnPoint));
                    _nextDir.Add(StartDirection(spawn._spawnPoint));
                    _nextBehaviors.Add(spawn._enemyBehavior);
                    
                    
                }
                _beatsToSpawnOn[j] += _enemiesToSpawn[j]._repeatFrequency;
            }

            if (_beatsToSpawnOn2[j] == i + 1 && (i + 1 <= _beatsToStopOn[j] || _beatsToStopOn[j] == 0))
            {
                foreach (var spawn in _enemiesToSpawn[j]._enemiesToSpawn)
                {
                    _nextBeat2.Add(i - 2);
                    _nextDir2.Add(StartDirection(spawn._spawnPoint));
                    _nextPos2.Add(StartPosition(spawn._spawnPoint));
                    _nextBehaviors2.Add(spawn._enemyBehavior); 
                }
                _beatsToSpawnOn2[j] += _enemiesToSpawn[j]._repeatFrequency;
            }
        }
    }

    void UpdatePosition(int i)
    {
        for (int j = 0; j < _spawnedBeat.Count; j++)
        {
            if (i < _spawnedBeat[j] + 1) continue;
            
            foreach (var moves in _spawnedBehaviors[j].movementBehavior)
            {
                if (moves.moveDir == Direction.Forward ||
                    _spawnedBeat[j] + moves.moves + 1 != i) continue;
                
                _currentDir[j] = ChangeDirection(moves.moveDir, _currentDir[j]);
            }
            _currentPos[j] += _currentDir[j];
        }
        
    }

    void UpdateNextPosition(int i)
    {

        for (int j = 0; j < _nextBeat.Count; j++)
        {
            if (i > _nextBeat[j] - 1)
            {
                foreach (var moves in _nextBehaviors[j].movementBehavior)
                {
                    if (moves.moveDir == Direction.Forward ||
                        _nextBeat[j] + moves.moves + 1 != i) continue;
                    _nextDir[j] = ChangeDirection(moves.moveDir, _nextDir[j]);
                }
            
                _nextPos[j] += _nextDir[j];
            }
        }
        
    }
    
    void UpdateNextPosition2(int i)
    {
        
        for (int j = 0; j < _nextDir2.Count; j++)
        {
            if (i > _nextBeat2[j] - 1)
            {
                foreach (var moves in _nextBehaviors2[j].movementBehavior)
                {
                    if (moves.moveDir == Direction.Forward ||
                        _nextBeat2[j] + moves.moves != i - 2) continue;
                    _nextDir2[j] = ChangeDirection(moves.moveDir, _nextDir2[j]);
                }
            
                _nextPos2[j] += _nextDir2[j];
            }
        }
        
    }
    
    void UpdateTexture(int i)
    {
        foreach (var pos in _currentPos)
        {
            
            if (Mathf.Abs(pos.x) > 8 || pos.x < 0 || Mathf.Abs(pos.y) > 5 || pos.y >= 0)
            {

            }
            else
            {
                heatMaps[i].heatMapOutput[(int) pos.x, (int) -pos.y - 1].r += (addToGradient + (nextPosColorOffset * 2));
                //heatMapOutput[(int) pos.x + (i * 9), (int) -pos.y - 1].r += (addToGradient + (nextPosColorOffset * 2));
                heatMaps[i].heatMapOutput[(int) pos.x, (int) -pos.y - 1].b += (addToGradient + (nextPosColorOffset * 4));
                //heatMapOutput[(int) pos.x + (i * 9), (int) -pos.y - 1].b += (addToGradient + (nextPosColorOffset * 4));
                heatMaps[i].heatMapOutput[(int) pos.x, (int) -pos.y - 1].g += (addToGradient + (nextPosColorOffset * 4));
                //heatMapOutput[(int) pos.x + (i * 9), (int) -pos.y - 1].g += (addToGradient + (nextPosColorOffset * 4));
            }
        }
        
        if(numberOfCubes > 1)
        {
            foreach (var pos in _nextPos)
            {
                bool test = true;
                if (Mathf.Abs(pos.x) > 8 || pos.x < 0 || Mathf.Abs(pos.y) > 5 || pos.y >= 0)
                {
                }
                else
                {
                    for (int j = 0; j < _nextBeat.Count; j++)
                    {
                        if (i >= _nextBeat[j] + 1 || i == 0)
                        {
                            if (test)
                            {
                                heatMaps[i].heatMapOutput[(int) pos.x, (int) -pos.y - 1].r +=
                                    (addToGradient + nextPosColorOffset);
                                heatMaps[i].heatMapOutput[(int) pos.x, (int) -pos.y - 1].b += (addToGradient + (nextPosColorOffset));
                                test = false;
                            }
                        }
                    }
                }
            }
        }
        if(numberOfCubes > 2)
        {
            foreach (var pos in _nextPos2)
            {
                bool test = true;
                if (Mathf.Abs(pos.x) > 8 || pos.x < 0 || Mathf.Abs(pos.y) > 5 || pos.y >= 0)
                {
                }
                else
                {
                    for (int j = 0; j < _nextBeat2.Count; j++)
                    {
                        if (i >= _nextBeat2[j] + 1 || i == 0)
                        {
                            if (test)
                            {
                                heatMaps[i].heatMapOutput[(int) pos.x, (int) -pos.y - 1].r += addToGradient;
                                heatMaps[i].heatMapOutput[(int) pos.x, (int) -pos.y - 1].b += (addToGradient + (nextPosColorOffset / 8));
                                test = false;
                            }
                        }
                    }
                }
            }
        } 
        
        //Debug.Log("Beat: " + i + "\n" +
        //            heatMapOutput[(0 + (i * 9)), (int) 0].r + " , " + heatMapOutput[1 + (i * 9), (int) 0].r + " , " + heatMapOutput[2 + (i * 9), (int) 0].r + " , " + heatMapOutput[3 + (i * 9), (int) 0].r + " , " + heatMapOutput[4 + (i * 9), (int) 0].r + " , " + heatMapOutput[5 + (i * 9), (int) 0].r + " , " + heatMapOutput[6 + (i * 9), (int) 0].r + " , " + heatMapOutput[7 + (i * 9), (int) 0].r + " , " + heatMapOutput[8 + (i * 9), (int) 0].r + "\n" +
        //            heatMapOutput[0 + (i * 9), (int) 1].r + " , " + heatMapOutput[1 + (i * 9), (int) 1].r + " , " + heatMapOutput[2 + (i * 9), (int) 1].r + " , " + heatMapOutput[3 + (i * 9), (int) 1].r + " , " + heatMapOutput[4 + (i * 9), (int) 1].r + " , " + heatMapOutput[5 + (i * 9), (int) 1].r + " , " + heatMapOutput[1 + (i * 9), (int) 1].r + " , " + heatMapOutput[7 + (i * 9), (int) 1].r + " , " + heatMapOutput[8 + (i * 9), (int) 1].r + "\n" +
        //            heatMapOutput[0 + (i * 9), (int) 2].r + " , " + heatMapOutput[1 + (i * 9), (int) 2].r + " , " + heatMapOutput[2 + (i * 9), (int) 2].r + " , " + heatMapOutput[3 + (i * 9), (int) 2].r + " , " + heatMapOutput[4 + (i * 9), (int) 2].r + " , " + heatMapOutput[5 + (i * 9), (int) 2].r + " , " + heatMapOutput[1 + (i * 9), (int) 2].r + " , " + heatMapOutput[7 + (i * 9), (int) 2].r + " , " + heatMapOutput[8 + (i * 9), (int) 2].r + "\n" +
        //            heatMapOutput[0 + (i * 9), (int) 3].r + " , " + heatMapOutput[1 + (i * 9), (int) 3].r + " , " + heatMapOutput[2 + (i * 9), (int) 3].r + " , " + heatMapOutput[3 + (i * 9), (int) 3].r + " , " + heatMapOutput[4 + (i * 9), (int) 3].r + " , " + heatMapOutput[5 + (i * 9), (int) 3].r + " , " + heatMapOutput[1 + (i * 9), (int) 3].r + " , " + heatMapOutput[7 + (i * 9), (int) 3].r + " , " + heatMapOutput[8 + (i * 9), (int) 3].r + "\n" +
        //            heatMapOutput[0 + (i * 9), (int) 4].r + " , " + heatMapOutput[1 + (i * 9), (int) 4].r + " , " + heatMapOutput[2 + (i * 9), (int) 4].r + " , " + heatMapOutput[3 + (i * 9), (int) 4].r + " , " + heatMapOutput[4 + (i * 9), (int) 4].r + " , " + heatMapOutput[5 + (i * 9), (int) 4].r + " , " + heatMapOutput[1 + (i * 9), (int) 4].r + " , " + heatMapOutput[7 + (i * 9), (int) 4].r + " , " + heatMapOutput[8 + (i * 9), (int) 4].r + "\n");

    }

    void RemoveObj(int i)
    {
        int length = _spawnedPoints.Count;
        int[] beat = _spawnedBeat.ToArray();
        Vector2[] currentPos = _currentPos.ToArray();
        
        for (int j = 0; j < length; j++)
        {
            if(beat[j] + 2 < i)
            {
                if (Mathf.Abs(currentPos[j].x) > 8 || currentPos[j].x <= -1 ||
                    Mathf.Abs(currentPos[j].y) > 5 || currentPos[j].y >= 0)
                {
                    _spawnedPoints.RemoveAt(j);
                    _spawnedBeat.RemoveAt(j);
                    _currentPos.RemoveAt(j);
                    _currentDir.RemoveAt(j);
                    _spawnedBehaviors.RemoveAt(j);
                    break;
                }
            }
        }

        if (numberOfCubes > 1)
        {
            int nextLength = _nextPos.Count;
            Vector2[] nextPos = _nextPos.ToArray();
            for (int j = 0; j < nextLength - 3; j++)
            {
                if (_nextBeat[j] + 1 < i)
                {
                    if (Mathf.Abs(nextPos[j].x) > 8 || nextPos[j].x <= -1 ||
                        Mathf.Abs(nextPos[j].y) > 5 || nextPos[j].y >= 0)
                    {
                        _nextPos.RemoveAt(j);
                        _nextDir.RemoveAt(j);
                        _nextBeat.RemoveAt(j);
                        _nextBehaviors.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        if(numberOfCubes > 2)
        {
            int nextLength2 = _nextPos2.Count;
            Vector2[] nextPos2 = _nextPos2.ToArray();
            for (int j = 0; j < nextLength2 - 3; j++)
            {
                if (_nextBeat2[j] < i)
                {
                    if (Mathf.Abs(nextPos2[j].x) > 8 || nextPos2[j].x <= -1 ||
                        Mathf.Abs(nextPos2[j].y) > 5 || nextPos2[j].y >= 0)
                    {
                        _nextPos2.RemoveAt(j);
                        _nextDir2.RemoveAt(j);
                        _nextBeat2.RemoveAt(j);
                        _nextBehaviors2.RemoveAt(j);
                        break;
                    }
                }
            }
        }
    }
    
    Vector2 StartPosition(EnemiesToSpawn.SpawnPoints _spawnPoint)
    {
        int x = 0, y = 0;
        switch (_spawnPoint)
        {
            case EnemiesToSpawn.SpawnPoints.Spawnpoint1:
                x = 0;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint2:
                x = 1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint3:
                x = 2;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint4:
                x = 3;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint5:
                x = 4;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint6:
                x = 5;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint7:
                x = 6;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint8:
                x = 7;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint9:
                x = 8;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint10:
                x = 9;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint11:
                x = 9;
                y = -2;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint12:
                x = 9;
                y = -3;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint13:
                x = 9;
                y = -4;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint14:
                x = 9;
                y = -5;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint15:
                x = 8;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint16:
                x = 7;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint17:
                x = 6;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint18:
                x = 5;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint19:
                x = 4;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint20:
                x = 3;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint21:
                x = 2;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint22:
                x = 1;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint23:
                x = 0;
                y = -6;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint24:
                x = -1;
                y = -5;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint25:
                x = -1;
                y = -4;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint26:
                x = -1;
                y = -3;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint27:
                x = -1;
                y = -2;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint28:
                x = -1;
                y = -1;
                break;
        }
        return new Vector2(x,y);
    } 
    
    Vector2 StartDirection(EnemiesToSpawn.SpawnPoints _spawnPoint)
    {
        int x = 0, y = 0;
        switch (_spawnPoint)
        {
            case EnemiesToSpawn.SpawnPoints.Spawnpoint1:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint2:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint3:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint4:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint5:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint6:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint7:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint8:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint9:
                x = 0;
                y = -1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint10:
                x = -1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint11:
                x = -1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint12:
                x = -1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint13:
                x = -1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint14:
                x = -1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint15:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint16:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint17:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint18:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint19:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint20:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint21:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint22:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint23:
                x = 0;
                y = 1;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint24:
                x = 1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint25:
                x = 1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint26:
                x = 1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint27:
                x = 1;
                y = 0;
                break;
            case EnemiesToSpawn.SpawnPoints.Spawnpoint28:
                x = 1;
                y = 0;
                break;
        }
        return new Vector2(x,y);
    }

    Vector2 ChangeDirection(Direction _direction, Vector2 currentDir)
    {
        int x = 0, y = 0;
        switch (_direction)
        {
            case Direction.Forward:
                x = (int) currentDir.x;
                y = (int) currentDir.y;
                break;
            case Direction.Right:
                if ((int)currentDir.x == 1)
                {
                    x = 0;
                    y = -1;
                }
                if ((int)currentDir.x == -1)
                {
                    x = 0;
                    y = 1;
                }
                if ((int)currentDir.y == 1)
                {
                    x = 1;
                    y = 0;
                }
                if ((int)currentDir.y == -1)
                {
                    x = -1;
                    y = 0;
                }
                break;
            case Direction.Left:
                if ((int)currentDir.x == 1)
                {
                    x = 0;
                    y = 1;
                }
                if ((int)currentDir.x == -1)
                {
                    x = 0;
                    y = -1;
                }
                if ((int)currentDir.y == 1)
                {
                    x = -1;
                    y = 0;
                }
                if ((int)currentDir.y == -1)
                {
                    x = 1;
                    y = 0;
                }
                break;
        }

        return new Vector2(x, y);
    }
    
}

public class HeatMapPerBeat
{
    public Color[,] heatMapOutput = new Color[9,5];
}