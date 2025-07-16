using UnityEngine;
using DG.Tweening;
using System.Collections;

public class TVShutdownEffect : MonoBehaviour
{
    [Header("오브젝트 설정")]
    [SerializeField] private SpriteRenderer tvScreen;

    [Header("속도 설정 (값이 작을수록 빨라짐)")]
    [SerializeField] private float turnOnDuration = 1.1f;

    [Tooltip("TV가 켜지기 전, 0초부터 이 시간(초)까지 무작위로 기다립니다.")]
    //[SerializeField] private float randomTime = 3f;
    [SerializeField] private float waitTime;
    private Vector3 originalScale;

    public AudioSource CCTVSound;

    private void Awake()
    {
        if (tvScreen != null)
        {
            originalScale = tvScreen.transform.localScale;

            // 초기에는 완전히 꺼진 상태(스케일 0, 알파 0)로 시작
            tvScreen.transform.localScale = Vector3.zero;
            tvScreen.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    /// <summary>
    /// [추가된 부분] 게임이 시작되면 자동으로 랜덤 딜레이 코루틴을 실행합니다.
    /// </summary>
    private void Start()
    {
        StartCoroutine(RandomTurnOnCoroutine());
    }

    /// <summary>
    /// [추가된 부분] 랜덤 시간만큼 기다린 후, TV 켜는 함수를 호출하는 코루틴입니다.
    /// </summary>
    private IEnumerator RandomTurnOnCoroutine()
    {
        // 1. 0초부터 randomTime(3초) 사이의 무작위 대기 시간 생성
        //float waitTime = Random.Range(0f, randomTime);

        // 2. 생성된 시간만큼 대기
        yield return new WaitForSeconds(waitTime);

        // 3. 대기 후 TV 켜는 애니메이션 실행
        PlayTurnOn();
    }

    /// <summary>
    /// TV가 켜지는 효과를 연출합니다.
    /// </summary>
    [ContextMenu("Start TurnOn")]
    public void PlayTurnOn()
    {
        // 이전 애니메이션이 실행 중이라면 중지
        DOTween.Kill(this);
        if (tvScreen == null) return;

        tvScreen.enabled = true;

        // 애니메이션 시작 전 초기 상태 설정: 투명하고 스케일이 0
        tvScreen.color = new Color(1f, 1f, 1f, 0f);
        tvScreen.transform.localScale = Vector3.zero;

        Sequence turnOnSequence = DOTween.Sequence().SetId(this);
        CCTVSound.PlayOneShot(CCTVSound.clip);
        // 1단계: 중앙에 밝은 점이 나타나면서 가로로 빠르게 확장 (가는 선 생성)
        turnOnSequence.Append(tvScreen.transform.DOScaleX(originalScale.x, turnOnDuration * 0.3f).SetEase(Ease.OutExpo));
        // 동시에 선이 나타날 때 밝아짐
        turnOnSequence.Join(tvScreen.DOFade(1f, turnOnDuration * 0.2f));

        // 2단계: 생성된 선이 세로로 확장되면서 전체 화면을 채움 (통통 튀는 효과)
        turnOnSequence.Append(tvScreen.transform.DOScaleY(originalScale.y, turnOnDuration * 0.7f).SetEase(Ease.OutBounce));
    }
}