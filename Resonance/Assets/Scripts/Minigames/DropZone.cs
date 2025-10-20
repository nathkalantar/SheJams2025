using UnityEngine;
using UnityEngine.UI;

public class DropZone : MonoBehaviour
{
    [HideInInspector] public int zoneIndex;
    [HideInInspector] public bool hasItem = false;
    
    private Image image;
    private Color originalColor;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
            originalColor = image.color;
    }

    public bool AcceptItem(DraggableItem item)
    {
        if (hasItem) return false;
        
        bool isCorrect = item.correctDropZoneIndex == zoneIndex;
        if (isCorrect)
        {
            hasItem = true;
            if (image != null)
                image.color = Color.green;
            
            UIMinigameManager.Instance.CheckCompletion();
        }
        else
        {
            StartCoroutine(ShowIncorrectFeedback());
        }
        
        return isCorrect;
    }

    public void ResetZone()
    {
        hasItem = false;
        if (image != null)
            image.color = originalColor;
    }

    System.Collections.IEnumerator ShowIncorrectFeedback()
    {
        if (image != null)
        {
            image.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            image.color = originalColor;
        }
    }
}