using UnityEngine;

[ExecuteInEditMode]
public class ColorSphere : MonoBehaviour
{
    [Header("Sphere Settings")]
    [Tooltip("Radio de la esfera de color")]
    public float radius = 5f;
    
    [Range(0f, 2f)]
    [Tooltip("Ancho de la transición entre B&N y color")]
    public float fadeWidth = 0.5f;
    
    [Header("Animation")]
    public bool animateRadius = false;
    public float pulseSpeed = 1f;
    public float minRadius = 3f;
    public float maxRadius = 7f;
    
    [Header("Gizmo")]
    public bool showGizmo = true;
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.5f);
    
    [Header("Auto Register")]
    [Tooltip("Buscar automáticamente ScreenDesaturationEffect en la escena")]
    public bool autoRegister = true;
    
    private float currentRadius;
    private ScreenDesaturationEffect desaturationEffect;
    private bool isRegistered = false;

    private void Start()
    {
        currentRadius = radius;
        
        if (autoRegister)
        {
            RegisterToEffect();
        }
    }

    private void OnEnable()
    {
        if (autoRegister && Application.isPlaying)
        {
            RegisterToEffect();
        }
    }

    private void OnDisable()
    {
        UnregisterFromEffect();
    }

    private void Update()
    {
        if (animateRadius)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            currentRadius = Mathf.Lerp(minRadius, maxRadius, t);
        }
        else
        {
            currentRadius = radius;
        }
    }

    private void RegisterToEffect()
    {
        if (isRegistered) return;
        
        if (desaturationEffect == null)
        {
            desaturationEffect = FindObjectOfType<ScreenDesaturationEffect>();
        }
        
        if (desaturationEffect != null)
        {
            desaturationEffect.RegisterColorSphere(this);
            isRegistered = true;
        }
    }

    private void UnregisterFromEffect()
    {
        if (!isRegistered) return;
        
        if (desaturationEffect != null)
        {
            desaturationEffect.UnregisterColorSphere(this);
        }
        
        isRegistered = false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        float drawRadius = Application.isPlaying ? currentRadius : radius;
        
        // Esfera exterior (zona de color completo)
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, drawRadius);
        
        // Esfera interior (inicio del fade)
        Gizmos.color = new Color(
            gizmoColor.r, 
            gizmoColor.g, 
            gizmoColor.b, 
            0.2f
        );
        Gizmos.DrawWireSphere(transform.position, drawRadius - fadeWidth);
        
        // Punto central
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

    private void OnValidate()
    {
        // Asegurar que los valores mínimos/máximos sean coherentes
        if (minRadius > maxRadius)
        {
            minRadius = maxRadius;
        }
        
        currentRadius = radius;
    }

    // Métodos públicos para control externo
    public float GetCurrentRadius()
    {
        return currentRadius;
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        currentRadius = newRadius;
    }

    public void SetFadeWidth(float newFadeWidth)
    {
        fadeWidth = newFadeWidth;
    }
}
