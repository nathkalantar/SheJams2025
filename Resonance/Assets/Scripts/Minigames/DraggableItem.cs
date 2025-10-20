using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int correctDropZoneIndex;
    [HideInInspector] public Vector2 originalPosition;
    
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isPlaced = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        DropZone dropZone = eventData.pointerEnter?.GetComponent<DropZone>();
        if (dropZone != null && dropZone.AcceptItem(this))
        {
            isPlaced = true;
            rectTransform.anchoredPosition = dropZone.GetComponent<RectTransform>().anchoredPosition;
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    public void ResetItem()
    {
        isPlaced = false;
        rectTransform.anchoredPosition = originalPosition;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}