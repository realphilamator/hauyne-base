using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

/// <summary>
/// Manages the player's item inventory. Handles slot selection, item collection,
/// item use, and inventory UI updates.
///
/// Setup:
///   1. Attach this to your HUD or Player GameObject.
///   2. Assign all Inspector references.
///   3. Set slotCount to the number of slots you want (3 default, 9 max).
///   4. Make sure your slot UI RawImages array matches slotCount.
///
/// Slot hotkeys are handled here directly (Alpha1–Alpha9) since they are
/// fixed UI shortcuts, not rebindable gameplay actions.
/// </summary>
public class InventoryManager : Singleton<InventoryManager>
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------

    [BoxGroup("References")]
    [Required]
    [Tooltip("The ItemUseHandler component responsible for executing item logic.")]
    public ItemUseHandler itemUseHandler;

    [BoxGroup("References")]
    [Required]
    [Tooltip("The ItemRegistry asset containing all item definitions.")]
    public ItemRegistry itemRegistry;

    [BoxGroup("UI")]
    [Required]
    [Tooltip("RawImage components for each inventory slot, left to right. " +
             "Array length must match Slot Count.")]
    public RawImage[] slotImages;

    [BoxGroup("UI")]
    [Tooltip("Texture displayed in empty slots.")]
    public Texture emptySlotTexture;

    [BoxGroup("UI")]
    [Tooltip("Text element that displays the selected item's name.")]
    public TMP_Text itemNameText;

    [BoxGroup("UI")]
    [Tooltip("The selection indicator RectTransform that slides between slots.")]
    public RectTransform itemSelectIndicator;

    [BoxGroup("UI")]
    [Tooltip("The anchored X positions the selection indicator lerps to, one per slot. " +
             "Array length must match Slot Count.")]
    public float[] slotIndicatorPositions;

    [BoxGroup("UI")]
    [Tooltip("The anchored Y position of the selection indicator (stays constant).")]
    public float slotIndicatorY;

    [BoxGroup("UI")]
    [Tooltip("How fast the selection indicator slides between slots.")]
    public float indicatorLerpSpeed = 10f;

    [BoxGroup("Settings")]
    [Range(3, 9)]
    [Tooltip("Number of active inventory slots. Default is 3, maximum is 9.")]
    public int slotCount = 3;

    // -------------------------------------------------------------------------
    // Runtime State (visible in Inspector for debugging, not editable)
    // -------------------------------------------------------------------------

    [BoxGroup("Debug")]
    [ReadOnly]
    [Tooltip("The currently selected slot index.")]
    public int selectedSlot = 0;

    [BoxGroup("Debug")]
    [ReadOnly]
    [Tooltip("The item currently in each slot. Null means the slot is empty.")]
    public BaseItem[] slots;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Coroutine _indicatorCoroutine;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    protected override void Awake()
    {
        base.Awake();
        slots = new BaseItem[slotCount];
    }

    private void Start()
    {
        RefreshAllSlotUI();
        UpdateItemNameUI();
        SnapIndicatorToSlot(selectedSlot);
    }

    private void Update()
    {
        HandleScrollWheel();
        HandleHotkeys();
    }

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------

    /// <summary>
    /// Cycles the selected slot with the scroll wheel.
    /// Scroll up = previous slot, scroll down = next slot.
    /// </summary>
    private void HandleScrollWheel()
    {
        if (Time.timeScale == 0f) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)      SelectSlot((selectedSlot - 1 + slotCount) % slotCount);
        else if (scroll < 0f) SelectSlot((selectedSlot + 1) % slotCount);
    }

    /// <summary>
    /// Allows direct slot selection via number keys (1 through 9).
    /// Only keys within the active slot count are recognised.
    /// </summary>
    private void HandleHotkeys()
    {
        if (Time.timeScale == 0f) return;

        for (int i = 0; i < slotCount && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectSlot(i);
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Selects the given slot, updates the UI indicator and item name.
    /// </summary>
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;

        selectedSlot = index;
        UpdateItemNameUI();
        MoveIndicatorToSlot(index);
    }

    /// <summary>
    /// Adds an item to the first empty slot. If all slots are full,
    /// the item replaces the currently selected slot.
    /// Returns the slot index the item was placed in.
    /// </summary>
    public int CollectItem(BaseItem item)
    {
        if (item == null) return -1;

        // Find the first empty slot
        int targetSlot = -1;
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null) { targetSlot = i; break; }
        }

        // No empty slot — overwrite the selected slot
        if (targetSlot == -1)
            targetSlot = selectedSlot;

        slots[targetSlot] = item;
        RefreshSlotUI(targetSlot);
        UpdateItemNameUI();

        return targetSlot;
    }

    /// <summary>
    /// Uses the item in the currently selected slot.
    /// If the item is successfully consumed it is removed from the slot.
    /// Does nothing if the selected slot is empty.
    /// </summary>
    public void UseSelectedItem()
    {
        if (slots[selectedSlot] == null) return;

        itemUseHandler.Execute(slots[selectedSlot], () => ClearSlot(selectedSlot));
    }

    /// <summary>
    /// Removes the item from a specific slot and updates the UI.
    /// </summary>
    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;

        slots[index] = null;
        RefreshSlotUI(index);
        UpdateItemNameUI();
    }

    /// <summary>
    /// Clears all slots. Useful for game over or scene transitions.
    /// </summary>
    [Button("Clear All Slots")]
    public void ClearAllSlots()
    {
        for (int i = 0; i < slotCount; i++)
            ClearSlot(i);
    }

    // -------------------------------------------------------------------------
    // UI
    // -------------------------------------------------------------------------

    /// <summary>Updates the icon for a single slot.</summary>
    private void RefreshSlotUI(int index)
    {
        if (index >= slotImages.Length) return;

        slotImages[index].texture = slots[index] != null
            ? slots[index].icon
            : emptySlotTexture;
    }

    /// <summary>Refreshes all slot icons at once. Call after loading a save or resetting inventory.</summary>
    private void RefreshAllSlotUI()
    {
        for (int i = 0; i < slotCount; i++)
            RefreshSlotUI(i);
    }

    /// <summary>Updates the item name text to reflect the currently selected slot.</summary>
    private void UpdateItemNameUI()
    {
        if (itemNameText == null) return;

        itemNameText.text = slots[selectedSlot] != null
            ? slots[selectedSlot].itemName
            : "Nothing";
    }

    /// <summary>Starts a coroutine to smoothly slide the indicator to a slot's position.</summary>
    private void MoveIndicatorToSlot(int index)
    {
        if (itemSelectIndicator == null || index >= slotIndicatorPositions.Length) return;

        if (_indicatorCoroutine != null)
            StopCoroutine(_indicatorCoroutine);

        _indicatorCoroutine = StartCoroutine(LerpIndicator(slotIndicatorPositions[index]));
    }

    /// <summary>Instantly snaps the indicator to a slot's position. Used on startup.</summary>
    private void SnapIndicatorToSlot(int index)
    {
        if (itemSelectIndicator == null || index >= slotIndicatorPositions.Length) return;

        itemSelectIndicator.anchoredPosition = new Vector2(slotIndicatorPositions[index], slotIndicatorY);
    }

    private System.Collections.IEnumerator LerpIndicator(float targetX)
    {
        Vector2 target = new Vector2(targetX, slotIndicatorY);

        while (Vector2.Distance(itemSelectIndicator.anchoredPosition, target) > 0.1f)
        {
            itemSelectIndicator.anchoredPosition = Vector2.Lerp(
                itemSelectIndicator.anchoredPosition,
                target,
                Time.deltaTime * indicatorLerpSpeed
            );
            yield return null;
        }

        // Snap to final position to avoid floating point drift
        itemSelectIndicator.anchoredPosition = target;
        _indicatorCoroutine = null;
    }
}
