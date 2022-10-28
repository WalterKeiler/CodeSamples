using System;
using System.Collections;
using System.Collections.Generic;
using Com.SuperSalteGames.NeonDrifter;
using Photon.Pun;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MovementLogic
{

    [Header("UI")]
    [SerializeField]
    public Image speedMeter;
    [SerializeField] 
    public TMP_Text speedText;

    [Header("Photon Stuff")] [SerializeField]
    private float fallenHeight;
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    [SerializeField] 
    private GameObject cameraPrefab;
    [HideInInspector]
    public GameObject cameraActive;
    
    [SerializeField] 
    private GameObject playerUIPrefab;
    [HideInInspector]
    public GameObject playerUIActive;

    [Header("Race Stuff")] [SerializeField]
    private Transform mostRecentCheckpoint;

    public int numberOfCheckpointsPassed = 0;

    private float fallTimer = 3;
    float timer = 0; 
    public void Awake()
    {
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
            cameraActive = Instantiate(cameraPrefab, transform.position, Quaternion.identity, null);
            playerUIActive = Instantiate(playerUIPrefab, transform.position, Quaternion.identity, null);
            speedMeter = playerUIActive.GetComponentsInChildren<Image>()[2];
            speedText = playerUIActive.GetComponentInChildren<TMP_Text>();
            //RaceManager.Instance.StartScoreBoard(playerUIActive);
            //rbIn = GetComponent<Rigidbody>();
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_UpdatePlayers", RpcTarget.AllBuffered);
            
            
            
            CameraController cam = cameraActive.GetComponentInChildren<CameraController>();
            if (cam != null)
            {
                cam.target = this.transform;
                cam.isFollowing = true;
            }
        }

    }

    public void FixedUpdate()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        if (photonView.IsMine)
        {
            if (!grounded)
            {
                CheckGrounded();
            }
            Move();
            Gravity();
            Hover();   
        }

    }

    public void Update()
    {


        if (photonView.IsMine)
        {
            inputs = GetInput();
            float speedLocal = 0;
            if (rb != null)
            {
                speedLocal = rb.velocity.magnitude;
            }
            //float lerp = Mathf.c
            speedMeter.fillAmount = Mathf.LerpUnclamped(0f, .25f, speedLocal/110);
            speedMeter.color = Color.Lerp(Color.green, Color.red, speedLocal/120);
            speedText.text = Mathf.RoundToInt(speedLocal).ToString();

            if (!grounded)
            {
                timer += Time.deltaTime;
                if (timer >= fallTimer)
                {
                    rb.velocity = Vector3.zero;
                    transform.position = mostRecentCheckpoint.position;
                    transform.rotation = mostRecentCheckpoint.rotation;
                    timer = 0;
                }
            }
            else
            {
                timer = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)// && other.name != mostRecentCheckpoint.name)
        {
            mostRecentCheckpoint = other.transform;
            numberOfCheckpointsPassed++;
            photonView.RPC("RPC_UpdateScoreboard",RpcTarget.AllBuffered,new string(photonView.Owner.NickName.ToCharArray()), numberOfCheckpointsPassed);
        }
    }

    [PunRPC]
    void RPC_UpdateScoreboard(string name, int playerCheckpointNum)
    {
        //RaceManager.Instance.UpdateScoreBoard(name, playerCheckpointNum);
    }
    
    [PunRPC]
    void RPC_UpdatePlayers()
    {
        //GameObject player = this.gameObject;
        //RaceManager.Instance.players.Clear();
        //RaceManager.Instance.UpdatePlayers(player);
    }
}
