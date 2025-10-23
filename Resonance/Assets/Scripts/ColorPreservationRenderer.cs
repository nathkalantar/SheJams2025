using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Sistema que renderiza sprites con colores preservados DESPUÉS del post-processing
/// usando CommandBuffer para forzar el render order
/// </summary>
[RequireComponent(typeof(Camera))]
public class ColorPreservationRenderer : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    private Camera cam;
    private CommandBuffer commandBuffer;
    private static readonly string COMMAND_BUFFER_NAME = "ColorPreservationRenderer";
    
    // Lista de renderers que deben preservar color
    private System.Collections.Generic.List<Renderer> colorPreservedRenderers = new System.Collections.Generic.List<Renderer>();
    
    void Start()
    {
        cam = GetComponent<Camera>();
        SetupCommandBuffer();
    }
    
    void SetupCommandBuffer()
    {
        // Crear command buffer
        commandBuffer = new CommandBuffer();
        commandBuffer.name = COMMAND_BUFFER_NAME;
        
        // Añadir al final del pipeline de render (después de post-processing)
        cam.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
        
        if (showDebugInfo)
        {
            Debug.Log("ColorPreservationRenderer: CommandBuffer configurado");
        }
    }
    
    /// <summary>
    /// Registra un renderer para preservar color
    /// </summary>
    public void RegisterColorPreservedRenderer(Renderer renderer)
    {
        if (renderer != null && !colorPreservedRenderers.Contains(renderer))
        {
            colorPreservedRenderers.Add(renderer);
            UpdateCommandBuffer();
            
            if (showDebugInfo)
            {
                Debug.Log($"ColorPreservationRenderer: Registrado {renderer.name}");
            }
        }
    }
    
    /// <summary>
    /// Desregistra un renderer
    /// </summary>
    public void UnregisterColorPreservedRenderer(Renderer renderer)
    {
        if (colorPreservedRenderers.Remove(renderer))
        {
            UpdateCommandBuffer();
            
            if (showDebugInfo)
            {
                Debug.Log($"ColorPreservationRenderer: Desregistrado {renderer.name}");
            }
        }
    }
    
    /// <summary>
    /// Actualiza el command buffer con los renderers actuales
    /// </summary>
    void UpdateCommandBuffer()
    {
        if (commandBuffer == null) return;
        
        // Limpiar command buffer
        commandBuffer.Clear();
        
        // Añadir cada renderer preservado
        foreach (var renderer in colorPreservedRenderers)
        {
            if (renderer != null && renderer.gameObject.activeInHierarchy)
            {
                // Renderizar con su material actual
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    commandBuffer.DrawRenderer(renderer, renderer.materials[i], i);
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"ColorPreservationRenderer: CommandBuffer actualizado con {colorPreservedRenderers.Count} renderers");
        }
    }
    
    void LateUpdate()
    {
        // Limpiar renderers nulos pero NO actualizar automáticamente
        // porque puede causar desregistros prematuros
        int removedCount = colorPreservedRenderers.RemoveAll(r => r == null);
        
        if (removedCount > 0 && showDebugInfo)
        {
            Debug.Log($"ColorPreservationRenderer: Removed {removedCount} null renderers");
            UpdateCommandBuffer();
        }
    }
    
    void OnDestroy()
    {
        // Limpiar command buffer
        if (cam != null && commandBuffer != null)
        {
            cam.RemoveCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
        }
        
        if (commandBuffer != null)
        {
            commandBuffer.Release();
        }
    }
    
    /// <summary>
    /// Encuentra o crea el ColorPreservationRenderer en la cámara principal
    /// </summary>
    public static ColorPreservationRenderer GetOrCreateInstance()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("ColorPreservationRenderer: No se encontró cámara principal");
            return null;
        }
        
        ColorPreservationRenderer instance = mainCam.GetComponent<ColorPreservationRenderer>();
        if (instance == null)
        {
            instance = mainCam.gameObject.AddComponent<ColorPreservationRenderer>();
            Debug.Log("ColorPreservationRenderer: Instancia creada automáticamente");
        }
        
        return instance;
    }
    
    // Métodos estáticos para fácil acceso
    public static void RegisterRenderer(Renderer renderer)
    {
        var instance = GetOrCreateInstance();
        instance?.RegisterColorPreservedRenderer(renderer);
    }
    
    public static void UnregisterRenderer(Renderer renderer)
    {
        var instance = GetOrCreateInstance();
        instance?.UnregisterColorPreservedRenderer(renderer);
    }
    
    /// <summary>
    /// Debug: Mostrar información del estado actual
    /// </summary>
    public static void ShowStatus()
    {
        var instance = GetOrCreateInstance();
        if (instance != null)
        {
            Debug.Log($"ColorPreservationRenderer Status: {instance.colorPreservedRenderers.Count} renderers registered");
            for (int i = 0; i < instance.colorPreservedRenderers.Count; i++)
            {
                var renderer = instance.colorPreservedRenderers[i];
                Debug.Log($"  [{i}] {(renderer != null ? renderer.name : "NULL")} - Material: {(renderer?.material?.name ?? "NULL")}");
            }
        }
    }
}