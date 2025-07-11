using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpriteAlphaControl : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] Sprites;

    [SerializeField] private float Alpha;
    //[SerializeField] private float Duration;

    private void Awake()
    {
        Sprites = transform.GetComponentsInChildren<SpriteRenderer>();


    }

    public void SetAlpha(float alpha)
    {
        foreach (var spriteRenderer in Sprites)
        {
            spriteRenderer.DOFade(alpha, 4);
                
        }
    }
}
