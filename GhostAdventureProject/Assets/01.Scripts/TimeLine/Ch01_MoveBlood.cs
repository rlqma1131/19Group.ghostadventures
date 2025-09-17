using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Ch01_MoveBlood : MonoBehaviour
{

    //챕터1 최종 퍼즐 풀면 피가 아래로 떨어지는 스크립트
    public void Move()
    {


        transform.DOMoveY(transform.position.y-0.7f, 0f);
        
        
    }
}
