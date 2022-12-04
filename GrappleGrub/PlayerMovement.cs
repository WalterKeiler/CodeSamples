using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;

// Grapple Grub

// This script holds all the player movement and related logic

// All code written by Walter Keiler 2022

public class PlayerMovement : MonoBehaviour
{
    
#region Misc Vars

    [Header("Generic Variables")]
    public Transform playerCam;
    public Transform orientation;

    public Rigidbody rb;
    
    //rotation and Camera
    float xRot;
    float sensitivity = 50f;
    float sensMult = 1f;
    
    //input
    float x, y;
    private Vector3 inputVector;
    bool jumping, sliding;
    public bool dash;
    
    Grapple grapple;
    float timer;

    public bool debugUI = false;

    //Particles
    public ParticleSystem particle;

#endregion

#region Movement Variables

    //move
    [Header("Movement Variables")]
    public float currentSpeed;
    public TMP_Text speed;
    [Tooltip("How fast the character accelerates")]
    public float moveSpeed = 4500;
    public float maxGroundSpeed = 20;
    public float maxSpeed = 45;
    public bool isNormalized = true;
    [Range(0,1),Tooltip("Percent of movement control you get in the air")]
    public float airMovementMultiplier;
    public float gravity = 10;
    [Tooltip("Used for downward velocity when jumping")]
    public float gravityMultiplier = 1;
    public bool grounded;
    public LayerMask ground;
    [Range(0,1),Tooltip("How fast the character slows down while grounded")]
    public float counterMove = .175f;
    public float airBreakForce = .175f;
    float threshold = .01f;
    public float maxSlopeAngle = 35f;
    Vector2 mag;
    public float stepSpeed = 0.2f;
    private float stepTimer;

#endregion

#region Jump Variables

    //jump
    [Header("Jump Variables")]
    bool rdyToJump = true;
    float jumpCooldown = .25f;
    public float jumpForce = 550f;

#endregion

#region Slide Variables

    //Slide
    [Header("Slide Variables")]
    public float startSlideSpeedThreshold;
    public float stopSlideSpeedThreshold;
    [Range(0,1)]
    public float slideDrag;
    public float maxSlideSpeed;
    public float slideHeight;
    public float slideJumpMultiplier;
    public float slidingGravity;
    public float slidingStrafeClamp;

    [SerializeField] private LayerMask stopSlide;
    [SerializeField] Transform slideCheck;
    public bool isSliding;

    private CapsuleCollider PlayerCol;
    private float startCounterMove, startHeight, startMaxSpeed;
    Vector3 normalVector = Vector3.up;

#endregion

#region Wall Running Variables

    //Wall Running
    [Header("Wall Running Variables")]
    public Transform wallCheckLft;
    public Transform wallCheckRt;
    public float wallDistance;
    public float wallRunningGavity;
    public float wallRunSlipModifier;
    public float wallRunPostGrappleSlipModifier;
    public float wallJumpCoolDown;
    public float wallJumpUpwardMultiplier;
    public float wallJumpOutwardMultiplier;
    public LayerMask wallMask;
    public bool isWallRunningL;
    public bool isWallRunningR;
    public TrailRenderer lftTrail, rtTrail;
    bool canWallJump = true;
    private bool isWallRunning;
    private GameObject previousWall;
    RaycastHit lftHit;
    RaycastHit rtHit;
    private bool onWall;
    private bool hasGrappled = false;

#endregion

#region Future Vars

    // Futrue movement abilities
    //Dash
    //[Header("Dash Variables")]
    //bool rdyToDash = true;
    //public float dashCooldown;
    //public float dashSpeed;

#endregion

// Here we setup our variables 
#region Setup
    void Awake()
    {
       rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
       debugUI = true;
       
       startCounterMove = counterMove;
       
       PlayerCol = GetComponent<CapsuleCollider>();
       grapple = gameObject.GetComponent<Grapple>();
    }
#endregion

// This is the heart of the script where all of our main functions are called
#region Update Loops

    private void FixedUpdate()
    {
        if (GameManager.StartGame)
        {
            Movement();
            if(!grounded) NotGrounded();   
        }
    }

    private void Update()
    {
        Effects();
        MyInput();
        Look();
    }

#endregion

// In this region we have our functions called in Update
#region Update Functions

