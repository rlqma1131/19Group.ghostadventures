using UnityEngine;

public class Ch1_Drawing : MonoBehaviour
{
    [SerializeField] private GameObject zoomCamera;

    private bool zoomActivatedOnce = false;
    private bool isPlayerInside = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!zoomCamera.activeSelf && !isPlayerInside)
                return;

            bool isZoomActive = !zoomCamera.activeSelf;
            zoomCamera.SetActive(isZoomActive);

            if (!isZoomActive) // 줌 꺼짐
            {
                if (!zoomActivatedOnce)
                {
                    Ch1_HideAreaEvent.Instance.RestoreHideAreaTags();
                    zoomActivatedOnce = true;
                }

                if (isPlayerInside)
                    PlayerInteractSystem.Instance.AddInteractable(gameObject);
            }
            else // 줌 켜짐
            {
                PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (!zoomCamera.activeSelf)
                PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            if (zoomCamera.activeSelf)
                zoomCamera.SetActive(false);

            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
