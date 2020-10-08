using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    //movement
    public float movementSpeed = 10.0f;
    private float movementInputDirection;
    private bool isFacingRight = true;
    private bool isWalking;
    private bool canMove = true;
    private bool canFlip = true;
    private int facingDirection = 1;

    //jump
    public float jumpForce = 16.0f;
    public Transform groundCheck;
    public float groundCheckRadius;
    public int amountOfJumps = 2;
    public float movementForceInAir = 6.0f;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float jumpTimerSet = 0.15f;
    private int amountOfJumpsLeft;
    public LayerMask whatIsGround;
    private bool isGrounded;
    private bool canJump;
    private float jumpTimer;
    private bool isAttempingToJump;
    private bool checkJumpMultiplier;

    //Ledge climb
    public Transform wallCheck;
    public Transform ledgeCheck;
    public float wallCheckDistance;
    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;
    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;
    private bool isTouchingLedge;
    private bool isTouchingWall;
    private bool canClimbLedge = false;
    private bool ledgeDetected;

    //Roll
    public float maxRollSpeed = 10f;
    private float rollDeceleration = 0.55f;
    private float rollSpeed;
    private bool shouldStopRolling;
    private bool isRolling;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
    }

    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        CheckIfCanJump();
        CheckJump();
        CheckLedgeClimb();
        CheckRoll();

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if (isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }

    private void CheckLedgeClimb()
    {
        if (ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            if (isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1,
                    Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2,
                    Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1,
                    Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2,
                    Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;

            anim.SetBool("canClimbLedge", canClimbLedge);
        }

        if (canClimbLedge)
        {
            transform.position = ledgePos1;
        }
    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y < 0.01f)
        {
            amountOfJumpsLeft = amountOfJumps;
        }

        if (amountOfJumpsLeft <= 0 || isRolling)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if (!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if (Mathf.Abs(rb.velocity.x) >= 0.01f)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    public void EnableFlip()
    {
        canFlip = true;
        canMove = true;
    }

    public void DisableFlip()
    {
        canFlip = false;
        canMove = false;
    }
    private void Flip()
    {
        if (canFlip)
        {
            isFacingRight = !isFacingRight;
            facingDirection *= -1;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            if ((isGrounded || (amountOfJumpsLeft > 0)) && !isRolling)
            {
                Jump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttempingToJump = true;
            }
        }

        if (checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }

        if (Input.GetButtonDown("Roll") && CheckIfCanRoll())
        {
            Roll();
        }
    }

    private void CheckRoll()
    {
        if (isRolling)
        {
            HandleRoll();
        }
    }

    private bool CheckIfCanRoll()
    {
        return !canClimbLedge;
    }

    private void Roll()
    {
        isRolling = true;
        rb.velocity = new Vector2(0, rb.velocity.y);
        rollSpeed = maxRollSpeed;
        canMove = false;
        canFlip = false;
        shouldStopRolling = false;
    }

    private void HandleRoll()
    {
        Vector3 direction = !isFacingRight ? Vector2.left : Vector2.right;

        transform.position += direction * rollSpeed * Time.deltaTime;

        rollSpeed -= rollDeceleration;

        if (shouldStopRolling || rollSpeed <= 0)
        {
            RollStop();
        }
    }

    public void HandleStopRolling()
    {
        shouldStopRolling = true;
    }

    private void RollStop()
    {
        canMove = true;
        canFlip = true;
        isRolling = false;
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    private void CheckJump()
    {
        if (jumpTimer > 0)
        {
            if (isGrounded && !isRolling)
            {
                Jump();
            }
        }

        if (isAttempingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }
    }

    private void Jump()
    {
        if (canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAttempingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void ApplyMovement()
    {
        if (!isGrounded && movementInputDirection == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if (!isGrounded && movementInputDirection != 0 && canMove)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * movementInputDirection, 0);
            rb.AddForce(forceToAdd);

            if (Mathf.Abs(rb.velocity.x) > movementSpeed)
            {
                rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
            }
        }
        else if (isGrounded && canMove)
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isRolling", isRolling);
        anim.SetFloat("yVelocity", rb.velocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position,
            new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));

        Gizmos.DrawLine(ledgeCheck.position,
            new Vector3(ledgeCheck.position.x + wallCheckDistance, ledgeCheck.position.y, ledgeCheck.position.z));
        
        Gizmos.DrawSphere(ledgePos1, 0.1f);
        Gizmos.DrawSphere(ledgePos2, 0.1f);
    }

    public int GetFacingDirection()
    {
        return facingDirection;
    }
}