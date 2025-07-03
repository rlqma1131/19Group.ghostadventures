using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 1. 온도조절 가능 키를 띄운다
/// 2. 컨트롤모드로 넘어간다 ( 손잡이 왼쪽- 뜨거운 / 오른쪽- 차가운 )
/// </summary>
public class Ch1_Shower : BasePossessable
{
    public bool IsHotWater => isWater && temperature == 1;

    [SerializeField] private Animator waterAnimator;
    [SerializeField] private GameObject steamEffect;
    private bool isWater = false;
    private int temperature = 0;

    protected override void Update()
    {
        base.Update();
        
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            temperature = Mathf.Max(-1, temperature - 1);
            Debug.Log("온도 조절: " + temperature);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            temperature = Mathf.Min(1, temperature + 1);
            Debug.Log("온도 조절: " + temperature);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            isWater = !isWater;
            waterAnimator?.SetBool("IsRunning", isWater); // 애니메이션 트리거 호출
            Debug.Log($"물 상태: {(isWater ? "ON" : "OFF")}, 온도: {temperature}");
        }
        
        if (isWater && temperature == 1)
        {
            if (steamEffect != null && !steamEffect.activeSelf)
                steamEffect.SetActive(true);
        }
        else
        {
            if (steamEffect != null && steamEffect.activeSelf)
                steamEffect.SetActive(false);
        }
    }
}
