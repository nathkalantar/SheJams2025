using UnityEngine;

/// <summary>
/// Script de testing para probar la preservación de color con CommandBuffer
/// Añadir a cualquier GameObject con SpriteRenderer para testing manual
/// </summary>
public class ColorPreservationTester : MonoBehaviour
{
    [Header("Testing Controls")]
    [SerializeField] private KeyCode testKey = KeyCode.T;
    [SerializeField] private bool preserveColorOnStart = false;
    [SerializeField] private bool showDebugInfo = true;
    
    private SpriteRenderer spriteRenderer;
    private bool isPreserving = false;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("ColorPreservationTester: No SpriteRenderer found!");
            return;
        }
        
        if (preserveColorOnStart)
        {
            RegisterForColorPreservation();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"ColorPreservationTester: Ready on {gameObject.name}. Press {testKey} to toggle.");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            ToggleColorPreservation();
        }
    }
    
    public void ToggleColorPreservation()
    {
        if (isPreserving)
        {
            UnregisterFromColorPreservation();
        }
        else
        {
            RegisterForColorPreservation();
        }
    }
    
    public void RegisterForColorPreservation()
    {
        if (spriteRenderer == null) return;
        
        ColorPreservationRenderer.RegisterRenderer(spriteRenderer);
        isPreserving = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"ColorPreservationTester: {gameObject.name} REGISTERED for color preservation");
        }
    }
    
    public void UnregisterFromColorPreservation()
    {
        if (spriteRenderer == null) return;
        
        ColorPreservationRenderer.UnregisterRenderer(spriteRenderer);
        isPreserving = false;
        
        if (showDebugInfo)
        {
            Debug.Log($"ColorPreservationTester: {gameObject.name} UNREGISTERED from color preservation");
        }
    }
    
    void OnDestroy()
    {
        if (isPreserving && spriteRenderer != null)
        {
            ColorPreservationRenderer.UnregisterRenderer(spriteRenderer);
        }
    }
    
    // Para mostrar estado en el Inspector
    void OnValidate()
    {
        if (Application.isPlaying && spriteRenderer != null)
        {
            if (preserveColorOnStart && !isPreserving)
            {
                RegisterForColorPreservation();
            }
            else if (!preserveColorOnStart && isPreserving)
            {
                UnregisterFromColorPreservation();
            }
        }
    }
}