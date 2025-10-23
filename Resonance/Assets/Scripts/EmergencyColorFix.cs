using UnityEngine;

/// <summary>
/// Script de emergencia para aplicar CommandBuffer a NPCs manualmente
/// Añadir a cualquier NPC que ya esté transformado pero no preserve color
/// </summary>
public class EmergencyColorFix : MonoBehaviour
{
    [Header("Emergency Fix")]
    [SerializeField] private KeyCode fixKey = KeyCode.F;
    [SerializeField] private bool applyOnStart = false;
    [SerializeField] private bool forceDefaultMaterial = true;
    
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError($"EmergencyColorFix: No SpriteRenderer found on {gameObject.name}!");
            return;
        }
        
        Debug.Log($"🆘 EmergencyColorFix ready on {gameObject.name}. Press {fixKey} to fix color.");
        
        if (applyOnStart)
        {
            ApplyFix();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(fixKey))
        {
            ApplyFix();
        }
    }
    
    public void ApplyFix()
    {
        if (spriteRenderer == null) return;
        
        Debug.Log($"🔧 EMERGENCY FIX for {gameObject.name}");
        Debug.Log($"Current material: {spriteRenderer.material?.name ?? "Default"}");
        
        if (forceDefaultMaterial)
        {
            // Forzar material por defecto
            spriteRenderer.material = null;
            Debug.Log("✅ Applied DEFAULT material (null)");
        }
        
        // Registrar para CommandBuffer
        ColorPreservationRenderer.RegisterRenderer(spriteRenderer);
        Debug.Log("✅ Registered with CommandBuffer");
        
        // Verificar estado
        ColorPreservationRenderer.ShowStatus();
    }
    
    void OnGUI()
    {
        if (spriteRenderer != null)
        {
            GUILayout.BeginArea(new Rect(10, 150, 400, 80));
            GUILayout.Label($"Emergency Fix: {gameObject.name}");
            GUILayout.Label($"Material: {spriteRenderer.material?.name ?? "Default"}");
            
            if (GUILayout.Button($"Fix Color (Press {fixKey})"))
            {
                ApplyFix();
            }
            
            GUILayout.EndArea();
        }
    }
}