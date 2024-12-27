
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


  void Awake()
  {
    isFacingRight=true;

    playerRB=GetComponent<Rigidbody2D>();

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
      moveVelocity=Vector2.Lerp(moveVelocity,Vector2.zero,deceleration*Time.deltaTime);
      playerRB.velocity=new Vector2(moveVelocity.x,playerRB.velocity.y);
    }
  }
#endregion


#region TurnChecking
private void TurnCheck(Vector2 moveInput){
  if(isFacingRight&&moveInput.x<0){
      Turn(false);
  }
  else if(!isFacingRight&&moveInput.x>0){
      Turn(true);
  }
}

private void Turn(bool isTurnRight){
  if(isTurnRight){
    isFacingRight=true;
    transform.Rotate(0f,100f,0f);
  }else{
    isFacingRight=false;
    transform.Rotate(0f,-100f,0f);
  }
}
#endregion
//collision check

}
