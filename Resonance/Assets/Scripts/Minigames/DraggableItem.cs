using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int correctDropZoneIndex;
    [HideInInspector] public Vector2 originalPosition;
    [SerializeField] private float snapDistance = 100f;
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color debugColor = Color.blue;
    
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
        Debug.Log("Begin drag started");
        
        rectTransform.DOKill();
        rectTransform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutBack).SetUpdate(true);
        canvasGroup.DOFade(0.6f, 0.1f).SetUpdate(true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        Vector2 newPosition = rectTransform.anchoredPosition + (eventData.delta / canvas.scaleFactor);
        rectTransform.anchoredPosition = newPosition;
        Debug.Log($"Dragging to: {newPosition}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        Debug.Log($"Item dropped at anchored position: {rectTransform.anchoredPosition}, world position: {rectTransform.position}");
        
        DropZone closestZone = FindNearestValidDropZone();
        
        if (closestZone != null)
        {
            Debug.Log($"Found closest zone {closestZone.zoneIndex} at distance");
            
            Debug.Log($"Item expects zone {correctDropZoneIndex}, found zone {closestZone.zoneIndex}");
            
            if (closestZone.zoneIndex == correctDropZoneIndex)
            {
                Debug.Log("Correct zone! Snapping...");
                if (closestZone.AcceptItem(this))
                {
                    isPlaced = true;
                    Vector3 targetWorldPos = closestZone.GetComponent<RectTransform>().position;
                    
                    rectTransform.DOMove(targetWorldPos, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
                    rectTransform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBounce).SetUpdate(true);
                    canvasGroup.DOFade(1f, 0.1f).SetUpdate(true);
                }
            }
            else
            {
                Debug.Log($"Wrong zone! Item wants {correctDropZoneIndex} but closest is {closestZone.zoneIndex}");
                
                rectTransform.DOShakePosition(0.3f, 10f, 20, 90f).SetUpdate(true);
                rectTransform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBounce).SetUpdate(true);
                canvasGroup.DOFade(1f, 0.1f).SetUpdate(true);
                
                rectTransform.DOAnchorPos(originalPosition, 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetDelay(0.3f);
            }
        }
        else
        {
            Debug.Log("No zone found, returning to start");
            rectTransform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBounce).SetUpdate(true);
            canvasGroup.DOFade(1f, 0.1f).SetUpdate(true);
            rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    DropZone FindNearestValidDropZone()
    {
        DropZone[] allZones = FindObjectsOfType<DropZone>();
        Debug.Log($"Found {allZones.Length} zones total");
        
        DropZone nearestZone = null;
        float minDistance = float.MaxValue;
        
        Vector3 itemWorldPos = rectTransform.position;
        Debug.Log($"Item world position: {itemWorldPos}");

        foreach (DropZone zone in allZones)
        {
            if (zone.hasItem) continue;

            Vector3 zoneWorldPos = zone.GetComponent<RectTransform>().position;
            float distance = Vector2.Distance(itemWorldPos, zoneWorldPos);
            
            Debug.Log($"Zone {zone.zoneIndex}: world position {zoneWorldPos}, distance {distance:F1}, snapDistance {snapDistance}");

            if (distance < snapDistance && distance < minDistance)
            {
                minDistance = distance;
                nearestZone = zone;
            }
        }

        Debug.Log($"Closest zone: {(nearestZone != null ? nearestZone.zoneIndex.ToString() : "none")}, distance: {minDistance:F1}");
        return nearestZone;
    }

    public void ResetItem()
    {
        isPlaced = false;
        rectTransform.DOKill();
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        Gizmos.color = debugColor;
        
        Vector3 worldPos = rectTransform.position;
        Vector2 size = rectTransform.sizeDelta;
        Vector3 scale = rectTransform.lossyScale;
        
        Vector3 scaledSize = new Vector3(size.x * scale.x, size.y * scale.y, 1f);
        
        Gizmos.DrawWireCube(worldPos, scaledSize);
        
        Gizmos.color = new Color(debugColor.r, debugColor.g, debugColor.b, 0.1f);
        Gizmos.DrawCube(worldPos, scaledSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(worldPos, snapDistance * scale.x);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(worldPos + Vector3.up * (scaledSize.y * 0.6f), $"Item→{correctDropZoneIndex}");
#endif
    }
}