using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
//using Debug = System.Diagnostics.Debug;

using Debug = UnityEngine.Debug;

[CustomEditor(typeof(SpawnerGUI))]
public class SpawnEditor : Editor
{
    
    int _repeatFrequency = 0;
    int _stopRepeatBeat = 0;
    bool _repeat = false;
    //bool _stopRepeat = false;
    bool _firstEnemy = true;
    //bool _savedEnemy = false;
    bool _start = true;

    private int _spawnPoint = 1;
    private static Vector2 widthHeight = new Vector2(12, 8);
    private bool[,] _toggle = new bool[(int)widthHeight.x, (int)widthHeight.y];

    private List<List<SavedEnemy>> savedEnemiesByBeat = new List<List<SavedEnemy>>();
    private List<SavedEnemy> savedEnemies = new List<SavedEnemy>();
    
    //Texture2D _board = new Texture2D(90, 50);
    List<Texture2D> _beatTextures = new List<Texture2D>();
    private Texture2D _board;// = new Texture2D(90, 50);
    private Texture2D _heatMap;// = new Texture2D(90, 50);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        SpawnerGUI spawnerGUI = (SpawnerGUI)target;
        if (_start)
        {
            //_heatMap = GetHeatMapTexture(spawnerGUI);
            _start = false;
        }

        spawnerGUI.beatNum = Mathf.Clamp(spawnerGUI.beatNum, 1, 10000);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New Enemy"))
        {
            NewEnemy(spawnerGUI);
        }

        
        // _repeat = GUILayout.Toggle(_repeat, "Repeat Frequency", GUILayout.Width(120));
        // if (_repeat)
        // {
        //     _repeatFrequency = EditorGUI.IntField(new Rect(275, 104, 50, 20),_repeatFrequency);
        //     GUILayout.Label("                     ");
        //     
        //     GUILayout.BeginHorizontal(GUILayout.Width(50));
        //     GUILayout.Label("Stop Repeating");
        //     
        //     _stopRepeatBeat = EditorGUI.IntField(new Rect(350, 104, 50, 20),_stopRepeatBeat);
        //     GUILayout.EndHorizontal();
        // }
        
