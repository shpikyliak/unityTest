using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PatrolEnemy : Enemy
{
   
    public float speed = 2f; 
    
    //attack rate
    [SerializeField] public GameObject attackPoint;
    public LayerMask playerLayer;
    public float attackRate = 2f;
    public Vector2 attackRange;
    private float nextAttackTime = 0f;
    private IEnumerator stopMoveWhileAttackCoroutine;
    private float standWhileAttackTime = 2.0f;
    
    private bool facingRight = false;
    private Rigidbody2D rb;
 
    protected bool canMove = true;
    
    //patrol
    public Transform[] patrolPoints;
    public bool isPatrol = false;
    protected int patrolPointIndex = 0;
    protected Transform nextPatrolPoint;
    protected float miniumDistanceToReach = 0.5f;
    private float waitOnPatrolPointTime = 4.0f;

    
    protected new void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        
        nextPatrolPoint = patrolPoints[patrolPointIndex];
        
        
        if (isNeedToFlipForNextPatrolPoint())
        {
            Flip();
        }
    }

    private void Update()
    {
        if (isPatrol)
        {
            Patrol();
        }
        
    }

    private void Patrol()
    {
       
        if (IsReachedPatrolDot())
        {
            WaitOnPatrolPoint();
            SetNextPatrolPoint();
            
            
            if (isNeedToFlipForNextPatrolPoint())
            {
                Flip();
            }
            
        }
        
        Move();
    }


    protected bool isNeedToFlipForNextPatrolPoint()
    {
        bool isOnRightSide = (transform.position.x - nextPatrolPoint.position.x) < 0;
      
        return isOnRightSide != facingRight;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireCube(attackPoint.transform.position, attackRange);
        
        Gizmos.color = Color.green;
         
    }

    private void Move()
    {
        if (!canMove)
        {
            return;
        }
                  
        Vector2 directionVector = (facingRight) ? Vector2.right : Vector2.left;
        
        animator.SetFloat("Speed", speed);
        
        directionVector *= speed;
        Vector2 velocity = rb.velocity;
        velocity.x = directionVector.x;
        rb.velocity = velocity;       
    }
    
    private bool IsReachedPatrolDot()
    {
       
        return (Vector2.Distance(transform.position, nextPatrolPoint.position) <= miniumDistanceToReach);
    }
    
    void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        facingRight = !facingRight;
    }

    public void Attack()
    {
        if (Time.time > nextAttackTime)
        {
            animator.SetTrigger("Attack");
            nextAttackTime = Time.time + attackRate;
            Stop();
            
            stopMoveWhileAttackCoroutine = WaitAndStand(standWhileAttackTime);
            StartCoroutine(stopMoveWhileAttackCoroutine);
        }
       
    }

    protected void Stop()
    {
        rb.velocity = Vector2.zero;
        animator.SetFloat("Speed", 0.0f);
    }

    private void WaitOnPatrolPoint()
    {
        Stop();
        stopMoveWhileAttackCoroutine = WaitAndStand(waitOnPatrolPointTime);
        StartCoroutine(stopMoveWhileAttackCoroutine);
    }
    
    private IEnumerator WaitAndStand(float waitTime)
    {
        canMove = false;
        yield return new WaitForSeconds(waitTime);
        canMove = true;
    }

    public void CheckAttack()
    {
        
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.transform.position, attackRange, transform.eulerAngles.z, playerLayer);

      
        foreach (Collider2D enemy in hitEnemies)
        {
            
            IDamage enemyObj = enemy.GetComponent<IDamage>();
           
            if (enemyObj != null)
            {
                enemyObj.TakeDamage(attack);
            }
            
        }      

    }

    public void SetNextPatrolPoint()
    {
        patrolPointIndex++;

        if (patrolPointIndex > (patrolPoints.Length - 1))
        {
            patrolPointIndex = 0;
        }

        nextPatrolPoint = patrolPoints[patrolPointIndex];
    }
    
}