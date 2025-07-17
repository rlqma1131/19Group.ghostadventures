using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Ch2_BookShelf : BasePossessable
{
    [SerializeField] private GameObject q_Key;
    
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    
    [SerializeField] private Ch2_BookSlot[] bookSlots;
    [SerializeField] private GameObject doorToOpen;

    private bool isControlMode = false;

    protected override void Update()
    {
        if (!isPossessed || !hasActivated)
        {
            q_Key.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isControlMode)
            {
                q_Key.SetActive(false);
                EnterControlMode();
            }
            else
            {
                q_Key.SetActive(true);
                ExitControlMode();
            }
        }

        if (!isControlMode && Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
        }
        q_Key.SetActive(true);
    }

    private void EnterControlMode()
    {
        isControlMode = true;
        zoomCamera.Priority = 20;
        UIManager.Instance.PlayModeUI_CloseAll();
        Debug.Log(" 책장 컨트롤 모드 진입");
    }

    private void ExitControlMode()
    {
        isControlMode = false;
        zoomCamera.Priority = 5;
        UIManager.Instance.PlayModeUI_OpenAll();
        Debug.Log(" 책장 컨트롤 모드 종료");
    }

    // 마우스 클릭 처리 (Raycast)
    private void LateUpdate()
    {
        if (!isControlMode || !Input.GetMouseButtonDown(0))
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Ch2_BookSlot slot = hit.collider.GetComponent<Ch2_BookSlot>();
            if (slot != null)
            {
                slot.ToggleBook();
                CheckPuzzleSolved();
            }
        }
    }

    private void CheckPuzzleSolved()
    {
        int count = 0;

        foreach (var slot in bookSlots)
        {
            if (slot.IsCorrectBook && slot.IsPushed)
                count++;
        }

        if (count == 5)
        {
            Debug.Log(" 정답 책 선택 완료");
            doorToOpen.SetActive(true);
            ExitControlMode();
            hasActivated = false;
        }
    }
}