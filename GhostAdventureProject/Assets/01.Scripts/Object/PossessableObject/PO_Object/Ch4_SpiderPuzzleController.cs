using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Object.MemoryObject;
using DG.Tweening;
using UnityEngine;

public class Ch4_SpiderPuzzleController : MonoBehaviour
{
    [Header("Patterns (Set in order)")]
    public Ch4_PatternAsset pattern1;
    public Ch4_PatternAsset pattern2;
    public Ch4_PatternAsset pattern3;

    [Header("References")]
    [SerializeField] Ch4_Spider spider;
    [SerializeField] Ch4_DollReveal dollA;
    [SerializeField] Ch4_DollReveal dollB;
    [SerializeField] Ch4_DollReveal dollC;

    // finalWall은 지금 방식에선 힌트 벽 오브젝트 대신 쓸 일 없지만
    // 기존 필드 유지 (null이어도 문제 없게 둠)
    [SerializeField] Ch4_FinalWallPuzzle finalWall;

    [Header("Reward Object (Reveal & Drop)")]
    [SerializeField] Ch4_MemoryObject_CassetteTape rewardObject;          // 씬에 미리 배치된 보상 오브젝트 (처음엔 SetActive(false))
    [SerializeField] Transform rewardGroundPoint;      // 최종 착지 지점
    [SerializeField] float rewardDropHeight = 1.5f;    // 위에서 얼마나 띄워서 등장시킬지
    [SerializeField] float rewardAppearDuration = 0.4f;// 알파 페이드 인 시간
    [SerializeField] float rewardDropDuration = 0.6f;  // 밑으로 내려오는 시간
    [SerializeField] Ease rewardDropEase = Ease.OutQuad;

    [Header("SFX")]
    public AudioClip sfxPatternSuccess;
    public AudioClip sfxPatternFail;
    public AudioClip sfxBump;
    public AudioClip sfxReset;

    [Header("Final Phase Settings")]
    [SerializeField] List<Ch4_PatternAsset> targetOrder = new List<Ch4_PatternAsset>();
    
    List<Ch4_PatternAsset> inputOrder = new List<Ch4_PatternAsset>();

    [SerializeField] AudioClip sfxFinalSuccess;
    [SerializeField] AudioClip sfxFinalFail;

    enum Step
    {
        None,
        Note1Ready,
        Spider1Done,
        Spider2Done,
        Spider3Done,
        FinalReady,
        Cleared
    }
    Step step = Step.None;

    // 과거 버전에서 쓰던 리스트(필수는 아님)
    readonly List<Ch4_PatternAsset> discoveredOrder = new();

    // ===== 시작 트리거: 쪽지 확인 =====
    public void OnFirstNoteRevealed(Ch4_PatternAsset p1)
    {
        if (step != Step.None) return;
        discoveredOrder.Clear();
        step = Step.Note1Ready;

        UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode(
            "거미로 쪽지 패턴을 따라 그려보자", 2f);
        
        if (spider != null)
        {
            spider.SetActivated(true);
        }
    }

    // ===== 거미에 빙의할 때, 지금 단계용 패턴 넣어주기 =====
    public void ApplyCurrentPatternToSpider(Ch4_Spider s)
    {
        if (!s) return;

        switch (step)
        {
            case Step.Note1Ready:
                s.SetPattern(pattern1);
                break;
            case Step.Spider1Done:
                s.SetPattern(pattern2);
                break;
            case Step.Spider2Done:
                s.SetPattern(pattern3);
                break;
            // FinalReady 이후엔 거미를 더이상 쓰지 않으므로 없음
        }
    }

    // ===== 거미 패턴 성공 보고 =====
    public void OnSpiderPatternSolved(Ch4_PatternAsset solved)
    {
        // 1라운드 클리어
        if (solved == pattern1 && step == Step.Note1Ready)
        {
            discoveredOrder.Add(pattern1);
            step = Step.Spider1Done;

            if (dollA)
            {
                // dollA.SetPattern(pattern2); // 다음 문양을 인형 A에 새겨서 내려오게
                dollA.Lower();
            }

            UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode(
                "인형 A에 새로운 문양!", 1.6f);
        }
        // 2라운드 클리어
        else if (solved == pattern2 && step == Step.Spider1Done)
        {
            discoveredOrder.Add(pattern2);
            step = Step.Spider2Done;

            if (dollB)
            {
                // dollB.SetPattern(pattern3);
                dollB.Lower();
            }

            UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode(
                "인형 B에 또 다른 문양!", 1.6f);
        }
        // 3라운드 클리어
        else if (solved == pattern3 && step == Step.Spider2Done)
        {
            discoveredOrder.Add(pattern3);
            step = Step.Spider3Done;

            if (dollC) dollC.Lower();

            BeginFinalPhase();
        }
    }
    
    public Ch4_PatternAsset GetNextPatternAfter(Ch4_PatternAsset solvedPattern)
    {
        // 패턴1 끝났을 때: 다음은 패턴2
        if (solvedPattern == pattern1 && step == Step.Spider1Done)
            return pattern2;

        // 패턴2 끝났을 때: 다음은 패턴3
        if (solvedPattern == pattern2 && step == Step.Spider2Done)
            return pattern3;

        // 패턴3 끝났으면 더 없음
        return null;
    }

