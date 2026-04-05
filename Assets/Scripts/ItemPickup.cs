using UnityEngine;

[RequireComponent(typeof(ItemPickupCallback))] // optional, remove if not always present
public class ItemPickup : MonoBehaviour, IInteractable
{
    [Tooltip("Override pickup range. Leave at 0 to use the Interactor's default.")]
    public float interactionDistance = 10f;

    public float InteractionDistance => interactionDistance;

    public bool CanInteract(GameObject interactor) => true;

    public void Interact(GameObject interactor)
    {
        PickupScript pickup = interactor.GetComponent<PickupScript>();
        if (pickup == null) return;

        BaseItem def = pickup.itemRegistry.GetByPickupName(GetMatchingName(pickup.itemRegistry));
        if (def == null) return;

        if (!def.stayOnPickup)
            gameObject.SetActive(false);

        if (def.addToInventory)
            InventoryManager.Instance.CollectItem(def);

        var callback = GetComponent<ItemPickupCallback>();
        if (callback != null)
            callback.OnPickup();
    }

    private string GetMatchingName(ItemRegistry registry)
    {
        Transform check = transform;
        while (check != null)
        {
            if (registry.GetByPickupName(check.name) != null)
                return check.name;
            check = check.parent;
        }
        return null;
    }
}