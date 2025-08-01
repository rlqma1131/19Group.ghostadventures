using System.Collections.Generic;
using UnityEngine;

public class Ch3_Scanner : BasePossessable
{
    [SerializeField] private Ch3_MemoryPuzzleUI memoryPuzzleUI;

    private MemoryStorage memoryStorage;
    private List<MemoryData> memories;

    protected override void Start()
    {
        base.Start();

        memoryStorage = UIManager.Instance.GetComponentInChildren<MemoryStorage>();
    }
    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            memoryPuzzleUI.Close();
            UIManager.Instance.PlayModeUI_OpenAll();
            Unpossess();
        }
    }

    public override void OnPossessionEnterComplete() 
    {
        UIManager.Instance.PlayModeUI_CloseAll();
        // UI 띄우기
        memoryPuzzleUI.StartFlow(MemoryManager.Instance.GetCollectedMemories());
    }
}

