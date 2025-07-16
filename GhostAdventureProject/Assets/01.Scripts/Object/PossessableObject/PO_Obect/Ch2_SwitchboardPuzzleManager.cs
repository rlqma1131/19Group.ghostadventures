using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SwitchboardPuzzleManager : MonoBehaviour
{
    public Ch2_SwitchboardButton[] pieces; // 0 ~ 5

    public bool CanControl { get; private set; } = false;

    public void EnablePuzzleControl()
    {
        CanControl = true;
    }

    public void DisablePuzzleControl()
    {
        CanControl = false;
    }

    public void CheckSolution()
    {
        if (Ch2_SwichboardPuzzleSolver.IsPathConnected(pieces))
        {
            Debug.Log("퍼즐 해결됨!");
            DisablePuzzleControl();

            // 이펙트나 사운드, 후속 로직 실행
        }
    }
}
