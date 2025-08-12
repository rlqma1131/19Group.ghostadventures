using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Background (옵션)")]
    [SerializeField] private Image background;                // 배경 색 Lerp용(없으면 비워도 됨)
    [SerializeField] private Color bgStart = Color.black;
    [SerializeField] private Color bgBase  = new Color(0.05f, 0.05f, 0.05f);

    [Header("Random Backgrounds & Tips")]
    [SerializeField] private Sprite[] backgroundSprites;  // 여러 장 등록(3~4장)
    [SerializeField] private bool preserveAspect = true;  // 원본 비율 유지 여부
    [SerializeField] private TextMeshProUGUI tipTextUI;   //팁 표시 Text
    [TextArea] [SerializeField] private string[] tips;    // 팁 문구 후보들
    
    [Header("Progress (Scrollbar만 사용)")]
    [SerializeField] private Scrollbar progressBar;           // 기본 Scrollbar

    [Header("Dot 이동 (옵션)")]
    [SerializeField] private RectTransform track; // 점 트랙
    [SerializeField] private RectTransform dot; // 하얀 점

    [Header("Possess Animation (좌하단)")]
    [SerializeField] private Animator possessAnim;            // 로딩씬 전용 오브젝트의 Animator
    public enum AnimStartMode { DefaultStateAutoPlay, Trigger, PlayByStateName }
    [SerializeField] private AnimStartMode animStartMode = AnimStartMode.DefaultStateAutoPlay;
    [SerializeField] private string triggerName = "Start";    // Trigger 모드일 때
    [SerializeField] private string stateName = "";           // State 이름 재생 모드일 때
    [SerializeField] private AnimationClip possessClip;       // 있으면 길이를 clip.length로 사용(권장)

    [Header("Timings")]
    [SerializeField] private float activationDelay = 0.3f;    // 로딩 끝난 뒤 약간의 여유
    
    [Header("Progress Smoothing")]
    [SerializeField] private bool smoothBar = true;          // 스무딩 on/off
    [SerializeField, Range(0.05f, 2f)] private float smoothTime = 0.35f; // 부드럽게 차오르는 시간상수
    [SerializeField, Range(0f, 5f)] private float minShowTime = 1.2f;    // 로딩 화면 최소 노출 시간(초)

    private float visualProgress;   // 화면에 보여줄 진행도
    private float visualVel;        // SmoothDamp용 내부 속도

    private void Start()
    {
        if (background && backgroundSprites != null && backgroundSprites.Length > 0)
        {
            int idx = Random.Range(0, backgroundSprites.Length);
            background.sprite = backgroundSprites[idx];
            background.preserveAspect = preserveAspect;
            background.enabled = true;
        }

        // 팁 랜덤 적용 (선택)
        if (tipTextUI && tips != null && tips.Length > 0)
        {
            tipTextUI.text = tips[Random.Range(0, tips.Length)];
        }
        
        // 다음 씬 이름이 없으면 안전 복귀
        if (string.IsNullOrEmpty(SceneLoadContext.RequestedNextScene))
        {
            Debug.LogError("[Loading] RequestedNextScene이 비어있음. GameManager.LoadThroughLoading(...)로 진입해야 합니다.");
            SceneManager.LoadScene("StartScene");
            return;
        }

        if (SceneLoadContext.RequestedBaseBgColor.HasValue)
            bgBase = SceneLoadContext.RequestedBaseBgColor.Value;

        if (background) background.color = bgStart;
        SetProgress01(0f);

        StartCoroutine(RunWithPossessThenLoad(SceneLoadContext.RequestedNextScene));

        // 1회성 값 정리
        SceneLoadContext.RequestedBaseBgColor = null;
    }

    private IEnumerator RunWithPossessThenLoad(string nextScene)
    {
        // 빙의 애니 먼저
        if (possessAnim != null)
        {
            switch (animStartMode)
            {
                case AnimStartMode.DefaultStateAutoPlay:
                    break; // Default 상태 자동 재생
                case AnimStartMode.Trigger:
                    if (possessAnim.HasParameterOfType(triggerName, AnimatorControllerParameterType.Trigger))
                    {
                        possessAnim.ResetTrigger(triggerName);
                        possessAnim.SetTrigger(triggerName);
                    }
                    else Debug.LogWarning($"[Loading] Trigger '{triggerName}' 파라미터 없음 → 기본 재생으로 진행");
                    break;
                case AnimStartMode.PlayByStateName:
                    if (!string.IsNullOrEmpty(stateName))
                        possessAnim.Play(stateName, 0, 0f);
                    else Debug.LogWarning("[Loading] stateName 비어있음 → 기본 재생으로 진행");
                    break;
            }

            // 전이 확정 한 프레임 대기
            yield return null;

            // 길이 계산 (clip 우선)
            float animLen = possessClip ? possessClip.length : possessAnim.GetCurrentAnimatorStateInfo(0).length;
            if (animLen < 0.01f) animLen = 0.3f; // 최소 보정
            yield return new WaitForSeconds(animLen);
        }
        else
        {
            Debug.LogWarning("[Loading] possessAnim 미지정 → 빙의 연출 스킵 후 즉시 로딩");
        }

        // 비동기 로딩 & 진행도 갱신
        yield return StartCoroutine(LoadRoutine(nextScene));
    }

    private IEnumerator LoadRoutine(string nextScene)
    {
        yield return null;

        var op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float elapsed = 0f;
        visualProgress = 0f;
        visualVel = 0f;

        while (!op.isDone)
        {
            elapsed += Time.unscaledDeltaTime;

            // Unity: 0~0.9까지 로딩 → 0.9에서 대기
            float target = (op.progress < 0.9f) ? (op.progress / 0.9f) : 1f;

            // 시각용 진행도 스무딩
            if (smoothBar)
                visualProgress = Mathf.SmoothDamp(
                    visualProgress, target, ref visualVel,
                    smoothTime, Mathf.Infinity, Time.unscaledDeltaTime
                );
            else
                visualProgress = target;

            SetProgress01(visualProgress);
            if (background) background.color = Color.Lerp(bgStart, bgBase, visualProgress);

            // 준비 완료: 최소 노출시간 경과 + 게이지가 거의 꽉 찼을 때만 활성화
            if (op.progress >= 0.9f && visualProgress >= 0.999f && elapsed >= minShowTime)
            {
                yield return new WaitForSecondsRealtime(activationDelay);
                SceneLoadContext.CameThroughLoading = true;
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
    
    private void SetProgress01(float t)
    {
        if (!progressBar) return;

        float s = Mathf.Clamp01(t);
        progressBar.size  = (s > 0f) ? s : 0.0001f; // 길이
        progressBar.value = 0f;         
    }
}

// Animator 파라미터 존재 확인 유틸
public static class AnimatorExt
{
    public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in self.parameters)
            if (p.type == type && p.name == name) return true;
        return false;
    }
}
