using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoToEnding : MonoBehaviour
{
    // Asigna el botón desde el Inspector
    public Button menuButton;

    private void Start()
    {
        // Asegúrate de que el botón esté asignado
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(LoadLevel);
        }
    }

    private void LoadLevel()
    {
        SceneManager.LoadScene("Ending"); // Cambia el nombre de la escena según sea necesario
    }
}
