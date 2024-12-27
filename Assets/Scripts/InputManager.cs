using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
   public static PlayerInput playerInput;

   public static Vector2 movement;

   public static bool jumpWasPressed;
   public static bool jumpIsHeld;
   public static bool jumpWasReleased;
   public static bool runIsHeld;

   private InputAction moveAction;
   private InputAction jumpAction;
   private InputAction runAction;



   private void  Awake()
   {
    playerInput=GetComponent<PlayerInput>();

    moveAction=playerInput.actions["Move"];
    jumpAction=playerInput.actions["Jump"];
    runAction=playerInput.actions["Run"];
   }


   void Update()
   {
    movement=moveAction.ReadValue<Vector2>();

    jumpWasPressed=jumpAction.WasPressedThisFrame();
    jumpIsHeld=jumpAction.IsPressed();
    jumpWasReleased=jumpAction.WasReleasedThisFrame();

    runIsHeld=runAction.IsPressed();
   }
}
