using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch03_04TimeLineBGM : MonoBehaviour
{
    //3챕에서 4챕 이동중 타임라인 재생될때 BGM 재생 위한 스크립트


    AudioSource bgmAudioSource;

    void Start()
    {
        bgmAudioSource = GetComponent<AudioSource>();
        if (bgmAudioSource == null)
        {
            Debug.LogError("AudioSource component not found on this GameObject.");
            return;
        }
        // Play the BGM

    }

    public void PlayBGM()
    {
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }
    }


}
