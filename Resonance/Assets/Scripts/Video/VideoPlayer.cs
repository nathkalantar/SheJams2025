using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class VideoPlayer : MonoBehaviour
{
    [Header("Video Sources")]
    [SerializeField] private VideoClip localVideoClip;
    [SerializeField] private string webGLVideoURL = "https://github.com/user/repo/raw/main/video.mp4";
    
    [Header("Components")]
    [SerializeField] private UnityEngine.Video.VideoPlayer videoPlayer;
    [SerializeField] private bool playOnAwake = true;
    
    [Header("Next Scene Button")]
    [SerializeField] private Button nextSceneButton;
    [SerializeField] private string nextSceneName = "NextScene";
    [SerializeField] private float buttonDelayTime = 2f;
    [SerializeField] private float animationDuration = 0.5f;
    
    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        }
        
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer component not found!");
            return;
        }
        
        SetupVideoSource();
        
        // Configurar el botón
        SetupNextSceneButton();
        
        if (playOnAwake)
        {
            PlayVideo();
        }
    }
    
    void SetupVideoSource()
    {
#if UNITY_WEBGL
        // En WebGL, usar URL
        Debug.Log("Platform: WebGL - Using URL video");
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = webGLVideoURL;
#else
        // En otras plataformas (incluyendo Editor), usar VideoClip local
        Debug.Log("Platform: " + Application.platform + " - Using local VideoClip");
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = localVideoClip;
#endif
        
        // Configuración común
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
    }
    
    void SetupNextSceneButton()
    {
        if (nextSceneButton != null)
        {
            // Ocultar el botón inicialmente
            nextSceneButton.gameObject.SetActive(false);
            
            // Configurar el evento del botón
            nextSceneButton.onClick.RemoveAllListeners();
            nextSceneButton.onClick.AddListener(LoadNextScene);
            
            // Mostrar el botón después del delay
            StartCoroutine(ShowButtonAfterDelay());
        }
        else
        {
            Debug.LogWarning("Next Scene Button not assigned!");
        }
    }
    
    IEnumerator ShowButtonAfterDelay()
    {
        yield return new WaitForSeconds(buttonDelayTime);
        ShowButtonWithAnimation();
    }
    
    void ShowButtonWithAnimation()
    {
        if (nextSceneButton == null) return;
        
        // Activar el botón
        nextSceneButton.gameObject.SetActive(true);
        
        // Configurar estado inicial para la animación
        nextSceneButton.transform.localScale = Vector3.zero;
        nextSceneButton.GetComponent<CanvasGroup>()?.DOFade(0f, 0f);
        
        // Crear secuencia de animación
        Sequence buttonSequence = DOTween.Sequence();
        
        // Animación de escala con bounce
        buttonSequence.Append(nextSceneButton.transform.DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack));
        
        // Animación de fade in si tiene CanvasGroup
        if (nextSceneButton.GetComponent<CanvasGroup>() != null)
        {
            buttonSequence.Join(nextSceneButton.GetComponent<CanvasGroup>()
                .DOFade(1f, animationDuration));
        }
        
        Debug.Log("Next Scene button appeared!");
    }
    
    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Animación de salida del botón
            nextSceneButton.transform.DOScale(Vector3.zero, 0.2f)
                .OnComplete(() => {
                    SceneManager.LoadScene(nextSceneName);
                });
        }
        else
        {
            Debug.LogWarning("Next Scene Name is not set!");
        }
    }
    
    public void PlayVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            Debug.Log("Playing video...");
        }
    }
    
    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }
    
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }
    
    public void SetLoop(bool loop)
    {
        if (videoPlayer != null)
        {
            videoPlayer.isLooping = loop;
        }
    }
}
