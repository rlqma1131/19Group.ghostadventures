using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ch3_Scanner : BasePossessable
{
    private MemoryStorage memoryStorage;
    private List<MemoryData> memories;

    protected override void Start()
    {
        base.Start();

        memoryStorage = UIManager.Instance.GetComponentInChildren<MemoryStorage>();
    }

    public override void OnPossessionEnterComplete() 
    {
        memories = memoryStorage.CollectedMemories;

        // UI 띄우기
    }
}

