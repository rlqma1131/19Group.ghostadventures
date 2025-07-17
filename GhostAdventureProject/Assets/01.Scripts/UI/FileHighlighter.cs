using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FileHighlighter : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject; // File2Highlight 같은 오브젝트
    [SerializeField] private ClueData fileClue;
    [SerializeField] private CinemachineVirtualCamera ZoomCamera;
    private UIManager uimanager;

    void Start()
    {
        uimanager = UIManager.Instance;
    }

    private void OnMouseEnter()
    {
        highlightObject.SetActive(true);
    }

    private void OnMouseExit()
    {
        if(highlightObject != null)
        {
            highlightObject.SetActive(false);
            return;
        }
    }

    private void OnMouseDown()
    {
        uimanager.Inventory_PlayerUI.AddClue(fileClue);
        uimanager.InventoryExpandViewerUI.ShowClue(fileClue);
        uimanager.InventoryExpandViewerUI.HideClue(() =>
        {
            UIManager.Instance.PromptUI.ShowPrompt("단서를 획득했습니다. 인벤토리를 확인하세요", 2f);
            StartCoroutine(ResetCamera());
        });
    }

    IEnumerator ResetCamera()
    {
        yield return new WaitForSeconds(2f);
        ZoomCamera.Priority = 5;
        Destroy(highlightObject);
    }
}
