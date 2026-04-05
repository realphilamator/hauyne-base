using UnityEngine;

public interface IInteractable
{
    bool CanInteract(GameObject interactor);
    float InteractionDistance { get; }
    void Interact(GameObject interactor);
}