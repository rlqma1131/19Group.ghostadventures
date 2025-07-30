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
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if(!PuzzleStateManager.Instance.IsPuzzleSolved("시계"))
        {
            hasActivated = false;
        }

        if(PuzzleStateManager.Instance.IsPuzzleSolved("시계"))
        {
            hasActivated = true;
        }
    }

    // public override void CantPossess() 
    // { 
    //     TutorialManager.Instance.Show(TutorialStep.HideArea_QTE);
    // }
}
