using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIMinigameManager : MonoBehaviour
{
    public static UIMinigameManager Instance;
    
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private Transform dropZonesContainer;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject dropZonePrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private AudioSource audioSource;
    
    private MinigameData currentData;
    private DraggableItem[] items;
    private DropZone[] dropZones;
    
    void Awake()
    {
        Instance = this;
        minigamePanel.SetActive(false);
        closeButton.onClick.AddListener(CloseMinigame);
    }

    void OnEnable()
    {
        MinigameEventSystem.OnMinigameStart += StartMinigame;
    }

    void OnDisable()
    {
        MinigameEventSystem.OnMinigameStart -= StartMinigame;
    }

    void StartMinigame(MinigameData data)
    {
        currentData = data;
        SetupMinigame();
        minigamePanel.SetActive(true);
        
        CanvasGroup panelCanvasGroup = minigamePanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = minigamePanel.AddComponent<CanvasGroup>();
            
        panelCanvasGroup.alpha = 0f;
        minigamePanel.transform.localScale = Vector3.one * 0.8f;
        
        panelCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        minigamePanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        
        Time.timeScale = 0f;
    }

    void SetupMinigame()
    {
        backgroundImage.sprite = currentData.backgroundSprite;
        backgroundImage.color = currentData.backgroundColor;
        
        ClearContainers();
        CreateItems();
        CreateDropZones();
    }

    void ClearContainers()
    {
        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);
        foreach (Transform child in dropZonesContainer)
            Destroy(child.gameObject);
    }

    void CreateItems()
    {
        items = new DraggableItem[currentData.items.Length];
        for (int i = 0; i < currentData.items.Length; i++)
        {
            GameObject itemObj = Instantiate(itemPrefab, itemsContainer);
            DraggableItem item = itemObj.GetComponent<DraggableItem>();
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            
            itemObj.GetComponent<Image>().sprite = currentData.items[i].sprite;
            itemRect.anchoredPosition = currentData.items[i].startPosition;
            itemRect.sizeDelta = currentData.items[i].size;
            
            item.correctDropZoneIndex = currentData.items[i].correctDropZoneIndex;
            item.originalPosition = currentData.items[i].startPosition;
            items[i] = item;
        }
    }

    void CreateDropZones()
    {
        dropZones = new DropZone[currentData.dropZones.Length];
        for (int i = 0; i < currentData.dropZones.Length; i++)
        {
            GameObject zoneObj = Instantiate(dropZonePrefab, dropZonesContainer);
            DropZone zone = zoneObj.GetComponent<DropZone>();
            RectTransform rt = zoneObj.GetComponent<RectTransform>();
            Image img = zoneObj.GetComponent<Image>();
            
            rt.anchoredPosition = currentData.dropZones[i].position;
            rt.sizeDelta = currentData.dropZones[i].size;
            img.color = Color.clear;
            zone.zoneIndex = i;
            dropZones[i] = zone;
        }
    }

    public void CheckCompletion()
    {
        bool allPlaced = true;
        foreach (DropZone zone in dropZones)
        {
            if (!zone.hasItem)
            {
                allPlaced = false;
                break;
            }
        }

        if (allPlaced)
        {
            if (audioSource && currentData.successSound)
                audioSource.PlayOneShot(currentData.successSound);
            
            MinigameEventSystem.CompleteMinigame(true);
            Invoke(nameof(CloseMinigame), 1f);
        }
    }

    void CloseMinigame()
    {
        CanvasGroup panelCanvasGroup = minigamePanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = minigamePanel.AddComponent<CanvasGroup>();
            
        panelCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        minigamePanel.transform.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true)
            .OnComplete(() => {
                minigamePanel.SetActive(false);
                Time.timeScale = 1f;
                MinigameEventSystem.CloseMinigame();
            });
    }

    public void ResetMinigame()
    {
        foreach (DraggableItem item in items)
            item.ResetItem();
        foreach (DropZone zone in dropZones)
            zone.ResetZone();
    }
}
