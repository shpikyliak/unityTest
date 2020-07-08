using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LivesController : MonoBehaviour
{
    [SerializeField] public GameObject heartContainerPrefab;
    [SerializeField] public Transform heartsParent;

    private GameObject[] heartContainers;

    private float offset = 0.1f;
    private int iterator = 0;

    public void InstantiateHeart(int lives, int maxLives)
    {
        heartContainers = new GameObject[maxLives];

        for (int i = 0; i < lives; i++)
        {
            AddLive();
        }
    }


    public void AddLive()
    {             
        GameObject temp = Instantiate(heartContainerPrefab);
        temp.GetComponent<SpriteRenderer>().enabled = true;


        Vector2 newPos = new Vector2(
            heartContainerPrefab.transform.position.x +
            ((heartContainerPrefab.GetComponent<Renderer>().bounds.size.x + offset) * iterator),
            heartContainerPrefab.transform.position.y
        );
        Debug.Log(temp.gameObject);
        temp.transform.SetParent(heartsParent, false);
        temp.transform.position = newPos;
        heartContainers[iterator] = temp;
        iterator++;
    }

    public void subLive()
    {
        iterator--;
        GameObject lastHeart = heartContainers[iterator];
        
        Destroy(lastHeart);
    }
}