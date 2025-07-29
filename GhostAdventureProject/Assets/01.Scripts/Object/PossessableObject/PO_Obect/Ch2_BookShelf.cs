using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class Ch2_BookShelf : BasePossessable
{
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    
    [SerializeField] private Ch2_BookSlot[] bookSlots;
    [SerializeField] private List<Ch2_BookSlot> correctSequence;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private GameObject doorToOpen;
    private List<Ch2_BookSlot> clickedSequence = new List<Ch2_BookSlot>();
    
    [SerializeField] private Transform moveTargetPosition;
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 0.3f;
    // private int currentIndex = 0;
    
    [SerializeField] private List<Transform> moveTargets;

    private bool isControlMode = false;

    [SerializeField] private List<ClueData> needClues;
    private bool promptShown = false;

    protected override void Update()
    {
        if (!isPossessed || !hasActivated)
        {
            //q_Key.SetActive(false);
            return;
        }
        
        if (!promptShown && HasAllClues())
        {
            UIManager.Instance.PromptUI.ShowPrompt_2("이 책들은 여기 없는데?", "책장을 잘 살펴보자");
            promptShown = true;
        }
        
        if(!isControlMode) 
            EnterControlMode();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            //isControlMode = false;
            ExitControlMode();
            Unpossess();
            promptShown = false;
        }
        // if (!isControlMode) return;
        // q_Key.SetActive(true);
    }

    private void EnterControlMode()
    {
        isControlMode = true;
        zoomCamera.Priority = 20;
        UIManager.Instance.PlayModeUI_CloseAll();
    }

    private void ExitControlMode()
    {
        isControlMode = false;
        zoomCamera.Priority = 5;
        UIManager.Instance.PlayModeUI_OpenAll();
    }
    
    private void LateUpdate()
    {
        if (!isControlMode || !Input.GetMouseButtonDown(0))
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Ch2_BookSlot slot = hit.collider.GetComponent<Ch2_BookSlot>();
            
            if (slot.IsResetBook)
            {
                ResetAllBooks();
                return;
            }

            if (!HasAllClues())
            {
                UIManager.Instance.PromptUI.ShowPrompt("아직 단서가 부족해", 1.5f);
                return;
            }
            
            slot.ToggleBook();
            
            if (slot.IsPushed)
                clickedSequence.Add(slot);
            else
                clickedSequence.Remove(slot);

            CheckPuzzleSolved();
        }
    }

    private void CheckPuzzleSolved()
    {
        if (clickedSequence.Count != correctSequence.Count)
            return;

        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (clickedSequence[i] != correctSequence[i])
                return;
        }
        
        Sequence moveSequence = DOTween.Sequence();

        for (int i = 0; i < correctSequence.Count; i++)
        {
            var slot = correctSequence[i];
            var target = moveTargets[i];

            if (slot != null && target != null && slot.booknameRenderer != null)
            {
                var imgTransform = slot.booknameRenderer.transform;

                moveSequence.Join(
                    imgTransform.DOMove(target.position, 1.0f).SetEase(Ease.InOutSine)
                );

                moveSequence.Join(
                    imgTransform.DOScale(3f, 1.0f).SetEase(Ease.OutQuad)
                );
            }
        }
        
        moveSequence.AppendInterval(2f);
        
        moveSequence.OnComplete(() =>
        {
            doorToOpen.SetActive(true);
            ExitControlMode();
            hasActivated = false;
            Unpossess();
            ConsumeClue(needClues);
            transform.DOShakePosition(shakeDuration, shakeStrength)
                     .OnComplete(() =>
                     {
                         transform.DOMove(moveTargetPosition.position, moveDuration)
                                  .SetEase(Ease.InOutSine);
                     });
        });
    }
    
    public void ResetAllBooks()
    {
        foreach (var slot in bookSlots)
        {
            if (slot.IsPushed)
                slot.ToggleBook();
        }
        // currentIndex = 0;
        clickedSequence.Clear();
    }

    private void ConsumeClue(List<ClueData> clues)
    {
        UIManager.Instance.Inventory_PlayerUI.RemoveClue(clues.ToArray());
    }

    private bool HasAllClues()
    {
        foreach (var clue in needClues)
        {
            if (!UIManager.Instance.Inventory_PlayerUI.collectedClues.Contains(clue))
                return false;
        }
        return true;
    }
}