    // Effects controls the speed line effects that appear when you go fast
    private void Effects()
    {
       currentSpeed = rb.velocity.magnitude;
       
       if (currentSpeed >= 21)
       {
           var em = particle.emission;
           em.enabled = true;
           em.rateOverTime = currentSpeed / 1.5f;
       }
       if (currentSpeed < 21)
       {
           var em = particle.emission;
           em.enabled = false;
           em.rateOverTime = 10;
       }
    }

    // In MyInput we are taking in play input and turning it into useful information as well as checking if we are wall running
    private void MyInput()
    {
       if (Input.GetKeyDown(KeyCode.Escape))
       {
           if(Cursor.lockState == CursorLockMode.None)
           {
               debugUI = true;
               Cursor.lockState = CursorLockMode.Locked;
               counterMove = startCounterMove;
               Cursor.visible = false;
           }
           else
           {
               debugUI = false;
               Cursor.lockState = CursorLockMode.None;
               grapple.GrappleDisconnect();
               Cursor.visible = true;
           }
       }
      
       if(Cursor.lockState == CursorLockMode.None) return;
      
       x = Input.GetAxisRaw("Horizontal");
       y = Input.GetAxisRaw("Vertical");

       inputVector = new Vector3(x,0, y);
       
       if(isNormalized)
       {
           x = inputVector.normalized.x;
           y = inputVector.normalized.z;
       }
       
       jumping = Input.GetButton("Jump");
       sliding = Input.GetKey(KeyCode.LeftShift);

       isWallRunningR = Physics.CheckBox(wallCheckRt.position, new Vector3 (.25f,.55f,.05f), Quaternion.identity, wallMask);
       isWallRunningL = Physics.CheckBox(wallCheckLft.position, new Vector3 (.25f,.55f,.05f), Quaternion.identity, wallMask);

       if ((isWallRunningL || isWallRunningR) && !sliding)
       {
           isWallRunning = true;
       }
       else
       {
           onWall = false;
           isWallRunning = false;
       }
    }
    
    // Look controls the camera and making it look in the right direction
    float desiredX;
    private void Look()
    {
        if(Cursor.lockState == CursorLockMode.None) return;
      
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMult;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMult;

        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        playerCam.transform.localRotation = Quaternion.Euler(xRot, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

#endregion

// This is all of our movement logic and movement related abilities
#region Movement and Abilities

    // Movement holds our main movement logic focusing on running and logic to swap to other movement styles
    private void Movement()
    {
       Gravity();

       mag = FindVelRelativeToLook();
       float xMag = mag.x, yMag = mag.y;

       counterMovement(x, y, mag);

       if (rdyToJump && jumping && !isSliding) Jump();

       float maxSpeed = this.maxGroundSpeed;

       if (x > 0 && xMag > maxSpeed) x = 0;
       if (x < 0 && xMag < -maxSpeed) x = 0;
       if (y > 0 && yMag > maxSpeed) y = 0;
       if (y < 0 && yMag < -maxSpeed) y = 0;

       float multiplier = 1f, multiplierV = 1f;

       if (!grounded)
       {
           multiplier = airMovementMultiplier;
           float facingVelocity = (new Vector3(Mathf.Abs(transform.forward.normalized.x),0,Mathf.Abs(transform.forward.normalized.z))
                                   - new Vector3(Mathf.Abs(rb.velocity.normalized.x),0,Mathf.Abs(rb.velocity.normalized.z))).magnitude;
           multiplierV = airMovementMultiplier;
           if(y < 0) rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * airBreakForce);
       }

       //WallRunning

       lftTrail.emitting = isWallRunningL;
       rtTrail.emitting = isWallRunningR;

       if (isWallRunning)
       {
           WallRunning();
       }

       //slide movement goes here
       if (!isSliding && sliding && grounded && rb.velocity.magnitude > startSlideSpeedThreshold)
       {
           StartSlide();
       }

       if (isSliding && sliding && grounded)
       {
           Slide();
       }

       if (isSliding && (!sliding || rb.velocity.magnitude < stopSlideSpeedThreshold))
       {
           StopSlide();
       }

       rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
       rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);

       rb.velocity = Vector3.ClampMagnitude(rb.velocity, this.maxSpeed);
      
       if(rb.velocity.magnitude <= .1f) rb.velocity = Vector3.zero;
       
       PlayRunAudio();
    }
    
