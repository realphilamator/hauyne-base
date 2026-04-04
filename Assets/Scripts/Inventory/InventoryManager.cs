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
///   3. Set slotCount (3 default, 9 max).
///   4. Assign the three slot prefabs (start, middle, end) and the Slots container.
///      Place 3 prefab instances inside Slots at edit time for a correct edit-time preview.
///      BuildSlotUI() always rebuilds from scratch at runtime to ensure correct slot order.
///
/// Slot hotkeys are handled here directly (Alpha1-Alpha9) since they are
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

    // -------------------------------------------------------------------------
    // UI
    // -------------------------------------------------------------------------

    [BoxGroup("UI")]
    [Required]
    [Tooltip("Prefab for the leftmost slot.")]
    public GameObject startPrefab;

    [BoxGroup("UI")]
    [Required]
    [Tooltip("Prefab repeated for every middle slot.")]
    public GameObject middlePrefab;

    [BoxGroup("UI")]
    [Required]
    [Tooltip("Prefab for the rightmost slot.")]
    public GameObject endPrefab;

    [BoxGroup("UI")]
    [Required]
    [Tooltip("Parent RectTransform that slot GameObjects are spawned into. " +
             "Use a Horizontal Layout Group for automatic spacing.")]
    public RectTransform inventoryTransform;

    [BoxGroup("UI")]
    [Tooltip("Tint applied to SlotImg on all unselected slots.")]
    public Color inventoryColor = Color.white;

    [BoxGroup("UI")]
    [Tooltip("Tint applied to SlotImg on the selected slot.")]
    public Color selectionColor = Color.red;

    [BoxGroup("UI")]
    [Tooltip("Text element that displays the selected item's name.")]
    public TMP_Text inventoryText;

    // -------------------------------------------------------------------------
    // Settings
    // -------------------------------------------------------------------------

    [BoxGroup("Settings")]
    [Range(3, 9)]
    [Tooltip("Number of active inventory slots. Default is 3, maximum is 9.")]
    public int slotCount = 3;

    [BoxGroup("Settings")]
    [Tooltip("Sprite shown in a slot when it is empty.")]
    public Sprite nothingSprite;

    // -------------------------------------------------------------------------
    // Runtime State
    // -------------------------------------------------------------------------

    [BoxGroup("Debug")]
    [ReadOnly]
    [Tooltip("The currently selected slot index.")]
    public int selectedSlot = 0;

    [BoxGroup("Debug")]
    [ReadOnly]
    [Tooltip("The item currently in each slot. Null means the slot is empty.")]
    public BaseItem[] items;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    /// <summary>SlotImg Image components, used for selection highlight.</summary>
    private Image[] _slotBackgrounds;

    /// <summary>ItemImg Image components, used for item icon display.</summary>
    private Image[] _slotIcons;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    protected override void Awake()
    {
        base.Awake();
        items = new BaseItem[slotCount];
    }

    private void Start()
    {
        BuildSlotUI();
        RefreshAllSlotUI();
        UpdateItemNameUI();
    }

    private void Update()
    {
        HandleScrollWheel();
        HandleHotkeys();
    }

    // -------------------------------------------------------------------------
    // Slot UI Construction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Destroys all existing slot children and rebuilds from scratch to ensure
    /// start, middle, and end prefabs are always in the correct order.
    /// </summary>
    private void BuildSlotUI()
    {
        for (int i = inventoryTransform.childCount - 1; i >= 0; i--)
            Destroy(inventoryTransform.GetChild(i).gameObject);

        _slotBackgrounds = new Image[slotCount];
        _slotIcons = new Image[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(GetPrefabForSlot(i), inventoryTransform);
            slot.name = $"Slot_{i}";

            Image whiteBG = slot.transform.Find("WhiteBG")?.GetComponent<Image>();
            if (whiteBG != null)
                _slotBackgrounds[i] = whiteBG;

            Image itemImg = slot.transform.Find("ItemImg")?.GetComponent<Image>();
            if (itemImg != null)
                _slotIcons[i] = itemImg;
        }

        UpdateSelectionHighlight();
    }

    /// <summary>Returns the correct prefab for a slot at the given index.</summary>
    private GameObject GetPrefabForSlot(int index)
    {
        if (slotCount <= 1) return startPrefab;
        if (index == 0) return startPrefab;
        if (index == slotCount - 1) return endPrefab;
        return middlePrefab;
    }

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------

    private void HandleScrollWheel()
    {
        if (Time.timeScale == 0f) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SelectSlot((selectedSlot - 1 + slotCount) % slotCount);
        else if (scroll < 0f) SelectSlot((selectedSlot + 1) % slotCount);
    }

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
    /// Selects the given slot, updates the UI highlight and item name.
    /// </summary>
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;

        selectedSlot = index;
        UpdateItemNameUI();
        UpdateSelectionHighlight();
    }

    /// <summary>
    /// Adds an item to the first empty slot. If all slots are full,
    /// the item replaces the currently selected slot.
    /// Returns the slot index the item was placed in, or -1 on failure.
    /// </summary>
    public int CollectItem(BaseItem item)
    {
        if (item == null) return -1;

        int targetSlot = -1;
        for (int i = 0; i < slotCount; i++)
        {
            if (items[i] == null) { targetSlot = i; break; }
        }

        if (targetSlot == -1)
            targetSlot = selectedSlot;

        items[targetSlot] = item;
        RefreshSlotUI(targetSlot);
        UpdateItemNameUI();

        return targetSlot;
    }

    /// <summary>
    /// Uses the item in the currently selected slot.
    /// If the item is consumed it is removed from the slot.
    /// Does nothing if the selected slot is empty.
    /// </summary>
    public void UseSelectedItem()
    {
        if (items[selectedSlot] == null) return;
        itemUseHandler.Execute(items[selectedSlot], () => ClearSlot(selectedSlot));
    }

    /// <summary>Removes the item from a specific slot and updates the UI.</summary>
    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;

        items[index] = null;
        RefreshSlotUI(index);
        UpdateItemNameUI();
    }

    /// <summary>Clears all slots. Useful for game over or scene transitions.</summary>
    [Button("Clear All Slots")]
    public void ClearAllSlots()
    {
        for (int i = 0; i < slotCount; i++)
            ClearSlot(i);
    }

    // -------------------------------------------------------------------------
    // UI Helpers
    // -------------------------------------------------------------------------

    private void RefreshSlotUI(int index)
    {
        if (_slotIcons == null || index >= _slotIcons.Length) return;
        if (_slotIcons[index] == null) return;

        BaseItem current = items[index];
        _slotIcons[index].sprite = current != null ? current.icon : nothingSprite;
        _slotIcons[index].enabled = true;
    }

    private void RefreshAllSlotUI()
    {
        for (int i = 0; i < slotCount; i++)
            RefreshSlotUI(i);
    }

    private void UpdateItemNameUI()
    {
        if (inventoryText == null) return;
        inventoryText.text = items[selectedSlot] != null ? items[selectedSlot].itemName : "Nothing";
    }

    /// <summary>
    /// Tints SlotImg on the selected slot with selectionColor,
    /// and all others with inventoryColor.
    /// </summary>
    private void UpdateSelectionHighlight()
    {
        if (_slotBackgrounds == null) return;

        for (int i = 0; i < _slotBackgrounds.Length; i++)
        {
            if (_slotBackgrounds[i] == null) continue;
            _slotBackgrounds[i].color = (i == selectedSlot) ? selectionColor : inventoryColor;
        }
    }
}