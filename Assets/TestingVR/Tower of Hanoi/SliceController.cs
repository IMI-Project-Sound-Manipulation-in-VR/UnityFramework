using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace TestingVR.Tower_of_Hanoi
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class SliceController : MonoBehaviour
    {
        public XRGrabInteractable GrabInteractable { get; set; }

        public RodController lastRod;

        private void Awake()
        {
            GrabInteractable = GetComponent<XRGrabInteractable>();
            StartCoroutine(LateFixedUpdate());
        }

        private IEnumerator LateFixedUpdate()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                if (!IsInGrab() && transform.position.x != lastRod.transform.position.x)
                {
                    lastRod.StackSlice(this);
                }
            }
        }

        public void DisableGrab()
        {
            GrabInteractable.enabled = false;
        }

        public void EnableGrab()
        {
            GrabInteractable.enabled = true;
        }

        public bool IsInGrab()
        {
            return GrabInteractable.isSelected;
        }

        public UnityEvent<SelectEnterEventArgs> GetOnGrabEvent()
        {
            return GrabInteractable.selectEntered;
        }
    }
}