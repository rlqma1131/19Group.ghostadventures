using UnityEngine;
using DG.Tweening;

public class DoorLock : MonoBehaviour
{
    [SerializeField] private ItemData wantItem;
    private bool doorOpen = false;
    [SerializeField] private GameObject q_Key;

    void Start()
    {
    }
    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Q))
        {   
            Inventory_PossessableObject inventory = Inventory_PossessableObject.Instance;
            if(wantItem == inventory.selectedItem() && wantItem != null)
            {
                OpenDoorLock();
                doorOpen = true;
                q_Key.SetActive(false);
                inventory.TryUseSelectedItem();
                UIManager.Instance.PromptUI.ShowPrompt("문이 열렸습니다.", 1.5f);
                return;
            }
            UIManager.Instance.PromptUI.ShowPrompt("문을 열 수 없습니다", 1.5f);
        }
        if(doorOpen == false)
            q_Key.SetActive(true);
        
    }

    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     ItemData selectItem = collision.GetComponent<Inventory_PossessableObject>().selected;
    // }
    private void OpenDoorLock()
    {
        Debug.Log("문 열림");
    }
    // private void TriggerPlateEvent()
    // {
    //     Sequence shakeSeq = DOTween.Sequence();
    //     int shakeCount = 3;
    //     float startAngle = 5f;
    //     float durationPerShake = 0.05f;

    //     SoundManager.Instance.PlaySFX(isShaking);
    //     Debug.Log("Plate is shaking!");
    //     SoundTriggerer.TriggerSound(transform.position);

    //     for (int i = 0; i < shakeCount; i++)
    //     {
    //         float angle = Mathf.Lerp(startAngle, 0f, (float)i / shakeCount);
    //         shakeSeq.Append(transform.DOLocalRotate(new Vector3(0, 0, angle), durationPerShake))
    //                 .Append(transform.DOLocalRotate(new Vector3(0, 0, -angle), durationPerShake));
    //     }

    //     shakeSeq.Append(transform.DOLocalRotate(Vector3.zero, 0.03f));

    //     // 고양이는 눈 깜빡이기만
    //     cat.Blink();
    // }


}
