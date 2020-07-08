using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : Enemy
{
    private IEnumerator attackCoroutine;
    private bool isAttacking = false;

    public Transform firePoint;
    public GameObject arrowPrefab;

    protected new void Start()
    {
        base.Start();

        isAttacking = false;
    }

    private void Update()
    {
        if (!isAttacking)
        {
            AttackLoop();
        }
    }


    private void AttackLoop()
    {
        attackCoroutine = WaitAndAttack(3.0f);
        StartCoroutine(attackCoroutine);
    }

    private IEnumerator WaitAndAttack(float waitTime)
    {
        isAttacking = true;
        yield return new WaitForSeconds(waitTime);
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        Attack();
        isAttacking = false;
    }

    void Attack()
    {
       
        Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
    }
}