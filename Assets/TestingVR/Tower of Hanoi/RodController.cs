using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Threading.Tasks;

namespace TestingVR.Tower_of_Hanoi
{
    public class RodController : MonoBehaviour
    {
        private const float ROD_STACK_ORIGIN = 0.1f;
        
        private readonly Stack<SliceController> _stack = new ();
        
        [SerializeField]
        private RodCanvasDebug _debugCanvas;

        private void Update()
        {
            _debugCanvas.SetTextValue($"{_stack.Count}");
        }

        public void StackSlice(SliceController slice)
        {
            slice.lastRod = this;
            if (_stack.TryPeek(out var peek))
            {
                peek.DisableGrab(); 
                var peekrb = peek.GetComponent<Rigidbody>();
                var constraints = peekrb.constraints;
            
                constraints |= RigidbodyConstraints.FreezePositionY;
                peekrb.constraints = constraints;
            }
            
            _stack.Push(slice);
            
            slice.transform.position = transform.position + Vector3.up * ROD_STACK_ORIGIN;
            
            var rb = slice.GetComponent<Rigidbody>();
            rb.constraints = (RigidbodyConstraints)122;
            
            slice.GetOnGrabEvent().AddListener(OnSliceInStackSelection);
            FreezeY(rb);
        }
        
        private async Task FreezeY(Rigidbody rb)
        {
            await Task.Delay(500);
            var constraints = rb.constraints;
            
            constraints |= RigidbodyConstraints.FreezePositionY;
            rb.constraints = constraints;
        }

        public void PopSlice()
        {
            var poppingSlice = _stack.Pop();
            var rb = poppingSlice.GetComponent<Rigidbody>();
            var constraints = rb.constraints;
            
            constraints &= ~RigidbodyConstraints.FreezePositionX;
            constraints &= ~RigidbodyConstraints.FreezePositionZ;
            
            constraints &= ~RigidbodyConstraints.FreezeRotation;
            constraints &= ~RigidbodyConstraints.FreezePositionY;

            rb.constraints = constraints;

            if (_stack.TryPeek(out var peek))
            {
                peek.EnableGrab();
            }
                
        }

        private void OnSliceInStackSelection(SelectEnterEventArgs arg0)
        {
            var interactionSlice = arg0.interactableObject.transform.GetComponent<SliceController>();

            if (_stack.Count > 0)
            {
                if (interactionSlice == _stack.Peek())
                {
                    PopSlice();
                }
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