using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoToEnding : MonoBehaviour
{
    // Asigna el bot�n desde el Inspector
    public Button menuButton;

    private void Start()
    {
        // Aseg�rate de que el bot�n est� asignado
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(LoadLevel);
        }
    }

    private void LoadLevel()
    {
        SceneManager.LoadScene("Ending"); // Cambia el nombre de la escena seg�n sea necesario
    }
}
