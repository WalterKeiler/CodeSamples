using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Grapple Grub

// This script holds all the grappling hook logic

// All code written by Walter Keiler 2022

public class Grapple : MonoBehaviour
{

#region Grapple Variables

    [Header("Grapple Variables")]
    public KeyCode swingGrappleKey;
    public KeyCode pullGrappleKey;
    public Transform grappleSpawn;
    public Camera cam;
    public Transform grapple;
    public float distance;
    public float pullSpeed;
    public LayerMask grappleMask;
    public LayerMask blockGrapple;
    public float hitDis;
    public bool connect;

#endregion

#region Joint Variables

    [Header("Joint Variables")]
    public float maxMult = .8f;
    public float minMult = .25f;
    public float damp = 5f;
    public float spring = 4.5f;
    public float massScl = 4.5f;
    public float tolerance = .025f;
    public float breakForce = Mathf.Infinity;
    public float breakTorque = Mathf.Infinity;
    SpringJoint joint;
    public Vector3 hitPoint;
    private Transform hitObject;
    public Vector3 localPoint;
    Quaternion grappleStart;

#endregion

#region Visual Variables

    [Header("visual Variables")]
    public LineRenderer line;
    public GameObject grappleIndicator;

#endregion

#region Misc Veriables

    private PlayerManager pm;
    Rigidbody rb;

#endregion

// Setup all the variables and refrences
#region Setup

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerManager>();
    }

    void Start()
    {
        grappleStart = grapple.localRotation;
    }

#endregion

#region Main Logic

    // We are calling all of our relevant functions if the cursor is looked in and the game is running
    void Update()
    {
        if(Cursor.lockState == CursorLockMode.None) return;

        InputLogic();

        GrappleLogic();
        
        if (Input.GetKeyUp(swingGrappleKey) || (Input.GetKeyUp(pullGrappleKey) && pm.pullGrapple))
        {
            GrappleDisconnect();
        }

        Visuals();
    }

    // Get all input and if you have a valid grapple target grapple to it
    void InputLogic()
    {
        if (((Input.GetKeyDown(swingGrappleKey) && !Input.GetKey(pullGrappleKey)) ||
             (Input.GetKeyDown(pullGrappleKey) && !Input.GetKey(swingGrappleKey))) && !connect)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, distance, grappleMask, QueryTriggerInteraction.Ignore))
            {
                if (( blockGrapple & (1 << hit.transform.gameObject.layer)) == 0)
                {
                    hitPoint = hit.point;
                    hitObject = hit.transform;
                    hitDis = hit.distance;
                    Debug.Log(1 << hit.transform.gameObject.layer);
                    Debug.Log(hit.transform.gameObject.layer);
                    Debug.DrawLine(ray.origin, hit.point, Color.yellow, 100);
                    connect = true;
                }
                else
                {
                    connect = false;
                }
            }
            else
            {
                connect = false;
            }
        }
    }

    // Main logic for setting up the joint and breaking off the grapple when you let go
    void GrappleLogic()
    {
        if (connect)
        {
            if(hitObject == null)
            {
                GrappleDisconnect();
                return;
            }
            
            if ((Input.GetKeyDown(swingGrappleKey) && !Input.GetKey(pullGrappleKey)))
            {
                SetupSwingGrapple();
            }
            
            if ((!Input.GetKey(swingGrappleKey) && Input.GetKeyDown(pullGrappleKey)) && pm.pullGrapple)
            {
                SetupPullGrapple();
            }
            
            Vector3 actualPoint = hitObject.TransformPoint(localPoint);
            
            if (Input.GetKey(swingGrappleKey))
            {
                line.enabled = true;
                line.SetPosition(1, actualPoint);
                line.SetPosition(0, grappleSpawn.position);
                grapple.LookAt(actualPoint);
                CheckLine();
                joint.connectedAnchor = actualPoint;
            }

            if (Input.GetKey(pullGrappleKey) && pm.pullGrapple)
            {
                line.enabled = true;
                line.SetPosition(1, actualPoint);
                line.SetPosition(0, grappleSpawn.position);
                grapple.LookAt(actualPoint);
                CheckLine();
                joint.connectedAnchor = actualPoint;
                joint.maxDistance -= Time.deltaTime * pullSpeed;
            }

        }
    }
    
    // Setup the Spring Joint for swinging and all of the values related to it
    void SetupSwingGrapple()
    {
        PlayerAudioManager.CallOnPlayGrappleAudio();
        localPoint = hitObject.InverseTransformPoint(hitPoint);
        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = hitPoint;

        joint.maxDistance = hitDis * maxMult;
        joint.minDistance = hitDis * minMult;

        //Testing Zone
        joint.axis = Vector3.up;

        joint.spring = spring;
        joint.damper = damp;
        joint.massScale = massScl;

        joint.tolerance = tolerance;
        joint.breakForce = breakForce;
        joint.breakTorque = breakTorque;
    }

    // Setup the Spring Joint for pulling and all of the values related to it
    void SetupPullGrapple()
    {
        PlayerAudioManager.CallOnPlayGrappleAudio();
        localPoint = hitObject.InverseTransformPoint(hitPoint);
        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = hitPoint;

        joint.maxDistance = hitDis * maxMult;
        joint.minDistance = 1;

        //Testing Zone
        joint.axis = Vector3.up;

        joint.spring = spring;
        joint.damper = 0;
        joint.massScale = massScl;

        joint.tolerance = 0;
        joint.breakForce = breakForce;
        joint.breakTorque = breakTorque;
        
        rb.velocity = Vector3.zero;
    }
    
    // Check to see if an object is breaking ine of sight between the player and the grapple point
    public void CheckLine()
    {
        if(Physics.Raycast(grapple.position, grapple.forward, out RaycastHit hitInfo, Mathf.Infinity, grappleMask,
            QueryTriggerInteraction.Ignore))
        {
            if ((blockGrapple & (1 << hitInfo.transform.gameObject.layer)) != 0)
            {
                GrappleDisconnect();
            }
        }
        else
        {
            GrappleDisconnect();
        }
    }
    
    // Disconnect the grapple and destroy the joint
    public void GrappleDisconnect()
    {
        rb.mass = 1.5f;
        foreach (var joint in GetComponents<Joint>())
        {
            Destroy(joint);
        }
        connect = false;
        line.enabled = false;
        grapple.localRotation = grappleStart;
    }

    // Set the line to the grapple point and grapple gun
    void Visuals()
    {
        Ray ray1 = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit1;

        if (Physics.Raycast(ray1, out hit1, distance, grappleMask, QueryTriggerInteraction.Ignore) && connect == false)
        {
            
            grappleIndicator.GetComponent<Image>().color = 
                (blockGrapple & (1 << hit1.transform.gameObject.layer)) == 0 ? Color.green : Color.gray;
        }
        else
        {
            grappleIndicator.GetComponent<Image>().color = Color.gray;
        }
    }

#endregion

}
