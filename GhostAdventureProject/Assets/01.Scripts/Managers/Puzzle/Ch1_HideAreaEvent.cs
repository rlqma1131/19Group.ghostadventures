using System.Collections.Generic;
using UnityEngine;

public class Ch1_HideAreaEvent : Singleton<Ch1_HideAreaEvent>
{
    [SerializeField] private List<string> correctOrder = new() { "A", "B", "C" };
    private List<string> currentOrder = new();
    private Ch1_Closet closet;

    private void Start()
    {
        closet = FindObjectOfType<Ch1_Closet>();
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
                closet.Unlock();
                Debug.Log("정답입니다!");
                // 정답 효과음
                // SoundManager.Instance.PlaySFX("Unlock");
            }
            else
            {
                currentOrder.Clear();
                // 틀림 효과음
                // SoundManager.Instance.PlaySFX("Wrong");
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
}
