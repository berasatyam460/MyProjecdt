using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [Header("References")]
  public PlayerMovementStats moveStats;
  [SerializeField]private Collider2D feetCol;
  [SerializeField]private Collider2D bodyCol;


  private Rigidbody2D playerRB;

  //movement variables
  public  float horizontalVelocity{get;private set;}
  private bool isFacingRight;


  //checking collisions vars
  private RaycastHit2D groundHit;
  private RaycastHit2D headHit;

  private RaycastHit2D wallHit;
  private RaycastHit2D lastWallHit;

  public bool isGrounded;
  private bool bumpedHead;

  private bool isTouchingWall;


  //jump variables
  

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


  //wall slide vars
  private bool isWallSliding;
  private bool isWallSlideFalling;


  //wall jump vars
  private bool useWallJumpMoveStats;
  private bool isWallJumping;
  private float wallJumpTime;
  private bool iswallJumpFastFalling;
  private float wallJumpFastFallTime;
  private float wallJumpFastFallReleaseSpeed;

  private float wallJumpPostBufferTimer;

  private float wallJumpApexPoint;
  private float timePastWallJumpApexThreshold;
  private bool isPastWallJumpApexThreshold;
  private bool isWallJumpFalling;
  

  //dash vars
  private bool isDashing;
  private bool isAirDashing;
  private float dashTimer;
  private float dashOnGroundTimer;
  private int noOfDashUsed;
  private Vector2 dashDirection;
  private bool isDashFastFalling;
  private float dashFastFallTime;
  private float dashFastFallReleaseTime;

  void Awake()
  {
    isFacingRight=true;

    playerRB=GetComponent<Rigidbody2D>();

  }
  private void Update() {
    CountTimers();
    JumpChecks();
    LandCheck();
    WallSlideCheck();
  }

  private void FixedUpdate() {
    CollisionChecks();

    Jump();
    Fall();
    WallSlide();

    if(isGrounded){

      Move(moveStats.groundAcceleration,moveStats.groundDeceleration,InputManager.movement);
    
    }else{

      Move(moveStats.airAcceleration,moveStats.airDeceleration,InputManager.movement);

    }
    ApplyVelocity();
  }
  private void ApplyVelocity(){
     //clamp fall speed
        verticalVelocity = Mathf.Clamp(verticalVelocity, -moveStats.maxFallSpeed, 50f);

        playerRB.velocity = new Vector2(horizontalVelocity, verticalVelocity);
  }
  #region Movement

  private void Move(float acceleration,float deceleration,Vector2 moveInput){
    if(Mathf.Abs(moveInput.x)>=moveStats.moveThreshold){
      //check if he needs to turn

      TurnCheck (moveInput);
      float targetVelocity=0f;
      //run and walk input
      if(InputManager.runIsHeld){
        targetVelocity=moveInput.x*moveStats.maxRunSpeed;
      }else{
        targetVelocity=moveInput.x*moveStats.maxWalkSpeed;
      }

       horizontalVelocity=Mathf.Lerp(horizontalVelocity,targetVelocity,acceleration*Time.fixedDeltaTime);
       

    }else if(Mathf.Abs(moveInput.x)<moveStats.moveThreshold){
      horizontalVelocity=Mathf.Lerp(horizontalVelocity,0f,deceleration*Time.fixedDeltaTime);
     
    }
  }
