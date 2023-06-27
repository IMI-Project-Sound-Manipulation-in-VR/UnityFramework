using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace TestingVR.Tower_of_Hanoi
{
    public class RodController : MonoBehaviour
    {
        private const float ROD_STACK_ORIGIN = 0.1f;
        
        private readonly Stack<SliceController> _stack = new ();

        public void StackSlice(SliceController slice)
        {
            if (_stack.TryPeek(out var peek))
                peek.DisableGrab(); 
            
            _stack.Push(slice);
            
            slice.transform.position = transform.position + Vector3.up * ROD_STACK_ORIGIN;
            
            var rb = slice.GetComponent<Rigidbody>();
            rb.constraints = (RigidbodyConstraints)122;
            
            slice.GetOnGrabEvent().AddListener(OnSliceInStackSelection);
        }

        public void PopSlice()
        {
            var popingSlice = _stack.Pop();
            var rb = popingSlice.GetComponent<Rigidbody>();
            var constraints = rb.constraints;
            
            constraints &= ~RigidbodyConstraints.FreezePositionX;
            constraints &= ~RigidbodyConstraints.FreezePositionZ;
            
            constraints &= ~RigidbodyConstraints.FreezeRotation;

            rb.constraints = constraints;

            if(_stack.TryPeek(out var peek))
                peek.EnableGrab();
        }

        public void OnSliceInStackSelection(SelectEnterEventArgs arg0)
        {
            var interactionSlice = arg0.interactableObject.transform.GetComponent<SliceController>();

            if (interactionSlice == _stack.Peek())
            {
                PopSlice();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            var slice = other.GetComponent<SliceController>();
            if (
                slice != null            &&
                !_stack.Contains(slice)  &&
                !slice.IsInGrab())     
            {
                StackSlice(slice);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var slice = other.GetComponent<SliceController>();
            if (
                slice != null            &&
                slice.IsInGrab())     
            {
                var grabInteractable = slice.GrabInteractable;
                grabInteractable.firstInteractorSelecting.transform.parent.GetComponent<ActionBasedController>().SendHapticImpulse(0.5f, 0.5f);
            }
        }
    }
}