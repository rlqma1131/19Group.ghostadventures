using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Rat_Event : MonoBehaviour
{
    public Ch1_Rat rat;

    // 애니메이션 이벤트에서 호출할 함수
    public void OnEscapeEnd()
    {
        if (rat != null)
            rat.OnEscapeEnd();
    }
}
