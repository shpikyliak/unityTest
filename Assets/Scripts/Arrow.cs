using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 20f;
    public int attackRate = 20;
    public float range = 7f;
    
    protected Rigidbody2D rb;
    protected float startPosX;
    protected float startPosY;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        startPosX = transform.position.x;
        startPosY = transform.position.y;
    }

    private void Update()
    {
        if (transform.position.x > startPosX + range || transform.position.x < startPosX - range)
        {
            Destroy(gameObject);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().TakeDamage(attackRate);
           
        }
        
       
    }
}
