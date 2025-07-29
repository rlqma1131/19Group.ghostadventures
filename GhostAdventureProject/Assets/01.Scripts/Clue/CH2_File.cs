using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class CH2_File : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject; // File2 Highlight 오브젝트 
    [SerializeField] private GameObject zoomSafeBox; // Zoom화면 끝날 시 Zoom오브젝트 삭제
    [SerializeField] private ClueData fileClue; // 파일단서
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
        uimanager.InventoryExpandViewerUI.OnClueHidden += CloseViewer;
    }

    private void CloseViewer()
    {
        StartCoroutine(ResetCamera());
    }
    
    IEnumerator ResetCamera()
    {
        yield return new WaitForSeconds(1f);
        ZoomCamera.Priority = 5;
        UIManager.Instance.PromptUI.ShowPrompt("이건 힌트 같은데...", 2f);
        PuzzleStateManager.Instance.MarkPuzzleSolved("금고");
        yield return new WaitForSeconds(2f);
        uimanager.InventoryExpandViewerUI.OnClueHidden -= CloseViewer;
        Destroy(zoomSafeBox);
    }
}
