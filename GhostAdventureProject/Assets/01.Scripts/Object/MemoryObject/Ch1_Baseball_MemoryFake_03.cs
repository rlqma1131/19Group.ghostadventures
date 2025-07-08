using UnityEngine;

public class Ch1_Baseball_MemoryFake_03 : MemoryFragment
{
    [SerializeField] private GameObject alphabet_A;

    void Start()
    {
        isScannable = true;
        alphabet_A.SetActive(false);
    }

    public override void AfterScan()
    {
        isScannable = false;
        alphabet_A.SetActive(true);
        ChapterEndingManager.Instance.CollectCh1Clue("A");
    }
}
