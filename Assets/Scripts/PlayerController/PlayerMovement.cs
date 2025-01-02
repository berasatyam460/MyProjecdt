

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [Header("References")]
  public PlayerMovementStats moveStats;
  [SerializeField]private Collider2D feetCol;
  [SerializeField]private Collider2D bodyCol;


  private Rigidbody2D playerRB;

  //movement variables
  private Vector2 moveVelocity;
  private bool isFacingRight;


  //checking collisions vars
  private RaycastHit2D groundHit;
  private RaycastHit2D headHit;

  public bool isGrounded;
  private bool bumpedHead;


  //jump Vars
  public float verticalVelocity{get;private set;}
  private bool isJumping;
  private bool isFastFalling;
  private bool isFalling;
  private float fastFallTime;
  private float fastFallReleaseSpeed;
  private int noOfJumpUsed;


  //apex vars
  private float apexPoint;
  private float timePAstApexThreshold;
  private bool isPastApexThreshold;

  //jump buffers vars
  private float jumpBufferTimer;
  private bool jumpReleasedDuringBuffer;

  //coyote Time vars
  private float coyoteTImer;




  void Awake()
  {
    isFacingRight=true;

    playerRB=GetComponent<Rigidbody2D>();

  }
  private void Update() {
    JumpChecks();
    CountTimers();
   
  }

  private void FixedUpdate() {
    CollisionChecks();

    Jump();

    if(isGrounded){

      Move(moveStats.groundAcceleration,moveStats.groundDeceleration,InputManager.movement);
    
    }else{

      Move(moveStats.airAcceleration,moveStats.airDeceleration,InputManager.movement);

    }
  }

  #region Movement

  private void Move(float acceleration,float deceleration,Vector2 moveInput){
    if(moveInput!=Vector2.zero){
      //check if he needs to turn

      TurnCheck (moveInput);
      Vector2 targetVelocity=Vector2.zero;

      if(InputManager.runIsHeld){
        targetVelocity=new Vector2(moveInput.x,0f)*moveStats.maxRunSpeed;
      }else{
        targetVelocity=new Vector2(moveInput.x,0f)*moveStats.maxWalkSpeed;
      }

       moveVelocity=Vector2.Lerp(moveVelocity,targetVelocity,acceleration*Time.fixedDeltaTime);
       playerRB.velocity=new Vector2(moveVelocity.x,playerRB.velocity.y);

    }else if(moveInput==Vector2.zero){
      moveVelocity=Vector2.Lerp(moveVelocity,Vector2.zero,deceleration*Time.fixedDeltaTime);
      playerRB.velocity=new Vector2(moveVelocity.x,playerRB.velocity.y);
    }
  }
#endregion

  #region Jump
  private void JumpChecks(){
    //when jump btn is pressed
    if(InputManager.jumpWasPressed){
      jumpBufferTimer=moveStats.jumpBufferTime;
      jumpReleasedDuringBuffer=false;
    }
    //when jump btn is released
    if(InputManager.jumpWasReleased){
      if(jumpBufferTimer>0f){
        jumpReleasedDuringBuffer=true;

      }
      if(isJumping&&verticalVelocity>0f){
        if(isPastApexThreshold){
          isPastApexThreshold=false;
          isFastFalling=true;
          fastFallTime=moveStats.timeForUpwardCancel;
          verticalVelocity=0f;
        }else{
          isFastFalling=true;
          fastFallReleaseSpeed=verticalVelocity;
        }
      }
    }

    //initiate jump with jump buffering and coyote time
    if(jumpBufferTimer>0f&&isJumping&&(isGrounded||coyoteTImer>0f)){
        InitiateJump(1);

        if(jumpReleasedDuringBuffer){
           isFastFalling=true;
           fastFallReleaseSpeed=verticalVelocity;
        }
    }

    //double jump
    else if(jumpBufferTimer>0f&&isJumping&&noOfJumpUsed<moveStats.noOfJumpAllowed){
      isFastFalling=false;
      InitiateJump(1);
    }
    //air jump after coyote time lapesed
    else if(jumpBufferTimer>0f&&isFalling&&noOfJumpUsed<moveStats.noOfJumpAllowed-1){
      InitiateJump(2);
      isFastFalling=false;

    }
    //landed

    if(isJumping||isFalling){
      isJumping=false;
      isFalling=false;
      isFastFalling=false;
      fastFallTime=0f;
      isPastApexThreshold=false;
      noOfJumpUsed=0;
      verticalVelocity=Physics2D.gravity.y;

    }


  }

  private void InitiateJump(int noOfJumpUsed){
    if(isJumping){
      isJumping=true;
    }
    jumpBufferTimer=0f;
    this.noOfJumpUsed+=noOfJumpUsed;
    verticalVelocity=moveStats.initialJumpVelocity;

  }
  private void Jump()
    {
        //apply gravity while jumping
        if (isJumping)
        {
            // check for head bump
            if (bumpedHead)
            {
                isFastFalling = true;

            }
            //gravity on ascending
            if (verticalVelocity >= 0f)
            {
                //apex control
                apexPoint = Mathf.InverseLerp(moveStats.initialJumpVelocity, 0f, verticalVelocity);

                if (apexPoint > moveStats.apexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePAstApexThreshold = 0f;

                    }
                    if(isPastApexThreshold)
                    {
                        timePAstApexThreshold += Time.deltaTime;
                        if (timePAstApexThreshold < moveStats.apexHangTime)
                        {
                            verticalVelocity = 0f;

                        }
                        else
                        {
                            verticalVelocity = -0.01f;
                        }
                    }
                }
                //gravity on ascending without past apex threshold
                else
                {
                    verticalVelocity += moveStats.gravity * Time.deltaTime;
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;

                    }
                }

            }
            //gravity on descending
            else if (!isFastFalling)
            {
                verticalVelocity += moveStats.gravity * moveStats.gravityOnReleaseMultiplier * Time.deltaTime;
            }
            else if (verticalVelocity < 0f)
            {
                if (!isFalling)
                {
                    isFalling = true;
                }
            }


        }




        //jump cut
        if (isFastFalling)
        {
            if (fastFallTime >= moveStats.timeForUpwardCancel)
            {
                verticalVelocity += moveStats.gravity * moveStats.gravityOnReleaseMultiplier * Time.deltaTime;

            }else if (fastFallTime < moveStats.timeForUpwardCancel)
            {
                verticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / moveStats.timeForUpwardCancel));

            }
            fastFallTime+= Time.deltaTime;
        }
        //normal gravity while falling

        if(!isGrounded&& !isJumping)
        {
            if (!isFalling)
                isFalling = true;
            verticalVelocity += moveStats.gravity * Time.deltaTime;
        }
        //clamp fall speed
        verticalVelocity = Mathf.Clamp(verticalVelocity, -moveStats.maxFallSpeed, 50f);

        playerRB.velocity = new Vector2(playerRB.velocity.x, verticalVelocity);
    }

  #endregion
  #region Timers
  private void CountTimers(){
    jumpBufferTimer-=Time.deltaTime;
    if(!isGrounded){
      coyoteTImer-=Time.deltaTime;

    }else{
      coyoteTImer=moveStats.jumpCoyoteTime;

    }
  }

  #endregion

