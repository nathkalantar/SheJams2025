using UnityEngine;
using System.Collections;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private MinigameData minigameData;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private Sprite npcPortrait;
    [SerializeField] private string successDialogue = "¡Gracias! Te seguiré.";
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float followDistance = 2f;
    
    [Header("Color Sphere")]
    [SerializeField] private GameObject colorSpherePrefab;
    [SerializeField] private float targetSphereRadius = 4f;
    [SerializeField] private float sphereGrowSpeed = 2f;
    
    private Transform player;
    private GameObject spawnedColorSphere;
    private ColorSphere colorSphereComponent;
    private bool playerInRange = false;
    private bool isMinigameCompleted = false;
    private bool isFollowing = false;
    private bool showingDialogue = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("Player found: " + playerObj.name);
        }
        else
        {
            Debug.LogWarning("No GameObject with 'Player' tag found!");
        }
        
        // Spawnear el prefab de ColorSphere con radio 0 (sin parent para que se quede fijo)
        if (colorSpherePrefab != null)
        {
            spawnedColorSphere = Instantiate(colorSpherePrefab, transform.position, Quaternion.identity);
            colorSphereComponent = spawnedColorSphere.GetComponent<ColorSphere>();
            
            if (colorSphereComponent != null)
            {
                colorSphereComponent.radius = 0f;
                colorSphereComponent.SetRadius(0f);
                colorSphereComponent.animateRadius = false;
            }
            else
            {
                Debug.LogWarning("ColorSphere component not found on prefab!");
            }
        }
        else
        {
            Debug.LogWarning("ColorSpherePrefab is not assigned!");
        }
        
        MinigameEventSystem.OnMinigameComplete += OnMinigameCompleted;
    }
    
    void OnDestroy()
    {
        MinigameEventSystem.OnMinigameComplete -= OnMinigameCompleted;
    }

    void Update()
    {
        if (isFollowing)
        {
            FollowPlayer();
        }
        else if (!showingDialogue)
        {
            CheckPlayerDistance();
            
            if (playerInRange && Input.GetKeyDown(interactionKey) && !isMinigameCompleted)
            {
                Debug.Log("E pressed! Starting minigame...");
                StartMinigame();
            }
        }
        
        if (showingDialogue && Input.GetKeyDown(interactionKey))
        {
            HideDialogue();
        }
    }

    void CheckPlayerDistance()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;
            
            if (playerInRange != wasInRange)
            {
                Debug.Log($"Player in range: {playerInRange}, Distance: {distance:F2}");
            }
        }
    }

    void StartMinigame()
    {
        if (minigameData != null)
        {
            Debug.Log("Starting minigame: " + minigameData.minigameName);
            MinigameEventSystem.StartMinigame(minigameData);
        }
        else
        {
            Debug.LogWarning("MinigameData is null!");
        }
    }

    void OnMinigameCompleted(MinigameData completedData, bool success)
    {
        if (success && completedData == minigameData)
        {
            isMinigameCompleted = true;
            StartCoroutine(ShowSuccessDialogue());
        }
    }
    
    System.Collections.IEnumerator ShowSuccessDialogue()
    {
        yield return new WaitForSeconds(0.5f);
        showingDialogue = true;
        
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowDialogue(npcName, successDialogue, npcPortrait);
        }
        else
        {
            Debug.Log($"{npcName}: {successDialogue}");
            Debug.Log("Press E to continue...");
        }
        
        // Expandir la esfera y activar animación
        if (colorSphereComponent != null)
        {
            StartCoroutine(GrowColorSphere());
        }
    }
    
    void HideDialogue()
    {
        showingDialogue = false;
        isFollowing = true;
        
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideDialogue();
        }
        
        Debug.Log("NPC started following!");
    }
    
    System.Collections.IEnumerator GrowColorSphere()
    {
        if (colorSphereComponent == null) yield break;
        
        float currentRadius = 0f;
        
        while (currentRadius < targetSphereRadius)
        {
            currentRadius += sphereGrowSpeed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, targetSphereRadius);
            
            colorSphereComponent.SetRadius(currentRadius);
            
            yield return null;
        }
        
        // Activar animación cuando alcance el tamaño objetivo
        colorSphereComponent.animateRadius = true;
        colorSphereComponent.minRadius = targetSphereRadius * 0.8f;
        colorSphereComponent.maxRadius = targetSphereRadius * 1.2f;
        
        Debug.Log("ColorSphere reached target radius and animation activated!");
    }
    
    void FollowPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > followDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Vector3 targetPosition = player.position - direction * followDistance;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (isFollowing)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, followDistance);
        }
    }
}