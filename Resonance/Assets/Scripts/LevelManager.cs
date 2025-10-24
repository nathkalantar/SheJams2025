using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    private void Start()
    {
        // Reproducir música de forma segura
        if (AudioManager.instance != null)
        {
            if (!AudioManager.instance.TryPlayMusic(1))
            {
                Debug.LogWarning("LevelManager: No se puede reproducir música en índice 1. Verifica que el array de música esté configurado en el AudioManager.");
                // Opcional: intentar con otro índice o mostrar el estado del AudioManager
#if UNITY_EDITOR
                AudioManager.instance.LogAudioInfo();
#endif
            }
        }
        else
        {
            Debug.LogError("LevelManager: AudioManager.instance es null. Asegúrate de que haya un AudioManager en la escena.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
