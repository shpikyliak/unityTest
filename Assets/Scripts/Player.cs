using UnityEngine;
using Helpers;
using Interfaces;
using TMPro;


public class Player : MonoBehaviour, IDamage
{
    
    private Animator m_animator;
    private Rigidbody2D m_body2d;   
    private Vector2 checkpoint;
    private bool knockout = false;
    
    //moving
    private Sensor_Bandit m_groundSensor;
    private bool m_faceLeft;
    private MovingState _movingState;    
    private enum MovingState
    {
        Normal,
        DodgeRollingSliding,
        Crouch,
        Climb
    }
    
    //jump
    public float m_speed = 1.0f;
    public float m_jumpForce = 2.0f;
    public float fallMultiplayer = 2.5f;
    private bool m_grounded = false;
    private bool m_hasSecondJump = false;
    private bool m_canJump = false;

    //collider 
    public BoxCollider2D crouchCollider;
    public CapsuleCollider2D standCollider;
    public CapsuleCollider2D rollCollider;

    //lives
    private LivesController livesController;
    private float health = 100f;
    private float maxHealth = 100f;
    public SimpleHealthBar healthBar;
    public int lives = 2;
    public int maxLives = 5;
    private LiveState _liveState; 
    public TextMeshProUGUI dieText;
    public TextMeshProUGUI finalDieText;
    public GameObject livesBar;
    private enum LiveState
    {
        Alive,
        MustDie,
        Dead
    }

    //attack
    public Transform attackPoint;
    public float attackRange;
    public int attackDamage = 20;
    public LayerMask enemyLayer;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    //roll
    public float maxRollSpeed = 10f;
    private float rollDeceleration = 0.85f;
    private float rollSpeed;
    private bool shouldStopRolling;
    
