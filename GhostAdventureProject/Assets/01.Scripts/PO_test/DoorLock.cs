using UnityEngine;

public class DoorLock : MonoBehaviour
{   
    [SerializeField] private LockedDoor door; // 문
    [SerializeField] private ItemData needItem; // 문을 여는데 필요한 아이템
    [SerializeField] private GameObject q_Key;
    private bool doorOpenAble; // 문을 열 수 있는 영역에 있는지 확인
    private bool doorOpen; // 문을 열었는지 확인
    Inventory_PossessableObject inventory; // 빙의 인벤토리(needItem을 갖고 있는지 확인용)

    void Start()
    {
        doorOpenAble = false;
        doorOpen = false;
        inventory = Inventory_PossessableObject.Instance;
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Q))
        {   
            if(needItem == inventory.selectedItem() && needItem != null && doorOpenAble)
            {
                UIManager.Instance.PromptUI.ShowPrompt("문이 열렸습니다.", 1.5f);
                OpenDoorLock();
                return;
            }
            if(doorOpenAble && !doorOpen)
            {
                UIManager.Instance.PromptUI.ShowPrompt("문을 열 수 없습니다", 1.5f);
            }
            return;
        }

        if(!doorOpen)
            q_Key.SetActive(true);
    }

    // 도어락 풀리고 문 열림
    private void OpenDoorLock()
    {
        doorOpen = true;
        q_Key.SetActive(false);
        inventory.TryUseSelectedItem();   
        door.SolvePuzzle();
        Destroy(this.gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Person") || collision.CompareTag("Player"))
        doorOpenAble = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Person") || collision.CompareTag("Player"))
        doorOpenAble = false;    
    }


}
