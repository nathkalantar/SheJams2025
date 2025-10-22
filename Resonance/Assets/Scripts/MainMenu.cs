using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string introSceneName = "IntroScene";

    private void Start()
    {
            AudioManager.instance.PlayMusic(0);
    }


    public void NextScene()
    {
        SceneManager.LoadScene(introSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
