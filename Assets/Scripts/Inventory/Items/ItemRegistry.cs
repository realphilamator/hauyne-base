using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// A ScriptableObject that holds all item definitions for the game.
/// Create one registry asset and assign it wherever items need to be looked up
/// (InventoryManager, PickupScript, etc.).
///
/// To add a new item:
///   1. Create your BaseItem subclass asset via the Assets/Create/Items menu.
///   2. Drag it into the Items list here.
/// </summary>
[CreateAssetMenu(fileName = "ItemRegistry", menuName = "Items/Item Registry")]
public class ItemRegistry : ScriptableObject
{
    [Tooltip("All item definitions in the game. Order does not matter.")]
    public BaseItem[] items;

    /// <summary>
    /// Returns the BaseItem whose pickupName matches the given string, or null if not found.
    /// Used by PickupScript to identify a pickup GameObject in the scene.
    /// </summary>
    public BaseItem GetByPickupName(string pickupName)
    {
        foreach (var item in items)
        {
            if (item != null && item.pickupName == pickupName)
                return item;
        }

        return null;
    }
}
