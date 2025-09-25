using UnityEditor.Rendering;
using UnityEngine;

public class Ch2_DoorLock : BaseInteractable
{   
    [SerializeField] private ItemData needItem;         // 문을 여는데 필요한 아이템
    [SerializeField] private GameObject q_Key;          // Q키
    [SerializeField] private LockedDoor officeDoor;     // 경비실 문
    [SerializeField] private CH2_SecurityGuard guard;   // 경비원
    private Inventory_PossessableObject inventory;      // 빙의 인벤토리(needItem을 갖고 있는지 확인용)

    private bool isDoorOpenAble = false;                // 문을 열 수 있는 영역에 있는지 확인
    public bool isDoorOpen = false;                     // 문을 열었는지 확인 // yamedoor의 islocked = false일 때로 변경하기

    protected override void Start() {
        base.Start();
        inventory = Inventory_PossessableObject.Instance;
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.Q)) {
            if(isDoorOpenAble && !isDoorOpen) {
                if(needItem != inventory.selectedItem()) {
                    UIManager.Instance.PromptUI.ShowPrompt("문을 열 수 있는 카드키가 필요해");
                    return;
                }
                else if(needItem == inventory.selectedItem()) {
                    OpenDoorLock();
                    return;
                }
                return;
            }
        }
    }

    // 도어락 풀리고 문 열림 -> 도어락 오브젝트 숨김
    private void OpenDoorLock() {
        isDoorOpen = true;
        officeDoor.SolvePuzzle();
        inventory.TryUseSelectedItem();
        gameObject.SetActive(false);
    }
    
    protected override void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player") || collision.CompareTag("Person") && guard.IsPossessed) {
            ShowHighlight(true);
            q_Key.SetActive(true);
            isDoorOpenAble = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        if(collision.CompareTag("Player") || collision.CompareTag("Person") && guard.IsPossessed) {
            ShowHighlight(false);
            q_Key.SetActive(false);
            isDoorOpenAble = false;    
        }
    }


}
