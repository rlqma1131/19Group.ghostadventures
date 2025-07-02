using UnityEngine;

public class Ch1_Drawing : MonoBehaviour
{
    [SerializeField] private GameObject zoomCamera;

    //private bool zoom = false;
    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            zoomCamera.SetActive(!zoomCamera.activeSelf);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if(!zoomCamera.activeSelf)
                PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // 범위를 벗어나면 카메라 꺼짐
            if (zoomCamera.activeSelf)
                zoomCamera.SetActive(false);

           PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
