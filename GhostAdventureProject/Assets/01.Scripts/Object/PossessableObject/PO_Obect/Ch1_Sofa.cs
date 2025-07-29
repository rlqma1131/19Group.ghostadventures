using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Ch1_Sofa : HideArea
{
    protected override void Update()
    {
        if (!isPossessed)
            return;

        InteractTutorial();

        if (Input.GetKeyDown(KeyCode.E))
        {
            isHiding = false;
            Unpossess();
        }
    }
    private void InteractTutorial()
    {
        TutorialManager.Instance.Show(TutorialStep.HideArea_Interact);
    }
    
    void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !PuzzleStateManager.Instance.IsPuzzleSolved("시계"))
        {
            hasActivated = false;

        }
        if(collision.CompareTag("Player")  && PuzzleStateManager.Instance.IsPuzzleSolved("시계"))
        {
            hasActivated = true;
        }

    }
}
