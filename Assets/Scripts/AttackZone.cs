using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            gameObject.transform.parent.GetComponent<PatrolEnemy>().Attack();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            gameObject.transform.parent.GetComponent<PatrolEnemy>().Attack();
        }
    }
}
