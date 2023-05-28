using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AntiPhysicsSleep : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Rigidbody>().sleepThreshold = 0f;
    }
}
