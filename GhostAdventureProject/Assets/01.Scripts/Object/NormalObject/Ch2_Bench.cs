using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_Bench : MonoBehaviour
{
    [SerializeField] private CH2_SecurityGuard guard;

    void OnTriggerEnter2D(Collider2D collision)
    {
        guard.SetCondition(PersonCondition.Normal);
    }
}
