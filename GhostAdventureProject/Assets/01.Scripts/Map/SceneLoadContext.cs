using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneLoadContext
{
    public static string RequestedNextScene;       // 다음에 로드할 씬 이름
    public static Color? RequestedBaseBgColor;     // (옵션) 로딩 배경 기본색
    // ▶ 이 플래그로 "방금 로딩씬을 통과했는지" 표시
    public static bool CameThroughLoading;
}
