using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        if(fileClue == null) return;
        
        uimanager.Inventory_PlayerUI.AddClue(fileClue);
        uimanager.InventoryExpandViewerUI.ShowClue(fileClue);
        uimanager.InventoryExpandViewerUI.OnClueHidden += ResetCameraAsync;
    }

    private async void ResetCameraAsync()
    {
        await Task.Delay(1000); // 1초 대기
        UIManager.Instance.PromptUI.ShowPrompt("이건 힌트 같은데...", 2f);
        uimanager.InventoryExpandViewerUI.OnClueHidden -= ResetCameraAsync;
        ZoomCamera.Priority = 5;
        SaveManager.MarkPuzzleSolved("금고");
        await Task.Delay(2000); // 2초 대기
        zoomSafeBox.SetActive(false);
    }
}
