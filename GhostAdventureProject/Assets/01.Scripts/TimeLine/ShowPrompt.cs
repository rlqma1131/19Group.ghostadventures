using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPrompt : MonoBehaviour
{



    public void ShowPrompt_TimeLine(string dialog)
    {


        UIManager.Instance.PromptUI.ShowPrompt(dialog, 2f);

    }

}
