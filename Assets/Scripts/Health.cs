using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
   private float restoreHealthValue = 30f;

   public float restoreHealth()
   {
      Destroy(gameObject);
      return restoreHealthValue;
   }
}
