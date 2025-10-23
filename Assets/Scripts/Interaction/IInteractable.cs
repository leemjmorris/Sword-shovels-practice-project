using UnityEngine;

namespace Interaction
{
    //LMJ: Base interface for all interactable objects
    public interface IInteractable
    {
        //LMJ: Execute the interaction
        void Interact();

        //LMJ: Check if this object can be interacted with
        bool CanInteract();

        //LMJ: Check if entity should stop when reaching this interaction object
        bool ShouldStopOnReach();

        //LMJ: Get the interaction trigger position
        Vector3 GetInteractionPosition();
    }
}
