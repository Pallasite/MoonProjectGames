using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Girl_Script : MonoBehaviour
{
    public Rigidbody girl;
    
    void Start()
    {
        girl = GetComponent<Rigidbody>();
    }
}
