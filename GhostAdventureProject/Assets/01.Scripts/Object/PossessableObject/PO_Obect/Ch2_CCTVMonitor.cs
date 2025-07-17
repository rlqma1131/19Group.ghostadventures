using UnityEngine;

public class Ch2_CCTVMonitor : MonoBehaviour
{
    [Header("CCTV 화면 순서대로")]
    public Animator[] monitorAnimators; // 각 모니터 화면 Animator

    // index번 모니터 화면과 연결된 CCTV 설정
    public void SetMonitorAnimBool(int idx, string param, bool value)
    {
        if (idx >= 0 && idx < monitorAnimators.Length && monitorAnimators[idx] != null)
            monitorAnimators[idx].SetBool(param, value);
    }
}
