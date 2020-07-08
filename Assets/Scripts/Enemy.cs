using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;

public class Enemy : MonoBehaviour, IAttack, IDamage
{
    [SerializeField] protected int attack = 2;

    public int maxHealth = 20;
    protected int health;
    protected Animator animator;
    [SerializeField] public GameObject dieEffectRef;

    protected void Start()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
        
    }

    public int GiveDamage()
    {
        return attack;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        animator.SetTrigger("Hurt");
        //take dmg anim
        
        if (health <= 0)
        {
            Die();
        }

    }

    public void Die()
    {
        GameObject dieEffect = Instantiate(dieEffectRef);

        dieEffect.transform.position = gameObject.transform.position;
        
        Destroy(gameObject);
    }
    
    
}
