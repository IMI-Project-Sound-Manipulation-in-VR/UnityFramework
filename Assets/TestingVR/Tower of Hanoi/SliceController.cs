using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace TestingVR.Tower_of_Hanoi
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class SliceController : MonoBehaviour
    {
        private XRGrabInteractable _grabInteractable;

        public XRGrabInteractable GrabInteractable
        {
            get => _grabInteractable;
            set => _grabInteractable = value;
        }


        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
        }

        public void DisableGrab()
        {
            _grabInteractable.enabled = false;
        }

        public void EnableGrab()
        {
            _grabInteractable.enabled = true;
        }

        public bool IsInGrab()
        {
            
            return _grabInteractable.isSelected;
        }

        public UnityEvent<SelectEnterEventArgs> GetOnGrabEvent()
        {
            return _grabInteractable.selectEntered;
        }
    }
}