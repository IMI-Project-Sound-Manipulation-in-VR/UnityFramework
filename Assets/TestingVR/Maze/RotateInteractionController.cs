using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR.Interaction.Toolkit.AR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class RotateInteractionController : MonoBehaviour
{
    private Transform _interactorTransform;
    private XRGrabInteractable _grabInteractable;
    
    private void Start() 
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _grabInteractable.selectEntered.AddListener(Selected);
        _grabInteractable.selectExited.AddListener(Deselected);
    }

    private void FixedUpdate() 
    {
        if (_interactorTransform != null)
        {
            var thisRotation = transform.rotation;
            Vector3 newRotation = new Vector3(thisRotation.eulerAngles.x, thisRotation.eulerAngles.y, _interactorTransform.rotation.eulerAngles.z);
            thisRotation = Quaternion.Euler(newRotation);
            transform.rotation = thisRotation;
        }
    }

    public void Selected(SelectEnterEventArgs arguments) 
    {
        _interactorTransform = arguments.interactorObject.transform;
    }

    public void Deselected(SelectExitEventArgs arguments) 
    {
        _interactorTransform = null;
    }
}
