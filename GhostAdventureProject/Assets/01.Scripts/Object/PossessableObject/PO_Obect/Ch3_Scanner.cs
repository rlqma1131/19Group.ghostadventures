using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public override void OnPossessionEnterComplete() 
    {
        memoryPuzzleUI.StartFlow(MemoryManager.Instance.GetCollectedMemories());

        // UI 띄우기
    }
}

