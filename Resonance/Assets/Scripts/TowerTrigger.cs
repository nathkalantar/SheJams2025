using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerTrigger : MonoBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private string nextSceneName = "VideoEnd";
    [SerializeField] private float waitTime = 2f;
    
    [Header("Audio/Visual Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip triggerSound;
    [SerializeField] private GameObject colorSpherePrefab;
    [SerializeField] private float targetSphereRadius = 100f;
    [SerializeField] private float sphereGrowSpeed = 50f;
    
    private bool isTriggered = false;
    private List<NPCInteraction> allNPCs = new List<NPCInteraction>();
    private GameObject spawnedColorSphere;
    private ColorSphere colorSphereComponent;

    private void Start()
    {
        // Encontrar todos los NPCs en la escena
        FindAllNPCs();
        
        // Spawnear el ColorSphere con radio 0
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
            Debug.LogWarning("ColorSphere prefab is not assigned!");
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
        if (other.CompareTag("Player") && !isTriggered)
        {
            if (AreAllNPCsFollowing())
            {
                isTriggered = true;
                StartCoroutine(ActivateTower());
            }
            else
            {
                Debug.Log("Not all NPCs are following yet!");
                ShowFeedback("Complete all tasks first!");
            }
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

    private IEnumerator ActivateTower()
    {
        Debug.Log("Tower activated! All NPCs are following the player.");
        
        // Efectos audio
        if (audioSource != null && triggerSound != null)
        {
            audioSource.PlayOneShot(triggerSound);
        }
        
        // Expandir la ColorSphere
        if (colorSphereComponent != null)
        {
            StartCoroutine(GrowColorSphere());
        }
        
        // Mostrar mensaje al jugador
        ShowFeedback("Tower activated! Proceeding to next area...");
        
        // Esperar el tiempo configurado
        yield return new WaitForSeconds(waitTime);
        
        // Cargar la siguiente escena
        LoadNextScene();
    }
    
    private IEnumerator GrowColorSphere()
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
        colorSphereComponent.minRadius = targetSphereRadius * 0.9f;
        colorSphereComponent.maxRadius = targetSphereRadius * 1.1f;
        
        Debug.Log("Tower ColorSphere reached target radius and animation activated!");
    }

    private void ShowFeedback(string message)
    {
        // Aquí puedes integrar con tu sistema de UI/diálogos
        Debug.Log(message);
        
        // Si tienes un sistema de diálogos, úsalo aquí:
        // if (DialogueUI.Instance != null)
        // {
        //     DialogueUI.Instance.ShowSimpleMessage(message);
        // }
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