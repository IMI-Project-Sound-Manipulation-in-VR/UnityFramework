using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace TestingVR.Maze
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class PickupInteractionController : MonoBehaviour
    {
        [SerializeField] private List<GameObject> setActiveObjects;
        
        private XRGrabInteractable _grabInteractable;
    
        private void Start() 
        {
            foreach (var activeObject in setActiveObjects)
            {
                activeObject.SetActive(false);
            }
            
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _grabInteractable.selectEntered.AddListener(Selected);
            _grabInteractable.selectExited.AddListener(Deselected);
        }
        
        private void Selected(SelectEnterEventArgs arguments)
        {
            foreach (var activeObject in setActiveObjects)
            {
                activeObject.SetActive(true);
            }
        }

        private void Deselected(SelectExitEventArgs arguments) 
        {
            foreach (var activeObject in setActiveObjects)
            {
                activeObject.SetActive(false);
            }
        }
    }
}
