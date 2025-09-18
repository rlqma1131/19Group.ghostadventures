using UnityEngine;

// 경비실 문에 붙이는 스크립트입니다.
// 빙의 상태에서 문에 닿았을 때 E키를 누르면 빙의해제 되지 않고 다른문으로 이동할 수 있게 됩니다.
public class Ch2_YameDoor : MonoBehaviour
{
    [SerializeField] private CH2_SecurityGuard guard;
    [SerializeField] private GameObject e_key;
    [SerializeField] private GameObject targetDoor;
    [SerializeField] private GameObject player;

    private bool moveAble = false;

     void Start()
    {
        player = GameManager.Instance.PlayerController.gameObject;
        e_key.SetActive(false);
    }

    void Update()
    {
        if(moveAble && guard.isdoorLockOpen && !guard.IsPossessed)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                Vector3 targetDoorPos = player.transform.position;
                targetDoorPos.x = targetDoor.transform.position.x;   
                player.transform.position = targetDoorPos;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (other.CompareTag("Person") || other.CompareTag("Player"))
        {
            guard.doorPass = true;
            moveAble = true;
            if(guard.isdoorLockOpen)
            {
                e_key.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            guard.doorPass = false;
            moveAble = false;
            if(guard.isdoorLockOpen)
            {
                e_key.SetActive(false);
            }

        }
    }
}