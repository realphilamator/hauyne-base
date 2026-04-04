using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Handles item pickup via a centre-screen raycast when the Interact key is pressed.
/// Attach this to the Player GameObject.
///
/// How pickup works:
///   1. Player presses Interact (bound in InputManager).
///   2. A raycast fires from the centre of the screen.
///   3. The hit object's name (and its parents) is matched against the ItemRegistry.
///   4. If a match is found and the player is close enough, the item is collected.
///
/// To make a pickup in the scene:
///   1. Create a GameObject whose name (or one of its parents' names) matches
///      the pickupName field on your BaseItem asset.
///   2. Optionally attach an ItemPickupCallback component for custom pickup logic.
/// </summary>
public class PickupScript : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------

    [BoxGroup("References")]
    [Required]
    [Tooltip("The ItemRegistry asset containing all item definitions.")]
    public ItemRegistry itemRegistry;

    [BoxGroup("References")]
    [Required]
    [Tooltip("The player's Transform, used for distance checks.")]
    public Transform playerTransform;

    [BoxGroup("Settings")]
    [Tooltip("Maximum distance the player can be from a pickup to collect it.")]
    public float pickupRange = 10f;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Update()
    {
        // Don't pick up while paused or if the interact key wasn't just pressed
        if (Time.timeScale == 0f) return;
        if (!Singleton<InputManager>.Instance.GetActionKeyDown(InputAction.Interact)) return;

        TryPickup();
    }

    // -------------------------------------------------------------------------
    // Private Logic
    // -------------------------------------------------------------------------

    private void TryPickup()
    {
        // Cast a ray from the centre of the screen
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // Walk up the hierarchy from the hit object to find a matching item definition.
        // This handles cases where the raycast hits a child object (e.g. a sprite on the pickup).
        Transform check = hit.transform;
        BaseItem def = null;

        while (check != null)
        {
            def = itemRegistry.GetByPickupName(check.name);
            if (def != null) break;
            check = check.parent;
        }

        if (def == null) return;

        // Range check
        if (Vector3.Distance(playerTransform.position, check.position) >= pickupRange) return;

        // Hide the pickup unless the item is flagged to stay visible
        if (!def.stayOnPickup)
            check.gameObject.SetActive(false);

        // Add to inventory if flagged
        if (def.addToInventory)
            InventoryManager.Instance.CollectItem(def);

        // Fire any custom pickup logic attached to the pickup GameObject
        var callback = check.GetComponent<ItemPickupCallback>();
        if (callback != null)
            callback.OnPickup();
    }
}
