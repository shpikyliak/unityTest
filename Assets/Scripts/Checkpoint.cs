using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool is_active;
    private SpriteRenderer _spriteRenderer;
    [SerializeField] public GameObject activateEffectReff;
    private Vector3 bottomPosition;

    private void Start()
    {
        is_active = false;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = Color.black;
        
        bottomPosition = transform.Find("bottomPosition").transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
     
        if (!is_active && other.gameObject.CompareTag("Player"))
        {
            SetActive();
            other.gameObject.GetComponent<Player>().SetCheckpoint(transform.position);
        }
    }

    private void SetActive()
    {
        is_active = true;
        _spriteRenderer.color = Color.red;
        
        GameObject activateEffect = Instantiate(activateEffectReff);
        
        activateEffect.transform.position = bottomPosition;
    }
}
