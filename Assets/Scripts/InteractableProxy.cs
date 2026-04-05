using UnityEngine;

// Attach to a child object to forward interactions to a parent IInteractable
public class InteractableProxy : MonoBehaviour, IInteractable
{
    [SerializeField] private MonoBehaviour target;

    private IInteractable _target;

    private void Awake()
    {
        _target = target as IInteractable;
    }

    public float InteractionDistance => _target.InteractionDistance;
    public bool CanInteract(GameObject interactor) => _target.CanInteract(interactor);
    public void Interact(GameObject interactor) => _target.Interact(interactor);
}