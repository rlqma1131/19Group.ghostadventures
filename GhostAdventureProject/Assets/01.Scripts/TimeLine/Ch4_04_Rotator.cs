using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch4_04_Rotator : MonoBehaviour
{
    public Transform target;
    public float speedDegPerSec = 90f;

    private Tween rotateTween;
    private int direction = -1; // -1: 시계, 1: 반시계 회전 방향

    void Start()
    {
        if (target == null) target = transform;
        if (speedDegPerSec > 0f) StartRotateClockwise();
    }

    public void StartRotateClockwise()
    {
        direction = -1;
        RestartTween();
    }

    public void StartRotateCounterclockwise()
    {
        direction = 1;
        RestartTween();
    }

    public void StopRotation()
    {
        if (rotateTween != null)
        {
            rotateTween.Kill();
            rotateTween = null;
        }
    }

    public void SetSpeed(float newSpeedDegPerSec)
    {
        speedDegPerSec = Mathf.Max(0f, newSpeedDegPerSec);
        if (speedDegPerSec <= 0f)
        {
            StopRotation();
            return;
        }
        RestartTween();
    }

    // === 타임라인 SignalReceiver에서 호출할 함수들 ===

    // 속도와 방향 동시에 바꾸고 싶을 때
    public void SetRotationConfig(float newSpeed, bool clockwise)
    {
        speedDegPerSec = Mathf.Max(0f, newSpeed);
        direction = clockwise ? -1 : 1;
        RestartTween();
    }

    // 방향만 토글
    public void ToggleDirection()
    {
        direction *= -1;
        RestartTween();
    }

   public void RestartTween()
    {
        if (speedDegPerSec <= 0f)
        {
            StopRotation();
            return;
        }

        if (rotateTween != null) rotateTween.Kill();

        float duration = 360f / speedDegPerSec;
        float angle = 360f * direction;

        rotateTween = target.DORotate(new Vector3(0f, 0f, angle), duration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetRelative(true)
            .SetLoops(-1, LoopType.Incremental)
            .SetUpdate(true);
    }

    void OnDisable() => StopRotation();
}