        else
        {
            _repeatFrequency = 0;
            _stopRepeatBeat = 0;
        }

        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous Beat"))
        {
            //_savedEnemy = false;
            _firstEnemy = true;
            _repeatFrequency = 0;
            _spawnPoint = 1;
            //_heatMap = GetHeatMapTexture(spawnerGUI);
            _toggle = new bool[(int)widthHeight.x, (int)widthHeight.y];
            NewEnemy(spawnerGUI);
            spawnerGUI.PreviousBeat();
        }
        if (GUILayout.Button("Next Beat"))
        {
            //_savedEnemy = false;
            _firstEnemy = true;
            _repeatFrequency = 0;
            _spawnPoint = 1;
            //_heatMap = GetHeatMapTexture(spawnerGUI);
            _toggle = new bool[(int)widthHeight.x, (int)widthHeight.y];
            NewEnemy(spawnerGUI);
            spawnerGUI.NextBeat();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Enemy Path");
        
        if (GUILayout.Button("Reset Enemy Path"))
        {
            _spawnPoint = 1;
            _toggle = new bool[(int)widthHeight.x, (int)widthHeight.y];
        }
        
        newToggleGrid((int)widthHeight.x, (int)widthHeight.y);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Enemy"))
        {
            GetTexture(spawnerGUI);
            spawnerGUI.CreateEnemy(GetSpawnPoint(),GetDirection(GetSpawnPoint()));

            SavedEnemy enemy = new SavedEnemy();
            enemy._moves = _toggle;
            enemy._isRepeating = _repeat;
            enemy._repeatFrequency = _repeatFrequency;
            enemy._stopRepeatBeat = _stopRepeatBeat;
            enemy._board = _board;
            savedEnemies.Add(enemy);

            //_toggle = new bool[(int)widthHeight.x, (int)widthHeight.y];
            _repeat = false;
            NewEnemy(spawnerGUI);
            //_savedEnemy = true;
        }
        if (GUILayout.Button("Save Beat"))
        {
            savedEnemiesByBeat.Add(savedEnemies);
            _firstEnemy = true;
            //_savedEnemy = false;
            spawnerGUI.CreateBeat();
            //_heatMap = GetHeatMapTexture(spawnerGUI);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        if (savedEnemies.Count > 0 && savedEnemies.Count > spawnerGUI.beatNum && savedEnemies[spawnerGUI.beatNum - 1] != null)
        {
            ////Debug.Log(savedEnemies.Count);
            for (int i = 0; i < savedEnemies.Count; i++)
            {
                ////Debug.Log(i);
                GUILayout.BeginVertical();
                if (GUILayout.Button(i.ToString()))
                {
                    SavedEnemy enemy = savedEnemies[i];
                    _toggle = enemy._moves;
                    _repeat = enemy._isRepeating;
                    _repeatFrequency = enemy._repeatFrequency;
                    _stopRepeatBeat = enemy._stopRepeatBeat;
                    _board = enemy._board;
                }
                if (GUILayout.Button("Overwrite Enemy"))
                {
                    GetTexture(spawnerGUI);
                    spawnerGUI.InsertEnemy(GetSpawnPoint(),GetDirection(GetSpawnPoint()),i);
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset All"))
        {
            spawnerGUI.Reset();
            Reset();
        }
        if (GUILayout.Button("Clear Textures"))
        {
            ClearTextures();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        
        //if (_beatTextures.Count > 0 && _beatTextures.Count > spawnerGUI.beatNum - 1 && _beatTextures[spawnerGUI.beatNum - 1] != null)
        if(spawnerGUI.heatMap != null)
        {
            _heatMap = GetHeatMapTexture(spawnerGUI);
            GUILayout.Box(_heatMap);
        }
        if (_beatTextures.Count > 0 && _beatTextures.Count > spawnerGUI.beatNum - 1 && _beatTextures[spawnerGUI.beatNum - 1] != null)
        //if(_beatTextures[spawnerGUI.beatNum - 1] != null && _beatTextures.Count > 0 && _beatTextures.Count > spawnerGUI.beatNum - 1 )
            GUILayout.Box(_beatTextures[spawnerGUI.beatNum - 1]);
        GUILayout.EndHorizontal();
    }

    void NewEnemy(SpawnerGUI spawnerGUI)
    {
        if (_firstEnemy)
        {
            _board = new Texture2D(90, 50);
            _firstEnemy = false;
        }
        _repeatFrequency = 0;
        spawnerGUI.NewEnemy();
        _spawnPoint = 1;
        _toggle = new bool[(int)widthHeight.x, (int)widthHeight.y];
    }
    
    public void newToggleGrid(int width, int height)
    {
        //toggle = bool[width, height];
        _spawnPoint = 1;
        int y = 0;
        while (y < height - 1)
        {
            y++;
            GUILayout.BeginHorizontal();
            int x = 0;
            while (x < width - 1)
            {
                x++;
                if (y == 1 || y == height - 1 || x == 1 || x == width - 1)
                {
                    if (x + y == 2 || x + y == height || x + y == width || x + y == (width - 1) + (height - 1))
                    {
                        if (x == 7)
                        {
                            _toggle[x, y] = GUILayout.Toggle(_toggle[x, y], _spawnPoint.ToString(), GUILayout.Width(40));
                            _spawnPoint++;
                        }
                        else if(x == 5)
                        {
                            _toggle[x, y] = GUILayout.Toggle(_toggle[x, y], _spawnPoint.ToString(), GUILayout.Width(40));
                            _spawnPoint++;
                        }
                        else
                        {
                            GUILayout.Space(43);
                        }
                    }
                    else
                    {
                        _toggle[x, y] = GUILayout.Toggle(_toggle[x, y], _spawnPoint.ToString(), GUILayout.Width(40));
                        _spawnPoint++;
                    }

                }
                else
                {
                    _toggle[x, y] = GUILayout.Toggle(_toggle[x, y], (x - 1).ToString() + "," + (y - 1).ToString(), GUILayout.Width(40));
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    public EnemiesToSpawn.SpawnPoints GetSpawnPoint()
    {
        EnemiesToSpawn.SpawnPoints spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint1;
        
        if (_toggle[2, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint1;
        }
        if (_toggle[3, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint2;
        }
        if (_toggle[4, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint3;
        }
        if (_toggle[5, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint4;
        }
        if (_toggle[6, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint5;
        }
        if (_toggle[7, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint6;
        }
        if (_toggle[8, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint7;
        }
        if (_toggle[9, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint8;
        }
        if (_toggle[10, 1])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint9;
        }
        //-------------------------------------------------------------
        if (_toggle[11, 2])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint10;
        }
        if (_toggle[11, 3])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint11;
        }
        if (_toggle[11, 4])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint12;
        }
        if (_toggle[11, 5])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint13;
        }
        if (_toggle[11, 6])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint14;
        }
        //-------------------------------------------------------------
        if (_toggle[10, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint15;
        }
        if (_toggle[9, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint16;
        }
        if (_toggle[8, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint17;
        }
        if (_toggle[7, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint18;
        }
        if (_toggle[6, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint19;
        }
        if (_toggle[5, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint20;
        }
        if (_toggle[4, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint21;
        }
        if (_toggle[3, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint22;
        }
        if (_toggle[2, 7])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint23;
        }
        //-------------------------------------------------------------
        if (_toggle[1, 6])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint24;
        }
        if (_toggle[1, 5])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint25;
        }
        if (_toggle[1, 4])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint26;
        }
        if (_toggle[1, 3])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint27;
        }
        if (_toggle[1, 2])
        {
            spawnPoint = EnemiesToSpawn.SpawnPoints.Spawnpoint28;
        }

        return spawnPoint;
    }

    public List<EnemyMove> GetDirection(EnemiesToSpawn.SpawnPoints spawnPoint)
    {
        bool turned = false;
        Vector2 dir = new Vector2();
        EnemyMove move = new EnemyMove();
        List<EnemyMove> moves = new List<EnemyMove>();
        int moveNum = -1;

        switch (spawnPoint)
        {
            ////Debug.Log(spawnPoint);
            case EnemiesToSpawn.SpawnPoints.Spawnpoint1:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint2:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint3:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint4:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint5:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint6:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint7:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint8:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint9:
            {
                for (int x = 1; x < 10; x++)
                {
                    if(_toggle[x,1])
                    {
                        //Debug.Log(x);
                        //moveNum++;
                        move.moveDir = Direction.Forward;
                        move.moves = 0;
                        turned = true;
                        dir = Vector2.down;
                        moves.Add(move);
                        Vector2 pos = new Vector2(x, 1);
                        GetTurns(moves, dir, turned, pos, moveNum);
                        
                        //Debug.Log("Moves " + moves.Count);
                        
                        return moves;
                    }
                }
                break;
            }
            case EnemiesToSpawn.SpawnPoints.Spawnpoint15:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint16:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint17:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint18:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint19:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint20:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint21:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint22:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint23:
            {
                for (int x = 1; x < _toggle.GetLength(0) - 1; x++)
                {
                    if(_toggle[x,7])
                    {
                        //Debug.Log(x);
                        //moveNum++;
                        move.moveDir = Direction.Forward;
                        move.moves = 0;
                        turned = true;
                        dir = Vector2.up;
                        moves.Add(move);
                        Vector2 pos = new Vector2(x, 7);
                        GetTurns(moves, dir, turned, pos, moveNum);
                        
                        //Debug.Log("Moves " + moves.Count);
                        
                        return moves;
                    }
                }
                break;
            }
            case EnemiesToSpawn.SpawnPoints.Spawnpoint10:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint11:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint12:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint13:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint14:
            {
                
                for (int y = 1; y < 7; y++)
                {
                    if(_toggle[11,y])
                    {
                        //Debug.Log(y);
                        //moveNum++;
                        move.moveDir = Direction.Forward;
                        move.moves = 0;
                        turned = true;
                        dir = Vector2.left;
                        moves.Add(move);
                        Vector2 pos = new Vector2(11, y);
                        GetTurns(moves, dir, turned, pos, moveNum);
                        
                        //Debug.Log("Moves " + moves.Count);
                        
                        return moves;
                    }
                }
                break;
            }
            case EnemiesToSpawn.SpawnPoints.Spawnpoint24:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint25:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint26:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint27:
            case EnemiesToSpawn.SpawnPoints.Spawnpoint28:
            {
                
                for (int y = 1; y < 7; y++)
                {
                    if (_toggle[1, y])
                    {
                        //Debug.Log(y);
                        ////moveNum++;
                        move.moveDir = Direction.Forward;
                        move.moves = 0;
                        turned = true;
                        dir = Vector2.right;
                        moves.Add(move);
                        Vector2 pos = new Vector2(1, y);
                        GetTurns(moves, dir, turned, pos, moveNum);
                        
                        //Debug.Log("Moves " + moves.Count);
                        
                        return moves;
                    }
                }

                break; 
            }
        }

        return moves;
    }

    public void GetTurns(List<EnemyMove> moves, Vector2 dir, bool turned, Vector2 pos, int moveNum)
    {
        //Debug.Log("Get Turns");
        bool[,] tempGrid = _toggle;
        int extraMoves = 1;
        for (int i = 0; i < extraMoves; i++)
        {
            ////Debug.Log("Turn Num: " + i);
            EnemyMove move = new EnemyMove();
            ////Debug.Log("Dir: " + dir);
            //Debug.Log("Pos: " + pos);
            
            moveNum++;
            
            switch (turned)
            {
                case true when dir == Vector2.up:
                {
                    //Debug.Log("Up");
                    if(tempGrid[(int) pos.x, (int) pos.y - 1])
                    {
                        tempGrid[(int) pos.x, (int) pos.y - 1] = false;
                        pos = new Vector2((int) pos.x, (int) pos.y - 1);
                        //dir = Vector2.up;
                        //Debug.Log("Forward");
                        extraMoves++;
                    }
                    else if(tempGrid[(int) pos.x - 1, (int) pos.y])
                    {
                        tempGrid[(int) pos.x - 1, (int) pos.y] = false;
                        ////Debug.Log("Left " + new Vector2((int) pos.x - 1, (int) pos.y - 1));
                        extraMoves++;
                        pos = new Vector2((int) pos.x - 1, (int) pos.y);
                        
                        move.moveDir = Direction.Left;
                        
                        move.moves = moveNum;

                        dir = Vector2.left;

                        moves.Add(move);
                    }
                    else if(tempGrid[(int) pos.x + 1, (int) pos.y])
                    {
                        tempGrid[(int) pos.x + 1, (int) pos.y] = false;
                        extraMoves++;
                        pos = new Vector2((int) pos.x + 1, (int) pos.y);
                        ////Debug.Log("Right");
                        move.moveDir = Direction.Right;
                        //move.moveDir = Direction.Left;
                        
                        move.moves = moveNum;

                        dir = Vector2.right;

                        moves.Add(move);
                    }
                    else
                    {
                        i++;
                        //Debug.Log("End");
                        break;
                    }
                    break;
                }
                //-----------------------------------------------------------------------------
                case true when dir == Vector2.down:
                {
                    //Debug.Log("Down");
                    if(tempGrid[(int) pos.x, (int) pos.y + 1])
                    {
                        tempGrid[(int) pos.x, (int) pos.y + 1] = false;
                        pos = new Vector2((int) pos.x, (int) pos.y + 1);
                        
                        //Debug.Log("Forward");
                        extraMoves++;
                    }
                    else if(tempGrid[(int) pos.x - 1, (int) pos.y])
                    {
                        tempGrid[(int) pos.x - 1, (int) pos.y] = false;
                        extraMoves++;
                        pos = new Vector2((int) pos.x - 1, (int) pos.y);
                        
                        move.moveDir = Direction.Right;

                        move.moves = moveNum;

                        dir = Vector2.left;

                        moves.Add(move);
                    }
                    else if(tempGrid[(int) pos.x + 1, (int) pos.y])
                    {
                        tempGrid[(int) pos.x + 1, (int) pos.y] = false;
                        extraMoves++;
                        pos = new Vector2((int) pos.x + 1, (int) pos.y);
                        
                        move.moveDir = Direction.Left;

                        move.moves = moveNum;

                        dir = Vector2.right;

                        moves.Add(move);
                    }
                    else
                    {
                        i++;
                        //Debug.Log("End");
                        break;
                    }
                    break;
                }
                //-----------------------------------------------------------------------------
                case true when dir == Vector2.left:
                {
                    //Debug.Log("Left");
                    if(tempGrid[(int) pos.x - 1, (int) pos.y])
                    {
                        tempGrid[(int) pos.x - 1, (int) pos.y] = false;
                        pos = new Vector2((int) pos.x - 1, (int) pos.y);
                        
                        //Debug.Log("Forward");
                        extraMoves++;
                    }
                    else if(tempGrid[(int) pos.x, (int) pos.y - 1])
                    {
                        tempGrid[(int) pos.x, (int) pos.y - 1] = false;
                        extraMoves++;
                        pos = new Vector2((int) pos.x, (int) pos.y - 1);
                        
                        move.moveDir = Direction.Right;

                        move.moves = moveNum;

                        dir = Vector2.up;

                        moves.Add(move);
                    }
                    else if(tempGrid[(int) pos.x, (int) pos.y + 1])
                    {
                        tempGrid[(int) pos.x, (int) pos.y + 1] = false;
                        extraMoves++;
                        pos = new Vector2((int) pos.x, (int) pos.y + 1);
                        
                        move.moveDir = Direction.Left;

                        move.moves = moveNum;

                        dir = Vector2.down;

                        moves.Add(move);
                    }
                    else
                    {
                        i++;
                        //Debug.Log("End");
                        break;
                    }
                    break;
                }
                //-----------------------------------------------------------------------------
                case true when dir == Vector2.right:
                {
                    //Debug.Log("Right");
                    if(tempGrid[(int) pos.x + 1, (int) pos.y])
                    {
                        tempGrid[(int) pos.x + 1, (int) pos.y] = false;
                        pos = new Vector2((int) pos.x + 1, (int) pos.y);
                        
                        //Debug.Log("Forward");
                        extraMoves++;
                    }
                    else if(tempGrid[(int) pos.x, (int) pos.y + 1])
                    {
                        tempGrid[(int) pos.x, (int) pos.y + 1] = false;
                        extraMoves++;
                        pos = new Vector2((int) pos.x, (int) pos.y + 1);
                        
                        move.moveDir = Direction.Right;

                        move.moves = moveNum;

                        dir = Vector2.down;

                        moves.Add(move);
                    }
                    else if(tempGrid[(int) pos.x, (int) pos.y - 1])
                    {
                        tempGrid[(int) pos.x, (int) pos.y - 1] = false;
                        extraMoves++;
                        pos = new Vector2((int) pos.x, (int) pos.y - 1);
                        
                        move.moveDir = Direction.Left;

                        move.moves = moveNum;

                        dir = Vector2.up;

                        moves.Add(move);
                    }
                    else
                    {
                        i++;
                        //Debug.Log("End");
                        break;
                    }
                    break;
                }
            }
        }
    }
    
    public Texture2D GetHeatMapTexture(SpawnerGUI spawnerGUI)
    {
        Texture2D map = new Texture2D(90, 50);
        spawnerGUI.heatMap.CreateHeatMap();
        
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                map.SetPixel(x,-y - 1, new Color(
                        spawnerGUI.heatMap.heatMaps[spawnerGUI.beatNum].heatMapOutput[(x / 10), (y / 10)].g,
                        spawnerGUI.heatMap.heatMaps[spawnerGUI.beatNum].heatMapOutput[(x / 10), (y / 10)].b,
                        0,
                        1));
                //map.SetPixel(x,y,heatMap.GetPixel((x / 10), (y / 10)));
            }
        }
        map.Apply();
        return map;
    }
    
    public void GetTexture(SpawnerGUI spawnerGUI)
    {
        if(spawnerGUI.beatNum > _beatTextures.Count)
        {
            for (int i = 0; i < (spawnerGUI.beatNum); i++)
            {
                _beatTextures.Insert(i, new Texture2D(90, 50));
            }
        }
        
        for (int x = 0; x < _board.width; x++)
        {
            for (int y = 0; y < _board.height; y++)
            {
                //board.SetPixel(x,y, Color.black);
                if (_toggle[(x / 10) + 2, (y / 10) + 2])
                {
                    _board.SetPixel(x,-y - 1,Color.red);
                }
            }
        }
        _board.Apply();

        ////Debug.Log(_beatTextures.Count);
        _beatTextures[spawnerGUI.beatNum - 1] = _board;
    }
    
    public void ClearTextures()
    {
        _beatTextures.Clear();
        _beatTextures = new List<Texture2D>();
    }

    public void Reset()
    {
        _repeatFrequency = 0;
        _stopRepeatBeat = 0;
        _repeat = false;
        //_stopRepeat = false;
        _firstEnemy = true;
        //_savedEnemy = false;

        _spawnPoint = 1;
        widthHeight = new Vector2(12, 8);
        _toggle = new bool[(int)widthHeight.x, (int)widthHeight.y];

        savedEnemiesByBeat = new List<List<SavedEnemy>>();
        savedEnemies = new List<SavedEnemy>();


        _beatTextures = new List<Texture2D>();
        _board = new Texture2D(90, 50);
    }
    
    [Serializable]
    public class SavedEnemy
    {
        public bool[,] _moves = new bool[12,8];
        public bool _isRepeating = false;
        public int _repeatFrequency = 0;
        public int _stopRepeatBeat = 0;
        public Texture2D _board;
    }
}
