using System.Collections.Generic;
using UnityEngine;

public class Ch1_HideAreaEventManager : MonoBehaviour
{
    // 싱글톤
    public static Ch1_HideAreaEventManager Instance { get; private set; }

    [SerializeField] private AudioClip UnlockCloset;

    [Header("퍼즐")]
    [SerializeField] private List<string> correctOrder = new() { "침대", "인형", "의자" };
    [SerializeField] private List<string> currentOrder = new();

    [Header("퍼즐 은신처 & 옷장")]
    [SerializeField] private HideAreaID[] hideAreas;
    [SerializeField] private Ch1_Closet closet;

    public bool Solved { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        // 퍼즐 풀기 전에는 퍼즐은신처 못쓰도록
        RemoveHideAreaComponent();
        Debug.Log("HideAreaEvent 초기화 완료");
    }

    public void RegisterArea(string id)
    {
        if (currentOrder.Count >= correctOrder.Count)
            return;

        currentOrder.Add(id);
        Debug.Log($"현재 순서: {string.Join(" ", currentOrder)}");

        if (currentOrder.Count == correctOrder.Count)
        {
            if (IsCorrectOrder())
            {
                Solved = true;

                // 기억조각 드러남
                closet.Unlock();

                // 옷장열리는 효과음
                 SoundManager.Instance.PlaySFX(UnlockCloset);

                // 플레이어 나타남
                PossessionSystem.Instance.CurrentTarget.Unpossess();

                // 퍼즐 풀면 아이방 HideArea에 못숨도록
                RemoveHideAreaComponent();

                PuzzleStateManager.Instance.MarkPuzzleSolved("그림일기");
                UIManager.Instance.PromptUI.ShowPrompt("열렸다… 안에 뭐가…");

                
            }
            else
            {
                currentOrder.Clear();
                // 틀림 메시지 출력
                UIManager.Instance.PromptUI.ShowPrompt_Random("다시… 처음부터 생각해보자.", "이게 아니야");
            }
        }
    }

    private bool IsCorrectOrder()
    {
        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (currentOrder[i] != correctOrder[i])
                return false;
        }
        return true;
    }

    public void RemoveHideAreaComponent()
    {
        foreach (var areaID in hideAreas)
        {
            if (areaID.CompareTag("HideArea"))
            {
                var hide = areaID.GetComponent<HideArea>();
                if (hide != null)
                {
                    Destroy(hide);
                }
            }
        }
    }

    // 그림 발견하고 부터 숨기 가능
    public void AddHideAreaComponent()
    {
        foreach (var areaID in hideAreas)
        {
            if (areaID.CompareTag("HideArea"))
            {
                areaID.gameObject.AddComponent<HideArea>();
                var hide = areaID.GetComponent<HideArea>();
                hide.highlight = areaID.highlight;
            }
        }
    }
}
