using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ch2_BookSlot : MonoBehaviour
{
    [SerializeField] private bool isCorrectBook;

    [Header("움직임 설정")]
    [SerializeField] private Vector3 pushedPositionOffset = new Vector3(-0.1f, -0.05f, 0);
    [SerializeField] private Vector3 pushedScale = new Vector3(0.85f, 0.95f, 1f);
    [SerializeField] private float animDuration = 0.1f;

    [Header("시각 효과")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color pushedColor = new Color(0.6f, 0.6f, 0.6f, 1f); // 어두운 색
    private Color originalColor;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isPushed = false;

    public bool IsCorrectBook => isCorrectBook;
    public bool IsPushed => isPushed;

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;
    }

    public void ToggleBook()
    {
        isPushed = !isPushed;

        if (isPushed)
        {
            transform.DOLocalMove(originalPosition + pushedPositionOffset, animDuration);
            transform.DOScale(pushedScale, animDuration);
            spriteRenderer.DOColor(pushedColor, animDuration);
        }
        else
        {
            transform.DOLocalMove(originalPosition, animDuration);
            transform.DOScale(originalScale, animDuration);
            spriteRenderer.DOColor(originalColor, animDuration);
        }
    }
}
