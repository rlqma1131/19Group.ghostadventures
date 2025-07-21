using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class test_postit : MonoBehaviour
{
    [SerializeField] private ClueData file;
    UIManager uimanager;
    private void Start() 
    {
        uimanager = UIManager.Instance;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        uimanager.PromptUI.ShowPrompt("단서들을 조합해보자. 뭔가 찾을 수 있을지도 몰라", 2f);
        uimanager.Inventory_PlayerUI.RemoveClue(file);
    }
}
