using UnityEngine;

public class Ch1_Sofa : HideArea
{
    protected override void Start()
    {
        base.Start();
        isPossessed = false;
        hasActivated = false;
    }

    public override void TriggerEvent() {
        InteractTutorial();
    }

    private void InteractTutorial()
    {
        TutorialManager.Instance.Show(TutorialStep.HideArea_Interact);
    }
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag("Player") && !SaveManager.IsPuzzleSolved("시계")) {
            hasActivated = false;
        }
        if (collision.CompareTag("Player") && SaveManager.IsPuzzleSolved("시계"))
        {
            hasActivated = true;
            MarkActivatedChanged();
        }
    }
}
