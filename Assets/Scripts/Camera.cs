using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
   
    [SerializeField] private GameObject player;
    

    // Update is called once per frame
    void FixedUpdate()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        Vector3 position = transform.position;
        position.x = player.transform.position.x;
        position.y = player.transform.position.y + 2;
        transform.position = position;
    }
}
