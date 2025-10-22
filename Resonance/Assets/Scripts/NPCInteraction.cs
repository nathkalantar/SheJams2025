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
    [SerializeField] private GameObject interactionIndicator;

    [Header("Color Sphere")]
    [SerializeField] private GameObject colorSpherePrefab;
    [SerializeField] private float targetSphereRadius = 4f;
    [SerializeField] private float sphereGrowSpeed = 2f;
    
    [Header("Transformation Effects")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private Material transformationMaterial; // Crear material con shader "Custom/MagicalTransformation"
    [SerializeField] private float transformationDuration = 2f;
    [SerializeField] private GameObject transformationEffect; // Opcional: prefab de partículas
    [SerializeField] private string transformationShaderProperty = "_TransformationProgress"; // NO CAMBIAR
    
    [Header("Transformation Colors")]
    [SerializeField] private Color magicColor = new Color(1f, 0.5f, 1f, 1f); // Color púrpura mágico
    [SerializeField] private Color transformToColor = Color.white; // Color final del NPC
    
    private Transform player;
    private GameObject spawnedColorSphere;
    private ColorSphere colorSphereComponent;
    private Renderer npcRenderer;
    private Material originalMaterial;
    private AudioSource audioSource; // Se crea automáticamente
    private bool playerInRange = false;
    private bool isMinigameCompleted = false;  // Keep private, but add public getter below
    private bool isFollowing = false;
    private bool showingDialogue = false;
    
    // Propiedad pública para acceder al estado de seguimiento
    public bool IsFollowing => isFollowing;

    // Public getter for external access (e.g., by LevelEndTrigger)
    public bool IsCompleted => isMinigameCompleted;

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

        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);
        
        // Obtener el renderer del NPC para la transformación
        npcRenderer = GetComponent<Renderer>();
        if (npcRenderer != null)
        {
            originalMaterial = npcRenderer.material;
        }
        
        // Configurar AudioSource si no existe
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
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
                UpdateInteractionIndicator();
            }
        }
    }

    void UpdateInteractionIndicator()
    {
        if (interactionIndicator != null && !isMinigameCompleted)
        {
            interactionIndicator.SetActive(playerInRange);
        }
        else if (interactionIndicator != null && isMinigameCompleted)
        {
            interactionIndicator.SetActive(false);
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

    public void OnMinigameCompleted(MinigameData completedData, bool success)
    {
        if (success && completedData == minigameData)
        {
            isMinigameCompleted = true;
            StartCoroutine(TransformationSequence());
        }
    }
    
    System.Collections.IEnumerator TransformationSequence()
    {
        Debug.Log($"{npcName}: Starting magical transformation!");
        
        // 1. Reproducir sonido de victoria
        if (audioSource != null && victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
            Debug.Log("Victory sound played!");
        }
        
        // 2. Activar efecto de transformación visual (partículas, etc.)
        GameObject activeTransformationEffect = null;
        if (transformationEffect != null)
        {
            activeTransformationEffect = Instantiate(transformationEffect, transform.position, transform.rotation, transform);
            Debug.Log("Transformation effect spawned!");
        }
        
        // 3. Iniciar shader de transformación mágica
        if (npcRenderer != null && transformationMaterial != null)
        {
            // Crear una instancia del material para este NPC
            Material instanceMaterial = new Material(transformationMaterial);
            
            // Asignar el sprite original del NPC al material
            SpriteRenderer spriteRenderer = npcRenderer as SpriteRenderer;
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                instanceMaterial.SetTexture("_MainTex", spriteRenderer.sprite.texture);
            }
            
            // Configurar colores personalizados
            if (instanceMaterial.HasProperty("_MagicColor"))
                instanceMaterial.SetColor("_MagicColor", magicColor);
            if (instanceMaterial.HasProperty("_TransformColor"))
                instanceMaterial.SetColor("_TransformColor", transformToColor);
            
            npcRenderer.material = instanceMaterial;
            
            // Animar la transformación
            yield return StartCoroutine(AnimateTransformation(instanceMaterial));
        }
        else
        {
            // Si no hay shader, solo esperamos la duración
            yield return new WaitForSeconds(transformationDuration);
        }
        
        // 4. Limpiar efecto de partículas
        if (activeTransformationEffect != null)
        {
            Destroy(activeTransformationEffect);
        }
        
        Debug.Log($"{npcName}: Transformation complete!");
        
        // 5. Continuar con el diálogo y la esfera
        yield return StartCoroutine(ShowSuccessDialogue());
    }
    
    System.Collections.IEnumerator AnimateTransformation(Material material)
    {
        Material originalMaterial = npcRenderer.material; // Guardar material original
        
        // Fase 1: Transformación hacia adelante (0 a 1)
        float elapsedTime = 0f;
        
        while (elapsedTime < transformationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transformationDuration;
            
            // Animar la propiedad del shader (0 a 1)
            if (material.HasProperty(transformationShaderProperty))
            {
                material.SetFloat(transformationShaderProperty, progress);
            }
            
            yield return null;
        }
        
        // Asegurar que llegue al final
        if (material.HasProperty(transformationShaderProperty))
        {
            material.SetFloat(transformationShaderProperty, 1f);
        }
        
        // Fase 2: Mantener la transformación por 3 segundos
        yield return new WaitForSeconds(3f);
        
        // Fase 3: Volver gradualmente a la normalidad (1.5 segundos)
        float returnDuration = 1.5f;
        elapsedTime = 0f;
        
        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = 1f - (elapsedTime / returnDuration); // De 1 a 0
            
            if (material.HasProperty(transformationShaderProperty))
            {
                material.SetFloat(transformationShaderProperty, progress);
            }
            
            yield return null;
        }
        
        // Fase 4: Asegurar que vuelva completamente a 0 y restaurar material original
        if (material.HasProperty(transformationShaderProperty))
        {
            material.SetFloat(transformationShaderProperty, 0f);
        }
        
        yield return new WaitForSeconds(0.2f); // Pequeña pausa
        
        // Restaurar el material original del sprite
        if (originalMaterial != material)
        {
            npcRenderer.material = originalMaterial;
        }
        
        // Limpiar la instancia del material
        if (material != null && material != originalMaterial)
        {
            DestroyImmediate(material);
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

    public void FollowPlayer()
    {
        if (player == null) return;

        float npcColliderRadius = GetColliderRadius();
        float playerColliderRadius = GetPlayerColliderRadius();
        float safeDistance = followDistance + npcColliderRadius + playerColliderRadius + 0.5f;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > safeDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Vector3 targetPosition = player.position - direction * safeDistance;

            float remainingDistance = Vector3.Distance(transform.position, targetPosition);
            float moveStep = followSpeed * Time.deltaTime;

            if (remainingDistance > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveStep);
            }
        }
    }

    float GetColliderRadius()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return Mathf.Max(col.bounds.size.x, col.bounds.size.z) * 0.5f;
        }
        return 0.5f;
    }

    float GetPlayerColliderRadius()
    {
        if (player == null) return 0.5f;

        Collider playerCol = player.GetComponent<Collider>();
        if (playerCol != null)
        {
            return Mathf.Max(playerCol.bounds.size.x, playerCol.bounds.size.z) * 0.5f;
        }
        return 0.5f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (isFollowing && player != null)
        {
            float npcColliderRadius = GetColliderRadius();
            float playerColliderRadius = GetPlayerColliderRadius();
            float safeDistance = followDistance + npcColliderRadius + playerColliderRadius + 0.5f;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, safeDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, npcColliderRadius);

            if (player != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(player.position, playerColliderRadius);
            }
        }
    }
    
    // Métodos públicos para debug
    public void ForceFollow()
    {
        isMinigameCompleted = true;
        isFollowing = true;
        
        // Activar esfera inmediatamente para debug
        if (colorSphereComponent != null)
        {
            StopAllCoroutines();
            StartCoroutine(GrowColorSphere());
        }
        
        Debug.Log($"{npcName}: Forced to follow player");
    }

    public void ResetState()
    {
        isMinigameCompleted = false;
        isFollowing = false;
        showingDialogue = false;
        
        // Resetear esfera
        if (colorSphereComponent != null)
        {
            StopAllCoroutines();
            colorSphereComponent.SetRadius(0f);
            colorSphereComponent.animateRadius = false;
        }
        
        // Resetear material original
        if (npcRenderer != null && originalMaterial != null)
        {
            npcRenderer.material = originalMaterial;
        }
        
        Debug.Log($"{npcName}: State reset");
    }
    
    public void TestTransformation()
    {
        if (!isMinigameCompleted)
        {
            StartCoroutine(TransformationSequence());
        }
    }
}
