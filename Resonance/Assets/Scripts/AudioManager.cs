using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource walkingAudioSource;  // For walking sounds   
    public static AudioManager instance;
    public AudioSource audioS, audioM;
    public AudioClip[] pistas_Sfx, pistas_Musica;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(gameObject);

        // Ensure walkingAudioSource doesn't play on awake
        if (walkingAudioSource != null)
        {
            walkingAudioSource.playOnAwake = false;
            walkingAudioSource.Stop();  // Stop any auto-play
        }
    }

    public void PlaySound(int index, float delay = 0f)
    {
        // Validar que el índice esté dentro del rango del array
        if (pistas_Sfx == null || index < 0 || index >= pistas_Sfx.Length)
        {
            Debug.LogWarning($"AudioManager: Índice de SFX fuera de rango. Índice: {index}, Array length: {(pistas_Sfx?.Length ?? 0)}");
            return;
        }

        // Validar que el clip no sea null
        if (pistas_Sfx[index] == null)
        {
            Debug.LogWarning($"AudioManager: El clip de SFX en el índice {index} es null");
            return;
        }

        audioS.clip = pistas_Sfx[index];
        audioS.loop = false;
        if (delay == 0f)
            audioS.Play();
        else
        {
            audioS.time = delay;
            audioS.Play();
        }
    }

    public void PlayDesbloquearNPC()
    {
        PlaySound(0);
    }
    public void PlayCollectable()
    {
        PlaySound(1);
    }

    public void PlayDead()
    {
        PlaySound(2);
    }

    public void PlayClick()
    {
        PlaySound(3);
    }


    // Play walking sound (one-shot)
    // Play walking sound (looping)
    // Play walking sound (looping with reduced delay)
    public void PlayWalk()
    {
        Debug.Log("AudioManager: PlayWalk() called");  // Debug: Check if this appears
        if (walkingAudioSource != null && walkingAudioSource.clip != null)
        {
            Debug.Log("AudioManager: Playing walking sound (looping)");  // Debug: Check if this appears
            walkingAudioSource.loop = true;  // Enable looping
            walkingAudioSource.time = 0f;  // Reset to start of clip for instant play
            walkingAudioSource.Play();  // Start looping immediately
        }
        else
        {
            Debug.LogWarning("AudioManager: walkingAudioSource or clip is null");  // Debug: Check for null issues
        }
    }
    // Stop the walking sound
    public void StopWalk()
    {
        Debug.Log("AudioManager: StopWalk() called");  // Debug: Check if this appears
        if (walkingAudioSource != null && walkingAudioSource.isPlaying)
        {
            Debug.Log("AudioManager: Stopping walking sound");  // Debug: Check if this appears
            walkingAudioSource.Stop();
        }
        else
        {
            Debug.LogWarning("AudioManager: walkingAudioSource is null or not playing");  // Debug: Check for null issues
        }
    }

    public void PlayMusic(int index)
    {
        // Validar que el índice esté dentro del rango del array
        if (pistas_Musica == null || index < 0 || index >= pistas_Musica.Length)
        {
            Debug.LogError($"AudioManager: Índice de música fuera de rango. Índice: {index}, Array length: {(pistas_Musica?.Length ?? 0)}");
            return;
        }

        // Validar que el clip no sea null
        if (pistas_Musica[index] == null)
        {
            Debug.LogWarning($"AudioManager: El clip de música en el índice {index} es null");
            return;
        }

        audioM.clip = pistas_Musica[index];
        audioM.loop = true;
        audioM.Play();
    }

    public void OnMusicValueChange(float value)
    {
        audioM.volume = value;
    }

    public void OnSfxValueChange(float value)
    {
        audioS.volume = value;
    }

    /// <summary>
    /// Detiene la música actual
    /// </summary>
    public void StopMusic()
    {
        if (audioM != null && audioM.isPlaying)
        {
            audioM.Stop();
        }
    }

    /// <summary>
    /// Pausa/reanuda la música
    /// </summary>
    public void ToggleMusicPause()
    {
        if (audioM != null)
        {
            if (audioM.isPlaying)
                audioM.Pause();
            else
                audioM.UnPause();
        }
    }

    /// <summary>
    /// Verifica si un índice de música es válido
    /// </summary>
    public bool IsValidMusicIndex(int index)
    {
        return pistas_Musica != null && index >= 0 && index < pistas_Musica.Length && pistas_Musica[index] != null;
    }

    /// <summary>
    /// Verifica si un índice de SFX es válido
    /// </summary>
    public bool IsValidSfxIndex(int index)
    {
        return pistas_Sfx != null && index >= 0 && index < pistas_Sfx.Length && pistas_Sfx[index] != null;
    }

    /// <summary>
    /// Intenta reproducir música de forma segura
    /// </summary>
    public bool TryPlayMusic(int index)
    {
        if (IsValidMusicIndex(index))
        {
            PlayMusic(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Intenta reproducir SFX de forma segura
    /// </summary>
    public bool TryPlaySound(int index, float delay = 0f)
    {
        if (IsValidSfxIndex(index))
        {
            PlaySound(index, delay);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Información de debug del AudioManager
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogAudioInfo()
    {
        Debug.Log($"AudioManager Info:");
        Debug.Log($"- SFX Array Length: {(pistas_Sfx?.Length ?? 0)}");
        Debug.Log($"- Music Array Length: {(pistas_Musica?.Length ?? 0)}");
        Debug.Log($"- Current Music: {(audioM?.clip?.name ?? "None")}");
        Debug.Log($"- Music Volume: {audioM?.volume ?? 0f}");
        Debug.Log($"- SFX Volume: {audioS?.volume ?? 0f}");
    }
}

