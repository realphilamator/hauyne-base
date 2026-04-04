using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Builds an ItemUseContext from its Inspector references and dispatches
/// item use calls to each item's BaseItem.Use() override.
///
/// No item-specific logic lives here. To add a new item, create a BaseItem
/// subclass — you never need to touch this script.
///
/// Attach this to your Player GameObject alongside PlayerController.
/// </summary>
public class ItemUseHandler : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector References
    // -------------------------------------------------------------------------

    [BoxGroup("References")]
    [Required]
    [Tooltip("The PlayerController on the player GameObject.")]
    public PlayerController player;

    [BoxGroup("References")]
    [Required]
    [Tooltip("The player's Transform.")]
    public Transform playerTransform;

    [BoxGroup("References")]
    [Required]
    [Tooltip("The Main Camera's Transform.")]
    public Transform cameraTransform;

    [BoxGroup("References")]
    [Required]
    [Tooltip("AudioSource used by items to play sounds. Typically on the player GameObject.")]
    public AudioSource audioSource;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    // Built once in Awake and reused for every item use call
    private ItemUseContext _ctx;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _ctx = new ItemUseContext
        {
            player          = player,
            playerTransform = playerTransform,
            cameraTransform = cameraTransform,
            audioSource     = audioSource,
        };
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to use the given item. Calls onConsumed if the item was successfully used.
    /// Returns true if the item was consumed, false if the use attempt failed
    /// (e.g. no valid target, item returned false from Use()).
    /// Called by InventoryManager when the player uses the selected slot.
    /// </summary>
    public bool Execute(BaseItem item, System.Action onConsumed)
    {
        if (item == null)
        {
            Debug.LogWarning("ItemUseHandler: Execute called with a null item.");
            return false;
        }

        return item.Use(_ctx, onConsumed);
    }
}
