using UnityEngine;

/// <summary>
/// Controlador para el shader de cielo animado
/// Permite ajustar todos los parámetros dinámicamente y crear presets
/// </summary>
[RequireComponent(typeof(Renderer))]
public class AnimatedSkyController : MonoBehaviour
{
    [Header("Sky Colors")]
    [SerializeField] private Color skyColorTop = new Color(0.5f, 0.8f, 1.0f, 1.0f);
    [SerializeField] private Color skyColorBottom = new Color(0.8f, 0.9f, 1.0f, 1.0f);
    [Range(0f, 1f)]
    [SerializeField] private float horizonLine = 0.3f;
    [Range(0.01f, 1f)]
    [SerializeField] private float horizonSoftness = 0.1f;
    
    [Header("Cloud Settings")]
    [SerializeField] private Color cloudColor = Color.white;
    [SerializeField] private Color cloudShadowColor = new Color(0.6f, 0.6f, 0.7f, 1.0f);
    [Range(0f, 1f)]
    [SerializeField] private float cloudCoverage = 0.5f;
    [Range(0.1f, 2f)]
    [SerializeField] private float cloudDensity = 1.0f;
    [Range(0.1f, 5f)]
    [SerializeField] private float cloudSharpness = 2.0f;
    
    [Header("Animation")]
    [Range(-2f, 2f)]
    [SerializeField] private float cloudSpeedX = 0.5f;
    [Range(-2f, 2f)]
    [SerializeField] private float cloudSpeedY = 0.1f;
    [Range(0f, 2f)]
    [SerializeField] private float windTurbulence = 0.3f;
    
    [Header("Noise Settings")]
    [Range(0.1f, 10f)]
    [SerializeField] private float noiseScale = 2.0f;
    [Range(1, 4)]
    [SerializeField] private int noiseOctaves = 3;
    [Range(0.1f, 1f)]
    [SerializeField] private float noisePersistence = 0.5f;
    
    [Header("Lighting")]
    [SerializeField] private Vector3 sunDirection = new Vector3(0.3f, 0.7f, 0.6f);
    [Range(0f, 2f)]
    [SerializeField] private float sunInfluence = 1.0f;
    [Range(0f, 2f)]
    [SerializeField] private float atmosphericPerspective = 0.5f;
    
    [Header("Presets")]
    [SerializeField] private bool usePresets = false;
    [SerializeField] private SkyPreset currentPreset = SkyPreset.Default;
    
    [Header("Animation Controls")]
    [SerializeField] private bool animateTimeOfDay = false;
    [Range(0f, 24f)]
    [SerializeField] private float timeOfDay = 12f;
    [SerializeField] private float dayDuration = 60f; // Duración del día en segundos
    
    private Material skyMaterial;
    private Renderer skyRenderer;
    
    public enum SkyPreset
    {
        Default,
        MorningClear,
        AfternoonCloudy,
        Evening,
        Storm,
        Peaceful,
        Dramatic
    }
    
    void Start()
    {
        skyRenderer = GetComponent<Renderer>();
        skyMaterial = skyRenderer.material;
        
        if (skyMaterial.shader.name != "Custom/AnimatedSky")
        {
            Debug.LogWarning("AnimatedSkyController: El material no usa el shader 'Custom/AnimatedSky'");
        }
        
        // Aplicar preset inicial
        if (usePresets)
        {
            ApplyPreset(currentPreset);
        }
        
        // Aplicar configuración inicial
        UpdateShaderProperties();
    }
    
    void Update()
    {
        // Animación del tiempo del día
        if (animateTimeOfDay)
        {
            timeOfDay += (24f / dayDuration) * Time.deltaTime;
            if (timeOfDay >= 24f) timeOfDay -= 24f;
            
            UpdateTimeOfDay();
        }
        
        // Actualizar propiedades del shader si cambian en el inspector
        UpdateShaderProperties();
    }
    
    void UpdateShaderProperties()
    {
        if (skyMaterial == null) return;
        
        // Colores del cielo
        skyMaterial.SetColor("_SkyColorTop", skyColorTop);
        skyMaterial.SetColor("_SkyColorBottom", skyColorBottom);
        skyMaterial.SetFloat("_HorizonLine", horizonLine);
        skyMaterial.SetFloat("_HorizonSoftness", horizonSoftness);
        
        // Configuración de nubes
        skyMaterial.SetColor("_CloudColor", cloudColor);
        skyMaterial.SetColor("_CloudShadowColor", cloudShadowColor);
        skyMaterial.SetFloat("_CloudCoverage", cloudCoverage);
        skyMaterial.SetFloat("_CloudDensity", cloudDensity);
        skyMaterial.SetFloat("_CloudSharpness", cloudSharpness);
        
        // Animación
        skyMaterial.SetFloat("_CloudSpeedX", cloudSpeedX);
        skyMaterial.SetFloat("_CloudSpeedY", cloudSpeedY);
        skyMaterial.SetFloat("_WindTurbulence", windTurbulence);
        
        // Ruido
        skyMaterial.SetFloat("_NoiseScale", noiseScale);
        skyMaterial.SetInt("_NoiseOctaves", noiseOctaves);
        skyMaterial.SetFloat("_NoisePersistence", noisePersistence);
        
        // Iluminación
        skyMaterial.SetVector("_SunDirection", sunDirection.normalized);
        skyMaterial.SetFloat("_SunInfluence", sunInfluence);
        skyMaterial.SetFloat("_AtmosphericPerspective", atmosphericPerspective);
    }
    
