using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_Memory_Dust : MemoryFragment
{
    [SerializeField] private GameObject letterE;
    public override void AfterScan()
    {
        letterE.SetActive(true);
        PuzzleStateManager.Instance.MarkPuzzleSolved("먼지");
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player") && !PuzzleStateManager.Instance.IsPuzzleSolved("먼지"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("여기… 무언가 남아 있어. 이게… 다음 힌트야.");
        }
    }
}
