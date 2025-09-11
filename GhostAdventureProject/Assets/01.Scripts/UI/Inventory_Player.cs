using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Inventory_Player : MonoBehaviour
{
    public List<ClueData> collectedClues = new List<ClueData>(); // 단서데이터를 모아놓은 리스트
    
    [Header("Slots")]
    public List<InventorySlot_Player> frontSlots; // 앞면 4칸(아래줄)
    public List<InventorySlot_Player> backSlots;  // 뒷면 4칸(윗줄)

    [Header("Row Containers")]
    [SerializeField] private RectTransform frontRow; // 앞줄(아래)
    [SerializeField] private RectTransform backRow;  // 뒷줄(위)

    [Header("Toggle Anim")]
    [SerializeField] private float flipDur = 0.25f;   // 반쪽 회전 시간
    [SerializeField] private float gapDur  = 0.05f;   // 살짝 끊어가는 연출
    [SerializeField] private Ease flipEase = Ease.InOutSine;
    private bool isAnimating = false;

    private const int cluesPerFace = 4;
    private bool frontShowsFirstPage = true; 
    [SerializeField] KeyCode[] frontKeys = 
        { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };


    void Start()
    {
        frontRow.localScale = Vector3.one; 
        backRow.localScale  = Vector3.one * 0.8f;;
    }

    public void AddClue(ClueData clue)
    {
        if(clue == null) return;
        // 그림일기라면 기존 것 교체
        if (clue.clueType == ClueType.DrawingClue)
        {
            for (int i = 0; i < collectedClues.Count; i++)
            {
                if (collectedClues[i].clueType == ClueType.DrawingClue)
                {
                    collectedClues[i] = clue;
                    RefreshUI();
                    return;
                }
            }

            // 기존 그림일기 없으면 추가
            collectedClues.Add(clue);
            RefreshUI();
            return;
        }
        
        if (!collectedClues.Contains(clue)) //같은 clue 중복획득 방지 
        {
            collectedClues.Add(clue);
            //UI 갱신 이벤트 호출
            RefreshUI();
        }
    }
    
    public void RemoveCluesByStage(ClueStage stage)
    {
        collectedClues.RemoveAll(clue => clue.clue_Stage == stage);
        RefreshUI();
    }

    public void RemoveClueBeforeStage()
    {
        if(collectedClues != null)
        {
            collectedClues.Clear();
            RefreshUI();
        }
    }

    // 단서 삭제
    public void RemoveClue(ClueData clue)
    {
        if (collectedClues.Contains(clue))
        {
            collectedClues.Remove(clue);
            RefreshUI();
        }
        else
        {
            UIManager.Instance.PromptUI.ShowPrompt("이 단서가 아니야", 2f);
        }
    }
    public void RemoveClue(ClueData[] clues)
    {
        foreach(ClueData clue in clues)
        {
            if(collectedClues.Contains(clue))
            {
                collectedClues.Remove(clue);
            }
        }
        RefreshUI();
    }

    // 플레이어인벤토리UI 표시
     public void RefreshUI()
    {
        // 앞/뒤 시작 인덱스 결정
        int frontStart = frontShowsFirstPage ? 0 : cluesPerFace;
        int backStart  = frontShowsFirstPage ? cluesPerFace : 0; 

        // 앞면 세팅: 번호/키 표시 ON
        BindSlots(frontSlots, frontStart, showKey:true, dim:false);

        // 뒷면 세팅: 번호/키 표시 OFF
        BindSlots(backSlots,  backStart,  showKey:false, dim:true);

        SetPlayerKeyLabelsVisible(InventoryInputToggle.FocusIsPlayer);
    }

    // 슬롯에 숫자키(텍스트) 표시
    private void BindSlots(List<InventorySlot_Player> slots, int startIndex, bool showKey, bool dim)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int clueIndex = startIndex + i;
            var slot = slots[i];
            // slot.SetKeyVisible(showKey && InventoryInputToggle.FocusIsPlayer);
            slot.SetDim(dim);

            if (clueIndex < collectedClues.Count)
                slot.Setup(collectedClues[clueIndex]);
            else
                slot.Clear();
        }
    }

    // 버튼에 연결되어있음. 앞면/뒷면 바꾸기
    public void ToggleFace()
    {
        if(isAnimating) return;
        StartCoroutine(ToggleFaceFlipAnim());
    }
    
    // Toggle 애니메이션
    private IEnumerator ToggleFaceFlipAnim()
    {
        if (isAnimating) yield break;
        isAnimating = true;

        Vector3 smallScale  = Vector3.one * 0.8f;
        Vector3 normalScale = Vector3.one;

        // 스왑 "이전" 상태 저장
        bool wasFrontShowingFirst = frontShowsFirstPage; 
        // wasFrontShowingFirst == true 면 현재 frontRow가 앞(크고), backRow가 뒤(작음)

        // 1) 0 → 90도 (스케일은 건드리지 않음)
        Sequence s1 = DOTween.Sequence();
        s1.Join(frontRow.DOLocalRotate(new Vector3(90, 0, 0), flipDur).SetEase(flipEase))
        .Join(backRow.DOLocalRotate(new Vector3(90, 0, 0), flipDur).SetEase(flipEase));
        yield return s1.WaitForCompletion();

        yield return new WaitForSeconds(gapDur);

        // 2) 데이터 스왑 + UI 갱신
        frontShowsFirstPage = !frontShowsFirstPage;
        RefreshUI();

        // 3) 90 → 0도 (여기서도 스케일은 추가 변경 없음)
        Sequence s2 = DOTween.Sequence();
        s2.Join(frontRow.DOLocalRotate(Vector3.zero, flipDur).SetEase(flipEase))
        .Join(backRow.DOLocalRotate(Vector3.zero, flipDur).SetEase(flipEase));
        yield return s2.WaitForCompletion();

        isAnimating = false;
    }

    // 키 입력 받고 해당 단서 확대
    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            var alpha = frontKeys[i];
            var keypad = KeyCode.Keypad1 + i;
            if (Input.GetKeyDown(alpha) || Input.GetKeyDown(keypad))
            {
                if(!InventoryInputToggle.FocusIsPlayer) return;
                int frontStart = frontShowsFirstPage ? 0 : cluesPerFace;
                int clueIndex = frontStart + i;

                if (UIManager.Instance.InventoryExpandViewerUI.IsShowing())
                    UIManager.Instance.InventoryExpandViewerUI.HideClue();
                else if (clueIndex < collectedClues.Count)
                    UIManager.Instance.InventoryExpandViewerUI.ShowClue(collectedClues[clueIndex]);
            }
        }
    }

    public void SetPlayerKeyLabelsVisible(bool on)
    {
        if (frontSlots != null)
            foreach (var s in frontSlots) s.SetKeyVisible(on);

        if (backSlots != null)
            foreach (var s in backSlots) s.SetKeyVisible(false);
    }

    
}


