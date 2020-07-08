using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{

    private float length;
    private float startposX;
    private float startposY;
    public GameObject camera;
    public float parallaxEffect;
    public float parallaxEffectY;
    
   
    void Start()
    {
        startposX = transform.position.x;
        startposY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float temp = camera.transform.position.x * (1 - parallaxEffect);
        float distX = camera.transform.position.x * parallaxEffect;
        float distY = camera.transform.position.y * parallaxEffectY;
        
        transform.position = new Vector2(startposX + distX, startposY + distY);

        if (temp > startposX + length)
        {
            startposX += length;
        }else if (temp < startposX - length)
        {
            startposX -= length;
        }
    }
}