    //ledgeClimb
    public Transform wallSensor;
    public Transform ledgeSensor;
    public LayerMask whatIsGround;
    private float wallCheckDistance = 0.5f;
    private bool isTouchingLedge;
    private bool isTouchingWall;
    private bool ledgeDetected;
    private bool canClimbLedge = false;
    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;
    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;
    public bool canSetClimbPos = true;
    public int count = 0;
    
   
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
        _liveState = LiveState.Alive;
        SetCheckpoint(transform.position);
        healthBar.UpdateBar(health, maxHealth);
        livesController = livesBar.GetComponent<LivesController>();
        livesController.InstantiateHeart(lives, maxLives);
        _movingState = MovingState.Normal;
        m_faceLeft = false;
        //collider
        standCollider.enabled = true;
        crouchCollider.enabled = false;
    }

    void Update()
    {
        if (health <= 0)
        {
            Die();
        }

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            Landed();
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }


        ControlHandler();
        CheckLedgeClimb();
        FallMultiply();

        m_animator.SetFloat("AirSpeed", m_body2d.velocity.y);
    }
    
    private void FixedUpdate()
    {
        if (_liveState == LiveState.MustDie)
        {
            Die();
        }
        else if (knockout)
        {
            MakeKnockback();
        }
        
        CheckSurroundings();
    }

    private void ControlHandler()
    {
        if (_liveState == LiveState.Alive)
        {
            float inputX = Input.GetAxis("Horizontal");

            FlipHandler(inputX);

            if (Input.GetMouseButtonDown(0))
            {
                Attack();
            }
            else if (Input.GetKey("s"))
            {
                Crouch();
            }
            else if (Input.GetKey("f"))
            {
                Roll();
            }
            //Jump
            else if (Input.GetKeyDown("space"))
            {
                JumpHandler();
            }
            //Run
            else if (Mathf.Abs(inputX) > Mathf.Epsilon)
                m_animator.SetInteger("AnimState", 2);
            //Idle
            else
                m_animator.SetInteger("AnimState", 0);

            // Move
            MoveHorizontal(inputX);

            if (_movingState == MovingState.Crouch && !Input.GetKey("s"))
            {
                StandUp();
            }

            if (_movingState == MovingState.DodgeRollingSliding)
            {
                HandleDodgeRollSliding();
            }
        }

        if (_liveState == LiveState.Dead && lives > 0)
        {
            if (Input.GetKeyDown("return"))
            {
                RecoverOnCheckpoint();
            }
        }
    }

    private void FallMultiply()
    {
        if (m_body2d.velocity.y < 0)
        {
            m_body2d.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplayer - 1) * Time.deltaTime;
        }
    }

    private void MoveHorizontal(float inputX)
    {
        if (m_animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && m_grounded)
        {
            m_body2d.velocity = Vector2.zero;
        }
        else
        {
            if (CanMove())
            {
                m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
            }
        }
    }

    private bool CanMove()
    {
        return _movingState != MovingState.Crouch && _movingState != MovingState.DodgeRollingSliding && _movingState != MovingState.Climb;
    }

    private bool CanCrouch()
    {
        return _movingState != MovingState.Crouch && _movingState != MovingState.DodgeRollingSliding && _movingState != MovingState.Climb;
    }

    private void FlipHandler(float inputX)
    {
        // Swap direction of sprite depending on walk direction
        if ((inputX > 0 && m_faceLeft) || (inputX < 0 && !m_faceLeft))
        {
            m_faceLeft = !m_faceLeft;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
//
//        else if (inputX < 0)
//        {
//            m_faceLeft = true;
//            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
//        }
    }

    private void Crouch()
    {
        if (CanCrouch())
        {
            _movingState = MovingState.Crouch;
            crouchCollider.enabled = Toggle.Make(crouchCollider.enabled);
            standCollider.enabled = Toggle.Make(standCollider.enabled);

            m_body2d.velocity = Vector2.zero;

            m_animator.SetBool("Crouch", true);
        }
    }

    private void Roll()
    {
        if (CanRoll())
        {
            _movingState = MovingState.DodgeRollingSliding;
            rollSpeed = maxRollSpeed;
            m_animator.SetTrigger("Roll");
            Physics2D.IgnoreLayerCollision(9, 8, true);
            crouchCollider.enabled = false;
            standCollider.enabled = false;
            rollCollider.enabled = true;
            shouldStopRolling = false;
            m_body2d.velocity = new Vector2(0, m_body2d.velocity.y);
        }
    }

    private bool CanRoll()
    {
        return _movingState != MovingState.DodgeRollingSliding && _movingState != MovingState.Climb;
    }

    private void HandleDodgeRollSliding()
    {
        Vector3 direction = m_faceLeft ? Vector2.left : Vector2.right;

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
        _movingState = MovingState.Normal;
        Physics2D.IgnoreLayerCollision(9, 8, false);
        crouchCollider.enabled = false;
        standCollider.enabled = true;
        rollCollider.enabled = false;
        m_body2d.velocity = new Vector2(0, m_body2d.velocity.y);
    }

    private void StandUp()
    {
        _movingState = MovingState.Normal;
        crouchCollider.enabled = Toggle.Make(crouchCollider.enabled);
        standCollider.enabled = Toggle.Make(standCollider.enabled);
        m_animator.SetBool("Crouch", false);
    }

    private void Attack()
    {
        if (Time.time >= nextAttackTime)
        {
            if (!m_grounded)
            {
                m_body2d.velocity = Vector2.zero;
            }


            m_animator.SetTrigger("Attack");

            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    private void CheckAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }
    }

    private void JumpHandler()
    {
        m_canJump = true;

        if (m_grounded == false)
        {
            m_canJump = false;

            if (m_hasSecondJump)
            {
                m_hasSecondJump = false;
                m_canJump = true;
            }
        }


        if (m_canJump)
        {
            Jump(m_body2d.velocity.x);
        }
    }

    private void Jump(float x)
    {
        m_animator.SetTrigger("Jump");
        m_grounded = false;
        m_animator.SetBool("Grounded", m_grounded);
        m_body2d.velocity = new Vector2(x, m_jumpForce);
        m_groundSensor.Disable(0.2f);
    }

    private void Landed()
    {
        m_grounded = true;
        m_hasSecondJump = true;
        m_canJump = true;
        m_animator.SetBool("Grounded", m_grounded);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            TakeDamage(collision.gameObject.GetComponent<IAttack>().GiveDamage());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("health"))
        {
            RestoreHealth(other.gameObject.GetComponent<Health>().restoreHealth());
        }

        if (other.gameObject.CompareTag("live"))
        {
            if (lives != maxLives)
            {
                other.gameObject.GetComponent<Lives>().restore();
                AddLive();
            }
        }
    }

    private void AddLive()
    {
        lives++;
        livesController.AddLive();
    }

    private void SubLive()
    {
        lives--;
        livesController.subLive();
    }

    private void Knockback()
    {
        knockout = true;
    }

    private void MakeKnockback()
    {
        m_animator.SetTrigger("Hurt");
        float knockbackXForce = 50f;
        float x = (m_faceLeft) ? -1f : 1f;
        m_body2d.velocity = new Vector2(x * knockbackXForce, m_jumpForce);

        knockout = false;
    }

    public void TakeDamage(int damage)
    {
        Knockback();

        health -= damage;


        healthBar.UpdateBar(health, maxHealth);


        if (health <= 0)
        {
            _liveState = LiveState.MustDie;
        }
    }

    void RestoreHealth(float restoreHealth)
    {
        float newHealth = health + restoreHealth;

        if (newHealth >= maxHealth)
        {
            newHealth = maxHealth;
        }

        health = newHealth;

        healthBar.UpdateBar(health, maxHealth);
    }

    public void Die()
    {
        if (_liveState == LiveState.MustDie)
        {
            SubLive();
            m_animator.SetInteger("AnimState", 0);
            m_animator.SetFloat("AirSpeed", 0);
            m_animator.SetBool("Dead", true);
            m_body2d.bodyType = RigidbodyType2D.Static;
            _liveState = LiveState.Dead;
            knockout = false;

            if (lives <= 0)
            {
                finalDieText.enabled = true;
            }
            else
            {
                dieText.enabled = true;
            }
        }
    }

    private void RecoverOnCheckpoint()
    {
        health = maxHealth;
        healthBar.UpdateBar(health, maxHealth);
        m_animator.SetInteger("AnimState", 0);
        m_animator.SetBool("Grounded", true);
        m_animator.SetBool("Dead", false);
        transform.position = checkpoint;
        _liveState = LiveState.Alive;
        dieText.enabled = false;
        m_body2d.bodyType = RigidbodyType2D.Dynamic;
    }

    public void SetCheckpoint(Vector2 position)
    {
        checkpoint = position;
    }

    private void OnDrawGizmosSelected()
    {
        
        Gizmos.DrawLine(wallSensor.position, new Vector3(wallSensor.position.x + wallCheckDistance, wallSensor.position.y, wallSensor.position.y));
        
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        if (!m_faceLeft)
        {
            Vector2 ledgePosition1 = new Vector2(Mathf.Floor(wallSensor.position.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(wallSensor.position.y) + ledgeClimbYOffset1);
            Vector2 ledgePosition2 = new Vector2(Mathf.Floor(wallSensor.position.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(wallSensor.position.y) + ledgeClimbYOffset2);
            Gizmos.DrawSphere(ledgePosition1, 0.1f);
            Gizmos.DrawSphere(ledgePosition2, 0.1f);
        }
        else
        {
            Vector2 ledgePosition1 = new Vector2(Mathf.Ceil(wallSensor.position.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(wallSensor.position.y) + ledgeClimbYOffset1);
            Vector2 ledgePosition2 = new Vector2(Mathf.Ceil(wallSensor.position.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(wallSensor.position.y) + ledgeClimbYOffset2);
            Gizmos.DrawSphere(ledgePosition1, 0.1f);
            Gizmos.DrawSphere(ledgePosition2, 0.1f);
        }
            

    }

    private void CheckLedgeClimb()
    {
        if (ledgeDetected && !canClimbLedge && _movingState != MovingState.Climb)
        {
            Debug.Log("CheckLedgeClimb 1");
            Debug.Log(count);
            count++;
            
            canClimbLedge = true;

            if (!m_faceLeft)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            
          
            _movingState = MovingState.Climb;
            m_body2d.velocity = Vector2.zero;
            m_body2d.gravityScale = 0.0f;
            
          
            m_animator.SetBool("Climb", canClimbLedge);
            
        }
           
        if (canClimbLedge && canSetClimbPos)
        {
            Debug.Log("CheckLedgeClimb 2");
            Debug.Log(count);
            count++;
            
            transform.position = ledgePos1;
            canSetClimbPos = false;
        }
    }

    public void FinishLedgeClimb()
    {   
        transform.position = ledgePos2;
        _movingState = MovingState.Normal;
        ledgeDetected = false;
        canClimbLedge = false;
        canSetClimbPos = true;
        m_body2d.gravityScale = 4.0f;
        m_animator.SetBool("Climb", canClimbLedge);  
        Debug.Log("FinishLedgeClimb 1");
        Debug.Log(count);
        count++;
    } 
    
    private void CheckSurroundings()
    {
        isTouchingWall = Physics2D.Raycast(wallSensor.position, transform.right, wallCheckDistance, whatIsGround);
        isTouchingLedge = Physics2D.Raycast(ledgeSensor.position, transform.right, wallCheckDistance, whatIsGround);

        if (isTouchingWall && !isTouchingLedge && !ledgeDetected && !m_grounded && !canClimbLedge && _movingState != MovingState.Climb)
        {
            ledgeDetected = true;
            ledgePosBot = wallSensor.position;

        }
    }
}