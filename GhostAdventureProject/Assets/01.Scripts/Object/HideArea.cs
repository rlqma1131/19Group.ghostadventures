using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideArea : BasePossessable
{
    public AudioClip hideAreaEnterSFX;

    public override void OnQTESuccess()
    {
        Debug.Log("QTE 성공 - 빙의 완료");

        // 은신 효과음 (바스락)
        //SoundManager.Instance.PlaySFX(hideAreaEnterSFX);

        PossessionStateManager.Instance.StartPossessionTransition();
    }
}
