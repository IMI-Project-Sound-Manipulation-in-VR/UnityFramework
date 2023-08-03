using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResetPositionOnTrigger : MonoBehaviour
{
    [SerializeField] private List<Collider> validColliders;

    private Vector3 originPosition;
    
    private void Start()
    {
        originPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (validColliders.Any(col => col == other))
        {
            transform.position = originPosition;
        }
    }
}
