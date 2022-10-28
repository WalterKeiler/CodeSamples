using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grapple : MonoBehaviour
{
    [Header("Grapple Variables")]
    public KeyCode grappleKey;
    public Transform grappleSpawn;
    public Camera cam;
    public Transform grapple;
    public float distance;
    public LayerMask grappleMask;
    public LayerMask blockGrapple;
    public float hitDis;
    //public GameObject player;
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
    public bool connect;
    Quaternion grappleStart;

    [Header("visual Variables")]
    public LineRenderer line;
    public GameObject grappleIndicator;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        grappleStart = grapple.localRotation;
    }

    void Update()
    {
        if(Cursor.lockState == CursorLockMode.None) return;
        
        if (Input.GetKeyDown(grappleKey))
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, distance, grappleMask, QueryTriggerInteraction.Ignore))
            {
                if (( blockGrapple & (1 << hit.transform.gameObject.layer)) == 0)
                {
                    hitPoint = hit.point;
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
            //connect = (Physics.Raycast(ray, out hit, distance, grappleMask, QueryTriggerInteraction.Ignore));

        }
        if (connect)
        {
            if (Input.GetKey(grappleKey))
            {
                line.enabled = true;
                line.SetPosition(1, hitPoint);
                line.SetPosition(0, grappleSpawn.position);
                grapple.LookAt(hitPoint);
            }
            if (Input.GetKeyDown(grappleKey))
            {
                PlayerAudioManager.CallOnPlayGrappleAudio();
                
                joint = gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = hitPoint;

                joint.maxDistance = hitDis * maxMult;
                joint.minDistance = hitDis * minMult;

                //Testing Zone
                joint.axis = Vector3.up;

                joint.spring = spring;// / Time.deltaTime / 25;
                joint.damper = damp;//* Time.deltaTime * 100;
                joint.massScale = massScl;

                joint.tolerance = tolerance;
                joint.breakForce = breakForce;
                joint.breakTorque = breakTorque;
            }
        }
        if (Input.GetKeyUp(grappleKey))
        {
            GrappleDisconnect();
        }

        Ray ray1 = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit1;

        if (Physics.Raycast(ray1, out hit1, distance, grappleMask, QueryTriggerInteraction.Ignore) && connect == false)
        {
            //reticle.SetActive(true);
            grappleIndicator.GetComponent<Image>().color = 
                (blockGrapple & (1 << hit1.transform.gameObject.layer)) == 0 ? Color.green : Color.gray;
        }
        else
        {
            //reticle.SetActive(false);
            grappleIndicator.GetComponent<Image>().color = Color.gray;
        }
    }

    public void GrappleDisconnect()
    {
        rb.mass = 1.5f;
        Destroy(joint);
        connect = false;
        line.enabled = false;
        grapple.localRotation = grappleStart;
    }
}