    // ===== 최종 단계 시작 =====
    void BeginFinalPhase()
    {
        // 이제 인형 순서 입력 퍼즐 시작
        step = Step.FinalReady;
        inputOrder.Clear();

        // 1) 플레이어에게 안내
        UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode(
            "벽의 순서를 보고 인형을 순서대로 건드려라", 2f);

        // 2) 벽 힌트 처음 공개
        //    finalWall은 이전까지 비활성화였고,
        //    여기서 켜지면서 문양 순서(힌트)만 보여주고
        //    플레이어 입력은 받지 않도록 설정.
        if (finalWall)
        {
            finalWall.ActivateHintReveal();
        }
    }

    // ===== 인형을 눌렀을 때 (Ch4_DollReveal에서 호출) =====
    public void RegisterDollPress(Ch4_PatternAsset pressed)
    {
        if (step != Step.FinalReady) return;
        if (!pressed) return;

        inputOrder.Add(pressed);
        Debug.Log("[Controller] Doll pressed: " + pressed.name);

        // 다 눌렀으면(예: 3개) 판정
        if (inputOrder.Count >= targetOrder.Count)
        {
            CheckFinalOrder();
        }
    }

    // ===== 순서 판정 =====
    void CheckFinalOrder()
    {
        bool correct = ValidateFinalOrder(inputOrder);

        if (correct)
        {
            FinalSuccess();
        }
        else
        {
            FinalFail();
        }
    }

    void FinalSuccess()
    {
        Debug.Log("[Controller] Final Puzzle Success!");
        step = Step.Cleared;

        SoundManager.Instance?.PlaySFX(sfxFinalSuccess);
        UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode("정답입니다!", 1.5f);

        // 인형들 원위치로 (올라가면서 종료 연출)
        if (dollA) dollA.Raise();
        if (dollB) dollB.Raise();
        if (dollC) dollC.Raise();

        RevealAndDropReward();
    }

    void FinalFail()
    {
        Debug.Log("[Controller] Final Puzzle Failed!");
        SoundManager.Instance?.PlaySFX(sfxFinalFail);

        // 인형들 잠깐 올라갔다가 다시 내려오게 해서 리셋 연출
        if (dollA) dollA.Raise();
        if (dollB) dollB.Raise();
        if (dollC) dollC.Raise();

        StartCoroutine(RestartFinalPhase());
    }

    IEnumerator RestartFinalPhase()
    {
        yield return new WaitForSeconds(2f);

        inputOrder.Clear();

        if (dollA) dollA.Lower();
        if (dollB) dollB.Lower();
        if (dollC) dollC.Lower();

        // FinalReady 다시 활성화
        step = Step.FinalReady;

        // 다시 한번 벽 힌트 보고 입력하라는 메시지 띄워도 됨
        UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode(
            "다시 시도해라. 벽 순서를 잘 봐라.", 2f);

        // 벽 힌트는 계속 켜져 있으므로 굳이 다시 finalWall.ActivateHintReveal() 안 해도 됨
    }

    // ===== 전체 리셋용 (혹시 필요하면 사용) =====
    public void ResetAll()
    {
        step = Step.None;
        discoveredOrder.Clear();
        inputOrder.Clear();

        if (spider)
            spider.gameObject.SetActive(true);

        if (dollA) dollA.Raise();
        if (dollB) dollB.Raise();
        if (dollC) dollC.Raise();

        // 벽 힌트 숨기고 싶으면 여기서 finalWall.HideHint() 호출
        if (finalWall)
            finalWall.HideHint();

        UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode(
            "퍼즐이 리셋되었습니다.", 2.0f);

        SoundManager.Instance?.PlaySFX(sfxReset);
    }

    // ===== 순서 검증 로직 (인형 ←→ targetOrder 비교) =====
    public bool ValidateFinalOrder(List<Ch4_PatternAsset> attempt)
    {
        if (attempt == null) return false;
        if (attempt.Count != targetOrder.Count) return false;

        for (int i = 0; i < targetOrder.Count; i++)
        {
            if (attempt[i] != targetOrder[i])
                return false;
        }
        return true;
    }

    // FinalWallPuzzle이 내부에서 직접 호출할 수도 있어서 남겨둠
    public void OnFinalSolved()
    {
        FinalSuccess();
    }

    public void OnFinalFailed()
    {
        FinalFail();
    }
    
    void RevealAndDropReward()
    {
        if (!rewardObject || !rewardGroundPoint)
        {
            Debug.LogWarning("[Controller] Reward references are missing.");
            return;
        }

        // 1) 우선 보상 오브젝트를 켜고
        rewardObject.SetScannable(true);

        // 2) 시작 위치/시각 효과 준비
        //    - 최종 착지 지점에서 약간 위로 올려둔 위치에서 시작
        Vector3 startPos = rewardGroundPoint.position + Vector3.up * rewardDropHeight;
        Transform rewardTf = rewardObject.transform;
        rewardTf.position = startPos;

        //    - 알파 0으로 시작해서 서서히 나타나게 만들기 위해 SpriteRenderer(or CanvasGroup) 필요
        //      여기서는 SpriteRenderer 여러 개일 수 있으니까 전부 모아서 알파 0 -> 1로 Tween
        var sprites = rewardObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in sprites)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        // 3) 연출 시퀀스 (알파 페이드 인 + 아래로 착지)
        Sequence seq = DOTween.Sequence();

        // (a) 알파 페이드 인
        seq.AppendCallback(() =>
        {
            foreach (var sr in sprites)
            {
                sr.DOFade(1f, rewardAppearDuration).SetEase(Ease.Linear);
            }
        });

        // (b) 아래로 "툭" 내려오기
        seq.Append(rewardTf.DOMove(rewardGroundPoint.position, rewardDropDuration)
                           .SetEase(rewardDropEase));

        // 끝나면 그냥 멈춤. 필요하면 여기서 추가 사운드나 흔들림 가능.
    }
}
