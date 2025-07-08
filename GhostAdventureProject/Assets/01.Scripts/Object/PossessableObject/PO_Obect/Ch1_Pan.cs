using DG.Tweening;
using UnityEngine;

public class Ch1_Pan : BasePossessable
{
    [SerializeField] private AudioClip isFall;

    private Ch1_Cat cat => FindObjectOfType<Ch1_Cat>();
    private Ch1_Cake_MemoryFake_02 cake => FindObjectOfType<Ch1_Cake_MemoryFake_02>();
    private Ch1_Mouse mouse => FindObjectOfType<Ch1_Mouse>();

    protected override void Update()
    {
        base.Update();

        if (!isPossessed || !hasActivated)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TriggerPanEvent();
        }
    }

    private void TriggerPanEvent()
    {
        // 초기값 저장
        Vector3 originalPos = transform.localPosition;
        Quaternion originalRot = transform.localRotation;

        // 애니메이션 시퀀스 생성
        Sequence panSequence = DOTween.Sequence();

        // 1. 팬이 기울이며 아래로 떨어짐 (0.3초 동시에 실행)
        panSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -60f), 0.3f).SetEase(Ease.InQuad));
        panSequence.Join(transform.DOLocalMoveY(originalPos.y - 1.5f, 0.3f).SetEase(Ease.InQuad));

        // 2. 0.3초가 끝난 후 → 사운드 재생
        panSequence.AppendCallback(() =>
        {
            SoundManager.Instance.PlaySFX(isFall);

            // 적 AI 유인 메서드
            SoundTriggerer.TriggerSound(transform.position);
        });

        // 3. 회전 원래대로 복귀 (0.2초)
        panSequence.Append(transform.DOLocalRotateQuaternion(originalRot, 0.2f).SetEase(Ease.OutBounce));

        // 4. 0.2초 대기 후 쥐 도망 애니메이션 실행
        panSequence.AppendInterval(0.05f);
        panSequence.AppendCallback(() =>
        {
            mouse.ActivateMouse();
            cat.ActivateCat();
            cake.ActivateCake();

            hasActivated = false; // 이벤트 완료 후 초기화
            Unpossess();
        });
    }


}
