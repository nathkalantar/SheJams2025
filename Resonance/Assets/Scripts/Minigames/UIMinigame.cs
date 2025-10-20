using UnityEngine;
using UnityEngine.UI;

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
            itemObj.GetComponent<Image>().sprite = currentData.items[i].sprite;
            itemObj.GetComponent<RectTransform>().anchoredPosition = currentData.items[i].startPosition;
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
            if (currentData.dropZones[i].backgroundSprite)
                img.sprite = currentData.dropZones[i].backgroundSprite;
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
        minigamePanel.SetActive(false);
        Time.timeScale = 1f;
        MinigameEventSystem.CloseMinigame();
    }

    public void ResetMinigame()
    {
        foreach (DraggableItem item in items)
            item.ResetItem();
        foreach (DropZone zone in dropZones)
            zone.ResetZone();
    }
}