#region TurnChecking
   private void TurnCheck(Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
        {
            Turn(false);
           // camerasmooth.callTurn();
        }else if(!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
           // camerasmooth.callTurn();
        }
    }

    private void Turn(bool TurnRight)
    {
        if (TurnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
       
    }
#endregion
//collision check
#region Collision Checks

private void CollisionChecks(){
  IsGroundedCheck();
  BumpedHead();
}
public void IsGroundedCheck(){
  Vector2 boxCastOrigin=new Vector2(feetCol.bounds.center.x,feetCol.bounds.min.y);
  Vector2 boxCastSize=new Vector2(feetCol.bounds.size.x,moveStats.groundDetectionRayLength);
  
  //calculate
  groundHit=Physics2D.BoxCast(boxCastOrigin,boxCastSize,0f,Vector2.down,moveStats.groundDetectionRayLength,moveStats.GroundLayer);
  if(groundHit.collider!=null){
    isGrounded=true;

  }else{
    isGrounded=false;
  }

  #region Debug Visualization

  if(moveStats.debugShowIsGroundedBox){

    Color rayColor;

    if(isGrounded){

      rayColor=Color.green;

    }else{

      rayColor=Color.red;

    }
    
    Debug.DrawRay(new Vector2(boxCastOrigin.x-boxCastSize.x/2,boxCastOrigin.y),Vector2.down*moveStats.groundDetectionRayLength,rayColor);

    Debug.DrawRay(new Vector2(boxCastOrigin.x+boxCastSize.x/2,boxCastOrigin.y),Vector2.down*moveStats.groundDetectionRayLength,rayColor);
    
    Debug.DrawRay(new Vector2(boxCastOrigin.x-boxCastSize.x/2,boxCastOrigin.y-moveStats.groundDetectionRayLength),Vector2.right*boxCastSize.x,rayColor);
  }

  #endregion
}

private void BumpedHead(){
  Vector2 boxCastOrigins=new Vector2(feetCol.bounds.center.x,bodyCol.bounds.max.y);
  Vector2 boxCastSize=new Vector2(feetCol.bounds.size.x*moveStats.headWidth,moveStats.headDetectionRayLength);
  
 headHit=Physics2D.BoxCast(boxCastOrigins,boxCastSize,0f,Vector2.up,moveStats.headDetectionRayLength,moveStats.GroundLayer);
  if(headHit.collider!=null){
    bumpedHead=true;

  }else{
    bumpedHead=false;
  }
  #region Debug Visualization

  if(moveStats.debugShowHeadBumpedBox){
   float headWidth=moveStats.headWidth;
    Color rayColor;

    if(isGrounded){

      rayColor=Color.green;

    }else{

      rayColor=Color.red;

    }
    
    Debug.DrawRay(new Vector2(boxCastOrigins.x-boxCastSize.x/2*headWidth,boxCastOrigins.y),Vector2.up*moveStats.headDetectionRayLength,rayColor);

    Debug.DrawRay(new Vector2(boxCastOrigins.x+boxCastSize.x/2*headWidth,boxCastOrigins.y),Vector2.up*moveStats.headDetectionRayLength,rayColor);
    
    Debug.DrawRay(new Vector2(boxCastOrigins.x-boxCastSize.x/2*headWidth,boxCastOrigins.y-moveStats.headDetectionRayLength),Vector2.right*boxCastSize.x,rayColor);
  }
  #endregion
}

#endregion
}

