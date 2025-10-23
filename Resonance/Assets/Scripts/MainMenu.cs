using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string introSceneName = "IntroScene";

    private void Start()
    {
        // Reproducir música de forma segura
        if (AudioManager.instance != null)
        {
            if (!AudioManager.instance.TryPlayMusic(0))
            {
                Debug.LogWarning("MainMenu: No se puede reproducir música en índice 0. Verifica que el array de música esté configurado en el AudioManager.");
                // Opcional: intentar con otro índice o mostrar el estado del AudioManager
                #if UNITY_EDITOR
                AudioManager.instance.LogAudioInfo();
                #endif
            }
        }
        else
        {
            Debug.LogError("MainMenu: AudioManager.instance es null. Asegúrate de que haya un AudioManager en la escena.");
        }
    }


    public void NextScene()
    {
        StartCoroutine(LoadSceneWithDelay());
    }
    
    private IEnumerator LoadSceneWithDelay()
    {
        // Esperar 0.5 segundos antes de cambiar de escena
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(introSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
