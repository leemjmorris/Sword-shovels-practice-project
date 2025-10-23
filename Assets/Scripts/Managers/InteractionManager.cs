using UnityEngine;
using Interaction;

namespace Managers
{
    //LMJ: Manages all interaction objects in the scene
    public class InteractionManager : MonoBehaviour
    {
        public static InteractionManager Instance { get; private set; }

        [Header("Input Settings")]
        [SerializeField] private KeyCode interactionKey = KeyCode.F;

        private IInteractable currentInteractable;
        private bool isInInteractionZone = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (isInInteractionZone && currentInteractable != null)
            {
                if (Input.GetKeyDown(interactionKey) && currentInteractable.CanInteract())
                {
                    currentInteractable.Interact();
                }
            }
        }

        //LMJ: Register an interactable object when player enters its trigger zone
        public void EnterInteractionZone(IInteractable interactable)
        {
            currentInteractable = interactable;
            isInInteractionZone = true;

        }

        //LMJ: Unregister an interactable object when player exits its trigger zone
        public void ExitInteractionZone(IInteractable interactable)
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
                isInInteractionZone = false;

            }
        }

        //LMJ: Get the current interactable object
        public IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }

        //LMJ: Check if player is in an interaction zone
        public bool IsInInteractionZone()
        {
            return isInInteractionZone;
        }
    }
}
