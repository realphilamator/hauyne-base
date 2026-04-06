using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

public class InventoryManager : Singleton<InventoryManager>
{
    [BoxGroup("References")]
    public ItemUseHandler itemUseHandler;

    [BoxGroup("References")]
    public ItemRegistry itemRegistry;

    [BoxGroup("UI")]
    public GameObject startPrefab;

    [BoxGroup("UI")]
    public GameObject middlePrefab;

    [BoxGroup("UI")]
    public GameObject endPrefab;

    [BoxGroup("UI")]
    public RectTransform inventoryTransform;

    [BoxGroup("UI")]
    public Color inventoryColor = Color.white;

    [BoxGroup("UI")]
    public Color selectionColor = Color.red;

    [BoxGroup("UI")]
    public TMP_Text inventoryText;

    [BoxGroup("Settings")]
    [Range(3, 9)]
    public int slotCount = 3;

    [BoxGroup("Settings")]
    public Sprite nothingSprite;

    [BoxGroup("Debug")]
    [ReadOnly] public int selectedSlot = 0;

    [BoxGroup("Debug")]
    [ReadOnly] public BaseItem[] items;

    private Image[] _slotBackgrounds;
    private Image[] _slotIcons;

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

    public BaseItem GetSelectedItem()
    {
        if (selectedSlot < 0 || selectedSlot >= slotCount)
            return null;

        return items[selectedSlot];
    }

    public bool HeldItemIs<T>(out int slot) where T : BaseItem
    {
        slot = selectedSlot;

        BaseItem item = GetSelectedItem();
        if (item == null) return false;

        return item is T;
    }

    // =========================

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

    private GameObject GetPrefabForSlot(int index)
    {
        if (slotCount <= 1) return startPrefab;
        if (index == 0) return startPrefab;
        if (index == slotCount - 1) return endPrefab;
        return middlePrefab;
    }

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

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;

        selectedSlot = index;
        UpdateItemNameUI();
        UpdateSelectionHighlight();
    }

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

    public void UseSelectedItem()
    {
        if (items[selectedSlot] == null) return;
        itemUseHandler.Execute(items[selectedSlot], () => ClearSlot(selectedSlot));
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;

        items[index] = null;
        RefreshSlotUI(index);
        UpdateItemNameUI();
    }

    public void ClearAllSlots()
    {
        for (int i = 0; i < slotCount; i++)
            ClearSlot(i);
    }

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