using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    private void Start()
    {
        // Reproducir m�sica de forma segura
        if (AudioManager.instance != null)
        {
            if (!AudioManager.instance.TryPlayMusic(1))
            {
                Debug.LogWarning("LevelManager: No se puede reproducir m�sica en �ndice 1. Verifica que el array de m�sica est� configurado en el AudioManager.");
                // Opcional: intentar con otro �ndice o mostrar el estado del AudioManager
#if UNITY_EDITOR
                AudioManager.instance.LogAudioInfo();
#endif
            }
        }
        else
        {
            Debug.LogError("LevelManager: AudioManager.instance es null. Aseg�rate de que haya un AudioManager en la escena.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
