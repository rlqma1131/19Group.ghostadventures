using UnityEngine;

public class Ch3_Locker : BasePossessable
{
    [SerializeField] private GameObject q_Key;
    [SerializeField] private bool isCorrectBody;
    [SerializeField] private GameObject openObj;

    private bool isOpened = false;
    private Ch3_LockerSelector lockerSelector;
    public bool IsCorrectBody => isCorrectBody;

    private bool isSaved = false;

    protected override void Start()
    {
        base.Start();
        lockerSelector = FindObjectOfType<Ch3_LockerSelector>();
        openObj.SetActive(false);
        hasActivated = lockerSelector.HasAllClues();
        MarkActivatedChanged();
    }

    protected override void Update()
    {
        if (isOpened || lockerSelector.IsPenaltyActive || lockerSelector.IsSolved)
        {
            if (isSaved)
                return;

            hasActivated = false;
            q_Key.SetActive(false);
            MarkActivatedChanged();
            isSaved = true;
            return;
        }
        
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
        MarkActivatedChanged();
        
        lockerSelector.RegisterOpenedLocker(this);

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
        MarkActivatedChanged();
    }
    
    public void SetActivateState(bool state)
    {
        hasActivated = state;
        MarkActivatedChanged();
    }

    public override void CantPossess()
    {
        if (isOpened) return;

        // 단서가 부족할 때만 메시지
        if (!lockerSelector.HasAllClues())
        {
            UIManager.Instance.PromptUI.ShowPrompt("단서가 더 필요해", 2f);
        }
        
        if (lockerSelector.IsSolved)
        {
            UIManager.Instance.PromptUI.ShowPrompt("정말 끔찍해..", 2f);
        }
    }
}
