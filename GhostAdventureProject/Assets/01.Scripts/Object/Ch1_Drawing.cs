using UnityEngine;

public class Ch1_Drawing : MonoBehaviour
{
    [SerializeField] private GameObject zoomCamera;

    private bool zoomActivatedOnce = false;

    void Update()
    {
        if (IsPlayerInRange() && Input.GetKeyDown(KeyCode.E))
        {
            zoomCamera.SetActive(!zoomCamera.activeSelf);

            if (!zoomCamera.activeSelf && !zoomActivatedOnce)
            {
                Ch1_HideAreaEvent.Instance.RestoreHideAreaTags();
                zoomActivatedOnce = true;
            }
        }
    }

    private bool IsPlayerInRange()
    {
        return PlayerInteractSystem.Instance.CurrentClosest == gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(!zoomCamera.activeSelf)
                PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 범위를 벗어나면 카메라 꺼짐
            if (zoomCamera.activeSelf)
                zoomCamera.SetActive(false);

           PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
