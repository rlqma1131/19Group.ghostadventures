using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ch3_Locker : BasePossessable
{
    [SerializeField] private GameObject q_Key;
    [SerializeField] private bool isCorrectBody;
    [SerializeField] private GameObject openObj;

    private bool isOpened = false;
    private Ch3_LockerSelector lockerSelector;
    public bool IsCorrectBody => isCorrectBody;

    protected override void Start()
    {
        base.Start();
        lockerSelector = FindObjectOfType<Ch3_LockerSelector>();
        openObj.SetActive(false);
        hasActivated = lockerSelector.HasAllClues();
    }

    protected override void Update()
    {
        hasActivated = lockerSelector.HasAllClues();
        
        if (!hasActivated)
        {
            q_Key.SetActive(false);
            return;
        }
        
        if (!isPossessed)
        {
            q_Key.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isOpened)
            {
                q_Key.SetActive(false);
                TryOpen();
            }
            else
            {
                q_Key.SetActive(false);
                TryClose();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryClose();
            Unpossess();
        }
        
        q_Key.SetActive(true);
    }

    public void TryOpen()
    {
        if (isOpened || lockerSelector.RemainingOpens <= 0 || lockerSelector.IsSolved)
            return;
        
        if (!lockerSelector.HasAllClues())
            return;

        isOpened = true;
        lockerSelector.RemainingOpens--;

        openObj.SetActive(true);
        hasActivated = false;

        if (isCorrectBody)
        {
            lockerSelector.OnCorrectBodySelected();
            Unpossess();
        }
        else
        {
            UIManager.Instance.PromptUI.ShowPrompt("단서를 더 살펴보자.",2f);
            lockerSelector.OnWrongBodySelected();
            Unpossess();
        }
    }

    public void TryClose()
    {
        if (!isOpened)
            return;

        isOpened = false;
        openObj.SetActive(false);
        
        hasActivated = lockerSelector.HasAllClues();
    }
    
    public void SetActivateState(bool state)
    {
        hasActivated = state;
    }

    public override void CantPossess()
    {
        UIManager.Instance.PromptUI.ShowPrompt("단서가 더 필요해",2f);
    }
}
