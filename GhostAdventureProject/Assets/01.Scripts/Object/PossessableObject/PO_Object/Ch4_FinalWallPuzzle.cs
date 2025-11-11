using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch4_FinalWallPuzzle : MonoBehaviour
{
    [Header("Optional SFX")]
    [SerializeField] AudioClip sfxReveal;
    [SerializeField] AudioClip sfxHide;

    bool isRevealed = false;

    // 처음엔 비활성화 상태로 씬에 둬야 함
    void Start()
    {
        if (!isRevealed)
            gameObject.SetActive(false);
    }

    public void ActivateHintReveal()
    {
        isRevealed = true;
        gameObject.SetActive(true);

        if (sfxReveal)
            SoundManager.Instance?.PlaySFX(sfxReveal);
    }
    
    public void HideHint()
    {
        isRevealed = false;

        // 끄기 연출 사운드
        if (sfxHide)
            SoundManager.Instance?.PlaySFX(sfxHide);

        gameObject.SetActive(false);
    }
}