    void UpdateTimeOfDay()
    {
        // Cambiar colores basado en la hora del día
        if (timeOfDay >= 6f && timeOfDay < 8f) // Amanecer
        {
            float t = (timeOfDay - 6f) / 2f;
            skyColorTop = Color.Lerp(new Color(0.2f, 0.1f, 0.3f), new Color(1f, 0.6f, 0.3f), t);
            skyColorBottom = Color.Lerp(new Color(0.1f, 0.1f, 0.2f), new Color(1f, 0.8f, 0.5f), t);
        }
        else if (timeOfDay >= 8f && timeOfDay < 18f) // Día
        {
            skyColorTop = new Color(0.5f, 0.8f, 1.0f);
            skyColorBottom = new Color(0.8f, 0.9f, 1.0f);
        }
        else if (timeOfDay >= 18f && timeOfDay < 20f) // Atardecer
        {
            float t = (timeOfDay - 18f) / 2f;
            skyColorTop = Color.Lerp(new Color(0.5f, 0.8f, 1.0f), new Color(1f, 0.4f, 0.2f), t);
            skyColorBottom = Color.Lerp(new Color(0.8f, 0.9f, 1.0f), new Color(1f, 0.6f, 0.3f), t);
        }
        else // Noche
        {
            skyColorTop = new Color(0.1f, 0.1f, 0.3f);
            skyColorBottom = new Color(0.2f, 0.2f, 0.4f);
        }
    }
    
    public void ApplyPreset(SkyPreset preset)
    {
        switch (preset)
        {
            case SkyPreset.MorningClear:
                skyColorTop = new Color(0.6f, 0.8f, 1.0f);
                skyColorBottom = new Color(1f, 0.9f, 0.7f);
                cloudCoverage = 0.2f;
                cloudSpeedX = 0.3f;
                cloudSpeedY = 0.05f;
                windTurbulence = 0.1f;
                break;
                
            case SkyPreset.AfternoonCloudy:
                skyColorTop = new Color(0.4f, 0.7f, 0.9f);
                skyColorBottom = new Color(0.7f, 0.8f, 0.9f);
                cloudCoverage = 0.7f;
                cloudSpeedX = 0.8f;
                cloudSpeedY = 0.2f;
                windTurbulence = 0.5f;
                break;
                
            case SkyPreset.Evening:
                skyColorTop = new Color(1f, 0.5f, 0.3f);
                skyColorBottom = new Color(1f, 0.8f, 0.4f);
                cloudColor = new Color(1f, 0.7f, 0.5f);
                cloudShadowColor = new Color(0.8f, 0.4f, 0.3f);
                cloudCoverage = 0.4f;
                cloudSpeedX = 0.2f;
                windTurbulence = 0.2f;
                break;
                
            case SkyPreset.Storm:
                skyColorTop = new Color(0.3f, 0.3f, 0.4f);
                skyColorBottom = new Color(0.4f, 0.4f, 0.5f);
                cloudColor = new Color(0.6f, 0.6f, 0.7f);
                cloudShadowColor = new Color(0.2f, 0.2f, 0.3f);
                cloudCoverage = 0.9f;
                cloudDensity = 1.5f;
                cloudSpeedX = 1.5f;
                cloudSpeedY = 0.3f;
                windTurbulence = 1.0f;
                break;
                
            case SkyPreset.Peaceful:
                skyColorTop = new Color(0.5f, 0.8f, 1.0f);
                skyColorBottom = new Color(0.9f, 0.95f, 1.0f);
                cloudColor = Color.white;
                cloudCoverage = 0.3f;
                cloudSpeedX = 0.1f;
                cloudSpeedY = 0.02f;
                windTurbulence = 0.05f;
                break;
                
            case SkyPreset.Dramatic:
                skyColorTop = new Color(0.2f, 0.5f, 0.8f);
                skyColorBottom = new Color(0.8f, 0.6f, 0.4f);
                cloudColor = new Color(1f, 0.9f, 0.8f);
                cloudShadowColor = new Color(0.4f, 0.3f, 0.5f);
                cloudCoverage = 0.6f;
                cloudDensity = 1.2f;
                cloudSpeedX = 0.7f;
                windTurbulence = 0.8f;
                sunInfluence = 1.5f;
                break;
        }
        
        currentPreset = preset;
        UpdateShaderProperties();
    }
    
    // Métodos públicos para controlar desde otros scripts
    public void SetCloudSpeed(float speedX, float speedY)
    {
        cloudSpeedX = speedX;
        cloudSpeedY = speedY;
    }
    
    public void SetCloudCoverage(float coverage)
    {
        cloudCoverage = Mathf.Clamp01(coverage);
    }
    
    public void SetSkyColors(Color top, Color bottom)
    {
        skyColorTop = top;
        skyColorBottom = bottom;
    }
    
    public void SetTimeOfDay(float hour)
    {
        timeOfDay = Mathf.Clamp(hour, 0f, 24f);
        UpdateTimeOfDay();
    }
    
    void OnValidate()
    {
        // Actualizar cuando se cambien valores en el inspector
        if (Application.isPlaying && skyMaterial != null)
        {
            if (usePresets)
            {
                ApplyPreset(currentPreset);
            }
            UpdateShaderProperties();
        }
    }
}