using UnityEngine;
using UnityEngine.UI;

public class DropZone : MonoBehaviour
{
    [HideInInspector] public int zoneIndex;
    [HideInInspector] public bool hasItem = false;
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color debugColor = Color.red;
    
    public bool AcceptItem(DraggableItem item)
    {
        if (hasItem) return false;
        
        bool isCorrect = item.correctDropZoneIndex == zoneIndex;
        if (isCorrect)
        {
            hasItem = true;
            UIMinigameManager.Instance.CheckCompletion();
        }
        
        return isCorrect;
    }

    public void ResetZone()
    {
        hasItem = false;
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
        
        Gizmos.color = new Color(debugColor.r, debugColor.g, debugColor.b, 0.2f);
        Gizmos.DrawCube(worldPos, scaledSize);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(worldPos + Vector3.up * (scaledSize.y * 0.6f), $"Zone {zoneIndex}");
#endif
    }
}