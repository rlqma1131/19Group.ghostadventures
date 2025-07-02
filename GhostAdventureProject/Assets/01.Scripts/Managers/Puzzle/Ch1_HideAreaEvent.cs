using System.Collections.Generic;
using UnityEngine;

public class Ch1_HideAreaEvent : Singleton<Ch1_HideAreaEvent>
{
    [SerializeField] private AudioClip UnlockCloset;

    [SerializeField] private List<string> correctOrder = new() { "침대", "인형", "의자" };
    private List<string> currentOrder = new();

    private Ch1_Closet closet => FindObjectOfType<Ch1_Closet>();
    private PlayerHide PlayerHide => FindObjectOfType<PlayerHide>();

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
                closet.Unlock();
                // 옷장열리는 효과음
                 SoundManager.Instance.PlaySFX(UnlockCloset);
                PlayerHide.ShowPlayer();

                // 퍼즐 풀면 아이방 HideArea에 못숨도록
                UnTagAllHideAreas();
            }
            else
            {
                currentOrder.Clear();
                // 틀림 메시지 출력
                UIManager.Instance.PromptUI.ShowPrompt("...아무 일도 일어나지 않았다.", 2f);
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

    private void UnTagAllHideAreas()
    {
        HideAreaID[] areas = FindObjectsOfType<HideAreaID>();
        foreach (var area in areas)
        {
            if (area.CompareTag("HideArea"))
            {
                area.tag = "Untagged";
            }
        }
    }
}
