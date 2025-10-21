using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
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
    }

    public void PlaySound(int index, float delay = 0f)
    {
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

    public void PlayJump()
    {
        PlaySound(0);
    }
    public void PlayHurt()
    {
        PlaySound(1);
    }
    public void PlayCollectable()
    {
        PlaySound(2);
    }

    public void PlayDead()
    {
        PlaySound(3);
    }

    public void PlayClick()
    {
        PlaySound(4);
    }

    public void PlayPowerTechGirl()
    {
        PlaySound(5);
    }

    public void PlayPowerArtGirl()
    {
        PlaySound(6);
    }
    public void PlayWin()
    {
        PlaySound(7);
    }

    public void PlayMusic(int index)
    {
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
}
