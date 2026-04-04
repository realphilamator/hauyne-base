using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Base class for all item definitions. Create a subclass per item and override Use()
/// with that item's logic. Any references the item needs (prefabs, audio clips, floats)
/// should be declared as fields on the subclass — not on ItemUseContext.
///
/// How to create a new item:
///   1. Create a new script that inherits from BaseItem.
///   2. Add [CreateAssetMenu] to it so you can make assets from the Unity menu.
///   3. Declare any item-specific fields (prefabs, clips, values) on your subclass.
///   4. Override Use() with your item's behaviour.
///   5. Create the asset, fill in the fields, and add it to the ItemRegistry.
///
/// Example:
///
///     [CreateAssetMenu(fileName = "ZestyBarItem", menuName = "Items/Zesty Bar")]
///     public class ZestyBarItem : BaseItem
///     {
///         [Header("Zesty Bar Settings")]
///         public float staminaBoost = 2f;
///         public AudioClip eatSound;
///
///         public override bool Use(ItemUseContext ctx, System.Action onConsumed)
///         {
///             ctx.player.stamina += staminaBoost;
///             ctx.audioSource.PlayOneShot(eatSound);
///             onConsumed();
///             return true;
///         }
///     }
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Base Item")]
public class BaseItem : ScriptableObject
{
    [BoxGroup("Identity")]
    [Tooltip("Display name shown in the inventory UI.")]
    public string itemName = "New Item";

    [BoxGroup("Identity")]
    [Tooltip("The icon shown in the inventory slot. Assign a Texture asset.")]
    public Texture icon;

    [BoxGroup("Pickup")]
    [Tooltip("Must match the name of the pickup GameObject in the scene exactly. " +
             "Used by PickupScript to identify which item definition to use on interact.")]
    public string pickupName;

    [BoxGroup("Pickup")]
    [Tooltip("If true, the item is added to an inventory slot when picked up. " +
             "If false, OnPickup() fires immediately and the item is not stored.")]
    public bool addToInventory = true;

    [BoxGroup("Pickup")]
    [Tooltip("If true, the pickup GameObject stays visible in the scene after being picked up. " +
             "Useful for items that are triggered rather than physically collected.")]
    public bool stayOnPickup = false;

    /// <summary>
    /// Override in each subclass to implement what happens when the player uses this item.
    /// Return true if the item was successfully used and should be consumed.
    /// Return false if the use attempt failed — the item will NOT be removed from the inventory.
    /// Only call onConsumed() when returning true.
    /// </summary>
    public virtual bool Use(ItemUseContext ctx, System.Action onConsumed)
    {
        Debug.LogWarning($"BaseItem: No Use() override on '{itemName}'. Did you forget to override it?");
        return false;
    }
}
