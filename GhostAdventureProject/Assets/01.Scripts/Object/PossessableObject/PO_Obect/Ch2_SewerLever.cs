using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SewerLever : BasePossessable
{
    [SerializeField] private GameObject mazeGroupToDisable;
    [SerializeField] private GameObject q_Key;

    protected override void Update()
    {
        base.Update();

        if (!isPossessed || !hasActivated)
        {
            q_Key.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Ch2_SewerPuzzleManager.Instance.SetPuzzleSolved();
            mazeGroupToDisable.SetActive(false); // 미로와 관련 요소 전부 off
            Unpossess();
            q_Key.SetActive(false);
        }
        
        q_Key.SetActive(true);
    }
}
