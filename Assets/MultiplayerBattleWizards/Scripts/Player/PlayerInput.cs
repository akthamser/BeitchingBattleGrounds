using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Checks for player input and runs corresponding functions.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("Keys")]
    public string movementAxis = "Horizontal";          // Input axis to move horizontally.
    public KeyCode jump = KeyCode.UpArrow;              // Key that makes the player jump.
    public KeyCode attack = KeyCode.Space;              // Key that makes the player attack.

    // Events
    [HideInInspector] public UnityEvent onJumpDown;       // Called the frame when the 'jump' button is pressed.
    [HideInInspector] public UnityEvent onJumpHold;       // Called every frame the 'jump' button is pressed.
    [HideInInspector] public UnityEvent onJumpUp;         // Called the frame when the 'jump' button is released.
    [HideInInspector] public UnityEvent onAttackDown;     // Called the frame when the 'attack' button is pressed.

    [Header("Mobile Controls")]
    public bool enableMobileControls;                   // Are mobile controls enabled?
    [HideInInspector]
    public float mobileMovementAxis;                    // Horizontal movement axis for mobile.

    // Components
    private Player player;

    // Sets components referenced from the 'Player' class.
    public void SetComponents (Player playerClass)
    {
        player = playerClass;
    }

    void Update ()
    {
        // Detect player inputs client side - only detect this player's inputs if it's us.
        if(player.photonView.isMine)
        {
            // We can't do gameplay controls if we're dead.
            if(player.curState != PlayerState.Dead)
            {
                // Jump - Down
                if(Input.GetKeyDown(jump))
                    if(onJumpDown != null)
                        onJumpDown.Invoke();

                // Jump - Up
                if(Input.GetKeyUp(jump))
                    if(onJumpUp != null)
                        onJumpUp.Invoke();

                // Attack - Down
                if(Input.GetKeyDown(attack))
                    if(onAttackDown != null)
                        onAttackDown.Invoke();
            }

            // If the player presses the 'ESCAPE' key, quit the game.
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    // We're doing the JUMP - HOLD check in FixedUpdate, because every frame this will add force to the player.
    void FixedUpdate ()
    {
        // Detect player inputs client side - only detect this player's inputs if it's us.
        if(player.photonView.isMine)
        {
            // We can't do gameplay controls if we're dead.
            if(player.curState != PlayerState.Dead)
            {
                // Jump - Hold
                if(Input.GetKey(jump))
                    if(onJumpHold != null)
                        onJumpHold.Invoke();
            }
        }
    }

    void LateUpdate ()
    {
        if(enableMobileControls)
            mobileMovementAxis = 0.0f;
    }
}