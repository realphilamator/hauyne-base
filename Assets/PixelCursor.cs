using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Canvas))]
public class PixelCursor : MonoBehaviour
{
    [Header("Cursor Sprite")]
    public Image cursorImage;
    public Sprite cursorSprite;
    [Header("Size & Hot-spot")]
    public float cursorSizePx = 16f;
    public Vector2 hotspot = Vector2.zero;
    [Header("Reference Resolution")]
    public float referenceWidth = 480f;
    public float referenceHeight = 360f;
    private RectTransform _rt;
    private Canvas _canvas;
    private RectTransform _canvasRect;
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();
        _rt = cursorImage.GetComponent<RectTransform>();

        _rt.pivot = new Vector2(0f, 1f);
        _rt.anchorMin = new Vector2(0.5f, 0.5f);
        _rt.anchorMax = new Vector2(0.5f, 0.5f);
        _rt.sizeDelta = new Vector2(cursorSizePx, cursorSizePx);

        if (cursorSprite != null)
            cursorImage.sprite = cursorSprite;
        cursorImage.raycastTarget = false;
    }

    private void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (cursorImage.enabled) cursorImage.enabled = false;
            return;
        }
        if (!cursorImage.enabled) cursorImage.enabled = true;
        Cursor.visible = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            Input.mousePosition,
            null,
            out Vector2 localPoint
        );

        float snapX = Mathf.Round(localPoint.x) - hotspot.x;
        float snapY = Mathf.Round(localPoint.y) + hotspot.y;

        _rt.anchoredPosition = new Vector2(snapX, snapY);
    }
}