using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SwitchboardButton : MonoBehaviour
{
    private int currentRotation = 0;
    private Ch2_SwitchboardPuzzleManager puzzleManager;

    public int id; // 0 ~ 5
    public Ch2_SwitchboardSlotConnectionData.SlotConnection connection;
    private void Start()
    {
        puzzleManager = GetComponentInParent<Ch2_SwitchboardPuzzleManager>();
    }

    private void OnMouseDown()
    {
        if (!puzzleManager.CanControl) return;

        // 클릭하면 버튼 회전
        RotateButton();
    }

    private void RotateButton()
    {
        currentRotation = (currentRotation + 90) % 360;
        transform.rotation = Quaternion.Euler(0, 0, -currentRotation);

        // 현재 이 버튼의 회전 상태가 맞는지
        connection = Ch2_SwitchboardSlotConnectionData.GetConnectionFor(id, currentRotation);

        // 전체 버튼 상태 확인
        puzzleManager.CheckSolution();
    }
}
