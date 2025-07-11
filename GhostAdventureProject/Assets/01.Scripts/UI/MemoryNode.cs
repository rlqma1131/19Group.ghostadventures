using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class MemoryNode : MonoBehaviour
{
[SerializeField] private Image icon;
[SerializeField] private TextMeshProUGUI name;
private string sceneName;

    private MemoryData memory;

    public void Initialize(MemoryData memoryData)
    {
        memory = memoryData;
        icon.sprite = memory.MemoryCutSceneImage;
        name.text = memory.memoryTitle;
        sceneName = memory.CutSceneName;

    //     switch (memory.type)
    //     {
    //         case MemoryData.MemoryType.Positive:
    //             icon.sprite = memory.PositiveFragmentSprite;
    //             break;
    //         case MemoryData.MemoryType.Negative:
    //             icon.sprite = memory.NegativeFragmentSprite;
    //             break;
    //         case MemoryData.MemoryType.Fake:
    //             icon.sprite = memory.FakeFragmentSprite;
    //             break;
    //     }
    }

    public void GoToCutScene()
    {
        Debug.Log("씬 다시보기 버튼클릭");
        UIManager.Instance.PlayModeUI_CloseAll();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
    

}
