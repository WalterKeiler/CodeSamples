using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

// Neon Driftersprivate

// This script contains the movemnt logic for the player chararcter for Neon Drifters

// All code written by Walter Keiler 2022

// I am using MonoBehaviourPunCallbacks so that we can access Photon RPC calls
public class MovementLogic : MonoBehaviourPunCallbacks
{
    float lerpSpeed = .15f;
    float turnSmoothing = 20f;
    float gravityForce = 60f;
    public string velocity;

    float delta = 0;
    protected float sideRaycastMaxDst = 5f;
    
    protected Vector3 inputs;
    private float driftFactor = 0;

    [Header("Movement Vars")]
    [SerializeField]
    private float speed = 100f;
    [SerializeField]
    private float maxSpeed = 500f;
    [SerializeField]
    private float breakForce = 1f;
    [SerializeField]
    private float turnSpeed = 4f;
    [SerializeField]
    private float driftFactorSick = .7f;
    [SerializeField]
    private float driftFactorSlide = .4f;
    [SerializeField]
    private float maxTractionVelocity = 2f;
    [SerializeField]
    private float gravity = -10f;
    [SerializeField]
    private float hoverDistance = 1f;
    [SerializeField]
    private float hoverCorrectionSpeed = 1f;
    
    
    [Header("Other Vars")]
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private Transform forcePos;
    [SerializeField]
    private Transform turnPos;
    [SerializeField]

    private LayerMask groundMask;

    [SerializeField]
    private bool grounded = true;

    public enum DriftState
    {
        Drifting,
        NotDriting
    };
    public DriftState state;
    
    public Vector3 GetInput()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputCombined = new Vector3(x, 0, z);

