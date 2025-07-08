using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBoard_Shuffle : MonoBehaviour
{
    void Start()
    {
        ShuffleKeys();
    }

    void ShuffleKeys()
    {
        int childCount = transform.childCount;
        Transform[] children = new Transform[childCount];

        // 자식 Transform 배열에 저장
        for (int i = 0; i < childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        // Fisher-Yates 셔플 알고리즘으로 섞기
        for (int i = 0; i < childCount; i++)
        {
            int randomIndex = Random.Range(i, childCount);
            (children[i], children[randomIndex]) = (children[randomIndex], children[i]);
        }

        // 다시 SetSiblingIndex 순서대로 재배치
        for (int i = 0; i < childCount; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}
