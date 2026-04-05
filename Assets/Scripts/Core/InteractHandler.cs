using UnityEngine;
using UnityEngine.UI;

public class InteractHandler : MonoBehaviour
{
    [SerializeField] private float maxInteractionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private RawImage reticle;
    [SerializeField] private Texture2D reticleImage, interactImage;

    private IInteractable _currentTarget;

    private void Update()
    {
        FindTarget();
        if (_currentTarget != null && InputManager.Instance.GetActionKeyDown(InputAction.Interact))
        {
            _currentTarget.Interact(gameObject);
        }
    }

    private void FindTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        _currentTarget = null;
        reticle.texture = reticleImage;

        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                float dist = Vector3.Distance(transform.position, hit.point);
                if (dist <= interactable.InteractionDistance && interactable.CanInteract(gameObject))
                {
                    _currentTarget = interactable;
                    reticle.texture = interactImage;
                    return;
                }
            }
        }

        _currentTarget = null;
    }
}