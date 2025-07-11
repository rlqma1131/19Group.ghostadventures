using DG.Tweening;
using UnityEngine;

public class Ch1_Pan : BasePossessable
{
    [SerializeField] private AudioClip isFall;

    [Header("위치 설정")]
    [SerializeField] private Vector3 startLocalPosition;
    [SerializeField] private Quaternion startLocalRotation = Quaternion.identity;
    [SerializeField] private float dropYPos = -1.5f;

    [SerializeField] private Ch1_Cat cat;
    [SerializeField] private Ch1_MemoryFake_02_Cake cake;
    [SerializeField] private Ch1_Mouse mouse;

    protected override void Start()
    {
        base.Start();
        // 시작 시 위치와 회전 적용
        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;
    }

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
        // 애니메이션 시퀀스 생성
        Sequence panSequence = DOTween.Sequence();

        // 1. 팬이 기울이며 아래로 떨어짐 (0.3초)
        panSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -60f), 0.5f).SetEase(Ease.InQuad));
        panSequence.Join(transform.DOLocalMoveY(dropYPos, 0.5f).SetEase(Ease.InQuad));

        // 2. 낙하 후 사운드 재생 및 AI 유인
        panSequence.AppendCallback(() =>
        {
            SoundManager.Instance.PlaySFX(isFall);

            // SoundTriggerObject 컴포넌트 찾아서 호출
            var soundTrigger = GetComponent<SoundTriggerObject>();
            if (soundTrigger != null)
            {
                soundTrigger.TriggerSound();
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] SoundTriggerObject 컴포넌트가 없습니다!");
            }
        });

        // 3. 회전 원래대로 복귀 (0.2초)
        panSequence.Append(transform.DOLocalRotateQuaternion(startLocalRotation, 0.2f).SetEase(Ease.OutBounce));

        // 4. 0.05초 후 관련 이벤트 실행
        panSequence.AppendInterval(0.05f);
        panSequence.AppendCallback(() =>
        {
            mouse.ActivateMouse();
            cat.ActivateCat();
            cake.ActivateCake();

            hasActivated = false;
            Unpossess();
        });
    }
}