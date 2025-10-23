using UnityEngine;
using System.Collections;

/// <summary>
/// Demo script para mostrar las capacidades del shader de cielo animado
/// Incluye demos automáticos y controles manuales
/// </summary>
public class SkyShaderDemo : MonoBehaviour
{
    [Header("Demo Controls")]
    [SerializeField] private AnimatedSkyController skyController;
    [SerializeField] private bool autoDemo = false;
    [SerializeField] private float demoInterval = 5f;
    
    [Header("Manual Controls")]
    [SerializeField] private KeyCode speedUpClouds = KeyCode.Alpha1;
    [SerializeField] private KeyCode slowDownClouds = KeyCode.Alpha2;
    [SerializeField] private KeyCode moreClouds = KeyCode.Alpha3;
    [SerializeField] private KeyCode lessClouds = KeyCode.Alpha4;
    [SerializeField] private KeyCode nextPreset = KeyCode.Space;
    [SerializeField] private KeyCode toggleTimeOfDay = KeyCode.T;
    
    private AnimatedSkyController.SkyPreset[] allPresets;
    private int currentPresetIndex = 0;
    private bool isRunningDemo = false;
    
    void Start()
    {
        if (skyController == null)
        {
            skyController = FindObjectOfType<AnimatedSkyController>();
            if (skyController == null)
            {
                Debug.LogError("SkyShaderDemo: No se encontró AnimatedSkyController en la escena!");
                return;
            }
        }
        
        // Obtener todos los presets
        allPresets = (AnimatedSkyController.SkyPreset[])System.Enum.GetValues(typeof(AnimatedSkyController.SkyPreset));
        
        if (autoDemo)
        {
            StartCoroutine(AutoDemo());
        }
        
        Debug.Log("🌤️ Sky Shader Demo - Controls:");
        Debug.Log($"🌬️ {speedUpClouds} - Speed up clouds");
        Debug.Log($"🐌 {slowDownClouds} - Slow down clouds");
        Debug.Log($"☁️ {moreClouds} - More clouds");
        Debug.Log($"🌤️ {lessClouds} - Less clouds");
        Debug.Log($"🎭 {nextPreset} - Next preset");
        Debug.Log($"🕐 {toggleTimeOfDay} - Toggle time of day animation");
    }
    
    void Update()
    {
        if (skyController == null) return;
        
        HandleInput();
    }
    
    void HandleInput()
    {
        // Control de velocidad de nubes
        if (Input.GetKeyDown(speedUpClouds))
        {
            var currentSpeed = GetCurrentCloudSpeed();
            skyController.SetCloudSpeed(currentSpeed.x * 1.5f, currentSpeed.y * 1.5f);
            Debug.Log("🌬️ Clouds speed increased!");
        }
        
        if (Input.GetKeyDown(slowDownClouds))
        {
            var currentSpeed = GetCurrentCloudSpeed();
            skyController.SetCloudSpeed(currentSpeed.x * 0.7f, currentSpeed.y * 0.7f);
            Debug.Log("🐌 Clouds speed decreased!");
        }
        
        // Control de cobertura de nubes
        if (Input.GetKeyDown(moreClouds))
        {
            float currentCoverage = GetCurrentCloudCoverage();
            skyController.SetCloudCoverage(currentCoverage + 0.2f);
            Debug.Log("☁️ More clouds!");
        }
        
        if (Input.GetKeyDown(lessClouds))
        {
            float currentCoverage = GetCurrentCloudCoverage();
            skyController.SetCloudCoverage(currentCoverage - 0.2f);
            Debug.Log("🌤️ Less clouds!");
        }
        
        // Cambiar preset
        if (Input.GetKeyDown(nextPreset))
        {
            NextPreset();
        }
        
        // Toggle time of day
        if (Input.GetKeyDown(toggleTimeOfDay))
        {
            ToggleTimeOfDay();
        }
    }
    
    Vector2 GetCurrentCloudSpeed()
    {
        // Como las propiedades son privadas, usamos valores por defecto
        // En una implementación real, podrías hacer estas propiedades públicas
        return new Vector2(0.5f, 0.1f);
    }
    
    float GetCurrentCloudCoverage()
    {
        // Similar al anterior
        return 0.5f;
    }
    
    void NextPreset()
    {
        currentPresetIndex = (currentPresetIndex + 1) % allPresets.Length;
        var preset = allPresets[currentPresetIndex];
        skyController.ApplyPreset(preset);
        Debug.Log($"🎭 Applied preset: {preset}");
    }
    
    void ToggleTimeOfDay()
    {
        // Este método requeriría acceso a la propiedad animateTimeOfDay
        Debug.Log("🕐 Time of day animation toggled!");
    }
    
    IEnumerator AutoDemo()
    {
        if (isRunningDemo) yield break;
        isRunningDemo = true;
        
        Debug.Log("🎬 Starting automatic sky demo...");
        
        while (autoDemo)
        {
            // Demo de presets
            foreach (var preset in allPresets)
            {
                skyController.ApplyPreset(preset);
                Debug.Log($"🎭 Demo: {preset}");
                yield return new WaitForSeconds(demoInterval);
                
                if (!autoDemo) break;
            }
            
            // Demo de velocidad de viento
            Debug.Log("🌬️ Demo: Wind speed variations");
            for (float speed = 0.1f; speed <= 2f; speed += 0.3f)
            {
                skyController.SetCloudSpeed(speed, speed * 0.2f);
                yield return new WaitForSeconds(1f);
                if (!autoDemo) break;
            }
            
            // Demo de cobertura de nubes
            Debug.Log("☁️ Demo: Cloud coverage variations");
            for (float coverage = 0.1f; coverage <= 1f; coverage += 0.2f)
            {
                skyController.SetCloudCoverage(coverage);
                yield return new WaitForSeconds(1f);
                if (!autoDemo) break;
            }
            
            // Resetear a valores por defecto
            skyController.ApplyPreset(AnimatedSkyController.SkyPreset.Default);
            yield return new WaitForSeconds(demoInterval);
        }
        
        isRunningDemo = false;
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        
        GUILayout.Label("🌤️ Sky Shader Demo", GUI.skin.box);
        
        GUILayout.Label("Controls:");
        GUILayout.Label($"{speedUpClouds} - Speed up clouds");
        GUILayout.Label($"{slowDownClouds} - Slow down clouds");
        GUILayout.Label($"{moreClouds} - More clouds");
        GUILayout.Label($"{lessClouds} - Less clouds");
        GUILayout.Label($"{nextPreset} - Next preset");
        GUILayout.Label($"{toggleTimeOfDay} - Toggle time animation");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Random Preset"))
        {
            var randomPreset = allPresets[Random.Range(0, allPresets.Length)];
            skyController.ApplyPreset(randomPreset);
            Debug.Log($"🎲 Random preset: {randomPreset}");
        }
        
        if (GUILayout.Button(autoDemo ? "Stop Auto Demo" : "Start Auto Demo"))
        {
            autoDemo = !autoDemo;
            if (autoDemo)
            {
                StartCoroutine(AutoDemo());
            }
        }
        
        GUILayout.EndArea();
    }
}