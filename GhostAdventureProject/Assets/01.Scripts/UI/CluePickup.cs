using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 단서를 가지고 있는 오브젝트에 붙이는 스크립트입니다.
// 오브젝트에 컴포넌트로 추가하고 clueData에 ClueData(SO)를 넣으세요.
// 플레이어가 단서를 획득해야 할 때 PickupClue()를 호출하세요.
public class CluePickup : MonoBehaviour
{
    [Header("단서 ScriptableObject를 넣어주세요")]
    public ClueData clueData;

    private AudioClip pickSFX;

    private void Start()
    {
        pickSFX = SoundManager.Instance.cluePickUP;
    }

    void OnMouseEnter()
    {
        UIManager.Instance.SetCursor(UIManager.CursorType.FindClue);
    }
    void OnMouseExit()
    {
        UIManager.Instance.SetCursor(UIManager.CursorType.Default);
    }
    
    public void PickupClue()
    {
        SoundManager.Instance.PlaySFX(pickSFX);
        UIManager.Instance.Inventory_PlayerUI.AddClue(clueData);
        // Destroy(gameObject); // 단서 오브젝트 제거
        UIManager.Instance.Inventory_PlayerUI.RefreshUI(); // UI에 반영
    }
}
