using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch04_ChoiceUITrigger : MonoBehaviour
{


    [SerializeField] GameObject choiceUI;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(choiceUI != null)
                choiceUI.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (choiceUI != null)
                choiceUI.SetActive(true);
        }
    }


}
