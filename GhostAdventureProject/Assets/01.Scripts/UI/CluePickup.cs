using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CluePickup : MonoBehaviour
{
    [Header("단서 ScriptableObject를 넣어주세요")]
    public ClueData clueData;

    private AudioClip pickSFX;

    private void Awake()
    {
        pickSFX = SoundManager.Instance.cluePickUP;
    }

    void OnMouseEnter()
    {
        UIManager.Instance.FindClueCursor();
    }
    void OnMouseExit()
    {
        UIManager.Instance.SetDefaultCursor();
    }
    
    public void PickupClue()
    {
        SoundManager.Instance.PlaySFX(pickSFX);
        UIManager.Instance.Inventory_PlayerUI.AddClue(clueData);
        // Destroy(gameObject); // 단서 오브젝트 제거
        UIManager.Instance.Inventory_PlayerUI.RefreshUI(); // UI에 반영
    }
}
