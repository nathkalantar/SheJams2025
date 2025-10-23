using UnityEngine;

/// <summary>
/// Script de debug para mostrar el estado del ColorPreservationRenderer
/// Añadir a cualquier GameObject en la escena
/// </summary>
public class ColorPreservationDebugger : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private KeyCode statusKey = KeyCode.I; // Info
    [SerializeField] private KeyCode clearAllKey = KeyCode.C; // Clear
    [SerializeField] private bool showStatusOnStart = true;
    
    void Start()
    {
        if (showStatusOnStart)
        {
            Invoke(nameof(ShowStatus), 1f); // Delay para que todo esté inicializado
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(statusKey))
        {
            ShowStatus();
        }
        
        if (Input.GetKeyDown(clearAllKey))
        {
            ClearAllRegistrations();
        }
    }
    
    public void ShowStatus()
    {
        Debug.Log("=== COLOR PRESERVATION DEBUG ===");
        ColorPreservationRenderer.ShowStatus();
        
        // También mostrar información de la cámara principal
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            ColorPreservationRenderer renderer = mainCam.GetComponent<ColorPreservationRenderer>();
            if (renderer != null)
            {
                Debug.Log("✅ ColorPreservationRenderer encontrado en Main Camera");
            }
            else
            {
                Debug.LogWarning("❌ ColorPreservationRenderer NO encontrado en Main Camera");
            }
        }
        else
        {
            Debug.LogError("❌ Main Camera no encontrada");
        }
        
        Debug.Log("=== END DEBUG ===");
    }
    
    public void ClearAllRegistrations()
    {
        Debug.Log("Clearing all color preservation registrations...");
        
        // Encontrar todos los NPCs transformados
        NPCInteraction[] npcs = FindObjectsOfType<NPCInteraction>();
        foreach (var npc in npcs)
        {
            if (npc.IsCompleted)
            {
                // Forzar desregistro
                var renderer = npc.GetComponent<Renderer>();
                if (renderer != null)
                {
                    ColorPreservationRenderer.UnregisterRenderer(renderer);
                    Debug.Log($"Unregistered {npc.name}");
                }
            }
        }
        
        ShowStatus();
    }
    
    void OnGUI()
    {
        // Mostrar controles en pantalla
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label("Color Preservation Debug:");
        GUILayout.Label($"Press {statusKey} - Show Status");
        GUILayout.Label($"Press {clearAllKey} - Clear All");
        
        if (GUILayout.Button("Show Status"))
        {
            ShowStatus();
        }
        
        GUILayout.EndArea();
    }
}