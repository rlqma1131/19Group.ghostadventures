using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class Ch2_File : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;    // File2 Highlight 오브젝트 
    [SerializeField] private ClueData fileClue;             // 파일단서
    private UIManager uimanager;
    private Ch2_SafeBox safeBox;          
    private bool isShowfile = false;

    void Start()
    {
        uimanager = UIManager.Instance;
        safeBox = GetComponentInParent<Ch2_SafeBox>();
    }

    private void OnMouseEnter(){
        if(!isShowfile)
            highlightObject.SetActive(true);
    }
    private void OnMouseExit(){
        if(!isShowfile)
            highlightObject.SetActive(false);
    }
    private async void OnMouseDown()
    {
        if(fileClue == null) return;
        if(isShowfile == true) return;
        if(highlightObject == null) return;
            
        highlightObject.SetActive(false);
        uimanager.Inventory_PlayerUI.AddClue(fileClue);

        await Task.Delay(200);
        uimanager.InventoryExpandViewerUI.ShowClue(fileClue);
        uimanager.InventoryExpandViewerUI.OnClueHidden += ResetCameraAsync;
        isShowfile = true;
    }

    private async void ResetCameraAsync()
    {
        await Task.Delay(1000); // 1초 대기
        UIManager.Instance.PromptUI.ShowPrompt("이건 힌트 같은데...", 2f);
        SaveManager.MarkPuzzleSolved("금고");
        safeBox.ResetCamera();
    }
}
