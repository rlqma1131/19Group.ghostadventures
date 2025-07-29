using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum NurseState
{
    Work,
    Rest
}

public class Ch3_Nurse : MoveBasePossessable
{
    [Header("위치")]
    [SerializeField] private Transform[] workPositions; // 일 하는 두 지점
    [SerializeField] private Transform restPosition;    // 쉴 위치

    [Header("시간")]
    [SerializeField] private float waitDuration = 3f;   // 각 지점 대기 시간
    [SerializeField] private float stateChangeInterval = 15f; // 상태 변경 주기

    private PersonConditionUI condition;
    private PersonConditionHandler conditionHandler;
    private NurseState state = NurseState.Work;
    private int currentWorkIndex = 0;
    private float waitTimer = 0f;
    private float stateTimer = 0f;
    private bool isWaiting = false;

    protected override void Start()
    {
        base.Start();
        moveSpeed = 2f;
        conditionHandler = new VitalConditionHandler();
        condition = GetComponent<PersonConditionUI>();
    }

    protected override void Update()
    {
        // 빙의 상태
        if (isPossessed)
        {
            if (!PossessionSystem.Instance.CanMove)
                return;
             
            Move();

            if (Input.GetKeyDown(KeyCode.E))
            {
                zoomCamera.Priority = 5;
                Unpossess();
            }
        }

        if (isPossessed) return;

        // 빙의 상태가 아닐 때
        stateTimer += Time.deltaTime;
        if (stateTimer >= stateChangeInterval)
        {
            ToggleState();
            stateTimer = 0f;
        }

        // 일정 주기마다 상태 변경
        switch (state)
        {
            case NurseState.Work:
                condition.currentCondition = PersonCondition.Tired;
                HandleWork();
                break;
            case NurseState.Rest:
                condition.currentCondition = PersonCondition.Vital;
                HandleRest();
                break;
        }

        // 컨디션 UI & QTE 업데이트
        SetCondition(condition.currentCondition);
    }

    private void HandleWork()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitDuration)
            {
                // 다음 지점으로 이동
                isWaiting = false;
                currentWorkIndex = (currentWorkIndex + 1) % workPositions.Length;
            }
            else
            {
                // 일하기
                SetMoveAnimation(false);
                anim.Play("Work");
                return;
            }
        }

        Transform target = workPositions[currentWorkIndex];
        MoveTo(target.position);
    }

    private void HandleRest()
    {
        if (Vector2.Distance(transform.position, restPosition.position) > 0.1f)
        {
            MoveTo(restPosition.position);
        }
        else
        {
            SetMoveAnimation(false);
        }
    }

    private void MoveTo(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        SetMoveAnimation(true);
        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x < 0;

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    private void SetMoveAnimation(bool isMoving)
    {
        if (anim != null)
            anim.SetBool("Move", isMoving);
    }

    private void ToggleState()
    {
        if (state == NurseState.Work)
        {
            state = NurseState.Rest;
        }
        else
        {
            state = NurseState.Work;
            currentWorkIndex = 0;
            isWaiting = false;
        }
    }

    public void SetCondition(PersonCondition condition)
    {
        this.condition.currentCondition = condition;
        switch (condition)
        {
            case PersonCondition.Vital:
                conditionHandler = new VitalConditionHandler();
                break;
            case PersonCondition.Normal:
                conditionHandler = new NormalConditionHandler();
                break;
            case PersonCondition.Tired:
                conditionHandler = new TiredConditionHandler();
                break;
        }
        QTESettings qteSettings = conditionHandler.GetQTESettings();
        UIManager.Instance.QTE_UI_3.ApplySettings(qteSettings);
    }
}
