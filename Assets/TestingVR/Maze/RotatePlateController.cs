using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR.Interaction.Toolkit.AR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RotatePlateController : MonoBehaviour
{
    [SerializeField] private Transform xAxisModifier;
    [SerializeField] private Transform zAxisModifier;
    [SerializeField] private float multiplier;

    private Quaternion _startupRotation;

    private void Start()
    {
        _startupRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        Quaternion targetRotation = Quaternion.Euler(-xAxisModifier.eulerAngles.z, transform.rotation.eulerAngles.y, zAxisModifier.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(_startupRotation, targetRotation, multiplier);
    }
}
