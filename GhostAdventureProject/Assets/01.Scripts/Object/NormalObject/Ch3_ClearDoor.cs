using UnityEngine;

public class Ch3_ClearDoor : MonoBehaviour
{
    public GameObject openDoor;

    bool isPlayerNear = false;
    bool isOpen = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && isPlayerNear && isOpen)
        {
            // 컷씬 연결해주세여 혁준상
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
        openDoor.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
