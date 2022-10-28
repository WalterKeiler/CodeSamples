using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

//[CreateAssetMenu(fileName = "CarBaseMovement", menuName = "Car Logic/Base Movement")]
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
    public float speed = 100f;
    [SerializeField]
    public float maxSpeed = 500f;
    [SerializeField]
    public float breakForce = 1f;
    [SerializeField]
    public float turnSpeed = 4f;
    [SerializeField]
    public float driftFactorSick = .7f;
    [SerializeField]
    public float driftFactorSlide = .4f;
    [SerializeField]
    public float maxTractionVelocity = 2f;
    [SerializeField]
    public float gravity = -10f;
    [SerializeField]
    public float hoverDistance = 1f;
    [SerializeField]
    public float hoverCorrectionSpeed = 1f;
    
    
    [Header("Other Vars")]
    [SerializeField]
    public Rigidbody rb;

    [SerializeField]
    public Transform forcePos;
    [SerializeField]
    public Transform turnPos;
    [SerializeField]
    //public Collider groundCheck;
    public LayerMask groundMask;

    [SerializeField]
    public bool grounded = true;

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

    protected void Move()
    {
        grounded = Physics.Raycast(turnPos.position, -turnPos.up, hoverDistance + 1, groundMask, QueryTriggerInteraction.Ignore);

        //Debug.Log(inputs.x);
        //Debug.Log(RightVelocity(rb).magnitude);

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

        //Debug.Log("Drift Factor: " + driftFactor);

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

        //if(inputs.x == 0)
        //rb.angularVelocity = Vector3.zero;

        //rb.AddForceAtPosition(Vector3.up * inputs.x * tf, turnPos.localPosition, ForceMode.VelocityChange);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            driftFactor = driftFactorSlide;
            rb.velocity = rb.velocity / (breakForce);
        }

        float lerp = turnSmoothing * Time.deltaTime;
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, transform.up * inputs.x * tf, lerp);
        //rb.angularVelocity = Vector3.up * inputs.x * tf;
        //Debug.Log(rb.angularVelocity);
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
/*
        if (!Physics.Raycast(turnPos.position, -turnPos.up, 1f, groundMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Front Up");
            rb.AddForceAtPosition(-turnPos.up * gravityForce, turnPos.position);
        }
        if (!Physics.Raycast(forcePos.position, -forcePos.up, 1f, groundMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Back Up");
            rb.AddForceAtPosition(-forcePos.up * gravityForce, forcePos.position);
        }
*/
    }

    Vector3 ForwardVelocity(Rigidbody rb)
    {
        return transform.forward * Vector3.Dot(rb.velocity, transform.forward);
    }

    Vector3 RightVelocity(Rigidbody rb)
    {
        return transform.right * Vector3.Dot(rb.velocity, transform.right);
    }

    public void CheckGrounded()
    {
        //rb.AddForce(-transform.up * gravityForce, ForceMode.Force);
        /*
        if(!Physics.Raycast(groundCheck.position, Vector3.down, 1f))
        {
            Debug.Log("notGrounded");
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
        */
    }
    
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
    
    public void Gravity()
    {
        //Vector3 targetDir = (body.position - transform.position).normalized;
        Vector3 bodyUp = transform.up;

        //Ray ray = new Ray(transform.position, -transform.up);
        
        //Physics.SphereCast(ray, 1, out RaycastHit hitInfo, hoverDistance + 2, groundMask, QueryTriggerInteraction.Ignore);
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
    
    /*
    public void MoveNonRB(Transform transform, float m_Speed, float m_MaxSpeed, float m_TurnSpeed)
    {
        inputs = GetInput();
        //Vector3 direction = GetDirection(inputs);
        float speed = inputs.z * m_Speed * Time.deltaTime;
        speed = Mathf.Clamp(speed, 0, m_MaxSpeed);
        transform.Translate(Vector3.forward * speed);

        Coroutine vel = StartCoroutine(GetVelocity(inputs.z));

        transform.Rotate(Vector3.up * inputs.x * (speed * m_TurnSpeed));
        //transform.rotation = Quaternion.Euler(0, inputs.x * 90 * Time.deltaTime, 0);

        if(traction < 1)
        {
            Drift(inputs, speed);
        }
    }

        public IEnumerator GetVelocity(float interval)
    {
        float vel = 0;
        Vector3 pos1 = transform.position;
        yield return new WaitForSeconds(.05f);
        vel = Vector3.Distance(pos1, transform.position);
        velocity = vel;
        //return vel;
    }

        public Vector3 GetDirection(Vector3 input)
    {
        float tan = Mathf.Tan(Vector3.Angle(Vector3.forward * input.z, input));

        return new Vector3(tan,0,1);
    }

    public void Drift(Vector3 input, float speed)
    {
        //float tan = Mathf.Tan(Vector3.Angle(Vector3.forward * input.z, input));
        transform.Translate(Vector3.right * input.x * speed / traction);
        //transform.rotation = Quaternion.Euler(Vector3.up * (transform.rotation.y * (-input.x * traction)));
    }
    */

    private void OnDrawGizmos()
    {
        //Gizmos.DrawLine(turnPos.position, turnPos.position - (Vector3.up * 1.1f));
        //Gizmos.DrawLine(forcePos.position, forcePos.position - (Vector3.up * 1.1f));
    }
}