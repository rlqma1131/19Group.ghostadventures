using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ch3_Locker : BasePossessable
{
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
    }

    protected override void Update()
    {
        if (!hasActivated) return;
        if (!isPossessed) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isOpened)
                TryOpen();
            else
                TryClose();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryClose();
            Unpossess();
        }
    }

    public void TryOpen()
    {
        if (isOpened || lockerSelector.RemainingOpens <= 0 || lockerSelector.IsSolved)
            return;

        isOpened = true;
        lockerSelector.RemainingOpens--;

        openObj.SetActive(true);

        if (isCorrectBody)
        {
            lockerSelector.OnCorrectBodySelected();
            Unpossess();
        }
        else
        {
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
    }
    
    public void SetActivateState(bool state)
    {
        hasActivated = state;
    }
}