        return inputCombined;
    }

    // This contains all the logic for actually moving the car
    protected void Move()
    {
        grounded = Physics.Raycast(turnPos.position, -turnPos.up, hoverDistance + 1, groundMask, QueryTriggerInteraction.Ignore);

        if (RightVelocity(rb).magnitude > maxTractionVelocity)
        {
            float Lerp = lerpSpeed * Time.deltaTime;
            driftFactor = Mathf.Lerp(driftFactor, driftFactorSlide, lerpSpeed);
            state = DriftState.Drifting;
        }
        else
        {
            float Lerp = lerpSpeed * Time.deltaTime;
            driftFactor = Mathf.Lerp(driftFactor, driftFactorSick, lerpSpeed);
            state = DriftState.NotDriting;
        }

        if (inputs.z > 0)
        {
            rb.AddForce((transform.forward - transform.up.normalized) * inputs.z * speed); 
        }

        else if(inputs.z < 0)
        {
            rb.AddForce((transform.forward + transform.up.normalized) * inputs.z * speed);
            inputs.x *= -1;
        }
        
        float tf = Mathf.Lerp(0, turnSpeed, rb.velocity.magnitude / 30);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            driftFactor = driftFactorSlide;
            rb.velocity = rb.velocity / (breakForce);
        }

        float lerp = turnSmoothing * Time.deltaTime;
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, transform.up * inputs.x * tf, lerp);

        if (grounded)
        {
            Vector3 rtVel;
            if (inputs.z <= -0f)
            {
                rtVel = RightVelocity(rb);
            }
            else
            {
                rtVel = RightVelocity(rb);
            }
            rb.velocity = ForwardVelocity(rb) + rtVel * driftFactor;

        }

        if (grounded)
        {
            rb.velocity = rb.velocity;
        }

        velocity = rb.velocity.magnitude.ToString();
    }

    Vector3 ForwardVelocity(Rigidbody rb)
    {
        return transform.forward * Vector3.Dot(rb.velocity, transform.forward);
    }

    Vector3 RightVelocity(Rigidbody rb)
    {
        return transform.right * Vector3.Dot(rb.velocity, transform.right);
    }

    // This function is responsible for checking all around the car and finding if there are any collisions
    // and then returning the normal of all of the hit points to affect the orientation of the car
    public Vector3 HitNorm()
    {
        Ray fwrdRay = new Ray(transform.position, new Vector3(1, -1, 0).normalized);
        Ray backRay = new Ray(transform.position, new Vector3(-1, -1, 0).normalized);
        Ray lftRay = new Ray(transform.position, new Vector3(0, -1, 1).normalized);
        Ray rtRay = new Ray(transform.position, new Vector3(0, -1, -1).normalized);
        
        Ray fwrdLftRay = new Ray(transform.position, new Vector3(1, -1, 1).normalized);
        Ray fwrdRtRay = new Ray(transform.position, new Vector3(1, -1, -1).normalized);
        Ray backLftRay = new Ray(transform.position, new Vector3(-1, -1, 1).normalized);
        Ray backRtRay = new Ray(transform.position, new Vector3(-1, -1, -1).normalized);
        Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfoMid, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore);
        Physics.Raycast(fwrdRay, out RaycastHit hitInfoFwrd, sideRaycastMaxDst, groundMask, QueryTriggerInteraction.Ignore);
        Physics.Raycast(backRay, out RaycastHit hitInfoBack, sideRaycastMaxDst, groundMask, QueryTriggerInteraction.Ignore);
        Physics.Raycast(lftRay, out RaycastHit hitInfoLft, sideRaycastMaxDst, groundMask, QueryTriggerInteraction.Ignore);
        Physics.Raycast(rtRay, out RaycastHit hitInfoRt, sideRaycastMaxDst, groundMask, QueryTriggerInteraction.Ignore);
        
        Physics.Raycast(fwrdLftRay, out RaycastHit hitInfoFwrdLft, sideRaycastMaxDst, groundMask, QueryTriggerInteraction.Ignore);
        Physics.Raycast(fwrdRtRay, out RaycastHit hitInfoFwrdRt, sideRaycastMaxDst, groundMask, QueryTriggerInteraction.Ignore);
        Physics.Raycast(backLftRay, out RaycastHit hitInfoBackLft, sideRaycastMaxDst, groundMask, QueryTriggerInteraction.Ignore);
        Physics.Raycast(backRtRay, out RaycastHit hitInfoBackRt, sideRaycastMaxDst, groundMask, QueryTriggerInteraction.Ignore);

        Vector3 hitInfoAvg = (hitInfoMid.normal + hitInfoBack.normal + hitInfoFwrd.normal + hitInfoLft.normal +
                              hitInfoRt.normal + hitInfoFwrdLft.normal + hitInfoFwrdRt.normal + hitInfoBackLft.normal + hitInfoBackRt.normal)
                             / 9;
        return hitInfoAvg;
    }
    
    // This gives the car a custom gravity direction based on our orientation
    public void Gravity()
    {

        Vector3 bodyUp = transform.up;

        Vector3 hitInfo = HitNorm();
        Vector3 targetDir = transform.up;
        if (!grounded)
        {
            delta += (Time.deltaTime / 30);
            targetDir = Vector3.Lerp(targetDir, Vector3.up, delta).normalized;
            
            if (delta > 1)
            {
                delta = 0;
            }
        }
        else
        {
            targetDir = hitInfo.normalized;
        }
        
        float lerp =+ 10 * Time.deltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.FromToRotation(bodyUp, targetDir) * transform.rotation, lerp) ;

        if (lerp > 1)
        {
            lerp = 0;
        }
        
        rb.AddForce(targetDir * gravity);
    }

    // this just keeps the car a certain distance from the ground
    public void Hover()
    {
        Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore);
        float dst = hitInfo.distance;
        float diffrence = dst - hoverDistance;

        if (dst != hoverDistance && grounded)
        {
            rb.velocity = rb.velocity + (-hitInfo.normal.normalized * diffrence * hoverCorrectionSpeed * Time.deltaTime);
        }
    }
}