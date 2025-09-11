using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideButton : MonoBehaviour
{
    [SerializeField] GameObject highlight;
    private bool firstTimeHighlight = false;

    void Start()
    {
        gameObject.SetActive(false);
    }
    void Update()
    {
        if(!firstTimeHighlight)
        {
            gameObject.GetComponent<Button>().onClick.AddListener(() => { 
                highlight.SetActive(false);
                firstTimeHighlight = true; });
        }
    }

}