#endregion

  #region Jump
  private void ResetJumpValues(){
    isJumping=false;
    isFalling=false;
    isFastFalling=false;
    fastFallTime=0f;
    isPastApexThreshold=false;

  }
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
    if(jumpBufferTimer>0f && !isJumping && (isGrounded||coyoteTImer>0f)){
        InitiateJump(1);

        if(jumpReleasedDuringBuffer){
           isFastFalling=true;
           fastFallReleaseSpeed=verticalVelocity;
        }
    }

    //double jump
    else if(moveStats.isDoubleJumpAllowed&&jumpBufferTimer>0f&&isJumping&&noOfJumpUsed<moveStats.noOfJumpAllowed){
      isFastFalling=false;
      InitiateJump(2);
    }
    //air jump after coyote time lapesed
    else if(jumpBufferTimer>0f&&isFalling&&noOfJumpUsed<moveStats.noOfJumpAllowed-1){
      InitiateJump(2);
      isFastFalling=false;

    }
    
  }
 

  private void InitiateJump(int noOfJumpUsed){
    if(!isJumping){
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
                else if(!isFastFalling)
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
  IsTouchingWall();
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
private void IsTouchingWall(){
  float originEndPoint=0f;
  if(isFacingRight){
    originEndPoint=bodyCol.bounds.max.x;
  }else{
    originEndPoint=bodyCol.bounds.min.x;
  }
  float adjustedHeight=bodyCol.bounds.size.y*moveStats.wallRayHeightMultipler;
  Vector2 boxCastOrigin=new Vector2(originEndPoint,bodyCol.bounds.center.y);
  Vector2 boxCastSize=new Vector2(moveStats.wallDetectionRayLength,adjustedHeight);
  //check with the wall layer
  wallHit=Physics2D.BoxCast(boxCastOrigin,boxCastSize,0f,transform.right,moveStats.wallDetectionRayLength,moveStats.wallLayer);
  if(wallHit.collider!=null){
    lastWallHit=wallHit;
    isTouchingWall=true;
  }else{
    isTouchingWall=false;
  }

  #region Debug Visualization
  if(moveStats.debugShowWallHitBox){
    Color rayColor;
    if(isTouchingWall){
      rayColor=Color.green;
    }else{
      rayColor=Color.red;
    }

    Vector2 boxBottomLeft=new Vector2(boxCastOrigin.x-boxCastSize.x/2,boxCastOrigin.y-boxCastSize.y/2);
    Vector2 boxBottomRight=new Vector2(boxCastOrigin.x+boxCastSize.x/2,boxCastOrigin.y-boxCastSize.y/2);
    Vector2 boxTopLeft=new Vector2(boxCastOrigin.x-boxCastSize.x/2,boxCastOrigin.y+boxCastSize.y/2);
    Vector2 boxTopRight=new Vector2(boxCastOrigin.x+boxCastSize.x/2,boxCastOrigin.y+boxCastSize.y/2);

    Debug.DrawLine(boxBottomLeft,boxBottomRight,rayColor);
    Debug.DrawLine(boxBottomRight,boxTopRight,rayColor);
    Debug.DrawLine(boxTopRight,boxTopLeft,rayColor);
    Debug.DrawLine(boxTopLeft,boxBottomLeft,rayColor);

  }

  #endregion
}
#endregion

 #region  LandCheck/Fall
  private void LandCheck(){
    if((isJumping||isFalling) && isGrounded && verticalVelocity<=0f){
      isJumping=false;
      isFalling=false;
      isFastFalling=false;
      fastFallTime=0f;
      isPastApexThreshold=false;
      noOfJumpUsed=0;
      verticalVelocity=Physics2D.gravity.y;

    }
  }
  private void Fall(){
    //normal gravity while falling

        if(!isGrounded&& !isJumping)
        {
            if (!isFalling)
                isFalling = true;
            verticalVelocity += moveStats.gravity * Time.deltaTime;
        }
  }
  #endregion


  #region Wall Slide
  private void WallSlideCheck(){
    if(isTouchingWall && !isGrounded &&!isDashing){
      if(verticalVelocity<0f && !isWallSliding){
        ResetJumpValues();
        ResetWallJumpVAlues();
        ResestDashValues();

        if(moveStats.resetDashOnWallSlide){
          ResetDashes();
        }
        isWallSlideFalling=false;
        isWallSliding=true;
        if(moveStats.resetJumpOnWallSlide){
          noOfJumpUsed=0;
        }
      }
    }else if(isWallSliding && !isTouchingWall && !isGrounded && !isWallSlideFalling){
      isWallSlideFalling=true;
      StopWallSlide();
    }
    else{
      StopWallSlide();
    }
  }
  private void StopWallSlide(){
    if(isWallSliding){
      //varies with different game
      //if player fall consume 1 jump only give 1 air jump
      noOfJumpUsed++;

      isWallSliding=false;
    }
  }
  private void WallSlide(){
    if(isWallSliding){
      verticalVelocity=Mathf.Lerp(verticalVelocity,-moveStats.wallSlideSpeed,moveStats.wallSlideDecelaration*Time.fixedDeltaTime);
    }
  }
  #endregion

  #region Wall Jump
  private void ResetWallJumpVAlues(){
    isWallSlideFalling=false;
    useWallJumpMoveStats=false;
    isWallJumping=false;
    iswallJumpFastFalling=false;
    isPastWallJumpApexThreshold=false;
    isWallJumpFalling=false;

    wallJumpFastFallTime=0f;
    wallJumpTime=0f;
  }
  #endregion

  #region Dash
  private void ResestDashValues(){
    isDashFastFalling=false;
    dashOnGroundTimer=-0.0f;
  }
  private void ResetDashes(){
    noOfDashUsed=0;
  }

  #endregion
}

