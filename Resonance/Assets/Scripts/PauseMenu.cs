using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    //[SerializeField] private Button quitButton;
    
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private KeyCode[] pauseKeys = { KeyCode.Escape, KeyCode.P }; // Múltiples teclas
    [SerializeField] private float animationDuration = 0.3f;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip unpauseSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    // Private variables
    private bool isPaused = false;
    private bool isAnimating = false;
    
    // Static reference for easy access
    public static PauseMenu Instance { get; private set; }
    
    // Properties
    public bool IsPaused => isPaused;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            
            // Don't destroy on load if you want the pause menu to persist across scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize UI
        InitializePauseMenu();
    }
    
    private void Start()
    {
        // Setup button listeners
        SetupButtons();
        
        // Ensure game starts unpaused
        ResumeGame();
    }
    
    private void Update()
    {
        // Check for pause input from any of the assigned keys
        foreach (KeyCode key in pauseKeys)
        {
            if (Input.GetKeyDown(key))
            {
                TogglePause();
                break; // Exit loop once we detect any pause key
            }
        }
    }
    
    private void InitializePauseMenu()
    {
        // Hide pause menu initially
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        // Initialize canvas group if not assigned
        if (pauseCanvasGroup == null && pauseMenuPanel != null)
            pauseCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            
        // Create canvas group if it doesn't exist
        if (pauseCanvasGroup == null && pauseMenuPanel != null)
            pauseCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
    }
    
    private void SetupButtons()
    {
        // Resume button
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(() => {
                PlayButtonSound();
                ResumeGame();
            });
        }
        
        // Main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => {
                PlayButtonSound();
                GoToMainMenu();
            });
        }
        
        // Quit button
        /*if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(() => {
                PlayButtonSound();
                QuitGame();
            });
        }*/
    }
    
    /// <summary>
    /// Toggle between pause and resume
    /// </summary>
    public void TogglePause()
    {
        if (isAnimating) return;
        
        // Check if we can pause
        if (!isPaused && !CanPause()) return;
        
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (isPaused || isAnimating) return;
        
        isPaused = true;
        
        // Pause time
        Time.timeScale = 0f;
        
        // Play pause sound
        if (pauseSound != null)
            PlaySoundUnscaled(pauseSound);
        
        // Show pause menu with animation
        StartCoroutine(ShowPauseMenuCoroutine());
        
        Debug.Log("Game paused");
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused || isAnimating) return;
        
        isPaused = false;
        
        // Resume time
        Time.timeScale = 1f;
        
        // Play unpause sound
        if (unpauseSound != null)
            PlaySoundUnscaled(unpauseSound);
        
        // Hide pause menu with animation
        StartCoroutine(HidePauseMenuCoroutine());
        
        Debug.Log("Game resumed");
    }
    
    /// <summary>
    /// Go to main menu
    /// </summary>
    public void GoToMainMenu()
    {
        // Resume time before changing scenes
        Time.timeScale = 1f;
        isPaused = false;
        
        Debug.Log($"Loading main menu: {mainMenuSceneName}");
        
        // Load main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("Main menu scene name is not set!");
        }
    }
    
    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        // Resume time before quitting
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
            // Stop playing in editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Quit application
            Application.Quit();
        #endif
    }
    
    private IEnumerator ShowPauseMenuCoroutine()
    {
        isAnimating = true;
        
        if (pauseMenuPanel != null && pauseCanvasGroup != null)
        {
            // Show panel
            pauseMenuPanel.SetActive(true);
            
            // Animate fade in
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = true;
            
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time since we're paused
                pauseCanvasGroup.alpha = elapsed / animationDuration;
                yield return null;
            }
            
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
        }
        
        isAnimating = false;
    }
    
    private IEnumerator HidePauseMenuCoroutine()
    {
        isAnimating = true;
        
        if (pauseMenuPanel != null && pauseCanvasGroup != null)
        {
            // Animate fade out
            pauseCanvasGroup.interactable = false;
            
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time
                pauseCanvasGroup.alpha = 1f - (elapsed / animationDuration);
                yield return null;
            }
            
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.blocksRaycasts = false;
            
            // Hide panel
            pauseMenuPanel.SetActive(false);
        }
        
        isAnimating = false;
    }
    
    private void PlayButtonSound()
    {
        if (buttonClickSound != null)
            PlaySoundUnscaled(buttonClickSound);
    }
    
    private void PlaySoundUnscaled(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // Play sound unaffected by time scale
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// Force resume (useful for external scripts)
    /// </summary>
    public void ForceResume()
    {
        isPaused = false;
        isAnimating = false;
        Time.timeScale = 1f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Check if pause menu can be activated (override this for specific conditions)
    /// </summary>
    public virtual bool CanPause()
    {
        // No pausar si hay un minijuego activo
        if (UIMinigameManager.Instance != null && UIMinigameManager.Instance.IsMinigameActive())
        {
            return false;
        }

        // Add custom conditions here, for example:
        // - Don't pause during cutscenes
        // - Don't pause during specific gameplay moments
        // - Don't pause if other UI is open

        return true;
    }

#if !UNITY_EDITOR
    private void OnApplicationFocus(bool hasFocus)
    {
        // Auto-pause when losing focus (optional)
        if (!hasFocus && !isPaused && CanPause())
        {
            PauseGame();
        }
    }
#endif
    
    private void OnDestroy()
    {
        // Clean up singleton
        if (Instance == this)
            Instance = null;
            
        // Ensure time is resumed when destroyed
        Time.timeScale = 1f;
    }
    
    // Debug methods (remove in final build)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnGUI()
    {
        if (Application.isEditor)
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label($"Time Scale: {Time.timeScale:F2}");
            GUILayout.Label($"Is Paused: {isPaused}");
            GUILayout.Label($"Is Animating: {isAnimating}");
            GUILayout.EndArea();
        }
    }
}