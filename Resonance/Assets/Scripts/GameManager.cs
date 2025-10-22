using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("FPS Settings")]
    [SerializeField] private int targetFPS = 60;
    [SerializeField] private bool vsyncEnabled = false;
    [SerializeField] private bool showFPSCounter = false;
    [SerializeField] private int[] availableFPSLimits = {30, 60, 120, 144, -1}; // -1 = unlimited
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugControls = true;
    [SerializeField] private KeyCode toggleFPSCounterKey = KeyCode.F2;
    [SerializeField] private KeyCode cycleFPSKey = KeyCode.F3;
    [SerializeField] private KeyCode toggleVSyncKey = KeyCode.F4;
    
    // Singleton
    public static GameManager Instance { get; private set; }
    
    // FPS tracking
    private float currentFPS = 0f;
    private float fpsTimer = 0f;
    private int currentFPSIndex = 1; // Default to 60 FPS
    private GUIStyle fpsStyle;

    #region Unity Lifecycle
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeFPS();
        SetupFPSStyle();
    }

    private void Update()
    {
        if (enableDebugControls)
        {
            HandleDebugInput();
        }
        
        if (showFPSCounter)
        {
            UpdateFPSCounter();
        }
    }

    private void OnGUI()
    {
        if (showFPSCounter && fpsStyle != null)
        {
            string targetText = targetFPS == -1 ? "Unlimited" : targetFPS.ToString();
            string fpsText = $"FPS: {currentFPS:F1} (Target: {targetText})";
            
            if (vsyncEnabled)
                fpsText += " [V-Sync ON]";
                
            GUI.Label(new Rect(10, 10, 350, 30), fpsText, fpsStyle);
        }
    }

    #endregion

    #region Initialization

    private void InitializeFPS()
    {
        // Encontrar el índice del FPS actual en el array
        for (int i = 0; i < availableFPSLimits.Length; i++)
        {
            if (availableFPSLimits[i] == targetFPS)
            {
                currentFPSIndex = i;
                break;
            }
        }
        
        ApplyFPSSettings();
        Debug.Log($"GameManager: Initialized with {targetFPS} FPS, V-Sync: {vsyncEnabled}");
    }

    private void SetupFPSStyle()
    {
        fpsStyle = new GUIStyle();
        fpsStyle.fontSize = 16;
        fpsStyle.normal.textColor = Color.white;
        fpsStyle.alignment = TextAnchor.UpperLeft;
        fpsStyle.fontStyle = FontStyle.Bold;
        
        // Agregar fondo semi-transparente para mejor legibilidad
        Texture2D backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        backgroundTexture.Apply();
        fpsStyle.normal.background = backgroundTexture;
        fpsStyle.padding = new RectOffset(5, 5, 2, 2);
    }

    private void ApplyFPSSettings()
    {
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
        Application.targetFrameRate = targetFPS;
    }

    #endregion

    #region Input Handling

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(toggleFPSCounterKey))
        {
            ToggleFPSCounter();
        }
        
        if (Input.GetKeyDown(cycleFPSKey))
        {
            CycleFPSLimit();
        }
        
        if (Input.GetKeyDown(toggleVSyncKey))
        {
            ToggleVSync();
        }
    }

    #endregion

    #region FPS Management

    private void UpdateFPSCounter()
    {
        fpsTimer += Time.unscaledDeltaTime;
        
        if (fpsTimer >= 0.5f) // Actualizar cada 0.5 segundos
        {
            currentFPS = 1f / Time.unscaledDeltaTime;
            fpsTimer = 0f;
        }
    }

    public void SetTargetFPS(int fps)
    {
        targetFPS = fps;
        ApplyFPSSettings();
        Debug.Log($"GameManager: Target FPS set to {(fps == -1 ? "Unlimited" : fps.ToString())}");
    }

    public void CycleFPSLimit()
    {
        currentFPSIndex = (currentFPSIndex + 1) % availableFPSLimits.Length;
        targetFPS = availableFPSLimits[currentFPSIndex];
        ApplyFPSSettings();
        
        string fpsText = targetFPS == -1 ? "Unlimited" : targetFPS.ToString();
        Debug.Log($"GameManager: FPS cycled to {fpsText}");
    }

    public void ToggleVSync()
    {
        vsyncEnabled = !vsyncEnabled;
        ApplyFPSSettings();
        Debug.Log($"GameManager: V-Sync {(vsyncEnabled ? "enabled" : "disabled")}");
    }

    public void ToggleFPSCounter()
    {
        showFPSCounter = !showFPSCounter;
        Debug.Log($"GameManager: FPS counter {(showFPSCounter ? "enabled" : "disabled")}");
    }

    #endregion

    #region Public Getters

    public float GetCurrentFPS() => currentFPS;
    public int GetTargetFPS() => targetFPS;
    public bool IsVSyncEnabled() => vsyncEnabled;
    public bool IsFPSCounterShown() => showFPSCounter;

    #endregion

    #region Validation

    private void OnValidate()
    {
        // Validar que targetFPS esté en el array de opciones disponibles
        bool validFPS = false;
        foreach (int fps in availableFPSLimits)
        {
            if (fps == targetFPS)
            {
                validFPS = true;
                break;
            }
        }
        
        if (!validFPS && availableFPSLimits.Length > 0)
        {
            targetFPS = availableFPSLimits[0];
            Debug.LogWarning($"GameManager: Target FPS was invalid, set to {targetFPS}");
        }
        
        // Aplicar cambios si estamos en runtime
        if (Application.isPlaying)
        {
            ApplyFPSSettings();
        }
    }

    #endregion
}