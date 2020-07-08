using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;

public class Trap : MonoBehaviour, IAttack
{
    [SerializeField] private int attack = 2;
    
    public int GiveDamage()
    {
        return attack;
    }
  
}
