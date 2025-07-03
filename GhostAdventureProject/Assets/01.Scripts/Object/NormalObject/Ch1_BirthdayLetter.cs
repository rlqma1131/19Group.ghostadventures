using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_BirthdayLetter : MonoBehaviour
{
    [SerializeField] private GameObject Letter;

    private bool zoomActivatedOnce = false;
    private bool isPlayerInside = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!Letter.activeSelf && !isPlayerInside)
                return;

            bool isZoomActive = !Letter.activeSelf;
            Letter.SetActive(isZoomActive);

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

            if (!Letter.activeSelf)
                PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            if (Letter.activeSelf)
                Letter.SetActive(false);

            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
