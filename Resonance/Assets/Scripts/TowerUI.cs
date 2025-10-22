using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerUI : MonoBehaviour
{
    [Header("Warning Message")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private CanvasGroup warningCanvasGroup; // Referencia directa al CanvasGroup
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float messageDuration = 3f;
    
    [Header("Confirmation Dialog")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private CanvasGroup confirmationCanvasGroup; // Referencia directa al CanvasGroup
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    
    // Private variables
    private bool isShowingMessage = false;
    private System.Action onConfirm;
    private System.Action onCancel;
    
    // Singleton pattern for easy access
    public static TowerUI Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize UI
        InitializeUI();
    }
    
    private void Start()
    {
        // Setup button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }
    
    private void InitializeUI()
    {
        // Hide all panels initially
        if (warningPanel != null)
            warningPanel.SetActive(false);
            
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
    
    /// <summary>
    /// Shows a warning message with fade animation
    /// </summary>
    /// <param name="message">The message to display</param>
    public void ShowWarningMessage(string message)
    {
        // Prevent multiple messages from showing at once
        if (isShowingMessage) return;
        
        StartCoroutine(ShowWarningCoroutine(message));
    }
    
    /// <summary>
    /// Shows a confirmation dialog
    /// </summary>
    /// <param name="message">The confirmation message</param>
    /// <param name="onConfirmCallback">Action to execute when confirmed</param>
    /// <param name="onCancelCallback">Action to execute when cancelled</param>
    public void ShowConfirmationDialog(string message, System.Action onConfirmCallback, System.Action onCancelCallback = null)
    {
        // Store callbacks
        onConfirm = onConfirmCallback;
        onCancel = onCancelCallback;
        
        // Set message
        if (confirmationText != null)
            confirmationText.text = message;
        
        // Show dialog with animation
        StartCoroutine(ShowConfirmationCoroutine());
    }
    
    private IEnumerator ShowWarningCoroutine(string message)
    {
        isShowingMessage = true;
        
        // Set message text
        if (warningText != null)
            warningText.text = message;
        
        // Show panel
        if (warningPanel != null && warningCanvasGroup != null)
        {
            warningPanel.SetActive(true);
            
            // Fade in simple
            warningCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                warningCanvasGroup.alpha = elapsed / animationDuration;
                yield return null;
            }
            warningCanvasGroup.alpha = 1f;
            
            // Wait for message duration
            yield return new WaitForSeconds(messageDuration);
            
            // Fade out suave
            elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                warningCanvasGroup.alpha = 1f - (elapsed / animationDuration);
                yield return null;
            }
            warningCanvasGroup.alpha = 0f;
            
            // Hide panel
            warningPanel.SetActive(false);
        }
        
        isShowingMessage = false;
    }
    
    private IEnumerator ShowConfirmationCoroutine()
    {
        if (confirmationPanel != null && confirmationCanvasGroup != null)
        {
            confirmationPanel.SetActive(true);
            
            // Fade in simple
            confirmationCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                confirmationCanvasGroup.alpha = elapsed / animationDuration;
                yield return null;
            }
            confirmationCanvasGroup.alpha = 1f;
        }
    }
    
    private IEnumerator HideConfirmationCoroutine()
    {
        if (confirmationPanel != null && confirmationCanvasGroup != null)
        {
            // Fade out suave
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                confirmationCanvasGroup.alpha = 1f - (elapsed / animationDuration);
                yield return null;
            }
            confirmationCanvasGroup.alpha = 0f;
            
            confirmationPanel.SetActive(false);
        }
    }
    
    private void OnContinueClicked()
    {
        StartCoroutine(OnContinueCoroutine());
    }
    
    private void OnCancelClicked()
    {
        StartCoroutine(OnCancelCoroutine());
    }
    
    private IEnumerator OnContinueCoroutine()
    {
        // Hide dialog first
        yield return StartCoroutine(HideConfirmationCoroutine());
        
        // Execute callback
        onConfirm?.Invoke();
        
        // Clear callbacks
        onConfirm = null;
        onCancel = null;
    }
    
    private IEnumerator OnCancelCoroutine()
    {
        // Hide dialog first
        yield return StartCoroutine(HideConfirmationCoroutine());
        
        // Execute callback
        onCancel?.Invoke();
        
        // Clear callbacks
        onConfirm = null;
        onCancel = null;
    }
    
    /// <summary>
    /// Force hide all UI elements (useful for cleanup)
    /// </summary>
    public void HideAllUI()
    {
        StopAllCoroutines();
        
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
            if (warningCanvasGroup != null)
                warningCanvasGroup.alpha = 0f;
        }
            
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
            if (confirmationCanvasGroup != null)
                confirmationCanvasGroup.alpha = 0f;
        }
            
        isShowingMessage = false;
        onConfirm = null;
        onCancel = null;
    }
    
    private void OnDestroy()
    {
        // Clean up singleton
        if (Instance == this)
            Instance = null;
    }
}