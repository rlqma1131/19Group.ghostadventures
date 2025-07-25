using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance {get; private set;}
    // Start is called before the first frame update
    private HashSet<string> completedTutorials = new HashSet<string>();
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowTutorial(string tutorialID, string message, float duration = 2f)
    {
        if (completedTutorials.Contains(tutorialID))
            return;

        completedTutorials.Add(tutorialID);
        UIManager.Instance.PromptUI.ShowPrompt(message, duration);
    }

    public bool IsTutorialDone(string tutorialID)
    {
        return completedTutorials.Contains(tutorialID);
    }
}
