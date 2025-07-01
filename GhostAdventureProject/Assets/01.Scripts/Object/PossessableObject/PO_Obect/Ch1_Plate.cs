using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Plate : BasePossessable
{
    [SerializeField] private AudioClip isShaking;

    private Ch1_Cat cat;

    protected override void Start()
    {
        base.Start();
        cat = FindObjectOfType<Ch1_Cat>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TriggerPlateEvent();
        }
    }

    private void TriggerPlateEvent()
    {
        Sequence shakeSeq = DOTween.Sequence();
        int shakeCount = 3;
        float startAngle = 5f;
        float durationPerShake = 0.05f;

        SoundManager.Instance.PlaySFX(isShaking);
        Debug.Log("Plate is shaking!");

        for (int i = 0; i < shakeCount; i++)
        {
            float angle = Mathf.Lerp(startAngle, 0f, (float)i / shakeCount);
            shakeSeq.Append(transform.DOLocalRotate(new Vector3(0, 0, angle), durationPerShake))
                    .Append(transform.DOLocalRotate(new Vector3(0, 0, -angle), durationPerShake));
        }

        shakeSeq.Append(transform.DOLocalRotate(Vector3.zero, 0.03f));
        shakeSeq.OnComplete(() => hasActivated = false);

        // 고양이는 눈 깜빡이기만
        cat.Blink();
    }
}