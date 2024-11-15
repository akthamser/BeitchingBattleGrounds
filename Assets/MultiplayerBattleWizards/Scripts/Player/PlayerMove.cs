using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's movement and positioning.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerMove : MonoBehaviour
{
    [Header("Values")]
    public float moveSpeed;             // Horizontal velocity.
    public float jumpForce;             // Force applied upwards.
    public bool canMove;                // Can the player move?

    [Header("Ground Check")]
    public bool grounded;               // Are we standing on the ground?
    private bool jumped;                // Have we jumped? If so, we can't jump anymore until we touch the ground.

    private List<Collider2D> touchingColliders = new List<Collider2D>();    // Colliders we're currently standing on.

    private bool isJumping;             // Are we currently holding down the jump button and jumping?
    private float jumpTimer;            // Used to track time spent adding force upwards.
    private float jumpTime = 1.0f;      // Max jump hold time.

    public float facingDirection        // Direction we're facing (-1 = left, 1 = right).
    {
        get { return transform.localScale.x; }
    }

    // Jump Pre-press
    private float jumpPrePressThreshold = 0.1f;     // Leeway given between pressing the jump button and touching the ground.
    private float jumpPrePressTime;                 // Time the jump button was pressed.

    // Components
    private Player player;
    private Rigidbody2D rig;
    private Collider2D col;

    #region Subscribing to Input Events

    void OnEnable ()
    {
        // Subscribe to input events.
        player.input.onJumpDown.AddListener(OnJumpKeyDown);
        player.input.onJumpHold.AddListener(OnJumpKeyHold);
        player.input.onJumpUp.AddListener(OnJumpKeyUp);
    }

    void OnDisable ()
    {
        // Un-subscribe from input events.
        player.input.onJumpDown.RemoveListener(OnJumpKeyDown);
        player.input.onJumpHold.RemoveListener(OnJumpKeyHold);
        player.input.onJumpUp.RemoveListener(OnJumpKeyUp);
    }

    #endregion

    // Sets components referenced from the 'Player' class.
    public void SetComponents (Player playerClass)
    {
        player = playerClass;
        rig = player.rig;
        col = GetComponent<Collider2D>();
    }

    void Update ()
    {
        // If this isn't our player, return.
        if(!player.photonView.isMine)
            return;
        
        if(canMove)
            Move();
    }

    #region Input Events

    // Called the frame when the 'Jump' key is pressed.
    // Invoked from the 'onJumpDown' event in PlayerInput.
    void OnJumpKeyDown ()
    {
        if(!canMove) return;

        // If we're grounded or haven't jumped - Jump!
        if(grounded || !jumped)
            Jump(true);
        else
            jumpPrePressTime = Time.time;
    }

    // Called when the 'Jump' key is held down.
    // Invoked from the 'onJumpHold' event in PlayerInput.
    void OnJumpKeyHold ()
    {
        if(!canMove) return;

        // Jump button HOLD.
        if(isJumping && canMove)
        {
            if(jumpTimer > 0.0f)
            {
                Jump(false);
                jumpTimer -= Time.deltaTime;
            }
            else
                isJumping = false;
        }
    }

    // Called the frame when the 'Jump' key is released.
    // Invoked from the 'onJumpUp' event in PlayerInput.
    void OnJumpKeyUp ()
    {
        isJumping = false;
    }

    #endregion

    // Moves the player horizontally.
    // axisOverride - overrides the input movement axis (used for mobile controls).
    public void Move ()
    {
        // Get the movement axis.
        float x = player.input.enableMobileControls ? player.input.mobileMovementAxis : Input.GetAxis(player.input.movementAxis);

        rig.velocity = new Vector2(x * moveSpeed, rig.velocity.y);

        // Flip the player to face their moving direction.
        if(x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if(x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // Adds force upwards to the player.
    void Jump (bool initialJump)
    {
        // On jump key down.
        if(initialJump)
        {
            jumped = true;
            isJumping = true;
            jumpTimer = jumpTime;

            rig.velocity = new Vector2(rig.velocity.x, jumpForce);
        }
        // On jump key hold.
        else
        {
            rig.AddForce(Vector2.up * jumpForce * (jumpTimer / jumpTime), ForceMode2D.Force);
        }
    }

    void OnCollisionEnter2D (Collision2D other)
    {
        // Is the player standing on the collided object?
        if(ColliderJumpTest(other))
        {
            touchingColliders.Add(other.collider);
            grounded = true;
            jumped = false;
            JumpPrePressCheck();
        }
    }

    // Returns true if the player's classed as 'touching the ground'.
    bool ColliderJumpTest (Collision2D other)
    {
        // Get the collision contacts.
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        other.GetContacts(contacts);

        // Are we above the contact point?
        if(col.bounds.center.y > contacts[0].point.y)
        {
            // Is the contact within the width of our collider?
            if(col.bounds.max.x > contacts[0].point.x)
                if(col.bounds.min.x < contacts[0].point.x)
                    return true;
        }

        return false;
    }

    void OnCollisionExit2D (Collision2D other)
    {
        // Did the player just exit the collider of something they were just standing on?
        if(touchingColliders.Contains(other.collider))
        {
            touchingColliders.Remove(other.collider);

            if(touchingColliders.Count == 0)
                grounded = false;
        }
    }

    // Called when we land on the ground.
    // If we pressed the jump key just before touching ground - jump.
    void JumpPrePressCheck ()
    {
        if(Time.time - jumpPrePressTime <= jumpPrePressThreshold)
        {
            jumpPrePressTime = 0;
            Jump(true);
        }
    }
}