using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerTrigger : MonoBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private string nextSceneName = "VideoEnd";
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float desaturationFadeDuration = 2f; // Duración del fade del filtro
    
    [Header("Audio/Visual Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip triggerSound;
    
    private bool isTriggered = false;
    private bool hasShownWarning = false; // Para evitar múltiples warnings
    private bool hasActivatedSphere = false; // Para activar la esfera solo una vez
    private bool sphereInitializationComplete = false; // Para saber cuándo mostrar el popup
    private bool playerInTrigger = false; // Para saber si el jugador está en el trigger
    private List<NPCInteraction> allNPCs = new List<NPCInteraction>();
    private ScreenDesaturationEffect desaturationEffect;

    private void Start()
    {
        // Encontrar todos los NPCs en la escena
        FindAllNPCs();
        
        // Buscar el componente ScreenDesaturationEffect en la cámara
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            desaturationEffect = mainCamera.GetComponent<ScreenDesaturationEffect>();
            if (desaturationEffect == null)
            {
                Debug.LogWarning("ScreenDesaturationEffect not found on main camera!");
            }
        }
        else
        {
            Debug.LogWarning("Main camera not found!");
        }
    }

    private void FindAllNPCs()
    {
        NPCInteraction[] npcs = FindObjectsOfType<NPCInteraction>();
        allNPCs.AddRange(npcs);
        
        Debug.Log($"Found {allNPCs.Count} NPCs in the scene");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            
            // Primera vez que entra con todos los NPCs: activar la esfera
            if (AreAllNPCsFollowing() && !hasActivatedSphere)
            {
                hasActivatedSphere = true;
                StartCoroutine(ActivateSphereAndDesaturation());
            }
            
            // Si ya completó la inicialización de la esfera y no está triggeado, mostrar confirmación
            if (sphereInitializationComplete && !isTriggered)
            {
                ShowConfirmationDialog();
            }
            // Si no tiene todos los NPCs, mostrar warning
            else if (!AreAllNPCsFollowing() && !hasShownWarning)
            {
                ShowWarningMessage();
                hasShownWarning = true;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            // Reset warning flag cuando el jugador sale del trigger
            hasShownWarning = false;
        }
    }

    private bool AreAllNPCsFollowing()
    {
        if (allNPCs.Count == 0)
        {
            Debug.LogWarning("No NPCs found in the scene!");
            return false;
        }

        foreach (NPCInteraction npc in allNPCs)
        {
            if (npc == null) continue;
            
            // Verificar si el NPC está siguiendo al jugador
            // Necesitamos acceso a la variable isFollowing del NPC
            if (!IsNPCFollowing(npc))
            {
                return false;
            }
        }
        
        return true;
    }

    private bool IsNPCFollowing(NPCInteraction npc)
    {
        // Usar la propiedad pública IsFollowing
        return npc.IsFollowing;
    }

    private IEnumerator FadeOutDesaturation()
    {
        if (desaturationEffect == null) yield break;
        
        Debug.Log("Starting desaturation fade-out...");
        
        float elapsed = 0f;
        float startDesaturation = desaturationEffect.globalDesaturation;
        
        while (elapsed < desaturationFadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / desaturationFadeDuration;
            
            // Interpolar de la desaturación actual a 0 (color completo)
            float currentDesaturation = Mathf.Lerp(startDesaturation, 0f, progress);
            desaturationEffect.globalDesaturation = currentDesaturation;
            
            yield return null;
        }
        
        // Asegurar que llegue a 0 (color completo)
        desaturationEffect.globalDesaturation = 0f;
        
        Debug.Log("Desaturation fade-out complete! World restored to full color.");
    }

    private void ShowWarningMessage()
    {
        // Calcular cuántos NPCs faltan
        int followingCount = 0;
        foreach (var npc in allNPCs)
        {
            if (npc != null && IsNPCFollowing(npc))
                followingCount++;
        }
        
        int remaining = allNPCs.Count - followingCount;
        
        string message = remaining == 1 
            ? "Complete 1 more task before accessing the tower!" 
            : $"Complete {remaining} more tasks before accessing the tower!";
        
        // Mostrar mensaje usando TowerUI
        if (TowerUI.Instance != null)
        {
            TowerUI.Instance.ShowWarningMessage(message);
        }
        else
        {
            Debug.LogWarning("TowerUI instance not found! Make sure TowerUI is in the scene.");
            Debug.Log(message);
        }
    }
    
    private void ShowConfirmationDialog()
    {
        string message = "Ready to proceed to the final sequence?";
        
        if (TowerUI.Instance != null)
        {
            TowerUI.Instance.ShowConfirmationDialog(
                message,
                OnConfirmProceed,    // On confirm
                OnCancelProceed      // On cancel
            );
        }
        else
        {
            Debug.LogWarning("TowerUI instance not found! Proceeding automatically.");
            OnConfirmProceed();
        }
    }
    
    private void OnConfirmProceed()
    {
        if (!isTriggered)
        {
            isTriggered = true;
            StartCoroutine(HandleConfirmProceed());
        }
    }
    
    private IEnumerator HandleConfirmProceed()
    {
        // ===== ESPACIO PARA FUTURAS FUNCIONALIDADES =====
        // Aquí se pueden añadir efectos adicionales, animaciones, 
        // sonidos especiales, guardado de datos, etc.
        
        // Por ejemplo:
        // - Efectos visuales especiales
        // - Guardado del progreso
        // - Animaciones de transición
        // - Música/sonidos finales
        
        Debug.Log("Preparing final sequence...");
        
        // ===== FIN DEL ESPACIO PARA FUTURAS FUNCIONALIDADES =====
        
        // Esperar 1 segundo antes de cambiar a la escena del video
        yield return new WaitForSeconds(1f);
        
        // Proceder con el video final
        StartCoroutine(GoToFinalVideo());
    }
    
    private void OnCancelProceed()
    {
        Debug.Log("Player chose not to proceed to final sequence yet.");
        // El jugador puede salir y volver a entrar si cambia de opinión
    }
    
    // Nuevo método: activar esfera y quitar desaturación (primera vez)
    private IEnumerator ActivateSphereAndDesaturation()
    {
        Debug.Log("Tower accessed for the first time! Activating color restoration...");
        
        // Efectos audio
        if (audioSource != null && triggerSound != null)
        {
            audioSource.PlayOneShot(triggerSound);
        }
        
        // Quitar gradualmente el filtro de desaturación
        if (desaturationEffect != null)
        {
            yield return StartCoroutine(FadeOutDesaturation());
        }
        
        // Esperar 2 segundos a que la esfera se haya iniciado completamente
        Debug.Log("Waiting for sphere initialization...");
        yield return new WaitForSeconds(2f);
        
        // Marcar la inicialización como completa
        sphereInitializationComplete = true;
        
        Debug.Log("Color restoration complete! You can now proceed to the final sequence.");
        
        // Si el jugador todavía está en el trigger, mostrar el popup inmediatamente
        if (playerInTrigger && !isTriggered)
        {
            ShowConfirmationDialog();
        }
    }
    
    // Método separado: ir al video final
    private IEnumerator GoToFinalVideo()
    {
        Debug.Log("Proceeding to final video sequence...");
        
        // Mostrar mensaje al jugador
        ShowFeedback("Loading final sequence...");
        
        // Esperar un momento antes de cargar
        yield return new WaitForSeconds(waitTime);
        
        // Cargar la siguiente escena
        LoadNextScene();
    }
    
    private void ShowFeedback(string message)
    {
        // Método legacy - ahora usamos TowerUI
        Debug.Log(message);
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name is not set!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Mostrar el área del trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = isTriggered ? Color.green : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }

    // Método para verificar manualmente el estado (útil para debugging)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void CheckNPCStatus()
    {
        Debug.Log("=== NPC Status Check ===");
        foreach (var npc in allNPCs)
        {
            if (npc != null)
            {
                bool following = IsNPCFollowing(npc);
                Debug.Log($"{npc.name}: Following = {following}");
            }
        }
        Debug.Log($"All NPCs following: {AreAllNPCsFollowing()}");
    }
}