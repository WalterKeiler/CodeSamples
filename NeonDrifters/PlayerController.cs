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

// Neon Driftersprivate

// This script contains the player interaction and Photon implementation for the player

// All code written by Walter Keiler 2022

// I likely did not need to use inheritance for this system but it seems to work out fairly well
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
    
    // Set all the inital refrences and UI if this is the local player
    public void Awake()
    {
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
            cameraActive = Instantiate(cameraPrefab, transform.position, Quaternion.identity, null);
            playerUIActive = Instantiate(playerUIPrefab, transform.position, Quaternion.identity, null);
            speedMeter = playerUIActive.GetComponentsInChildren<Image>()[2];
            speedText = playerUIActive.GetComponentInChildren<TMP_Text>();
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // Set a few more refrences
    public void Start()
    {
        
        if (photonView.IsMine)
        {
            CameraController cam = cameraActive.GetComponentInChildren<CameraController>();
            if (cam != null)
            {
                cam.target = this.transform;
                cam.isFollowing = true;
            }
        }

    }

    // Do the basic movement logic 
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

    // Get player input and update UI elements
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

    // Logic for updating checkpoints and future scoreboard
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            mostRecentCheckpoint = other.transform;
            numberOfCheckpointsPassed++;
            photonView.RPC("RPC_UpdateScoreboard",RpcTarget.AllBuffered,new string(photonView.Owner.NickName.ToCharArray()), numberOfCheckpointsPassed);
        }
    }

    [PunRPC]
    void RPC_UpdateScoreboard(string name, int playerCheckpointNum)
    {
        // Future scoreboard implementation
    }
}