    // We are using a mix of Unity gravity and custom gravity so we can have more control
    void Gravity()
    {
       if (!grounded && !isWallRunning && !jumping && rb.velocity.y <= 0)
       {
           rb.AddForce(Vector3.down * Time.deltaTime * gravity * gravityMultiplier);
       }
       else if(!isWallRunning)
       {
           rb.AddForce(Vector3.down * Time.deltaTime * gravity);
       }
    }
    
    // This is used to adda force to the player oppisite to the movement direction to slow down and stop the player
    private void counterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping || dash) return;

        //sliding

        if(Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < .05f || (mag.x < -threshold && x > 0)|| (mag.x > threshold&& x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMove);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < .05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMove);
        }

        if (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2)) > maxGroundSpeed)
        {
            float fallSpeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxGroundSpeed;
            rb.velocity = new Vector3(n.x, fallSpeed, n.z);
        }
    }

    // This is used to find out if you are lookig in the direction you are moving
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngel = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngel);
        float v = 90 - u;

        float magnitude = rb.velocity.magnitude;
        float ymag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xmag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xmag, ymag);
    }
    
    // This function is our jump logic and a seperate function to reset the jump so there is a small cooldown
    private void Jump(float slideMultiplier = 1)
    {
        if(grounded && rdyToJump)
        {
            PlayerAudioManager.CallOnPlayJumpStartAudio();

            rdyToJump = false;

            rb.AddForce(Vector2.up * jumpForce * 3.5f * slideMultiplier);
            rb.AddForce(normalVector * jumpForce * .5f * slideMultiplier);

            Vector3 vel = rb.velocity;
            if (rb.velocity.y < .5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        rdyToJump = true;
    }

    // These three function govern our sliding logic
    // I split it into three due to the inherent setup required for starting and stoping sliding
    void StartSlide()
    {
       startCounterMove = counterMove;
       startHeight = PlayerCol.height;
       startMaxSpeed = maxGroundSpeed;
      
       isSliding = true;
      
       counterMove = slideDrag;
       PlayerCol.height = slideHeight;
       maxGroundSpeed = maxSlideSpeed;
       //PlayerAudioManager.CallOnPlaySlideStartAudio();
    }

    void Slide()
    {
       x = Mathf.Clamp(x, -slidingStrafeClamp, slidingStrafeClamp);

       if(Physics.CheckCapsule(slideCheck.position - (slideCheck.forward).normalized*.45f,
           slideCheck.position + (slideCheck.forward).normalized*.45f,.25f,stopSlide))
       {
           rb.velocity = Vector3.zero;
           StopSlide();
       }
       
       if (jumping && rdyToJump)
       {
           rb.velocity /= 2;
           Jump(slideJumpMultiplier);
           StopSlide();
       }
       else
       {
           rb.AddForce(Vector3.down * slidingGravity);
       }
       //PlayerAudioManager.CallOnPlaySlideHoldAudio();
    }

    void StopSlide()
    {
       counterMove = startCounterMove;
       PlayerCol.height = startHeight;
       maxGroundSpeed = startMaxSpeed;
      
       isSliding = false;
       //PlayerAudioManager.CallOnPlaySlideStopAudio();
    }

    // This holds wall running logic and we have three types of wall running that can be mixed and matched to change the feel of the game
    void WallRunning()
    {
       if (canWallJump && jumping && !grounded) wallJump();
      
       if(!jumping && !hasGrappled)
       {
           rb.velocity = wallRunSlipModifier == 0 ? new Vector3(rb.velocity.x,.2f,rb.velocity.z) : new Vector3(rb.velocity.x,Mathf.Clamp(rb.velocity.y,wallRunSlipModifier,0),rb.velocity.z);
       }

       else if (hasGrappled)
       {
           rb.velocity = 
               new Vector3(rb.velocity.x,Mathf.Clamp(rb.velocity.y,wallRunPostGrappleSlipModifier == 0? 0.2f : wallRunPostGrappleSlipModifier ,Mathf.Infinity),rb.velocity.z);
           rb.AddForce(Vector3.down * wallRunningGavity, ForceMode.Force);
       }
      
       if (isWallRunningL)
       {
           if(!grounded)
               rb.AddForce(-orientation.right * 5f, ForceMode.Force);
       }

       if (isWallRunningR)
       {
           if (!grounded)
               rb.AddForce(orientation.right * 5f, ForceMode.Force);
       }
    }
    
    // We have a different function for wall jump but it is very similar to normal jump
    // just some extra force to push us awayt from the wall
    private void wallJump()
    {
        canWallJump = false;
        if (!grounded)
        {
            rb.velocity = rb.velocity.y <= 0? new Vector3(rb.velocity.x, 0, rb.velocity.z):rb.velocity;
            rb.AddForce(Vector2.up * jumpForce * wallJumpUpwardMultiplier);
            rb.AddForce(normalVector * jumpForce * .5f);
            PlayerAudioManager.CallOnPlayJumpStartAudio();
        }

        if (isWallRunningL)
            rb.AddForce(orientation.right * jumpForce * wallJumpOutwardMultiplier);
        if (isWallRunningR)
            rb.AddForce(-orientation.right * jumpForce * wallJumpOutwardMultiplier);

        Invoke(nameof(ResetWallJump), wallJumpCoolDown);
    }
    private void ResetWallJump()
    {
        canWallJump = true;
    }

#endregion

// This has some code I am testing for a dash movement ability
#region Future Abilities

/*
private void Dash()
{
   if (rdyToDash && grapple.connect == false)
   {
       Debug.Log("Dash");
       rdyToDash = false;
       rb.AddForce(orientation.transform.forward * y * dashSpeed, ForceMode.VelocityChange);
       rb.AddForce(orientation.transform.right * x * dashSpeed, ForceMode.VelocityChange);
       timer = dashCooldown;
       var col = particle.trails.colorOverLifetime;
       col = Color.red;
       Invoke(nameof(ResetDash), 0);
   }
}

private void ResetDash()
{
   if (Mathf.Abs(mag.x) > threshold + 10 && Mathf.Abs(x) < .05f || (mag.x < -threshold - 10 && x > 0) || (mag.x > threshold + 10 && x < 0))
   {
       rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMove);
   }
   if (Mathf.Abs(mag.y) > threshold + 10 && Mathf.Abs(y) < .05f || (mag.y < -threshold - 10 && y > 0) || (mag.y > threshold + 10 && y < 0))
   {
       rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMove);
   }

   if (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2)) > maxSpeed)
   {
       float fallSpeed = rb.velocity.y;
       Vector3 n = rb.velocity.normalized * (15 + maxSpeed);
       rb.velocity = new Vector3(n.x, fallSpeed, n.z);
   }
   var col = particle.trails.colorOverLifetime;
   col = Color.white;
}
*/

#endregion

// Here is all of our more utility functions for checking grounded
#region Utility Functions

    // Check if the slope is low enough to be the floor
    bool IsFloor(Vector3 v)
    {
       float angle = Vector3.Angle(Vector3.up, v);
       return angle < maxSlopeAngle;
    }

    bool cancellingGrounded;

    // We are using collision to find the ground
    private void OnCollisionStay(Collision other)
    {
       int layer = other.gameObject.layer;

       if (ground != (ground | (1 << layer))) return;

       for(int i = 0; i < other.contactCount; i++)
       {
           Vector3 normal = other.contacts[i].normal;
           if (isWallRunning && !onWall)
           {
               previousWall = other.transform.gameObject;
               onWall = true;
           }
           if (IsFloor(normal))
           {
               if (grounded == false)
               {
                   PlayerAudioManager.CallOnPlayJumpLandAudio();
               }
               grounded = true;
               hasGrappled = false;
               cancellingGrounded = false;
               normalVector = normal;
               CancelInvoke(nameof(StopGrounded));
           }
       }

       float delay = 10f;
       if (!cancellingGrounded)
       {
           cancellingGrounded = true;
           Invoke(nameof(StopGrounded), Time.deltaTime * delay);
       }
    }

    private void StopGrounded()
    {
       grounded = false;
    }
    
    void NotGrounded()
    {
        if (grapple.connect) hasGrappled = true;
    }
    
    private void PlayRunAudio()
    {
       //Play audio
       currentSpeed = rb.velocity.magnitude;
       if (grounded || isWallRunning)
       {
           if (!isSliding)
           {
               if(currentSpeed >= 5)
               {
                   if (stepTimer <= 0)
                   {
                       PlayerAudioManager.CallOnPlayRunAudio();
                       stepTimer = stepSpeed;
                   }
                   else
                   {
                       stepTimer -= Time.deltaTime;
                   }
               }  
           }
       }
    }

#endregion

#region Gizmos and Debug Editor

    // Gizmos are used in the editor to check stuff like the wall run area and slide clipping checks
    private void OnDrawGizmos()
    {
       Gizmos.DrawWireCube(wallCheckLft.position, new Vector3(.25f, .55f, .05f));
       Gizmos.DrawWireCube(wallCheckRt.position, new Vector3(.25f, .55f, .05f));
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(slideCheck.position - (slideCheck.forward).normalized*.45f,.25f);
        Gizmos.DrawSphere(slideCheck.position + (slideCheck.forward).normalized*.45f,.25f);
    }
    
    // This is a debug UI where you can chage every value in this controller and in the grapple controller
    int toolbarInt = 0;
    string[] toolbarStrings = {"Movement", "WallRunning", "Grapple", "Slide", "Camera"};
    private void OnGUI()
    {
      
       if (debugUI)
       {
           GUILayout.Label("Escape to Open Menu");
           return;
       }
       GUILayout.Label("Escape to Close Menu");
       toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);
       GUILayout.BeginVertical();
       switch (toolbarInt)
       {
           case 0:
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Movement Speed");
               float.TryParse(GUILayout.TextField(moveSpeed.ToString()), out moveSpeed);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Max Ground Speed");
               float.TryParse(GUILayout.TextField(maxGroundSpeed.ToString()), out maxGroundSpeed);
               GUILayout.EndHorizontal();

               GUILayout.BeginHorizontal();
               GUILayout.Label("Max Speed");
               float.TryParse(GUILayout.TextField(maxSpeed.ToString()), out maxSpeed);
               GUILayout.EndHorizontal();
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("Normalize Input");
               isNormalized = GUILayout.Toggle(isNormalized, isNormalized.ToString());
               GUILayout.EndHorizontal();
               
               GUILayout.BeginHorizontal();
               GUILayout.Label("Stop Speed: " + $"{counterMove:0.00}");
               counterMove = GUILayout.HorizontalSlider(counterMove,0,1);
               GUILayout.EndHorizontal();
               
               GUILayout.BeginHorizontal();
               GUILayout.Label("Air Stop Speed: " + $"{airBreakForce:0.00}");
               counterMove = GUILayout.HorizontalSlider(airBreakForce,0,1);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Max Slope Angle");
               float.TryParse(GUILayout.TextField(maxSlopeAngle.ToString()), out maxSlopeAngle);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Jump Force");
               float.TryParse(GUILayout.TextField(jumpForce.ToString()), out jumpForce);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Air Movement Multiplier: " + $"{airMovementMultiplier:0.00}");
               airMovementMultiplier = GUILayout.HorizontalSlider(airMovementMultiplier,0,1);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Gravity");
               float.TryParse(GUILayout.TextField(gravity.ToString()), out gravity);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Fall Gravity Multiplier");
               float.TryParse(GUILayout.TextField(gravityMultiplier.ToString()), out gravityMultiplier);
               GUILayout.EndHorizontal();

               break;
           case 1:
        
               GUILayout.BeginHorizontal();
               GUILayout.Label("Max Wall Distance");
               float.TryParse(GUILayout.TextField(wallDistance.ToString()), out wallDistance);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("PreGrapple Wall Run Gravity");
               float.TryParse(GUILayout.TextField(wallRunSlipModifier.ToString()), out wallRunSlipModifier);
               GUILayout.EndHorizontal();
               
               GUILayout.BeginHorizontal();
               GUILayout.Label("PostGrapple Wall Slip Gravity");
               float.TryParse(GUILayout.TextField(wallRunPostGrappleSlipModifier.ToString()), out wallRunPostGrappleSlipModifier);
               GUILayout.EndHorizontal();

               GUILayout.BeginHorizontal();
               GUILayout.Label("PostGrapple Wall Run Gravity");
               float.TryParse(GUILayout.TextField(wallRunningGavity.ToString()), out wallRunningGavity);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Wall Jump Cooldown");
               float.TryParse(GUILayout.TextField(wallJumpCoolDown.ToString()), out wallJumpCoolDown);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Wall Jump Upwards Force");
               float.TryParse(GUILayout.TextField(wallJumpUpwardMultiplier.ToString()), out wallJumpUpwardMultiplier);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Wall Run Outwards Force");
               float.TryParse(GUILayout.TextField(wallJumpOutwardMultiplier.ToString()), out wallJumpOutwardMultiplier);
               GUILayout.EndHorizontal();

               break;
           case 2:
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("Max Grapple Length");
               float.TryParse(GUILayout.TextField(grapple.distance.ToString()), out grapple.distance);
               GUILayout.EndHorizontal();
               
               GUILayout.BeginHorizontal();
               GUILayout.Label("Grapple Pull Speed");
               float.TryParse(GUILayout.TextField(grapple.pullSpeed.ToString()), out grapple.pullSpeed);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Max Swing Distance Mult");
               float.TryParse(GUILayout.TextField(grapple.maxMult.ToString()), out grapple.maxMult);
               GUILayout.EndHorizontal();

               GUILayout.BeginHorizontal();
               GUILayout.Label("Min Swing Distance Mult");
               float.TryParse(GUILayout.TextField(grapple.minMult.ToString()), out grapple.minMult);
               GUILayout.EndHorizontal();
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("Min Max Tolerance");
               float.TryParse(GUILayout.TextField(grapple.tolerance.ToString()), out grapple.tolerance);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Swing Damping");
               float.TryParse(GUILayout.TextField(grapple.damp.ToString()), out grapple.damp);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Swing Springiness");
               float.TryParse(GUILayout.TextField(grapple.spring.ToString()), out grapple.spring);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Swing Mass Scale");
               float.TryParse(GUILayout.TextField(grapple.massScl.ToString()), out grapple.massScl);
               GUILayout.EndHorizontal();

               GUILayout.BeginHorizontal();
               GUILayout.Label("Force To Break Grapple");
               float.TryParse(GUILayout.TextField(grapple.breakForce.ToString()), out grapple.breakForce);
               GUILayout.EndHorizontal();
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("Torque To Break Grapple");
               float.TryParse(GUILayout.TextField(grapple.breakTorque.ToString()), out grapple.breakTorque);
               GUILayout.EndHorizontal();
              
               break;
           case 3:
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("Start Slide Speed Threshold");
               float.TryParse(GUILayout.TextField(startSlideSpeedThreshold.ToString()), out startSlideSpeedThreshold);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Stop Slide Speed Threshold");
               float.TryParse(GUILayout.TextField(stopSlideSpeedThreshold.ToString()), out stopSlideSpeedThreshold);
               GUILayout.EndHorizontal();

               GUILayout.BeginHorizontal();
               GUILayout.Label("Slide Drag: " + $"{slideDrag:0.00}");
               slideDrag = GUILayout.HorizontalSlider(slideDrag, 0, 1);
               GUILayout.EndHorizontal();
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("Max Slide Speed");
               float.TryParse(GUILayout.TextField(maxSlideSpeed.ToString()), out maxSlideSpeed);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Sliding Gravity");
               float.TryParse(GUILayout.TextField(slidingGravity.ToString()), out slidingGravity);
               GUILayout.EndHorizontal();
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("Sliding Jump Multiplier");
               float.TryParse(GUILayout.TextField(slideJumpMultiplier.ToString()), out slideJumpMultiplier);
               GUILayout.EndHorizontal();
          
               GUILayout.BeginHorizontal();
               GUILayout.Label("Sliding Strafe Speed");
               float.TryParse(GUILayout.TextField(slidingStrafeClamp.ToString()), out slidingStrafeClamp);
               GUILayout.EndHorizontal();

               break;
          
           case 4:
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("Sensitivity");
               float.TryParse(GUILayout.TextField(sensitivity.ToString()), out sensitivity);
               GUILayout.EndHorizontal();
              
               GUILayout.BeginHorizontal();
               GUILayout.Label("FOV: " + $"{playerCam.GetComponent<Camera>().fieldOfView:0.00}");
               playerCam.GetComponent<Camera>().fieldOfView = GUILayout.HorizontalSlider(playerCam.GetComponent<Camera>().fieldOfView, 30,120);
               GUILayout.EndHorizontal();
              
               break;
       }
       GUILayout.EndVertical();
    }

#endregion
}

