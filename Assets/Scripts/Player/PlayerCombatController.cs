using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField] private bool combatEnabled;
    [SerializeField] private float inputTimer, attack1_1Radius, attack1_2Radius, attack1Damage;
    [SerializeField] private Transform attack1_1HitBoxPos, attack1_2HitBoxPos;
    [SerializeField] private LayerMask whatIsDamageable;

    private bool gotInput, isAttacking, isFirstAttack;
    private float lastInputTime = Mathf.NegativeInfinity;
    private Animator anim;
    private Rigidbody2D rb;
    private float[] attackDetails = new float[2];

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        anim.SetBool("canAttack", combatEnabled);
    }

    private void Update()
    {
        CheckCompatInput();
        CheckAttacks();
    }

    private void CheckCompatInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (combatEnabled)
            {
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                isFirstAttack = !isFirstAttack;
                anim.SetBool("attack1", true);
                anim.SetBool("firstAttack", isFirstAttack);
                anim.SetBool("isAttacking", isAttacking);
                
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        if (Time.time >= lastInputTime + inputTimer)
        {
            gotInput = false;
        }
    }

    private void CheckAttackHitBox()
    {
        float radius = (isFirstAttack) ? attack1_1Radius : attack1_2Radius;
        Transform hitBoxPos = (isFirstAttack) ? attack1_1HitBoxPos : attack1_2HitBoxPos;
        
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(hitBoxPos.position, radius, whatIsDamageable);

        attackDetails[0] = attack1Damage;
        attackDetails[1] = transform.position.x;
        
        foreach (Collider2D collider in detectedObjects)
        {
            collider.transform.parent.SendMessage("Damage", attackDetails);
        }
    }

    private void FinishAttack()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attack1_1HitBoxPos.position, attack1_1Radius);
        Gizmos.DrawWireSphere(attack1_2HitBoxPos.position, attack1_2Radius);
    }
}