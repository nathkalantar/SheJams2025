using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class FinalCutSceneController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject endPanel; // UI con bot�n
    public float delayAfterEnd = 2f;

    void Start()
    {
        endPanel.SetActive(false);
        //videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Invoke(nameof(ShowEndPanel), delayAfterEnd);
    }

    void ShowEndPanel()
    {
        endPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
