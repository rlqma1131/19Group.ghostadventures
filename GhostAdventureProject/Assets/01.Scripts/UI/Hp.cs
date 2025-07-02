using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hp : MonoBehaviour
{
    [SerializeField] private Image[] HpBar; // 목숨(2개)
    private int currenHp;

    public void SetHp(int amount) // 목숨 1개당 amount: 1
    {   
        currenHp = amount;
        currenHp = Mathf.Clamp(currenHp, 0, HpBar.Length); 
        for (int i = 0; i < HpBar.Length; i++)
        {
            HpBar[i].fillAmount = i < currenHp ? 1f : 0f; // UI에 반영.
        }
    }

}
