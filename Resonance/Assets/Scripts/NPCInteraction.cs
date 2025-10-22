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
        
        // 2. Hacer aparecer y crecer la ColorSphere ANTES de la transformación
        if (colorSphereComponent != null)
        {
            Debug.Log("Starting ColorSphere growth before transformation...");
            StartCoroutine(GrowColorSphere());
            yield return new WaitForSeconds(1f); // Dar tiempo para que empiece a crecer
        }
        
        // 3. Activar efecto de transformación visual (partículas, etc.)
        GameObject activeTransformationEffect = null;
        if (transformationEffect != null)
        {
            activeTransformationEffect = Instantiate(transformationEffect, transform.position, transform.rotation, transform);
            Debug.Log($"Transformation effect spawned at position: {transform.position}");
            
            // Las partículas ahora están configuradas para durar exactamente 2 segundos
            // No necesitamos fade manual - se configuran en el ParticleSystem con:
            // - Duration: 2.0
            // - Start Lifetime: 1.5-2.0  
            // - Looping: Desactivado
            // - Color over Lifetime: Gradiente de opaco a transparente (opcional)
        }
        else
        {
            Debug.LogWarning("No transformation effect prefab assigned!");
        }
        
        // 4. Iniciar shader de transformación mágica
        if (npcRenderer != null && transformationMaterial != null)
        {
            // Crear una instancia del material para este NPC
            Material instanceMaterial = new Material(transformationMaterial);
            
            // Asignar el sprite original del NPC al material
            SpriteRenderer spriteRenderer = npcRenderer as SpriteRenderer;
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                instanceMaterial.SetTexture("_MainTex", spriteRenderer.sprite.texture);
                Debug.Log("Sprite texture assigned to material");
            }
            
            // Configurar colores personalizados para el shader
            if (instanceMaterial.HasProperty("_MagicColor"))
            {
                instanceMaterial.SetColor("_MagicColor", magicColor);
                Debug.Log($"Magic color set: {magicColor}");
            }
            
            if (instanceMaterial.HasProperty("_TransformColor"))
            {
                instanceMaterial.SetColor("_TransformColor", transformToColor);
                Debug.Log($"Transform color set: {transformToColor}");
            }
            
            // Configurar la información de la esfera para el shader
            if (colorSphereComponent != null)
            {
                Vector3 sphereWorldPos = colorSphereComponent.transform.position;
                float sphereRadius = colorSphereComponent.GetCurrentRadius();
                
                if (instanceMaterial.HasProperty("_SphereCenter"))
                {
                    instanceMaterial.SetVector("_SphereCenter", new Vector4(sphereWorldPos.x, sphereWorldPos.y, sphereWorldPos.z, 0));
                    Debug.Log($"Sphere center set: {sphereWorldPos}");
                }
                
                if (instanceMaterial.HasProperty("_SphereRadius"))
                {
                    instanceMaterial.SetFloat("_SphereRadius", sphereRadius);
                    Debug.Log($"Sphere radius set: {sphereRadius}");
                }
                
                if (instanceMaterial.HasProperty("_SphereFadeEdge"))
                {
                    instanceMaterial.SetFloat("_SphereFadeEdge", 0.5f); // Fade suave en los bordes
                }
            }
            else
            {
                Debug.LogWarning("ColorSphere component not found - transformation will be visible everywhere");
            }
            
            npcRenderer.material = instanceMaterial;
            Debug.Log("Material instance applied to NPC renderer");
            
            // Empezar el fade-out de partículas AHORA para que terminen con la transformación
            // NOTA: Como las partículas ahora duran exactamente 2 segundos desde el ParticleSystem,
            // no necesitamos hacer fade manual. Se destruirán automáticamente.
            
            // Animar la transformación (con actualización dinámica del radio de la esfera)
            yield return StartCoroutine(AnimateTransformation(instanceMaterial));
        }
        else
        {
            Debug.LogWarning($"Missing components - npcRenderer: {npcRenderer}, transformationMaterial: {transformationMaterial}");
            // Si no hay shader, solo esperamos la duración
            yield return new WaitForSeconds(transformationDuration);
        }
        
        // 5. Las partículas se destruyen automáticamente después de 2 segundos
        if (activeTransformationEffect != null)
        {
            // Esperar un poco para que las partículas terminen naturalmente
            yield return new WaitForSeconds(0.2f);
            
            // Verificar si el objeto todavía existe antes de destruirlo
            if (activeTransformationEffect != null)
            {
                Destroy(activeTransformationEffect);
            }
        }
        
        Debug.Log($"{npcName}: Transformation complete!");
        
        // 6. Continuar con el diálogo de éxito
        yield return StartCoroutine(ShowSuccessDialogue());
    }
    
    System.Collections.IEnumerator AnimateTransformation(Material material)
    {
        // Guardar material original ANTES de cambiar el material del renderer
        Material originalMaterial = null;
        SpriteRenderer spriteRenderer = npcRenderer as SpriteRenderer;
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial; // Usar sharedMaterial para obtener el original
        }
        
        Debug.Log($"Starting transformation animation. Original material: {originalMaterial?.name}");
        
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
                Debug.Log($"Transformation progress: {progress:F2}");
            }
            else
            {
                Debug.LogWarning($"Material does not have property: {transformationShaderProperty}");
            }
            
            // Actualizar el radio de la esfera dinámicamente
            if (colorSphereComponent != null && material.HasProperty("_SphereRadius"))
            {
                float currentRadius = colorSphereComponent.GetCurrentRadius();
                material.SetFloat("_SphereRadius", currentRadius);
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
            
            // Continuar actualizando el radio de la esfera
            if (colorSphereComponent != null && material.HasProperty("_SphereRadius"))
            {
                float currentRadius = colorSphereComponent.GetCurrentRadius();
                material.SetFloat("_SphereRadius", currentRadius);
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
        if (originalMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
            Debug.Log($"Original material restored: {originalMaterial.name}");
        }
        else
        {
            Debug.LogWarning("Could not restore original material - using default sprite material");
            if (spriteRenderer != null)
            {
                spriteRenderer.material = null; // Esto usa el material por defecto de sprites
            }
        }
        
        // Limpiar la instancia del material
        if (material != null && material != originalMaterial)
        {
            DestroyImmediate(material);
            Debug.Log("Transformation material instance destroyed");
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

        // La ColorSphere ya fue expandida antes de la transformación
        // No necesitamos hacer nada más aquí
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
    
    // Función para hacer aparecer las partículas gradualmente
    System.Collections.IEnumerator FadeInParticles(GameObject particleObject)
    {
        if (particleObject == null) yield break;
        
        // Obtener todos los sistemas de partículas
        ParticleSystem[] particleSystems = particleObject.GetComponentsInChildren<ParticleSystem>();
        
        if (particleSystems.Length == 0)
        {
            Debug.LogWarning("No particle systems found in transformation effect!");
            yield break;
        }
        
        Debug.Log($"Found {particleSystems.Length} particle systems to fade in");
        
        // Guardar las configuraciones originales
        float[] originalEmissionRate = new float[particleSystems.Length];
        
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var emission = particleSystems[i].emission;
            originalEmissionRate[i] = emission.rateOverTime.constant;
            
            // Empezar con emisión en 0
            emission.rateOverTime = 0f;
            
            Debug.Log($"Particle system {i}: {particleSystems[i].name} - Original emission: {originalEmissionRate[i]}");
        }
        
        // Fade in gradual durante 1.2 segundos
        float fadeInDuration = 1.2f;
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var emission = particleSystems[i].emission;
                
                // Interpolar hacia los valores originales
                emission.rateOverTime = Mathf.Lerp(0f, originalEmissionRate[i], progress);
            }
            
            yield return null;
        }
        
        // Asegurar que lleguen a los valores originales
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var emission = particleSystems[i].emission;
            emission.rateOverTime = originalEmissionRate[i];
        }
        
        Debug.Log("Particle fade-in complete");
    }
    
    // Función para hacer desaparecer las partículas gradualmente
    System.Collections.IEnumerator FadeOutParticles(GameObject particleObject)
    {
        if (particleObject == null) yield break;
        
        // Obtener todos los sistemas de partículas
        ParticleSystem[] particleSystems = particleObject.GetComponentsInChildren<ParticleSystem>();
        
        if (particleSystems.Length == 0) yield break;
        
        Debug.Log($"Found {particleSystems.Length} particle systems to fade out");
        
        // Guardar las configuraciones actuales y módulos de color
        float[] currentEmissionRate = new float[particleSystems.Length];
        ParticleSystem.ColorOverLifetimeModule[] colorModules = new ParticleSystem.ColorOverLifetimeModule[particleSystems.Length];
        bool[] hadColorOverLifetime = new bool[particleSystems.Length];
        
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var emission = particleSystems[i].emission;
            currentEmissionRate[i] = emission.rateOverTime.constant;
            
            // Configurar el módulo de color over lifetime para controlar alpha
            colorModules[i] = particleSystems[i].colorOverLifetime;
            hadColorOverLifetime[i] = colorModules[i].enabled;
            colorModules[i].enabled = true;
            
            // Crear un gradiente que va de opaco a transparente MÁS RÁPIDO
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[4]; // Más puntos de control
            
            // Color: mantener el color original
            colorKeys[0].color = Color.white;
            colorKeys[0].time = 0.0f;
            colorKeys[1].color = Color.white;
            colorKeys[1].time = 1.0f;
            
            // Alpha: fade más agresivo y rápido
            alphaKeys[0].alpha = 1.0f; // Completamente opaco al inicio
            alphaKeys[0].time = 0.0f;
            alphaKeys[1].alpha = 0.8f; // Mantener opacidad al 50% del lifetime
            alphaKeys[1].time = 0.5f;
            alphaKeys[2].alpha = 0.2f; // Fade rápido al 75%
            alphaKeys[2].time = 0.75f;
            alphaKeys[3].alpha = 0.0f; // Completamente transparente al final
            alphaKeys[3].time = 1.0f;
            
            gradient.SetKeys(colorKeys, alphaKeys);
            colorModules[i].color = gradient;
            
            Debug.Log($"Particle system {i}: {particleSystems[i].name} - Emission rate: {currentEmissionRate[i]}");
        }
        
        // Primero, detener la emisión de nuevas partículas gradualmente
        // Las partículas deben terminar exactamente cuando termine la transformación completa
        // Transformación total: 2s (ida) + 3s (pausa) + 1.5s (vuelta) = 6.5s
        float totalTransformationTime = 6.5f;
        float stopEmissionDuration = totalTransformationTime * 0.25f; // 25% del tiempo para parar emisión (1.6s)
        float fadeWaitTime = totalTransformationTime * 0.75f; // 75% restante para fade (4.9s)
        
        float elapsed = 0f;
        
        while (elapsed < stopEmissionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / stopEmissionDuration;
            float fadeValue = 1f - progress; // De 1 a 0
            
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var emission = particleSystems[i].emission;
                emission.rateOverTime = currentEmissionRate[i] * fadeValue;
            }
            
            yield return null;
        }
        
        // Detener completamente la emisión
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var emission = particleSystems[i].emission;
            emission.rateOverTime = 0f;
        }
        
        Debug.Log($"Emission stopped, waiting {fadeWaitTime:F1}s for particles to fade with transformation...");
        
        // Esperar el tiempo restante para sincronizar con el final de la transformación
        yield return new WaitForSeconds(fadeWaitTime);
        
        // Limpiar cualquier partícula restante al final
        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] != null)
            {
                particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        
        // Restaurar configuraciones originales si es necesario
        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (!hadColorOverLifetime[i])
            {
                colorModules[i].enabled = false;
            }
        }
        
        Debug.Log("Particle fade-out complete");
    }
}
