using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryFragment : MonoBehaviour
{
    public MemoryData data;
    public bool isScanned = false;
    

    [Header("드랍 조각 프리팹")]
    [SerializeField] private GameObject fragmentDropPrefab;

    [Header("드랍 연출 설정")]
    [SerializeField] private Vector3 dropOffset = Vector3.zero;
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceDuration = 0.5f;

    [Header("회전 연출 설정")]
    [SerializeField] private float rotateTime = 1.2f;
    [SerializeField] private float ellipseRadiusX = 0.5f;
    [SerializeField] private float ellipseRadiusZ = 1.0f;

    [Header("흡수 연출 설정")]
    [SerializeField] private float absorbTime = 0.6f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    public void IsScanned()
    {
        if (isScanned) return;
        isScanned = true;
        MemoryManager.Instance.TryCollect(data);

        Sprite dropSprite = GetFragmentSpriteByType(data.type);
        if (fragmentDropPrefab == null || dropSprite == null) return;

        GameObject drop = Instantiate(fragmentDropPrefab, transform.position + dropOffset, Quaternion.identity);

        if (drop.TryGetComponent(out SpriteRenderer sr))
            sr.sprite = dropSprite;

        StartCoroutine(PlayDropSequence(drop));
    }

    private IEnumerator PlayDropSequence(GameObject drop)
    {
        if (drop == null) yield break;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        Vector3 startPos = drop.transform.position;

        // === 1. 튕기기 애니메이션 ===
        yield return DOTween.Sequence()
            .Append(drop.transform.DOMoveY(startPos.y + bounceHeight, bounceDuration / 2f).SetEase(Ease.OutQuad))
            .Append(drop.transform.DOMoveY(startPos.y, bounceDuration / 2f).SetEase(Ease.InQuad))
            .Join(drop.transform.DOPunchScale(Vector3.one * 0.1f, bounceDuration, 5, 1))
            .WaitForCompletion();

        // === 2. 타원 궤도로 회전 ===
        Vector3 center = startPos;
        Vector3 local = drop.transform.position - center;

        // 시작 각도 계산
        float startAngleRad = Mathf.Atan2(local.z / ellipseRadiusZ, local.x / ellipseRadiusX);
        float startAngleDeg = startAngleRad * Mathf.Rad2Deg;
        float currentAngle = startAngleDeg;

        // 시작 위치 계산
        float rad = startAngleDeg * Mathf.Deg2Rad;
        Vector3 initialOffset = new Vector3(Mathf.Cos(rad) * ellipseRadiusX, 0f, Mathf.Sin(rad) * ellipseRadiusZ);
        Vector3 initialPos = center + new Vector3(initialOffset.x, 0f, 0f);

        // 현재 위치에서 회전 궤도 시작점으로 부드럽게 이동
        yield return drop.transform.DOMove(initialPos, 0.1f).SetEase(Ease.InOutSine).WaitForCompletion();

        // 반시계 방향 회전: angle 감소
        Tween rotate = DOTween.To(() => currentAngle, x =>
        {
            currentAngle = x;
            float r = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(r) * ellipseRadiusX, 0f, Mathf.Sin(r) * ellipseRadiusZ);

            drop.transform.position = center + new Vector3(offset.x, 0f, 0f);

            if (drop.TryGetComponent(out SpriteRenderer sr))
                sr.sortingOrder = (offset.z > 0) ? 1 : -1;

        }, startAngleDeg - 360f, rotateTime).SetEase(Ease.InOutSine);
        yield return rotate.WaitForCompletion();

        // === 3. 플레이어에게 흡수 ===
        Vector3 target = player.transform.position;
        var absorb = DOTween.Sequence()
            .Append(drop.transform.DOMove(target, absorbTime).SetEase(Ease.InCubic))
            .Join(drop.transform.DOScale(Vector3.zero, absorbTime).SetEase(Ease.InBack));

        if (drop.TryGetComponent(out SpriteRenderer finalSR))
            absorb.Join(finalSR.DOFade(0f, absorbTime));
        yield return absorb.WaitForCompletion();
        yield return CutsceneManager.Instance.PlayCutscene(); // 컷신 재생

        Destroy(drop);
        SceneManager.LoadScene(data.CutSceneName, LoadSceneMode.Additive); // 스캔 완료 후 씬 전환
        Time.timeScale = 0;
        ApplyMemoryEffect(); // 메모리 효과 적용
    }

    private Sprite GetFragmentSpriteByType(MemoryData.MemoryType type)
    {
        return type switch
        {
            MemoryData.MemoryType.Positive => data.PositiveFragmentSprite,
            MemoryData.MemoryType.Negative => data.NegativeFragmentSprite,
            MemoryData.MemoryType.Fake => data.FakeFragmentSprite,
            _ => null
        };
    }

    public void ApplyMemoryEffect()
    {
        switch (data.type)
        {
            case MemoryData.MemoryType.Positive:

                Debug.Log($"MemoryFragment: {data.memoryID} - 스캔 완료!"); // 디버그용 로그
                //퍼즐 조건 해금
                break;

            case MemoryData.MemoryType.Negative:
                //ApplyDebuff(); // 적 추적 활성화 등
                break;

            case MemoryData.MemoryType.Fake:
                //FakeEndingManager.Instance.CollectFakeMemory(data.memoryID);
                break;
        }
    }
}
