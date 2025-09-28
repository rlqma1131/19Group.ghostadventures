using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.U2D;
using TMPro;
public static class SpriteShapeRendererTweens
{
    public static Tweener DOColor(this SpriteShapeRenderer target, Color endValue, float duration)
    {
        return DOTween.To(() => target.color, x => target.color = x, endValue, duration);
    }
}
public class Ch04_Ending_BackgroundColor : MonoBehaviour
{

    public Color TargetColor;
    private Color originalColor;
    private SpriteShapeRenderer BG;
    [SerializeField] TextMeshProUGUI text;
    public Color TargetTextColor;
    private Color originalTextColor;
    private void Awake()
    {
        BG = GetComponent<SpriteShapeRenderer>();
        if (BG == null)
            Debug.LogError("Null");
    }

    private void Start()
    {

        originalColor = BG.color;
        originalTextColor = text.color;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BG.DOKill();
            text.DOKill();
            BG.DOColor(TargetColor, 0.7f).SetEase(Ease.InOutSine); ;
            text.DOColor(TargetTextColor, 0.7f).SetEase(Ease.InOutSine); ;

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BG.DOKill();
            text.DOKill();
            BG.DOColor(originalColor, 0.7f).SetEase(Ease.InOutSine); ;
            text.DOColor(originalTextColor, 0.7f).SetEase(Ease.InOutSine); ;
        }
    }

}
