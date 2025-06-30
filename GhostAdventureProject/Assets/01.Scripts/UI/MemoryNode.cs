using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MemoryNode : MonoBehaviour
{
    // 컷씬 이미지를 가져와야 하는데 어디서 가져와야함?
[SerializeField] private Image icon;

    private MemoryData memory;

    public void Initialize(MemoryData memoryData)
    {
        memory = memoryData;

        switch (memory.type)
        {
            case MemoryData.MemoryType.Positive:
                icon.sprite = memory.PositiveFragmentSprite;
                break;
            case MemoryData.MemoryType.Negative:
                icon.sprite = memory.NegativeFragmentSprite;
                break;
            case MemoryData.MemoryType.Fake:
                icon.sprite = memory.FakeFragmentSprite;
                break;
        }
    }

}
