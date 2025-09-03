using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch1_MemoryFake_03_Baseball : MemoryFragment
{
    [SerializeField] private GameObject alphabet_A;

    public override void AfterScan()
    {
        alphabet_A.SetActive(true);

        ChapterEndingManager.Instance.CollectCh1Clue("A");
        SaveManager.MarkPuzzleSolved("A");
        UIManager.Instance.PromptUI.ShowPrompt("A");

        SaveManager.SaveWhenScanAfter(data.memoryID, data.memoryTitle,
            SceneManager.GetActiveScene().name,
            GameManager.Instance.Player.transform.position,
            checkpointId: data.memoryID,
            autosave: true);
    }

    public void ActivateBaseball()
    {
        isScannable = true;
    }

    protected override void PlusAction()
    {
        
    }
}
