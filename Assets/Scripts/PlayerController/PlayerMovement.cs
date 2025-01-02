
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

  private bool isGrounded;
  private bool bumpedHead;
  

  //jump variables
  

  void Awake()
  {
    isFacingRight=true;

    playerRB=GetComponent<Rigidbody2D>();

  }

  private void FixedUpdate() {
    CollisionChecks();
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

#endregion
}
