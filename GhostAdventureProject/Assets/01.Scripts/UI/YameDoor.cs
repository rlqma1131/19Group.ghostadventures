using UnityEngine;

public class YameDoor : MonoBehaviour
{
    [SerializeField] private CH2_SecurityGuard guard;
    [SerializeField] private GameObject e_key;
    [SerializeField] private GameObject targetDoor;
    [SerializeField] private GameObject closeSprite;
    [SerializeField] private GameObject player;

    private bool moveAble = false;

     void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        e_key.SetActive(false);
    }

    void Update()
    {
        if(moveAble && guard.isdoorLockOpen && !guard.isPossessed)
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