using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    public VideoPlayer videoPlayer;        // Asigna en Inspector
    public GameObject mainMenuPanel;       // Panel con los botones
    public string levelToLoad = "Level 1";

    private void Start()
    {
        AudioManager.instance.PlayMusic(0);

        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(false);
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    public void PlayGame()
    {
        mainMenuPanel.SetActive(false); // Oculta los botones
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Destroy(videoPlayer.gameObject);
        SceneManager.LoadSceneAsync(levelToLoad);
    }


    public void QuitGame()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }
}
