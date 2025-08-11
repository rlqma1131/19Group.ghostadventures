using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Background (옵션)")]
    [SerializeField] private Image background;                // 배경 색 Lerp용(없으면 비워도 됨)
    [SerializeField] private Color bgStart = Color.black;
    [SerializeField] private Color bgBase  = new Color(0.05f, 0.05f, 0.05f);

    [Header("Progress (Scrollbar만 사용)")]
    [SerializeField] private Scrollbar progressBar;           // 기본 Scrollbar

    [Header("Dot 이동 (옵션)")]
    [SerializeField] private RectTransform track;             // 점 트랙
    [SerializeField] private RectTransform dot;               // 하얀 점

    [Header("Possess Animation (좌하단)")]
    [SerializeField] private Animator possessAnim;            // 로딩씬 전용 오브젝트의 Animator
    public enum AnimStartMode { DefaultStateAutoPlay, Trigger, PlayByStateName }
    [SerializeField] private AnimStartMode animStartMode = AnimStartMode.DefaultStateAutoPlay;
    [SerializeField] private string triggerName = "Start";    // Trigger 모드일 때
    [SerializeField] private string stateName = "";           // State 이름 재생 모드일 때
    [SerializeField] private AnimationClip possessClip;       // 있으면 길이를 clip.length로 사용(권장)

    [Header("Timings")]
    [SerializeField] private float activationDelay = 0.3f;    // 로딩 끝난 뒤 약간의 여유

    private void Start()
    {
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

        while (!op.isDone)
        {
            float p = Mathf.Clamp01(op.progress / 0.9f); // 0~1 정규화
            SetProgress01(p);

            if (background) background.color = Color.Lerp(bgStart, bgBase, p);

            if (op.progress >= 0.9f)
            {
                yield return new WaitForSecondsRealtime(activationDelay);
                // ▶ 다음 씬이 "로딩씬을 거쳤다"는 표시
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
